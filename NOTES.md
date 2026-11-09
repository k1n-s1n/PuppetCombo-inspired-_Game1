# Project notes

Running log for the report: scripts written and third-party assets used (with sources).

## Scripts

| Script | Location | What it does |
|---|---|---|
| `FirstPersonController.cs` | `Assets/_Project/Scripts/` | WASD movement, mouse look, Shift to walk faster, using a CharacterController (collides with walls and slides along them, follows slopes, gravity). Mouse sensitivity, speeds and look limit are exposed in the Inspector. Uses the Input System package (`Keyboard.current` / `Mouse.current`). Esc frees the cursor, clicking recaptures it. Finds the child camera automatically if the Inspector slot is empty. |
| `IInteractable.cs` | `Assets/_Project/Scripts/` | Interface ("contract") for anything usable with E: `GetPrompt()` and `Interact()`. Doors now; pickups/keys later. |
| `PlayerInteractor.cs` | `Assets/_Project/Scripts/` | On the Player. Raycasts from the camera centre each frame (2.5 m reach); shows "[E] prompt" and a crosshair dot via OnGUI; E calls `Interact()`. `ShowMessage()` shows temporary text (e.g. "It's locked."). |
| `Door.cs` | `Assets/_Project/Scripts/` | Swings a door open/closed around its hinge pivot, always away from the player. Optional linked door (double doors), `isLocked` + locked message (key support to come). Angle and speed in the Inspector. On the 5 store doors. |

## Third-party assets

| Asset | Author | Source | Licence | Location in project | Notes |
|---|---|---|---|---|---|
| 6TWELVE (PSX-style convenience store, with road, parking lot and forest backdrop) | Elbolillo (Sketchfab: Elbolilloduro) | https://sketchfab.com/3d-models/6twelve-8c2c49cab08e402d8681434a4ade4ed0 | CC Attribution 4.0 | `Assets/ThirdParty/6twelve/` | Imported the author's original FBX + textures (from `source/6twelve.rar` in the Sketchfab download). ~18.8k triangles, 88 materials. Kept unmodified. |
| psx_retroshader (PS1-style shaders, cPrecision image effect) | dsoft20 | https://github.com/dsoft20/psx_retroshader | MIT | `Assets/psxshaders/` | Imported by the developer. |
| PostEffects (BadTV, RGBShift, Posterize, Scanlines, etc.) + WebCam demo | _TODO: confirm source_ | _TODO_ | _TODO_ | `Assets/PostEffects/`, `Assets/WebCam/` | Imported by the developer; RGBShift, Posterize, Scanlines and BadTV are used on the PlayerCamera for the VHS look. |

## Asset findings / to-do

- 6TWELVE: real-world scale (store ~23 x 21 m). Five separate door meshes with pivots on the hinge edge (`Door`, `Door_01` = front double doors; `Door_02`, `Door_03`, `Door_04` = interior), so they can be scripted to swing open.
- Interior already has several areas: shop floor, counter, bathroom (toilet, sink, mirror), office (chair, monitor, keyboard), storage (boxes, fuse boxes, mop/broom).
- Done: `Glass` material (shop windows, door panes, freezer lids) set to Standard Transparent, light blue tint, glossy; window glass casts no shadows.
- Done: re-linked 3 lost textures; alpha-textured materials set to Cutout; FBX light import disabled (Blender lights came in at intensity ~1800).
