import test from 'node:test';
import assert from 'node:assert/strict';
import { isLightsCommand } from '../src/chat.js';

const notice = {
  metadata: { message_type: 'notification', subscription_type: 'channel.chat.message' },
  payload: { subscription: { type: 'channel.chat.message' },
    event: { broadcaster_user_id: '123', chatter_user_login: 'viewer', message: { text: '!lights' } } }
};

test('only complete !lights EventSub chat messages from the selected channel match', () => {
  assert.equal(isLightsCommand(notice, '123'), true);
  assert.equal(isLightsCommand(notice, 'other'), false);
  assert.equal(isLightsCommand({ ...notice, metadata: { message_type: 'session_keepalive' } }, '123'), false);
  assert.equal(isLightsCommand({ ...notice, payload: { ...notice.payload, event: { ...notice.payload.event, message: { text: '!lights now' } } } }, '123'), false);
});
