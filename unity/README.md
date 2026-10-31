# ChatHaunt Unity house

New Unity 6000.3.11f1 project using the 3D cross-platform URP template. The house scene and gameplay scripts were created for ChatHaunt; the GhostWoman asset chain and selected house materials are the documented pre-existing assets below. The existing Node/Twitch bridge remains separate at `C:\Users\shaun\OneDrive\Documents\ChatGPT\chathaunt`.

Open `Assets/ChatHaunt/Scenes/ChatHauntHouse.unity` and press Play. The earlier single hallway remains at `Assets/ChatHaunt/Scenes/ChatHauntMilestone.unity` as a technical milestone.

- WASD: move
- Mouse: look
- E: interact with the door when its prompt appears
- F: toggle flashlight
- L: toggle all room lights for local testing only
- Escape: release the cursor; click the Game view to recapture it

The new compact house connects Bedroom → Hallway → Living Room → Kitchen → Genkan. Bedroom and living room have interactable sliding shoji doors. Each room has its own `ChatHauntRoom` ID, light rig, and event anchor. `ChatHauntHouseEvents` handles the blackout, GhostWoman placements, and the separate door-slam sound event.

`ChatHauntSlidingDoor` recreates the old house's 0.65-second sideways travel, using ChatHaunt's own E interaction. The panel collider moves with the panel, a fixed pull remains focusable while it is open, and closing is refused if the player stands in the doorway. Each door has optional `openingClip` and `closingClip` fields and a spatial `AudioSource`; the clip fields are intentionally empty for the final sound selection.

Receiving `!lights` from Node calls `ChatHauntHorrorEvent.TriggerEvent()`. It cuts all five room lights and plays the existing generated placeholder sting. The lights stay off so the player must use the flashlight; **L** restores or toggles the room lights locally during testing. Walking through the hallway does not trigger the event. The temporary blackout silhouette has been removed; the separate reusable GhostWoman scare remains. With the room lights off, ambient and reflection lighting are zero, so the environment is nearly invisible until the short flashlight beam illuminates nearby surfaces. No video-style overlay was added.

## Local bridge

```text
Twitch EventSub channel.chat.message !lights (or local simulation)
  → existing Node/Twitch bridge on 127.0.0.1:3177
  → localhost HTTP poll and acknowledgement
  → ChatHauntLocalBridge
  → ChatHauntHorrorEvent.TriggerEvent()
```

Start Node with `npm start` in its project folder. Open `ChatHauntHouse.unity` in Unity and press Play, then run `npm run simulate:lights` in another Node terminal. Watch all room lights cut out and remain off; use the flashlight or press **L** to restore the room lights locally. The fake silhouette no longer appears. Unity retries polling after connection errors; Node reports failure if Unity does not acknowledge within five seconds. Keep Node's `PORT` at 3177 unless you also change `serverUrl` on the Unity bridge component.

Both the local simulation and a real Twitch chat `!lights` were verified on October 29, 2026. Twitch credentials belong only in the ignored Node `.env` file. See the Node README for setup and the real test record.

The local bridge accepts the three winner event IDs from Node's reusable 1/2/3 viewer vote: `LIGHTS_HALLWAY_OFF`, `HALLWAY_GHOST`, and `DOOR_SLAM`. These trigger the persistent blackout, standing GhostWoman with scream, or spatial door-closing/impact cue. `LIVING_ROOM_GHOST` remains an available Unity placement but is no longer a vote outcome. On every poll, Unity reports whether any of the five room light rigs is blacked out; the Node dashboard reflects the actual lighting state.

On October 30, the blackout was changed to remain active after its event coroutine finishes. In Play Mode, a local direct trigger and then a simulated Node `!lights` each left all five rigs blacked out after `Running=false`; Node reported `lightsOn=false`. Invoking the same local toggle method used by **L** restored all five rigs. Unity MCP reported zero console errors or warnings. This persistence check was local, not a new Twitch chat message.

Before the audio pass, local simulated votes triggered and received Unity acknowledgements for all three outcomes. Option 1 made Node's `/state.lightsOn` go `true → false → true`; at that time options 2 and 3 activated their respective GhostWoman placements. The ghost reset, all room rigs ended on, and Unity MCP reported no errors or warnings.

One **real Twitch vote** was then verified: the broadcaster sent `2` in `shaunybosch` chat during a 45-second window. Node selected `HALLWAY_GHOST` and received a triggered acknowledgement. Unity MCP showed `GhostScareEvent.TriggerCount` rise from two to three; the ghost later reset inactive, all five room light rigs remained on, and the Unity console had no errors or warnings. The previous real `!lights` test remains separately documented above.

