import test from 'node:test';
import assert from 'node:assert/strict';
import { createVote } from '../src/vote.js';

test('counts one current vote per viewer, chooses a deterministic winner and dispatches once', () => {
  const winners = [];
  let expire;
  const vote = createVote({ onChange() {}, onWinner: result => winners.push(result),
    setTimer: callback => { expire = callback; return 1; }, clearTimer() {} });
  assert.equal(vote.start(30), true);
  assert.equal(vote.start(30), false);
  assert.equal(vote.submit('Alice', '1'), true);
  assert.equal(vote.submit('alice', '2'), true);
  assert.equal(vote.submit('Bob', '2'), true);
  assert.equal(vote.submit('Cara', '3'), true);
  assert.equal(vote.submit('Dana', '!lights'), false);
  assert.deepEqual(vote.snapshot().counts, { 1: 0, 2: 2, 3: 1 });
  expire();
  assert.deepEqual(winners, [{ winner: 2, eventId: 'HALLWAY_GHOST', total: 3 }]);
  assert.equal(vote.submit('Eve', '1'), false);
  assert.equal(vote.finish(), null);
});

test('a vote with no ballots sends no Unity event', () => {
  const winners = [];
  let expire;
  const vote = createVote({ onChange() {}, onWinner: result => winners.push(result),
    setTimer: callback => { expire = callback; return 1; }, clearTimer() {} });
  vote.start(1);
  expire();
  assert.equal(vote.snapshot().result.winner, null);
  assert.deepEqual(winners, []);
});

test('option 3 selects the distinct door-slam event', () => {
  const winners = [];
  let expire;
  const vote = createVote({ onChange() {}, onWinner: result => winners.push(result),
    setTimer: callback => { expire = callback; return 1; }, clearTimer() {} });
  vote.start(1);
  vote.submit('viewer', '3');
  expire();
  assert.deepEqual(winners, [{ winner: 3, eventId: 'DOOR_SLAM', total: 1 }]);
});
