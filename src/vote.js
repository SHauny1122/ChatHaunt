export const VOTE_EVENTS = Object.freeze({
  1: 'LIGHTS_HALLWAY_OFF',
  2: 'HALLWAY_GHOST',
  3: 'DOOR_SLAM'
});

export function createVote({ onChange, onWinner, setTimer = setTimeout, clearTimer = clearTimeout }) {
  let timer = null;
  let endsAt = null;
  let ballots = new Map();
  let result = null;

  const snapshot = () => {
    const counts = { 1: 0, 2: 0, 3: 0 };
    for (const choice of ballots.values()) counts[choice]++;
    return { active: timer !== null, endsAt, counts, result };
  };
  const changed = () => onChange(snapshot());

  function finish() {
    if (timer === null) return null;
    clearTimer(timer);
    timer = null;
    const counts = snapshot().counts;
    const total = counts[1] + counts[2] + counts[3];
    const winner = total ? [1, 2, 3].reduce((best, choice) => counts[choice] > counts[best] ? choice : best, 1) : null;
    result = { winner, eventId: winner ? VOTE_EVENTS[winner] : null, total };
    endsAt = null;
    changed();
    if (winner) onWinner(result);
    return result;
  }

  return {
    snapshot,
    start(seconds = 30) {
      if (timer !== null || !Number.isInteger(seconds) || seconds < 1 || seconds > 60) return false;
      ballots = new Map();
      result = null;
      endsAt = new Date(Date.now() + seconds * 1000).toISOString();
      timer = setTimer(finish, seconds * 1000);
      changed();
      return true;
    },
    submit(user, text) {
      if (timer === null || !/^[123]$/.test(text?.trim() || '') || !user) return false;
      ballots.set(user.toLowerCase(), Number(text.trim()));
      changed();
      return true;
    },
    finish,
    stop() { if (timer !== null) clearTimer(timer); timer = null; }
  };
}
