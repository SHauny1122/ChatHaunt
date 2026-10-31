# Twitch setup for a local judge run

ChatHaunt reads the broadcaster's chat using Twitch EventSub `channel.chat.message` over WebSocket. It needs one **user access token** authorized by the broadcaster for **`user:read:chat`**. It does not need `user:write:chat`, `channel:manage:polls`, the older IRC scopes, a Client Secret, or a public callback server.

1. Sign in to the [Twitch Developer Console](https://dev.twitch.tv/console/apps) with the broadcaster account and register a **Public** application. The original demo used the `Chat Bot` category and `https://localhost:3000` as the required registration redirect; the device flow itself does not use that redirect.
2. Follow Twitch's [Device Code Grant Flow](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#device-code-grant-flow) for that application's Client ID, requesting only `user:read:chat`. On Twitch's activation page, sign in as the same broadcaster whose channel ChatHaunt will read and approve the permission.
3. Copy `.env.example` to `.env` in the repository root. Put the Client ID and returned user access token in the matching fields. Set both `TWITCH_READER_USERNAME` and `TWITCH_CHANNEL` to the broadcaster's lowercase Twitch login, without `@` or `#`. Keep `PORT=3177` unless you also change Unity's local bridge URL.
4. Run `npm start`. The dashboard at `http://127.0.0.1:3177` should say **Twitch: connected** once the WebSocket subscription succeeds. Start the Unity scene in Play Mode to make **Unity Game: Connected** appear.

`.env` is Git-ignored and must stay local. Do not paste tokens into issues, screenshots, Devpost answers, or commits. Twitch device-flow access tokens expire; repeat the device authorization when validation fails. The previous demo's token is not included in this repository.
