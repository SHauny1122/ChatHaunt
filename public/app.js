const twitchStatus = document.getElementById('twitch-status');
const unityStatus = document.getElementById('unity-status');
const lightStatus = document.getElementById('light-status');
const voteStatus = document.getElementById('vote-status');
const voteTimer = document.getElementById('vote-timer');
const startVoteButton = document.getElementById('start-vote');
const voteActionStatus = document.getElementById('vote-action-status');
const voteResult = document.getElementById('vote-result');
const lastAction = document.getElementById('last-action');
const counts = [1, 2, 3].map(choice => document.getElementById(`count-${choice}`));
const stream = new EventSource('/events');
let currentVote = null;
let startingVote = false;

startVoteButton.addEventListener('click', async () => {
  startingVote = true;
  startVoteButton.disabled = true;
  voteActionStatus.textContent = '';
  try {
    const response = await fetch('/vote/start', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ seconds: 15 })
    });
    if (!response.ok) throw new Error(response.status === 409 ? 'A vote is already open.' : 'Could not start the vote.');
  } catch (error) {
    voteActionStatus.textContent = error.message;
  } finally {
    startingVote = false;
    startVoteButton.disabled = currentVote?.active ?? false;
  }
});

function updateTimer() {
  if (!currentVote?.active) return;
  const seconds = Math.max(0, Math.ceil((Date.parse(currentVote.endsAt) - Date.now()) / 1000));
  voteTimer.textContent = `${seconds}s remaining · type 1, 2, or 3 in Twitch chat`;
}

stream.onmessage = ({ data }) => {
  const state = JSON.parse(data);
  twitchStatus.textContent = `Twitch: ${state.twitchStatus}`;
  twitchStatus.classList.toggle('bad', state.twitchStatus !== 'connected');
  unityStatus.textContent = state.unityConnected ? 'Connected' : 'Waiting for Unity';
  lightStatus.textContent = state.unityConnected ? (state.lightsOn ? 'ON' : 'OFF') : 'Unknown';
  lightStatus.classList.toggle('off', state.unityConnected && !state.lightsOn);
  currentVote = state.vote;
  voteStatus.textContent = currentVote?.active ? 'Voting now' : 'Closed';
  voteTimer.textContent = currentVote?.active ? '' : 'Click Start Vote to let chat choose';
  startVoteButton.disabled = startingVote || Boolean(currentVote?.active);
  if (currentVote?.active) voteActionStatus.textContent = '';
  updateTimer();
  [1, 2, 3].forEach((choice, index) => { counts[index].textContent = currentVote?.counts?.[choice] ?? 0; });
  const result = currentVote?.result;
  voteResult.textContent = result?.winner
    ? `Winner: ${result.winner} · ${result.eventId.replaceAll('_', ' ')} · ${result.total} viewer${result.total === 1 ? '' : 's'}`
    : result ? 'No votes were cast.' : 'No vote result yet.';
  lastAction.textContent = state.lastEvent
    ? `${state.lastEvent.user} · ${state.lastEvent.command} (${state.lastEvent.source})`
    : 'Waiting for chat.';
};

stream.onerror = () => {
  twitchStatus.textContent = 'Local dashboard disconnected';
  twitchStatus.classList.add('bad');
  unityStatus.textContent = 'Unknown';
  lightStatus.textContent = 'Unknown';
  startVoteButton.disabled = true;
};
setInterval(updateTimer, 250);
