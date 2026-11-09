# Project: Puppet Combo–style horror world (CS3VR coursework)

## What this is
A first-person retro horror virtual world in the style of Puppet Combo (Murder House, Nun Massacre): PS1-era 3D graphics viewed through a worn VHS tape filter.

This is **university coursework** (CS3VR "Simpler Individual Virtual World / Game", 25% of the module).
- **Deadline: noon, Friday 13 November 2026.** Target finishing the world by Wednesday night; Thursday is for recording the video and writing the report.
- The developer is new to Unity (but a CS student, comfortable with programming). AI tools are allowed to *support* the work, but it must be the developer's own work, and they must understand and be able to explain every script. So **explain what you build**, and keep code readable.
- Deliverables: a video (max 5 min, showing the world in play mode, not how it was made) and a 3-page PDF report. The world itself does not need to be a polished build, but it must run cleanly in Play mode for recording.

## Assignment requirements (must-haves)
- A **building** with at least one room, one door and one window. More rooms are better. Suitable furniture/contents consistent with the theme.
- The **window must have glass** and let the user see in or out (transparent glass material).
- The user must be able to **enter through a door**. Doors should open when the user commands it (press E) or automatically on approach, not just be permanently open.
- A **suitable external environment** around the building (terrain, forest, sky).
- **Textures** on both the building and the environment, in appropriate colours.
- **Movement** that is easy to navigate. Mouse turning must NOT be too fast (markers explicitly penalise this). Expose mouse sensitivity and move speed in the Inspector.
- **Interaction** with the world: the more, the better, as long as it works reliably.
- The developer's **student number** must be in the world, easy to find, and fit the theme (e.g. house number on a mailbox by the entrance, or a VHS tape label). Student number: `33010198`.

## How it is marked (prioritise accordingly)
| Area | Weight |
|---|---|
| Building, contents, doors, windows | 20% |
| Novel features / purpose | 20% |
| Interaction | 15% |
| Environment | 10% |
| Textures | 10% |
| Movement / ease of navigation | 10% |
| Report | 10% |
| Student number | 5% |

Novelty + interaction = 35%. A clear purpose (a simple game objective) and several working interactions are worth more than extra visual polish.

## Game concept (draft, developer may change)
- Setting: an isolated house in a dark forest at night.
- Objective: explore, find a key and collectible items (e.g. VHS tapes), unlock a locked door, then escape.
- Planned interactions: doors opened with E, a locked door + key, flashlight toggle, pickups that show text on screen, an objective / "you escaped" ending, ambient sound and at least one scare event.
- Optional extra if time allows: a key to toggle the retro effects on/off.

## Tech stack
- **Unity 6.3 LTS**, **Built-in Render Pipeline (BRP)**. Do NOT use URP/HDRP shaders, packages or APIs. Any shader written or imported must be BRP-compatible.
- C# scripts. Post Processing Stack v2 is acceptable for screen effects if needed.
- Unity is controlled via the MCP for Unity bridge (unityMCP tools).
- Target: Windows laptop with integrated graphics (Intel Arc). Keep performance light.

## Visual target (the "look")
PS1 layer (3D rendering):
- Vertex snapping / jitter, affine texture warping
- Point (nearest) filtering, small textures
- Low internal resolution via a render texture, upscaled with nearest-neighbour
- Fog and darkness, low-poly models

VHS layer (full-screen post effect):
- Colour bleed, noise and grain, scanlines, occasional tracking distortion, slight blur

**Keep effects moderate.** The markers judge the video: the building, furniture and textures must stay clearly visible, and effects must not be distracting. Atmospheric, not unreadable. All effect strengths should be tweakable in the Inspector.

## How to work in this project
- **Explain as you go.** After a change, briefly say what you did and why, in plain language. When introducing a Unity concept for the first time (e.g. render texture, material, prefab, collider), explain it in a sentence or two.
- Prefer simple, readable code with short comments over clever code. Use the old Input Manager or the Input System consistently; check which the project uses before writing input code.
- Make small, focused changes. Do one step at a time and check it works in Play mode before moving on.
- After editing or creating scripts: refresh assets, wait for compilation, then read the Unity console and fix any errors before reporting done.
- Before large or risky changes (deleting assets, restructuring folders, changing project settings), say what you plan to do and wait for approval.
- Never edit files in `Library/`, `Temp/`, `Logs/` or `obj/`. Never edit `.meta` files by hand.
- Commit to git after each working milestone with a clear message.
- Keep a running list of scripts written and assets imported (with sources) in `NOTES.md`. The report needs an overview of scripts and a list of sources.

## Folder layout
```
Assets/
  _Project/
    Scenes/
    Scripts/
    Shaders/
    Materials/
    Textures/
    Models/
    Prefabs/
    Audio/
  ThirdParty/   (imported assets and plugins, each in its own folder, kept unmodified)
```
Keep our own work inside `Assets/_Project/`.
