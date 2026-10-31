# ChatHaunt

ChatHaunt is a compact first-person horror house where a Twitch audience changes what the player experiences in real time. A streamer explores the Unity scene while viewers send `!lights` or vote for a scare in chat. The local dashboard shows connection status, vote counts, the winner, and the light state.

```text
Twitch chat → channel.chat.message EventSub WebSocket → Node bridge
            → localhost HTTP poll/ack → ChatHaunt Unity scene → horror event
```

The dashboard at `http://127.0.0.1:3177` is the streamer's control page; the game runs in Unity. Node listens only on loopback. It does not send messages to Twitch.

## What viewers can do

| Chat input | Result |
| --- | --- |
| `!lights` | Cuts all five room lights. They remain off; the player uses the flashlight. |
| `1` during a vote | Votes for the blackout. |
| `2` during a vote | Votes for standing hallway GhostWoman with a scream. |
| `3` during a vote | Votes for the spatial door-creak scare sound. |

The streamer clicks **Start Vote · 15s** on the dashboard. Each viewer's latest valid `1`/`2`/`3` ballot counts once. At 15 seconds, the highest count wins; ties favor the lowest number. With no votes, no event fires. The winner uses the same Node-to-Unity bridge as `!lights`. The original `POST /vote/start` endpoint and `/simulate/vote` local debug route remain available.

## Judge setup

1. Install [Node.js](https://nodejs.org/) (final audit used Node 21.6.1) and Unity **6000.3.11f1**. Clone this repository. In the repository root, run `npm ci`.
2. Open the **`unity/` folder** as a Unity project, then open `Assets/ChatHaunt/Scenes/ChatHauntHouse.unity`. The project uses URP and the Input System. Unity resolves packages from `unity/Packages/manifest.json`; its cache is generated locally.
3. For real Twitch chat, register a **Public** Twitch developer application, authorize the broadcaster's account through Twitch's [device code flow](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#device-code-grant-flow), and request **only `user:read:chat`**. Copy `.env.example` to `.env` and fill `TWITCH_CLIENT_ID`, `TWITCH_OAUTH_TOKEN`, `TWITCH_READER_USERNAME`, and `TWITCH_CHANNEL` with your own values. The reader and channel must be the same broadcaster login. `.env` is ignored by Git; never put tokens in a commit or screenshot. No Client Secret or write-chat scope is required. See [authentication details](docs/AUTH_SETUP.md).
4. Run `npm start` in the repository root. Open `http://127.0.0.1:3177` and wait for **Twitch: connected**. Press Play in Unity and wait for **Unity Game: Connected**.
5. Send `!lights` in the configured Twitch chat, or click **Start Vote · 15s** and have viewers type `1`, `2`, or `3`. The dashboard counts votes and displays the winner. Watch the Unity Game view for the scare. **F** toggles the flashlight; **L** is a local test key that restores/toggles room lights. **E** interacts with sliding doors.

For an offline Node smoke test, run `npm test` (seven tests). The local `npm run simulate:lights` route exercises Node → Unity only when Unity is in Play Mode; it is **not** a real Twitch test. A vote can also be opened from PowerShell with `Invoke-RestMethod http://127.0.0.1:3177/vote/start -Method Post` (30-second command-line default).

## Project layout

- `src/`, `public/`, `test/`: Node EventSub reader, local bridge, dashboard, and tests.
- `unity/Assets/`: ChatHaunt scenes, gameplay scripts, prefab, materials, textures, animation, and audio, including `.meta` GUIDs.
- `unity/Packages/`, `unity/ProjectSettings/`: Unity dependencies and project settings.
- `docs/ASSET_PROVENANCE.md`: reused asset origins and audio licences.
- `docs/DEVELOPMENT_LOG.md`, `unity/README.md`: detailed build and verification record.

## Verified scope and limitations

A real `!lights` message and a real chat vote for GhostWoman previously travelled through Twitch EventSub → Node → Unity. Local simulations verified all three vote outcomes, GhostWoman reset, and the door sound. The current dashboard button was verified with a **simulated** vote that triggered the Unity door event; this does not claim a new live Twitch test. See [development log](docs/DEVELOPMENT_LOG.md) for the dated evidence.

Twitch native polls require an Affiliate or Partner broadcaster; ChatHaunt uses chat voting so an ordinary channel can participate. The user access token expires and must be reauthorized for later demo sessions. This is a local Unity Editor demo, not a hosted game or standalone build. The pre-existing Area51Scanner project was used only as a reference and for the specifically documented assets; its full project and story are not included.