## Reusable GhostWoman scare

`GhostScareEvent` pools one GhostWoman prefab instance and accepts a placement ID. Each placement records its `ChatHauntRoomId`, anchor, animation state and duration. `HALLWAY_GHOST` now uses the existing standing `GW_Idle_Still_Loop` clip; `BEDROOM_DOORWAY_GHOST` and `LIVING_ROOM_GHOST` retain their peek clips. The imported floor-crawl clips remain in the project. These are Unity event IDs, **not Twitch commands**. The hallway test can be started in Play Mode with **ChatHaunt → Test Hallway Ghost Scare (Play Mode)** or by calling `ChatHauntHouseEvents.TriggerGhost("HALLWAY_GHOST")`. It hides and resets after six seconds and can be triggered again.

### User-supplied audio

Only the three files provided by the project owner were copied into `Assets/ChatHaunt/Audio/UserSupplied` and assigned in `ChatHauntHouse`:

- **Quiet looping house ambience:** `alex_jauk-eerie-atmosphere-ambience-372558.mp3` on `House Ambience Audio` → `ChatHauntAudio.suppliedClip`. Its source has the clip assigned directly, Play On Awake and Loop enabled, and uses test volume 0.25.
- **GhostWoman scream:** `freesound_community-demonic-woman-scream-6333.mp3` on `Reusable GhostWoman Scare Event` → `GhostScareEvent.screamClip`. Its spatial source plays once when she appears; `screamVolume` is 0.75.
- **Door-closing/impact scare:** `dragon-studio-door-creaking-335491.mp3` on `Door Slam Scare Event` → `ChatHauntAudio.suppliedClip`. Its spatial source sits at the living-room sliding door, plays once for vote option 3, and uses volume 0.7. It does not move or control the door.

The separate blackout sting still uses its original generated placeholder. The three supplied sounds have independent volume controls. The MP3s came from the user, not Area51Scanner.

### Pre-existing asset provenance

The GhostWoman character predates this hackathon. From `D:\Area51Scanner\Assets\_Project\Characters\GhostWoman`, only the prefab dependency chain was copied into `Assets/ChatHaunt/Characters/GhostWoman`, preserving Unity `.meta` GUIDs:

- `Prefabs/GhostWoman.prefab` and `Models/GhostWoman_Model.fbx` (Generic rig and embedded avatar)
- `Animations/GhostWoman.controller` and its ten `GW_*.anim` clips
- five `M_GhostWoman_*.mat` character materials, `M_ReviewFloor.mat` (an FBX importer remap reference), and fifteen referenced texture PNGs
- `Scripts/GhostWomanLoopTravel.cs`, which carries the authored crawl offset across loop boundaries

