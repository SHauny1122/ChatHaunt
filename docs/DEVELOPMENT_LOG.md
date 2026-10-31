# ChatHaunt Node bridge

This is the existing local Node bridge and live control dashboard at `http://127.0.0.1:3177`. The actual horror game runs in the separate Unity project at `D:\UnityProjects\ChatHaunt`. The Twitch EventSub → Node → Unity `!lights` path was verified with a real message on October 29, 2026.

## Architecture

```text
Twitch channel.chat.message
  → EventSub WebSocket (user access token, user:read:chat)
  → Node filters exact !lights and, during a vote, messages 1/2/3
  → localhost HTTP poll and acknowledgement
  → Unity ChatHauntLocalBridge
  → ChatHauntHorrorEvent.TriggerEvent(), hallway GhostScareEvent, or door-slam audio event
```

Node also sends state to the browser dashboard through Server Sent Events. It shows Twitch and Unity connection status, viewer controls, vote counts/result, last action, and the room light state reported by Unity's five rigs. The server binds to `127.0.0.1:3177`; no public local endpoint or extra service is required. Only one Unity event can be in flight. A missing acknowledgement fails after five seconds, and stale scares are not replayed. The Twitch socket reconnects and resubscribes after a lost connection; Twitch directed handoffs retain the subscription.

## Viewer vote

With Node and Unity running, open `http://127.0.0.1:3177` and click **Start Vote · 15s**. The vote window opens for 15 seconds, the button is disabled until it closes, and the dashboard shows the countdown and live counts. For the original 30-second command-line fallback:

```powershell
Invoke-RestMethod http://127.0.0.1:3177/vote/start -Method Post
```

During the window, each viewer types **1** (house blackout), **2** (standing hallway GhostWoman with scream), or **3** (spatial door-slam sound scare). The latest valid vote from each Twitch login counts once. The dashboard and `/state` show counts. At timeout, the highest count wins; ties choose the lowest numbered option. No votes means no event. A winner uses the same localhost Unity poll and acknowledgement as `!lights`. The vote can be started again after it closes. No Twitch message is sent back to chat, and no additional OAuth scope is needed.

For a **local simulation**, start a short vote with `-ContentType 'application/json' -Body '{"seconds":2}'` and send a simulated viewer ballot:

```powershell
Invoke-RestMethod http://127.0.0.1:3177/simulate/vote -Method Post -ContentType 'application/json' -Body '{"user":"local-viewer","choice":2}'
```

The simulation route is loopback only. These ballots are not Twitch messages.

**Dashboard button verification, October 31:** Clicking **Start Vote · 15s** opened the existing vote window. One local simulated ballot for option 3 appeared in the dashboard count; after 15 seconds the window closed, showed `DOOR SLAM` as winner, and Node logged `Unity vote result: triggered` while Unity was connected. The ballot was **simulated**, not a real Twitch chat vote. No Unity or Twitch authentication code changed.

