import 'dotenv/config';
import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import { connectTwitch, lookupBroadcasterId, validateTwitchToken } from './twitch.js';
import { createVote } from './vote.js';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const port = Number(process.env.PORT || 3177);
const channel = process.env.TWITCH_CHANNEL?.toLowerCase();
const username = process.env.TWITCH_READER_USERNAME?.toLowerCase();
const token = process.env.TWITCH_OAUTH_TOKEN?.replace(/^oauth:/i, '');
const clientId = process.env.TWITCH_CLIENT_ID;
const configured = Boolean(channel && username && token && clientId);
const validLogin = value => /^[a-z0-9_]{3,25}$/.test(value || '');

if (configured && (!validLogin(channel) || !validLogin(username))) {
  throw new Error('TWITCH_CHANNEL and TWITCH_READER_USERNAME must be Twitch login names.');
}
if (!Number.isInteger(port) || port < 1 || port > 65535) throw new Error('PORT must be a valid port number.');

const state = { lightsOn: true, unityConnected: false, twitchStatus: configured ? 'validating' : 'not configured', lastEvent: null, vote: null };
const clients = new Set();
let lastUnityPollAt = 0;
let nextUnityId = 1;
let pendingCommand = null;
const unityWaiters = new Map();

function dispatchUnity(command, user, source) {
  if (unityWaiters.size) return Promise.resolve({ triggered: false, reason: 'Unity busy' });
  const id = nextUnityId++;
  pendingCommand = { id, command };
  state.lastEvent = { user, source, command, at: new Date().toISOString() };
  broadcast();
  console.log(`Unity command: ${command} from ${user} (${source})`);
  return new Promise(resolve => {
    const timeout = setTimeout(() => {
      unityWaiters.delete(id);
      if (pendingCommand?.id === id) pendingCommand = null;
      resolve({ triggered: false, reason: 'Unity acknowledgement timed out' });
    }, 5000);
    unityWaiters.set(id, result => { clearTimeout(timeout); resolve(result); });
  });
}

function dispatchLights(user, source) { return dispatchUnity('!lights', user, source); }

const vote = createVote({
  onChange(snapshot) { state.vote = snapshot; broadcast(); },
  onWinner({ winner, eventId, total }) {
    console.log(`Vote winner: ${winner} (${eventId}, ${total} voter(s))`);
    dispatchUnity(eventId, 'viewers', 'vote').then(result =>
      console.log(`Unity vote result: ${result.triggered ? 'triggered' : result.reason}`));
  }
});
state.vote = vote.snapshot();

async function readSmallBody(request) {
  let body = '';
  for await (const chunk of request) {
    body += chunk;
    if (body.length > 256) throw new Error('Request body too large');
  }
  return body;
}
function broadcast() {
  const message = `data: ${JSON.stringify(state)}\n\n`;
  for (const client of clients) client.write(message);
}
setInterval(() => {
  if (state.unityConnected && Date.now() - lastUnityPollAt > 3000) {
    state.unityConnected = false;
    broadcast();
  }
}, 1000);

const assets = new Map([
  ['/', ['public/index.html', 'text/html; charset=utf-8']],
  ['/app.css', ['public/app.css', 'text/css; charset=utf-8']],
  ['/app.js', ['public/app.js', 'text/javascript; charset=utf-8']]
]);