The ten source-take FBXs, old test scene, review scripts, and old editor importer were not needed by the prefab and were not copied. The original project's [character source notes](</D:/Area51Scanner/ArtSource/Blender/Characters/GhostWoman/README.md>) identify the anatomical base and shape deltas as MakeHuman Community CC0 assets, with the [upstream CC0 licence copy](</D:/Area51Scanner/ArtSource/Blender/Characters/GhostWoman/sources/LICENSE.ASSETS.md>) preserved there. [MakeHuman Community's current licence page](https://static.makehumancommunity.org/about/license.html) also identifies its core assets as CC0. The character look, clothing, hair, textures, rig and clips were authored for the old project. Area51Scanner was inspected and copied from read-only; its files were not changed.

Unity MCP checked the imported prefab: Generic avatar present, ten animation clips, 15 skinned meshes, no null meshes/materials/bones, and no missing asset dependencies. In Play Mode, `HALLWAY_GHOST` showed the character, entered `GW_FloorCrawl_Loop`, advanced its normalized animation time and crawl position, hid after the timeout, and retriggered from time zero. A screenshot was inspected from the hallway. This is a local Unity test; placement composition and movement still need a human visual pass in the Game view. The simulated Node `!lights` event also still triggered the separate blackout scare after the ghost was added.

In the October 30 polish pass, Unity MCP showed the standing hallway clip playing, with a 2.42 m tall rendered bound. A hallway screenshot showed the full standing character; she later hid/reset, and a local vote retriggered her. Both `ChatHauntAudio` sources and the new spatial GhostWoman source were present with empty final clip fields. A simulated `!lights` still triggered and reported `true → false → true` light state to Node. Unity reported no console errors or warnings. Final sound playback remains untested until the user supplies clips.

In the subsequent user-supplied audio pass, Unity imported the three MP3s and the scene saved their clip references. Play Mode showed the 23.93-second ambience start automatically at volume 0.08, stay playing after its playhead wrapped, and keep `loop=true`. A local simulated vote for each of 1/2/3 received a Unity acknowledgement. Vote 2 played the 3.98-second scream while the standing hallway GhostWoman was active; the scream did not loop, and GhostWoman hid/reset. Vote 3 played the 8.10-second door-closing/impact clip at the living-room doorway, did not loop, and stopped. A simulated `!lights` still triggered and Node saw `lightsOn` go `true → false → true`. Node tests passed 7/7. An MCP inspection of `AudioSource.time` on one-shot sources briefly logged two diagnostic warnings; after clearing those, the running scene showed zero errors or warnings. These are local simulations, not new real Twitch chat tests.

After feedback that the ambience was too quiet, its test volume was raised from 0.08 to 0.25. The ambient `AudioSource` now stores the supplied clip directly with Play On Awake and Loop enabled; `ChatHauntAudio` preserves that playback and only starts it in `Start()` if it is not already playing. Unity MCP verified the active source played across programmatic player movement from bedroom through hallway to living room, with the playhead wrapping, 2D spatial blend, mute off, default mixer output, active Player Camera listener at volume 1, and zero console errors/warnings. Windows Volume Mixer initially showed no Unity audio session; a one-time runtime `AudioSettings.Reset` restored that session at 100% app volume. The user then confirmed the ambience was audible in Play Mode. The device reset was limited to the current Editor session; the clip, Play On Awake, Loop, and 0.25 volume settings are saved in the scene. GhostWoman and door cue volumes remained 0.75 and 0.7.

## Reference and asset provenance

Area51Scanner was inspected read-only for its hall, bedroom, living room, kitchen, genkan, and sliding door setup. The ChatHaunt house geometry, door geometry, and furniture shapes are new Unity primitives. The sliding behavior was recreated in a new ChatHaunt script; the old `HouseDoor`, `SlidingDoorAudioV1`, `HouseInteraction`, progression, puzzles, and door prefab/model were not copied.

Five pre-existing, user-authored house materials and their 20 referenced texture PNGs were copied from `D:\Area51Scanner\Assets\_Project\Art` into `Assets/ChatHaunt/Environment/Area51`, with their Unity `.meta` GUIDs preserved: `M_JH_DirtyPlaster_V3` (walls), `M_JH_AgedTimber_V3` (wood floors, ceilings, trims and furniture), `M_JH_DamagedShoji_V3` (sliding panels), `M_JH_AgedTatami` (bedroom mats), and `M_JH_StainedFabric` (soft furnishings). The [old Hallway V3 integration report](</D:/Area51Scanner/ArtSource/Blender/IntegrationV3/HallwayV3_Integration_Report.md>) records the original authored texture set and no downloaded assets; the [bedroom look-development report](</D:/Area51Scanner/ArtSource/Blender/JapaneseHouse_Bedroom_LookDev_V1_Report.md>) records the original bedroom materials. No audio was copied from Area51Scanner; the three new MP3s came from the user.

## Verification

Unity MCP confirmed five registered room light rigs, two sliding doors, and the local scare adapter. In Play Mode, a simulated `!lights` received Unity's acknowledgement and restored all five room lights after the blackout. Programmatic `CharacterController.Move` navigation crossed the open bedroom door, hall, open living room door, kitchen opening, and genkan entrance in both directions. Both doors were focused through the player E interaction path, opened, closed through their fixed pulls, blocked movement while shut, and allowed passage after reopening. The interaction focus stayed on each closed door in 200/200 facing samples, and simulated E key edges toggled each door six times without missed focus. The former hallway trigger location caused no automatic event. Lights-on, lights-off, and flashlight-only Game view captures were inspected; the lights-off view was black without the flashlight, while the beam revealed nearby surfaces. No fake silhouette was present; GhostWoman remained a separate asset and event. Material checks found no missing material/texture or unsupported shader among the room renderers. Manual WASD/mouse traversal of the whole house remains untested.

On October 29, 2026, a real `!lights` message in the broadcaster's Twitch chat reached the EventSub Node bridge, which logged Unity's acknowledgement as triggered. Unity MCP showed the event count rise from one prior simulated trigger to two, then `Running=false` with all five room light rigs restored (`IsBlackout=false`). The broadcaster saw the blackout and restoration in the Game view. Unity reported no console errors or warnings. The temporary L shortcut remains for local testing; an automated physical key press was not verified.
