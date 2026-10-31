# Asset provenance

ChatHaunt's gameplay code, compact house layout, primitive geometry, and sliding-door implementation were created for this project. The earlier Area51Scanner Unity project is not in this repository.

## Pre-existing assets from Area51Scanner

Only these dependency chains were brought into ChatHaunt, with Unity `.meta` GUIDs preserved so scene and prefab references resolve:

- `unity/Assets/ChatHaunt/Characters/GhostWoman/`: GhostWoman prefab and FBX, Generic rig/avatar, Animator Controller, ten clips, character materials/textures, and `GhostWomanLoopTravel.cs`. The character, clothing, hair, textures, rig, and animations were authored by the project owner before this hackathon. The anatomical base and shape deltas use [MakeHuman Community core assets (CC0)](https://static.makehumancommunity.org/about/license.html). The source project recorded that provenance in its `ArtSource/Blender/Characters/GhostWoman` notes.
- `unity/Assets/ChatHaunt/Environment/Area51/`: five user-authored house materials and their 20 referenced texture PNGs for plaster walls, aged timber, shoji panels, tatami, and stained fabric. The source project's house look-development reports identify these as authored texture sets.

The old game's story, puzzles, code, unrelated assets, and whole project were not migrated.

## Supplied audio

The project owner supplied three MP3 files, used as part of the Unity game under the [Pixabay Content License](https://pixabay.com/service/license-summary/). Pixabay permits use in creative works but restricts standalone redistribution of its content. These files are included only as assets of the playable ChatHaunt project, not as a separate sound library. Credit and source pages:

| In-game use | Creator and source |
| --- | --- |
| Looping house ambience | [Alex_Jauk — Eerie Atmosphere Ambience](https://pixabay.com/sound-effects/horror-eerie-atmosphere-ambience-372558/) |
| GhostWoman scream | [freesound_community / nick121087 — Demonic Woman Scream](https://pixabay.com/sound-effects/horror-demonic-woman-scream-6333/) |
| Door-creak scare | [DRAGON-STUDIO — Door Creaking](https://pixabay.com/sound-effects/film-special-effects-door-creaking-335491/) |

The separate blackout sting was generated locally for the ChatHaunt prototype. No audio was copied from Area51Scanner.