The Twitch side follows the current [EventSub WebSocket guidance](https://dev.twitch.tv/docs/eventsub/handling-websocket-events), [`channel.chat.message` specification](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchatmessage), and [chat authentication guide](https://dev.twitch.tv/docs/chat/authenticating/). ChatHaunt reads messages only and requests `user:read:chat`. It does not request `user:write:chat` because it sends nothing to Twitch.

## Local simulated test

With Node.js and npm installed, run in this folder:

```powershell
npm install
npm test
npm start
```

In Unity, open `D:\UnityProjects\ChatHaunt\Assets\ChatHaunt\Scenes\ChatHauntHouse.unity` and press Play. In another terminal in this Node folder run:

```powershell
npm run simulate:lights
```

This posts the literal `!lights` to the local Node server, then uses the same Node → Unity dispatch as Twitch. Success means Unity acknowledged that `TriggerEvent()` started. Watch the room lights cut out and stay off; the flashlight reveals nearby surfaces, and the local **L** test key restores the lights. The temporary silhouette has been removed. If Unity does not acknowledge, the command fails. `http://127.0.0.1:3177` shows the control dashboard. Without `.env`, Twitch status is **not configured**.

On October 30, Unity's blackout was changed to stay active. A **local simulated** `!lights` received Unity's triggered acknowledgement; after the scare coroutine ended, all five room light rigs remained off and `/state.lightsOn` was `false`. The Unity local light toggle restored them, and the console showed no errors or warnings. The earlier real Twitch test below was performed before this persistence change.

## Private Twitch setup

1. Register a **Public** application in the [Twitch Developer Console](https://dev.twitch.tv/console/apps). The verified ChatHaunt setup used the `Chat Bot` category and `https://localhost:3000` as the required registration redirect URL. Twitch required two-factor authentication on the developer account. The redirect URL is not used by the device authorization flow.
2. Use Twitch's [device code flow](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#device-code-grant-flow) with the app's Client ID to authorize the **broadcaster's account** for only `user:read:chat`. This public-client flow needs no Client Secret. Complete the authorization on Twitch's activation page and keep the returned user access token private.
3. Copy `.env.example` to the Git-ignored `.env` and fill in:

```dotenv
TWITCH_CLIENT_ID=your_twitch_app_client_id
TWITCH_OAUTH_TOKEN=your_user_access_token
TWITCH_READER_USERNAME=the_account_that_authorized_the_token
TWITCH_CHANNEL=the_streamers_channel_login
PORT=3177
```

The username and channel are the same lowercase broadcaster login name without `@` or `#`. `.env` is ignored by Git. Do not put tokens or secrets in source files, `.env.example`, screenshots, or commits. Node validates the token at startup and hourly, checks its app ID, account and `user:read:chat` scope, resolves the channel ID, then subscribes to `channel.chat.message` on the EventSub WebSocket.

Restart `npm start`, press Play in Unity, and wait for **Twitch: connected** and **Unity Game: Connected** in the browser dashboard. Send exactly `!lights` in the configured channel. Confirm both the Unity scare and `Unity !lights result: triggered` in the Node terminal. Device-flow access tokens expire; repeat authorization when validation fails.

## Real Twitch verification — October 29, 2026

The broadcaster sent `!lights` in the real `shaunybosch` channel chat. Node logged `Lights command: !lights from shaunybosch (twitch)` and `Unity !lights result: triggered`. Unity MCP showed the localhost bridge connected, `LastTriggered=true`, and the horror event count increasing from one prior simulated event to two. After the scare, `Running=false` and all five room light rigs reported `IsBlackout=false`. The broadcaster also saw the lights turn off and back on in the Game view. Unity reported no console errors or warnings. This is a **real Twitch chat test**, separate from the earlier local simulation.

On October 30, the preview state was fixed. Unity now reports the real room light state through its existing poll. A local vote for option 1 made `/state.lightsOn` change `true → false → true` as Unity blacked out and restored the rigs. This was a **local simulation**, not a second real Twitch test.

The browser page was later changed from a mock game room to a judge-facing live control dashboard. A local simulated vote displayed its active count and winner; a local simulated `!lights` was acknowledged and `/state.lightsOn` again changed `true → false → true`. The dashboard returned to **Room Lights: ON**. This polish pass did not change EventSub authentication or require another live Twitch chat test.

Before the audio pass, local votes for options 2 and 3 were acknowledged by Unity and started the hallway and living room GhostWoman placements. The ghost reset afterward, all five light rigs remained on, `npm test` passed 6/6, and Unity MCP reported no errors or warnings. Option 3 now maps to the door-slam audio event.

## Real Twitch vote verification — October 30, 2026

After refreshing the expired token with the same `user:read:chat` device authorization, EventSub reconnected. The broadcaster sent one real `2` in `shaunybosch` chat during a 45-second vote. Node counted one ballot for option 2, logged `Vote: shaunybosch chose 2`, selected `HALLWAY_GHOST`, and logged `Unity vote result: triggered`. Unity MCP showed the ghost trigger count rise from two local tests to three, then `Running=false` with the ghost instance inactive. All five room lights remained on and Unity reported no errors or warnings. This was a **real Twitch chat vote**, separate from the local vote simulations. The refreshed token remains only in the Git-ignored `.env`.

## User-supplied audio pass — October 30, 2026

Three MP3s supplied by the project owner were imported only into the Unity project: a quiet looping house ambience, a non-looping GhostWoman scream, and a non-looping spatial door-closing/impact cue. Vote option 3 now chooses `DOOR_SLAM` instead of `LIVING_ROOM_GHOST`; the living room GhostWoman placement remains available in Unity. The dashboard labels match the current mapping. No Twitch authentication or EventSub code changed.

**Local simulation only:** votes 1, 2, and 3 each won and received `Unity vote result: triggered`. Unity confirmed the ambience was playing at volume 0.08 and looping across its 23.93-second clip; GhostWoman played the supplied scream, remained in the standing state, then hid/reset; and the door cue played once and stopped. Simulated `!lights` still triggered, with Node reporting room lights `true → false → true`. `npm test` passed 7/7. One diagnostic Unity MCP query produced two `AudioSource.time` warnings on one-shot sources; after clearing those inspection warnings, the running scene reported zero errors or warnings. No new real Twitch test was run for this audio-only change.

After a Play Mode listening concern, the Unity house ambience test volume was raised to 0.25, with the clip assigned directly to the ambient AudioSource, Play On Awake enabled, and Loop enabled. Unity MCP verified playback across bedroom, hallway, and living-room movement; the scream and door-cue volumes were unchanged. See the Unity README for the audio-path check. Twitch bridge code was unchanged.

## Files

```text
src/chat.js             EventSub !lights filter
src/twitch.js           Token validation, channel lookup, EventSub connection/subscription
src/server.js           Local HTTP server, dashboard state, Unity relay
src/simulate-lights.js  Local simulated command
src/vote.js             Vote window, per-viewer ballot count, winner mapping
public/                 Live control dashboard
test/                   Local protocol tests
```
