# ChatHaunt — Devpost paste copy

This is a local draft for the **Codex x TwitchCon Hackathon**. It has not been sent to Devpost. The existing Devpost project is a draft at `https://devpost.com/software/chathaunt`.

## A. Project name

ChatHaunt

## B. One-line tagline

A Twitch-powered horror house where chat decides when the lights die and which scare comes next.

## C. Full project description

ChatHaunt turns a small first-person horror game into a live conversation between a streamer and their viewers. The player explores a connected bedroom, hallway, living room, kitchen, and entrance in Unity. Sliding doors, a flashlight, dark room lighting, ambient audio, and a standing GhostWoman create a readable horror scene without requiring a long story or puzzle sequence.

The audience can change the scene in real time. A viewer's `!lights` message cuts the house lights and leaves the player with only the flashlight. The streamer can open a 15-second vote from a simple local dashboard. Viewers type `1`, `2`, or `3` in ordinary Twitch chat to choose blackout, GhostWoman with a scream, or a spatial door-creak scare. The dashboard counts ballots, shows the winner, and the winning event runs in Unity. Each viewer's latest valid vote counts once, so the interaction works even on a normal Twitch channel without native polls.

Under the hood, Twitch EventSub delivers `channel.chat.message` over WebSocket to a small Node.js bridge. Node filters messages and sends event IDs through a localhost HTTP poll-and-acknowledgement connection to the Unity game. Unity reports its actual light state back to the dashboard. The bridge handles reconnects and failed acknowledgements for a live demo. Horror events and room anchors are modular so the same pattern can support other streamer-controlled scares.

ChatHaunt makes viewers participants in the stream rather than spectators. The streamer gets a clear, one-click control, viewers need only chat, and the audience sees the effect inside the game. The demo uses a compact house and reused, documented assets so the interaction itself remains the focus.

## D. Built with technologies

OpenAI Codex; Unity 6.3 LTS (6000.3.11f1); C#; Universal Render Pipeline; Unity Input System; Node.js; JavaScript; Twitch EventSub WebSocket and `channel.chat.message`; Twitch OAuth device flow (`user:read:chat`); localhost HTTP; Server-Sent Events; HTML and CSS; Unity MCP during development.

## E. Repository URL

**PENDING — no remote repository was connected at the start of this audit.** Replace this line with the verified GitHub URL after publication or judge access is arranged.

## F. Explain how your project meets the main technology requirement

I used OpenAI Codex as a development collaborator to build and verify ChatHaunt during the hackathon. Codex helped turn the idea into a small Unity milestone, recreated the first-person controller, connected house, sliding doors, lighting and scare events, inspected the GhostWoman dependency chain, and wired the supplied audio. It then built the Node bridge, integrated Twitch's modern EventSub WebSocket `channel.chat.message` path, added the local Unity poll-and-acknowledgement connection and live dashboard, and debugged issues found in Play Mode, including door interaction, light-state reporting, ambient playback, and blackout persistence. Unity MCP and local Node tests were used to inspect and verify behavior; real Twitch messages separately proved `!lights` and a GhostWoman vote reached Unity.

The running project connects to Twitch through EventSub, not legacy IRC or a mock poll. A broadcaster-authorized `user:read:chat` token lets Node receive chat messages. `!lights` triggers a blackout immediately. During a 15-second chat vote, viewers type `1`, `2`, or `3`; Node counts ballots, selects a winner, and forwards its existing horror event ID over localhost to Unity. Codex is the tool used to create and test the project, while Twitch is the live audience input.

## G. Explain how streamers and their community on Twitch use your project

The streamer opens ChatHaunt in Unity and the local dashboard in a browser. They play through the house with WASD, mouse, doors and flashlight. The dashboard shows Twitch and Unity connection status, current room-light state, and vote results. Viewers can type `!lights` to force a persistent blackout, making the streamer rely on the flashlight. When the streamer clicks **Start Vote · 15s**, chat chooses the next scare: `1` blackout, `2` a standing hallway GhostWoman with a scream, or `3` a door-creak sound at the living-room door. Votes are counted per Twitch login, and the winning outcome runs in the game when the timer ends. No Twitch Affiliate/Partner status, extension installation, or special viewer login is needed beyond ordinary Twitch chat.

## H. Short testing instructions for judges

Clone the repository, run `npm ci` and `npm test` in its root, then open `unity/` in Unity 6000.3.11f1 and load `Assets/ChatHaunt/Scenes/ChatHauntHouse.unity`. Configure a local `.env` from `.env.example` using your own Twitch Public app and broadcaster-authorized `user:read:chat` token; never commit it. Run `npm start`, visit `http://127.0.0.1:3177`, and press Play in Unity. Wait for **Twitch: connected** and **Unity Game: Connected**. Send `!lights` in that channel to see the house stay dark, or click **Start Vote · 15s**, type `2` in chat, and watch GhostWoman appear with a scream when the vote closes. The dashboard also supports local simulated ballots for debugging; these are clearly separate from real Twitch messages.

## Architecture and design notes

Twitch Chat → EventSub WebSocket → Node bridge → loopback HTTP → Unity room/event registry. Node pushes status to the browser with Server-Sent Events. The dashboard is a control and monitoring surface, while the Unity Game view is the actual horror scene.

## Known limitations

- Native Twitch polls require Affiliate/Partner status. ChatHaunt's 1/2/3 voting uses regular chat, which works on a normal broadcaster channel.
- This is a local Unity Editor demo; there is no hosted game or standalone build. The token expires and must be refreshed for later sessions.
- Area51Scanner predates the hackathon. Only its documented character and selected house materials were reused; the ChatHaunt project, house, and event wiring are new. See `docs/ASSET_PROVENANCE.md`.

## Demo media and remaining official fields

- **Demo video:** recorded by the project owner; upload to YouTube as a publicly visible video of at most three minutes and paste its URL into Devpost. Verify it shows Twitch input, Unity response, and Codex use.
- **Screenshots:** dashboard with both connections, vote count/result, lights-off flashlight view, standing GhostWoman in the hallway.
- **Codex session ID for the main build thread:** `01a18350-a99a-79b2-b152-fe8859ff9e96` (also confirm with `/status` in this thread).
- **Official form choices for the submitter:** submitter type, country of residence, skill/knowledge impact rating, likelihood of using Codex again, and eligibility/age/sponsor statements. These are personal answers and are intentionally not filled here.
- **Repository:** insert the verified URL and ensure it is public or, if private, share it with `testing@devpost.com` as the official form requires.
