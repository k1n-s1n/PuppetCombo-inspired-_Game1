# Project notes

Running log for the report: scripts written and third-party assets used (with sources).

## Scripts

| Script | Location | What it does |
|---|---|---|
| `FirstPersonController.cs` | `Assets/_Project/Scripts/` | WASD movement, mouse look, Shift to walk faster, using a CharacterController (collides with walls and slides along them, follows slopes, gravity). Mouse sensitivity, speeds and look limit are exposed in the Inspector. Uses the Input System package (`Keyboard.current` / `Mouse.current`). Esc frees the cursor, clicking recaptures it. Finds the child camera automatically if the Inspector slot is empty. |
| `IInteractable.cs` | `Assets/_Project/Scripts/` | Interface ("contract") for anything usable with E: `GetPrompt()` and `Interact()`. Doors now; pickups/keys later. |
| `PlayerInteractor.cs` | `Assets/_Project/Scripts/` | On the Player. Raycasts from the camera centre each frame (2.5 m reach); shows "[E] prompt" and a crosshair dot via OnGUI; E calls `Interact()`. `ShowMessage()` shows temporary text (e.g. "It's locked."). |
| `NightSky.shader` | `Assets/_Project/Shaders/` | Custom BRP skybox shader (material `_Project/Materials/NightSky.mat`). Per-pixel gradient (horizon to zenith), procedural twinkling stars (random stars in a 3D grid of cells, each with its own brightness, tint and twinkle timing) and a moon disc with glow. Also fades the horizon into the scene's fog colour (`unity_FogColor`), because Unity never fogs the skybox: without this, gaps such as the end of the road showed a dark hole in the fog. All values tweakable on the material. Hash functions from Dave Hoskins' "Hash without Sine" (MIT, https://www.shadertoy.com/view/4djSRW). |
| `Door.cs` | `Assets/_Project/Scripts/` | Swings a door open/closed around its hinge pivot, always away from the player. Optional linked door (double doors), `isLocked` + locked message (key support to come). Angle and speed in the Inspector. On the 5 store doors. |
| `GameUI.cs` | `Assets/_Project/Scripts/` | Story UI singleton (`GameUI.Instance`): queued subtitles in a speech bar at the bottom (`Say(speaker, text)`), objective text at the top on a dark strip, flashing when changed (`SetObjective`). Drawn with OnGUI so it sits above the VHS effects. On the `GameUI` object in Main. |
| `StoryManager.cs` | `Assets/_Project/Scripts/` | Runs Chapter 1's story beats in order as a simple state machine (`Stage` enum). Other scripts report events with `StoryManager.Instance.OnEvent("...")`; it only reacts if the event fits the current stage. Plays the opening subtitle after a short delay (coroutine). On the `StoryManager` object. |
| `StoryTrigger.cs` | `Assets/_Project/Scripts/` | Invisible trigger box: when the Player (tag `Player`) walks in, sends its `eventId` to the StoryManager. `Trigger_EnteredStore` sits just inside the front doors. |

## Third-party assets

| Asset | Author | Source | Licence | Location in project | Notes |
|---|---|---|---|---|---|
| 6TWELVE (PSX-style convenience store, with road, parking lot and forest backdrop) | Elbolillo (Sketchfab: Elbolilloduro) | https://sketchfab.com/3d-models/6twelve-8c2c49cab08e402d8681434a4ade4ed0 | CC Attribution 4.0 | `Assets/ThirdParty/6twelve/` | Imported the author's original FBX + textures (from `source/6twelve.rar` in the Sketchfab download). ~18.8k triangles, 88 materials. Kept unmodified. |
| psx_retroshader (PS1-style shaders, cPrecision image effect) | dsoft20 | https://github.com/dsoft20/psx_retroshader | MIT | `Assets/ThirdParty/psx_retroshader/` | Imported by the developer, not used yet (may be used later). Removed its obsolete `UnityVS/` Visual Studio plugin DLLs (failed to load in Unity 6). |
| Unity_PostEffect (BadTV, RGBShift, Posterize, Scanlines, etc. + WebCam demo) | tasdidahmedtah | https://github.com/tasdidahmedtah/Unity_PostEffect | No licence stated in repo | `Assets/ThirdParty/Unity_PostEffect/` | Imported by the developer. RGBShift, Posterize, Scanlines and BadTV are on the PlayerCamera for the VHS look. Its demo scene (`Main.unity`) was removed. |

## Scene setup (not scripts, but worth mentioning in the report)

- Night lighting: directional light renamed `Moonlight` (dim blue, soft shadows, aimed from the sky shader's moon direction); Trilight ambient (dark blue); exponential fog matching the sky's horizon colour so the treeline fades into the sky.
- `Lighting` object in the Hierarchy: 8 cool-white ceiling point lights + 2 canopy lights in the store (placed where the original Blender area lights were), 7 orange sodium-style spotlights in the street lamps. No shadows on these, for performance.
- Quality setting Pixel Light Count raised 4 -> 6 so the large shop floor is lit evenly.
- Developer's lighting pass: flat ambient (~0.5 grey) for readability, grey exponential-squared fog (density 0.03), dimmer shop lights, white street lamps.
- Post Processing Stack v2 (Unity package `com.unity.postprocessing` 3.5.4): Post-process Layer on PlayerCamera reading the `PostProcessing` layer; global `Post Processing Volume` object with profile `_Project/Scenes/Main_Profiles/PlayerCamera Profile.asset`. Effects used: Ambient Occlusion, Color Grading, Lens Distortion, Bloom, Vignette. Post Processing runs before the Unity_PostEffect VHS scripts (RGBShift, Posterize, Scanlines, BadTV), so the tape effects sit on top.

## Asset findings / to-do

- 6TWELVE: real-world scale (store ~23 x 21 m). Five separate door meshes with pivots on the hinge edge (`Door`, `Door_01` = front double doors; `Door_02`, `Door_03`, `Door_04` = interior), so they can be scripted to swing open.
- Interior already has several areas: shop floor, counter, bathroom (toilet, sink, mirror), office (chair, monitor, keyboard), storage (boxes, fuse boxes, mop/broom).
- Done: `Glass` material (shop windows, door panes, freezer lids) set to Standard Transparent, light blue tint, glossy; window glass casts no shadows.
- Done: re-linked 3 lost textures; alpha-textured materials set to Cutout; FBX light import disabled (Blender lights came in at intensity ~1800).
