import WebSocket from 'ws';
import { chatMessage } from './chat.js';

const EVENTSUB_URL = 'wss://eventsub.wss.twitch.tv/ws';

export async function validateTwitchToken({ token, clientId, username }) {
  const response = await fetch('https://id.twitch.tv/oauth2/validate', {
    headers: { Authorization: `OAuth ${token}` }, signal: AbortSignal.timeout(10000)
  });
  if (!response.ok) throw new Error(`Twitch token validation failed (HTTP ${response.status}).`);
  const data = await response.json();
  if (data.client_id !== clientId) throw new Error('Token belongs to a different Twitch app client ID.');
  if (!data.user_id || (username && data.login?.toLowerCase() !== username.toLowerCase()))
    throw new Error('TWITCH_READER_USERNAME must match the account that authorized the user token.');
  if (!data.scopes?.includes('user:read:chat')) throw new Error('Twitch token is missing user:read:chat.');
  return data;
}

export async function lookupBroadcasterId({ token, clientId, channel }) {
  const response = await fetch(`https://api.twitch.tv/helix/users?login=${encodeURIComponent(channel)}`, {
    headers: { Authorization: `Bearer ${token}`, 'Client-Id': clientId }, signal: AbortSignal.timeout(10000)
  });
  if (!response.ok) throw new Error(`Twitch channel lookup failed (HTTP ${response.status}).`);
  const data = await response.json();
  if (!data.data?.[0]?.id) throw new Error(`Twitch channel ${channel} was not found.`);
  return data.data[0].id;
}

export async function createChatSubscription({ token, clientId, broadcasterId, readerId, sessionId }) {
  const response = await fetch('https://api.twitch.tv/helix/eventsub/subscriptions', {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}`, 'Client-Id': clientId, 'Content-Type': 'application/json' },
    body: JSON.stringify({ type: 'channel.chat.message', version: '1',
      condition: { broadcaster_user_id: broadcasterId, user_id: readerId },
      transport: { method: 'websocket', session_id: sessionId } }),
    signal: AbortSignal.timeout(8000)
  });
  if (!response.ok) throw new Error(`EventSub subscription failed (HTTP ${response.status}): ${(await response.text()).slice(0, 300)}`);
  return response.json();
}

export function connectTwitch({ token, clientId, username, readerId, broadcasterId, onStatus, onMessage,
  WebSocketClient = WebSocket, subscribe = createChatSubscription, eventsubUrl = EVENTSUB_URL }) {
  let socket, replacement, retryTimer, validationTimer;
  let stopped = false, attempt = 0;
  const seen = new Map();

  function retry() {
    if (stopped || retryTimer) return;
    onStatus('reconnecting');
    const delay = Math.min(30000, 1000 * 2 ** Math.min(attempt++, 5));
    retryTimer = setTimeout(() => { retryTimer = null; open(eventsubUrl, false); }, delay);
  }

  function open(url, handoff) {
    if (stopped) return;
    if (!handoff) onStatus('connecting');
    const ws = new WebSocketClient(url);
    if (handoff) replacement = ws; else socket = ws;
    let lastMessage = Date.now(), keepaliveSeconds = 30;
    const health = setInterval(() => {
      if (ws.readyState === WebSocketClient.OPEN && Date.now() - lastMessage > (keepaliveSeconds + 10) * 1000) ws.terminate();
    }, 5000);

    ws.on('message', async raw => {
      lastMessage = Date.now();
      let message;
      try { message = JSON.parse(raw.toString()); } catch { return; }
      const type = message.metadata?.message_type;
      if (type === 'session_welcome') {
        keepaliveSeconds = message.payload?.session?.keepalive_timeout_seconds || 30;
        if (handoff) {
          const previous = socket;
          socket = ws;
          replacement = null;
          previous?.close();
          onStatus('connected');
        } else {
          try {
            await subscribe({ token, clientId, broadcasterId, readerId, sessionId: message.payload.session.id });
            attempt = 0;
            onStatus('connected');
          } catch (error) {
            console.error(error.message);
            onStatus('subscription failed');
            ws.close();
          }
        }
      } else if (type === 'session_reconnect' && ws === socket) {
        const nextUrl = message.payload?.session?.reconnect_url;
        if (nextUrl && !replacement) open(nextUrl, true);
      } else if (type === 'notification' && chatMessage(message, broadcasterId)) {
        const id = message.metadata?.message_id;
        if (id && seen.has(id)) return;
        if (id) seen.set(id, Date.now());
        for (const [key, time] of seen) if (Date.now() - time > 120000) seen.delete(key);
        onMessage(chatMessage(message, broadcasterId));
      } else if (type === 'revocation') {
        onStatus('subscription revoked');
        console.error(`EventSub subscription revoked: ${message.payload?.subscription?.status || 'unknown'}`);
      }
    });
    ws.on('error', error => console.error(`EventSub connection: ${error.message}`));
    ws.on('close', () => {
      clearInterval(health);
      if (stopped) return;
      if (ws === replacement) { replacement = null; return; }
      if (ws === socket) { socket = null; retry(); }
    });
  }

  open(eventsubUrl, false);
  validationTimer = setInterval(async () => {
    try { await validateTwitchToken({ token, clientId, username }); }
    catch (error) {
      console.error(`Twitch token revalidation: ${error.message}`);
      onStatus('validation failed');
      stopped = true;
      socket?.close(); replacement?.close();
      clearInterval(validationTimer);
    }
  }, 3600000);
  return () => {
    stopped = true;
    clearTimeout(retryTimer); clearInterval(validationTimer);
    socket?.close(); replacement?.close();
  };
}