const server = createServer(async (request, response) => {
  const url = new URL(request.url, 'http://localhost');
  const path = url.pathname;
  if (path === '/unity/poll' && request.method === 'GET') {
    lastUnityPollAt = Date.now();
    if (!state.unityConnected) { state.unityConnected = true; broadcast(); }
    const reported = url.searchParams.get('lightsOn');
    if (reported === 'true' || reported === 'false') {
      const lightsOn = reported === 'true';
      if (state.lightsOn !== lightsOn) { state.lightsOn = lightsOn; broadcast(); }
    }
    const message = pendingCommand;
    pendingCommand = null;
    response.writeHead(200, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' });
    response.end(JSON.stringify(message || {}));
    return;
  }
  if (path === '/unity/ack' && request.method === 'POST') {
    try {
      const { id, triggered } = JSON.parse(await readSmallBody(request));
      const finish = unityWaiters.get(id);
      if (!finish || typeof triggered !== 'boolean') { response.writeHead(400).end(); return; }
      unityWaiters.delete(id);
      finish({ triggered, reason: triggered ? undefined : 'Unity event unavailable or already running' });
      response.writeHead(204).end();
    } catch { response.writeHead(400).end(); }
    return;
  }
  if (path === '/simulate/lights' && request.method === 'POST') {
    try {
      if ((await readSmallBody(request)).trim() !== '!lights') { response.writeHead(400).end('Expected !lights'); return; }
      const result = await dispatchLights('local simulation', 'simulation');
      response.writeHead(result.triggered ? 200 : 503, { 'Content-Type': 'application/json' });
      response.end(JSON.stringify(result));
    } catch { response.writeHead(400).end(); }
    return;
  }
  if (path === '/vote/start' && request.method === 'POST') {
    try {
      const body = (await readSmallBody(request)).trim();
      const seconds = body ? JSON.parse(body).seconds : 30;
      if (!vote.start(seconds)) { response.writeHead(409).end('Vote already active or invalid duration'); return; }
      response.writeHead(200, { 'Content-Type': 'application/json' }).end(JSON.stringify(vote.snapshot()));
    } catch { response.writeHead(400).end(); }
    return;
  }
  if (path === '/simulate/vote' && request.method === 'POST') {
    try {
      const { user, choice } = JSON.parse(await readSmallBody(request));
      if (typeof user !== 'string' || !vote.submit(user, String(choice))) { response.writeHead(400).end(); return; }
      response.writeHead(200, { 'Content-Type': 'application/json' }).end(JSON.stringify(vote.snapshot()));
    } catch { response.writeHead(400).end(); }
    return;
  }
  if (request.method !== 'GET') { response.writeHead(405).end(); return; }
  if (path === '/events') {
    response.writeHead(200, {
      'Content-Type': 'text/event-stream',
      'Cache-Control': 'no-cache',
      Connection: 'keep-alive'
    });
    clients.add(response);
    response.write(`data: ${JSON.stringify(state)}\n\n`);
    request.on('close', () => clients.delete(response));
    return;
  }
  if (path === '/state') {
    response.writeHead(200, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' });
    response.end(JSON.stringify(state));
    return;
  }
  const asset = assets.get(path);
  if (!asset) { response.writeHead(404).end(); return; }
  try {
    const body = await readFile(join(root, asset[0]));
    response.writeHead(200, { 'Content-Type': asset[1], 'Cache-Control': 'no-store' });
    response.end(body);
  } catch (error) {
    console.error(error);
    response.writeHead(500).end();
  }
});

server.listen(port, '127.0.0.1', () => {
  console.log(`ChatHaunt demo: http://127.0.0.1:${port}`);
  if (!configured) {
    console.log('Twitch is not configured. See README.md for .env setup.');
    return;
  }
  validateTwitchToken({ token, clientId, username }).then(async identity => {
    const broadcasterId = await lookupBroadcasterId({ token, clientId, channel });
    if (identity.user_id !== broadcasterId)
      throw new Error('Installed EventSub setup requires a user token authorized by the channel broadcaster.');
    connectTwitch({
      token, clientId, username, readerId: identity.user_id, broadcasterId,
      onStatus(status) { state.twitchStatus = status; broadcast(); },
      onMessage({ user, text }) {
        if (text === '!lights') dispatchLights(user, 'twitch').then(result => {
          console.log(`Unity !lights result: ${result.triggered ? 'triggered' : result.reason}`);
        });
        else if (vote.submit(user, text)) console.log(`Vote: ${user} chose ${text}`);
      }
    });
  }).catch(error => {
    state.twitchStatus = 'validation failed';
    broadcast();
    console.error(error.message);
  });
});
