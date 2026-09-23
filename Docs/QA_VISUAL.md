# Null Pointer — Visual QA Contract

## Purpose

This checklist is the objective rejection gate for the production visual overhaul. Passing automated tests proves infrastructure integrity, not visual approval. Record screenshots or short captures for each supported resolution, the exact build, active accessibility profile, input device, and reviewer.

## Automatic Rejection Criteria

Reject a production candidate if any of the following is visible or reproducible:

- default Unity button, slider, toggle, dropdown, scrollbar, or input-field appearance;
- engineering/debug markers, world-space implementation labels, collider guides, or test instructions;
- solid-color interaction dots or evidence objects presented as RPG glowing pickups;
- placeholder capsule/rectangle player or a built-in UI sprite presented as a character;
- empty black-room environment or obvious greybox primitives presented as final scenery;
- unstyled generic uGUI/TMP text blocks with no hierarchy or spacing system;
- developer-only labels, stable IDs, asset IDs, scene names, or diagnostic copy in production;
- blurry, uneven, shimmering, or inconsistently scaled pixel art;
- rotated or fractionally scaled world pixels without an approved effect exception;
- unreadable glitch, channel split, tearing, bloom, grain, vignette, haze, or rain;
- evidence highlighted with constant glow instead of composition, value, framing, or authored motion;
- saturated rainbow cyberpunk accents that break the locked palette hierarchy;
- critical meaning communicated only by color, a short glitch frame, or audio;
- duplicate EventSystems, AudioListeners, visual roots, or stable final-art slot IDs;
- missing optional art producing an exception, invisible required interaction, or broken layout;
- an opening-builder regeneration clearing a manually assigned `FinalArtSlot` sprite;
- normal gameplay starting with memory-distortion overlays active.

## Composition and Readability

- [ ] The scene focal question reads within two seconds at gameplay zoom.
- [ ] Eren separates from the immediate background in grayscale and with reduced effects.
- [ ] Interactables are discoverable without an icon carpet or universal cyan outline.
- [ ] Foreground silhouettes frame rather than cover movement, clues, subtitles, or prompts.
- [ ] Cyan, violet, amber, and red retain their documented semantic roles.
- [ ] Red remains rare and does not become ambient wallpaper.
- [ ] The brightest value is reserved for text, faces, documents, or the active focal point.
- [ ] Evidence objects look physically embedded and do not resemble loot pickups.

## Resolution and Aspect-Ratio Matrix

For every row, check Main Menu, Gameplay HUD, Pause, Settings, Inspect, Dialogue choices, Terminal, Evidence Board, Journal, notification, and Memory presentation.

| Resolution | Aspect | Required checks |
| --- | --- | --- |
| 1920×1080 | 16:9 baseline | Pixel integer scale, framing, no clipping, UI reference match, prompt/subtitle safe area |
| 2560×1440 | 16:9 high resolution | Crisp scale, no fractional sprite shimmer, effects do not become visually stronger |
| 1920×1200 | 16:10 representative | No stretched world art; composition crop/extension is intentional; UI remains inside safe area |

At each resolution:

- [ ] Windowed and fullscreen rendering match expected framing.
- [ ] Minimize/restore does not leave overlays, focus, or post effects in the wrong state.
- [ ] Long Turkish strings wrap without clipping controls or hiding input focus.
- [ ] Pixel-art camera movement and player motion show no crawl, wobble, or unequal pixels.

## Input and Motion

- [ ] Keyboard focus is visible by shape/value/motion, not color alone.
- [ ] Controller focus is visible on every actionable control and initial focus is deterministic.
- [ ] Mouse hover never steals controller focus unpredictably.
- [ ] Normal focus/press transitions complete in 120–220 ms.
- [ ] Modal entrance/exit completes in 180–320 ms.
- [ ] Important authored reveals complete in 300–650 ms unless memory timing is explicitly authored.
- [ ] Rapid open/close, focus switching, or repeated submit interrupts transitions cleanly with no stuck alpha, scale, position, or raycast state.
- [ ] Pause-time UI motion uses unscaled time and remains responsive at `timeScale = 0`.
- [ ] No UI animation changes `GameMode`, progression, save state, or command execution timing.

## Effects and Accessibility

- [ ] Normal gameplay uses no chromatic split or tearing.
- [ ] Scanlines/grain/noise remain subordinate to text and silhouettes.
- [ ] Rain communicates depth without creating high-frequency flicker.
- [ ] Bloom does not expand text strokes or erase pixel clusters.
- [ ] Memory distortion returns completely to OFF after completion, skip, disable, and scene change.
- [ ] Reduced-glitch mode preserves every semantic cue with lower amplitude/frequency.
- [ ] Reduced-flashing mode removes full-field flashes while retaining a readable border/value cue.
- [ ] Zero-shake mode contains no camera displacement.
- [ ] Subtitles, choices, pause/settings, and safety prompts remain undistorted.

## Art Delivery and Import

- [ ] Asset ID and output filename match `ART_ASSET_MANIFEST.md`.
- [ ] Pixel dimensions, alpha, PPU, pivot, slicing, and category folder match the contract.
- [ ] Point filter, compression, mipmap, and NPOT behavior match the category convention.
- [ ] Sprite-sheet assets live only in `SpriteSheets`/`Atlases` and have reviewed slicing.
- [ ] Source/reference images are not silently treated as production sprites.
- [ ] Transparent edges have no dark/bright fringe at nearest-neighbor scale.
- [ ] AI-assisted outputs have provenance, prompt/reference ownership, date/model, and human cleanup recorded.
- [ ] Anatomy, perspective, light direction, palette, silhouette, tiling seams, and consistency are human-approved.

## Review Record

| Field | Value |
| --- | --- |
| Build / git state | |
| Reviewer / date | |
| GPU / Windows / display scaling | |
| Resolution / mode | |
| Keyboard / controller | |
| Effect accessibility profile | |
| Result | NOT RUN |
| Rejected items and asset IDs | |
