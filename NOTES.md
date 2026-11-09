# Project notes

Running log for the report: scripts written and third-party assets used (with sources).

## Scripts

| Script | Location | What it does |
|---|---|---|
| _(none yet)_ | | |

## Third-party assets

| Asset | Author | Source | Licence | Location in project | Notes |
|---|---|---|---|---|---|
| 6TWELVE (PSX-style convenience store, with road, parking lot and forest backdrop) | Elbolillo (Sketchfab: Elbolilloduro) | https://sketchfab.com/3d-models/6twelve-8c2c49cab08e402d8681434a4ade4ed0 | CC Attribution 4.0 | `Assets/ThirdParty/6twelve/` | Imported the author's original FBX + textures (from `source/6twelve.rar` in the Sketchfab download). ~18.8k triangles, 88 materials. Kept unmodified. |

## Asset findings / to-do

- 6TWELVE: real-world scale (store ~23 x 21 m). Five separate door meshes with pivots on the hinge edge (`Door`, `Door_01` = front double doors; `Door_02`, `Door_03`, `Door_04` = interior), so they can be scripted to swing open.
- Interior already has several areas: shop floor, counter, bathroom (toilet, sink, mirror), office (chair, monitor, keyboard), storage (boxes, fuse boxes, mop/broom).
- Window glass (`Glasses` object, `Glass` material) imports as opaque Standard - needs our own transparent glass material.
- Three materials lost their texture link on import (`Food_shelf_04`, `icecream_popsicles`, `Parking_lot`); the textures exist and need re-linking.
