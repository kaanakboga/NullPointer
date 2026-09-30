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
| 2560×1600 | 16:10 representative | No stretched world art; composition crop/extension is intentional; UI remains inside safe area |

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

### Phase 5B Main/Pause/Settings Gate

- [x] At 1920×1080, 2560×1440, and 2560×1600, the title, subtitle, command stack, settings controls, and pause actions remain inside explicit safe margins with no overlap or clipping.
- [x] Main Menu reveal may be accelerated and always leaves title/options fully visible and interactive.
- [x] New Game and Continue accept only one dispatch while their presentation transition is active; scene-load correctness does not depend on the visual animation surviving scene teardown.
- [x] Opening Settings transfers focus to Master Volume; closing via Back or Cancel restores the prior valid Main/Pause selection.
- [x] Pause enters/exits in unscaled time, preserves the live scene beneath the treatment, and never exposes a one-frame default control appearance.
- [x] Sliders show their numeric value, toggles show both shape and `AÇIK`/`KAPALI`, and the dropdown has a visible disclosure/focus state.
- [x] Reduced UI Motion removes parallax/slide/scale movement and shortens nonessential transitions; Reduced Visual FX lowers rain/haze/screen/title effects. Both survive relaunch in the settings file.
- [x] With all UI audio clips unassigned, focus/confirm/back/invalid hooks remain silent and exception-free.

Phase 5B.1 evidence: `Phase5BVisualCapture.CaptureAll` generated and the team inspected Main Menu, Main Menu Settings, Pause, and Pause Settings at all three required resolutions under ignored `Logs/VisualReview/`. The accepted pass removes colored interaction markers, eliminates blank/debug-formatted settings values, and preserves deliberate 16:10 composition.

### Phase 5C Investigation-Surface Gate

- [x] Dialogue, Inspect, Evidence Acquired, Evidence Board, Deduction Solved, Terminal, Journal, Objective Update, Interrogation, and Memory stay within safe bounds at 1920×1080, 2560×1440, and 2560×1600.
- [x] Every modal has deterministic initial focus, keyboard/controller navigation, mouse-selectable controls, immediate `GameMode` recovery, and no gameplay-input bleed while open.
- [x] Entrances, exits, staged rows/choices, connection emphasis, and feedback use unscaled interrupt-safe transitions; reduced-motion/effects settings preserve semantic state.
- [x] Missing portrait/object/thumbnail art produces an authored fallback while approved replacements remain isolated behind stable `FinalArtSlot` IDs.
- [x] Evidence selection, invalid deduction, solved deduction, and contradiction feedback do not use destructive state changes or color as the only cue.
- [x] Terminal hierarchy remains readable under scan/cursor treatment; Journal questions/timeline and compact Objective updates do not resemble generic menu or MMO tracker UI.
- [x] Memory is the only full-frame corruption treatment, uses supported authored text, and restores all transient FX on completion, skip, disable, and scene change.
- [x] Production scenes contain one EventSystem, no missing scripts, and no default Unity control presentation on Phase 5C surfaces.

Phase 5C evidence: `Phase5CVisualCapture.CaptureAll` produced 30 actual Unity-rendered production captures under ignored `Logs/VisualReview/`. All ten surfaces were inspected at all three required resolutions; no screenshot-diff assertions are used. Automated verification covers scene structure, variants/slots, modal state recovery, transition cleanup, connection pooling, and memory teardown.

### Phase 5C.1 Final Polish Gate

- [x] Dialogue and Interrogation missing portraits use intentional procedural silhouettes with no engineering placeholder language.
- [x] Inspect and Evidence Acquired missing-art states use compact forensic fallbacks and preserve readable copy/safe margins.
- [x] Sparse Journal categories use a narrower readable column while Questions and Timeline retain the wide layout path.
- [x] Evidence Board selection, active relations, and solved-column separation remain legible at 16:9 and 16:10.
- [x] Memory uses irregular omission strips, smaller displaced/echo regions, and a protected central text field rather than stacked full-screen placeholder panels.
- [x] Dialogue, Inspect, Evidence Acquired, Evidence Board, Interrogation, Journal, Memory, and Terminal were manually inspected at 1920×1080 and 2560×1600.
- [x] Automated verification passed at 106/106 EditMode and 15/15 PlayMode after the final capture-driven correction.

