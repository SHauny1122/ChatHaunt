import test from 'node:test';
import assert from 'node:assert/strict';
import { EventEmitter } from 'node:events';
import { connectTwitch, createChatSubscription, lookupBroadcasterId, validateTwitchToken } from '../src/twitch.js';

test('validates a user token with only user:read:chat', async () => {
  const previous = globalThis.fetch;
  globalThis.fetch = async () => new Response(JSON.stringify({
    client_id: 'app', user_id: 'reader-id', login: 'reader', scopes: ['user:read:chat']
  }), { status: 200 });
  try {
    const identity = await validateTwitchToken({ token: 'private-token', clientId: 'app', username: 'reader' });
    assert.equal(identity.user_id, 'reader-id');
  } finally { globalThis.fetch = previous; }
});

test('EventSub welcome subscribes; a matching notification dispatches once; directed reconnect retains subscription', async () => {
  class FakeSocket extends EventEmitter {
    static OPEN = 1;
    static sockets = [];
    constructor(url) { super(); this.url = url; this.readyState = 1; FakeSocket.sockets.push(this); }
    close() { this.readyState = 3; this.emit('close'); }
    terminate() { this.close(); }
    receive(message) { this.emit('message', Buffer.from(JSON.stringify(message))); }
  }
  const statuses = [], users = [], subscriptions = [];
  const stop = connectTwitch({ token: 'private-token', clientId: 'app', username: 'streamer',
    readerId: '123', broadcasterId: '123', onStatus: status => statuses.push(status),
    onMessage: message => users.push(message), WebSocketClient: FakeSocket,
    subscribe: async details => { subscriptions.push(details); }, eventsubUrl: 'wss://example.test/ws' });
  try {
    const first = FakeSocket.sockets[0];
    first.receive({ metadata: { message_type: 'session_welcome' }, payload: { session: { id: 'session-1', keepalive_timeout_seconds: 30 } } });
    await Promise.resolve();
    assert.equal(subscriptions[0].sessionId, 'session-1');
    assert.equal(statuses.at(-1), 'connected');
    const event = { metadata: { message_type: 'notification', subscription_type: 'channel.chat.message', message_id: 'same-id' },
      payload: { subscription: { type: 'channel.chat.message' }, event: { broadcaster_user_id: '123', chatter_user_login: 'viewer', message: { text: '!lights' } } } };
    first.receive(event); first.receive(event);
    assert.deepEqual(users, [{ user: 'viewer', text: '!lights' }]);
    first.receive({ ...event, metadata: { ...event.metadata, message_id: 'vote-id' },
      payload: { ...event.payload, event: { ...event.payload.event, message: { text: '2' } } } });
    assert.deepEqual(users, [{ user: 'viewer', text: '!lights' }, { user: 'viewer', text: '2' }]);
    first.receive({ metadata: { message_type: 'session_reconnect' }, payload: { session: { reconnect_url: 'wss://example.test/reconnect' } } });
    const second = FakeSocket.sockets[1];
    assert.equal(second.url, 'wss://example.test/reconnect');
    second.receive({ metadata: { message_type: 'session_welcome' }, payload: { session: { id: 'session-2' } } });
    assert.equal(subscriptions.length, 1);
    assert.equal(first.readyState, 3);
  } finally { stop(); }
});

test('resolves the channel and creates a websocket channel.chat.message subscription', async () => {
  const previous = globalThis.fetch;
  const calls = [];
  globalThis.fetch = async (url, options) => {
    calls.push({ url, options });
    return new Response(JSON.stringify(calls.length === 1 ? { data: [{ id: 'channel-id' }] } : { data: [{ id: 'subscription-id' }] }), { status: calls.length === 1 ? 200 : 202 });
  };
  try {
    const broadcasterId = await lookupBroadcasterId({ token: 'private-token', clientId: 'app', channel: 'streamer' });
    await createChatSubscription({ token: 'private-token', clientId: 'app', broadcasterId, readerId: 'reader-id', sessionId: 'session-id' });
    assert.equal(broadcasterId, 'channel-id');
    assert.deepEqual(JSON.parse(calls[1].options.body), {
      type: 'channel.chat.message', version: '1',
      condition: { broadcaster_user_id: 'channel-id', user_id: 'reader-id' },
      transport: { method: 'websocket', session_id: 'session-id' }
    });
    assert.equal(calls[1].options.headers.Authorization, 'Bearer private-token');
  } finally { globalThis.fetch = previous; }
});
