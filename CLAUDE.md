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
- The developer's **student number** must be in the world, easy to find, and fit the theme (licence plate on the player's car + ID card at the checkout). Student number: `33010198`.

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

## Game concept (final)
Setting: a convenience store beside a dark road, surrounded by dense forest, on a foggy night. The player is a junky teenager on the way to meet friends who are exploring an abandoned house rumoured to be linked to a serial killer. The friends text asking the player to grab snacks on the way. This scene is the first chapter and ends when the player drives off.

Story beats, in order:
1. Scene opens with the player stepping out of their car parked outside the store. Subtitle (speech bar at the bottom of the screen): an annoyed comment about having to run snack errands for the friends.
2. On entering the store, objective (top of screen): "Use the toilet".
3. Toilet minigame: the player aims a stream into the toilet; a progress bar fills only while the aim hits the bowl. Keep it cartoony, not explicit.
4. On leaving: subtitle "What a mess... not my problem, heh."
5. Jumpscare on exiting the toilet: a customer suddenly appears close up with a loud sting, complaining the player took forever and they're desperate.
6. Objective: "Grab a basket". Once the basket is held, objective: "Grab snacks and beer". Items are picked up from different shelves and appear inside the held basket.
7. When everything is collected, objective: "Pay for your items".
8. The cashier and any customers turn their heads to track the player's camera while the player is in their field of view (rotate the head bone in LateUpdate, clamped to a natural angle range). This is the creepy indie-horror stare.
9. Player places the basket on the counter and talks to the cashier. He asks the player's name, with a dialogue choice:
   - Tell him: the conversation continues and he warns the player about people going missing in the area and to be careful.
   - Refuse: player says "None of your business, old timer, scan my stuff and let me get going." The cashier replies "You'll get what's coming for ya" and the rest of the payment happens in silence.
10. The cashier states the total and asks for ID. The player shows an ID card on screen showing the student number and other details.
11. Player gets the items in a bag, walks back to the car, subtitle: "That guy gave me the creeps." Interacting with the car fades to black and shows an "End of Chapter 1" screen. No driving needed.

Student number: also show it on the car's licence plate so it's visible at the start, as well as on the ID card at the end.

### Jumpscare (beat 5), how it should look
The whole sequence is fast (a second or two before the dialogue starts):
1. The customer moves very quickly into frame, their body covered by a dark shadow at first.
2. The screen shakes as they appear (with the loud sting).
3. Once they're in position (centre of the screen, close up), the shadow clears and they become fully visible.
4. Then the complaint dialogue starts.

### Customers (beat 8), how they behave
- About 2 customers standing in the store, looking at shelves and shopping.
- Movement is simple and basic (e.g. idle at a shelf, occasionally walk to another spot). No complex AI.
- They stare at the player (head tracking) when the player walks past them.

## Build priority (deadline: world finished Wednesday night)
Build in tiers. Finish and test each tier in Play mode before starting the next, so there's always a complete, working game.
- Tier 1 (core loop): reusable subtitle + objective UI system; doors that open with E (front door, toilet door); basket and item pickups shown in the basket; cashier dialogue with the name choice and both branches; ID card with student number; walk to car, fade to black, end screen; licence plate with student number.
- Tier 2 (creepy polish): head tracking on cashier and customers; jumpscare customer.
- Tier 3 (if time allows): the toilet aiming minigame. Until it's built, use a simple placeholder (e.g. hold E for a few seconds while a bar fills) so the story still flows.

## References
Visual references for the jumpscare, character movement and interactions go in a `References/` folder in the project root (outside Assets). Check there before building those features. The folder is empty for now: the developer will add references later. Until then, follow the written descriptions above.

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