### Phase 5D Production-Environment Gate

- [x] Actual production scenes were captured with a graphics-enabled Unity batch process; `-nographics` is forbidden for URP visual capture because it cannot allocate the required render textures.
- [x] Eren and Mert each contain the documented named hierarchy, independent environment `FinalArtSlot` ownership, one Global Light 2D, four visible-source practical lights, one AudioListener, one EventSystem, and no visible interaction markers or world debug labels.
- [x] Eren reads as a warmer personal apartment with sleeping, circulation, workstation, city/window, rain, reflection, and foreground zones; Mert reads as a colder research/crime scene with workstation, displays, medical-memory equipment, evidence, cable/clutter, storage, and controlled red fault zones.
- [x] Player staging separates from the immediate background; approved Eren art can replace it while its structural fallback automatically hides.
- [x] Normal gameplay uses restrained Bloom, Color Adjustments, and Vignette only; Film Grain and Chromatic Aberration remain off. UI is rendered after the world treatment and stays readable.
- [x] Pooled rain varies position, length, angle, opacity, depth, and speed deterministically; reduced effects samples the pool while reduced motion freezes parallax and nonsemantic drift.
- [x] 33 captures were reviewed at 1920×1080, 2560×1440, and 2560×1600. Crushed-black/transparent-primitive and oversized colored-slot passes were rejected; the accepted pass hides missing-art targets and uses detailed separate staging without a dead lower viewport band.
- [x] 16:9 uses integer pixel scaling; representative 16:10 keeps the 16:9 world composition centered with deliberate bars and no stretch or UI clipping.

### Phase 5D.1 Environment-Rescue Gate

- [x] No enabled legacy blockout SpriteRenderer remains under either production `Visual Root`; only one 480×270 authored staging plate, irregular rain sprites, and assigned final-art slots may render.
- [x] The functional walk plane, colliders, movement bounds, interaction anchors, and evidence IDs remain present but have no visible gameplay-band or marker presentation.
- [x] Eren's sleeping, circulation, workstation, city/window, and foreground zones read as a composed room with distinct wall, floor, wood, metal, glass, cloth, screen, paper, and plastic value treatment.
- [x] Mert reads as a separate clinical/research environment with structured storage, monitor-grid workstation, dedicated implant micro-scene, disturbed chair/cables/papers, and integrated evidence rather than an Eren recolor.
- [x] City rooflines, service masses, clustered/broken windows, mullions, sill, glass tint/reflection, fake AO, floor perspective, and foreground framing replace visible giant flat blocks.
- [x] Rain uses a small irregular point-filtered atlas, deterministic non-uniform placement, varied scale/angle/opacity, sparse baked contact marks, and deliberate quiet areas.
- [x] The required six 1920×1080 views and representative 2560×1440 / 2560×1600 views were manually reviewed after the final density correction; no visible walk-plane band or regular debug-line field remains.

### Phase 5D.2 Cinematic-Polish Gate

- [x] Background, midground, focal gameplay areas, and foreground have distinct contrast bands; neither room is a uniform blue-grey field.
- [x] Eren has three readable anchors with workstation strongest, bed warmest, and door quieter; amber practicals remain localized against the cold city/window/terminal.
- [x] Mert uses graphite structure, tiered cyan monitor brightness, a localized implant examination pool, and rare meaningful fault red.
- [x] Rain, glass/reflection texture, city-window activity, and floor perspective contain quiet regions and do not obscure silhouettes or dominate the frame.
- [x] The player fallback remains visibly temporary but separates through backdrop value, motivated cyan edge, and contact shadow without a glowing outline or fabricated final character art.
- [x] Eren normal/workstation/window and Mert entrance/workstation/implant were reviewed at 1920×1080; representative 2560×1600 captures preserve hierarchy and integer-scaled framing.

## Effects and Accessibility

- [ ] Normal gameplay uses no chromatic split or tearing.
- [ ] Scanlines/grain/noise remain subordinate to text and silhouettes.
- [ ] Rain communicates depth without creating high-frequency flicker.
- [ ] Bloom does not expand text strokes or erase pixel clusters.
- [x] Memory distortion returns completely to OFF after completion, skip, disable, and scene change.
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
