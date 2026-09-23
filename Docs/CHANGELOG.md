# Changelog

All notable project changes are recorded here. This project uses an `Unreleased` section until a release/versioning policy is established.

## Unreleased

### Added

- Added Phase 5A.1 preview-layout regression coverage for the 1920×1080 reference, 2560×1440, and representative 2560×1600 16:10 viewport.
- Added the Phase 5A visual-production contract: a production-facing visual audit, objective visual QA rejection checklist, placeholder-to-asset inventory, and a 94-entry production asset manifest covering Eren, both apartments, Main Menu, shared UI, evidence, memory/VFX, and chapter transitions.
- Added the authored `VT_CyberNoir` semantic theme, locked palette/typography/motion/lighting/VFX/pixel standards, and the excluded `SCN_VisualStylePreview` demonstration with cyber-noir controls, panel motion, scanlines, and default-off memory distortion.
- Added reusable presentation-only fade/slide/scale/reveal/modal transitions, focus pulse, cyber-noir button states, screen/environment FX layers, and authored memory-distortion profile/controller infrastructure without a tween package or gameplay-state ownership.
- Added globally unique `FinalArtSlot` references and editor preservation logic so idempotent scene regeneration retains manually approved final sprites by stable slot ID while missing optional art falls back safely.
- Added category-scoped art import conventions for character, environment, UI, icon, effect, normal-map, sprite-sheet, and atlas paths while excluding source/reference and unrelated textures.
- Added Phase 5A EditMode validation for theme values, production visual roots, slot uniqueness/preservation/fallback, importer isolation/category rules, preview EventSystem ownership, and memory FX default state.

- Added schema-2 save payload integrity, sequential schema-1 migration with a checked-in legacy fixture, validated primary/backup loading, atomic backup restoration, and non-destructive future-schema handling.
- Added real-filesystem persistence tests covering atomic replacement, interrupted writes, corrupt-primary replacement, backup preservation/recovery, and a fresh-manager disk relaunch with checkpoint/mode restoration.
- Added editor-only save diagnostics for state/path inspection and safe create, reload, corruption, restore, and delete operations.
- Added defensive modal teardown recovery and nested pause/settings/journal cancel interception, including same-frame Pause/UI-Cancel deduplication.
- Added the production content/build validator for scene order, test-scene exclusion, stable IDs, catalog/reference integrity, Chapter 1 acquisition/gate links, missing scripts, installer wiring, EventSystems, AudioListeners, and UI scaling structure.
- Added reproducible Windows x86-64 Development/Release build automation with exact Unity-version checks, full-verifier integration, guarded clean output, ignored build logs/artifacts, and non-zero failure behavior.
- Added production pixel-art import enforcement, normal-map suffix handling, and deterministic 2D sorting-layer setup.
- Added `QA_CHAPTER1.md` separating automated evidence from the exact-player manual keyboard, gamepad, settings, resolution, save/relaunch, and clean-exit checklist.

- Established the planned production folders for art, audio, data, prefabs, scenes, scripts, and tests without relocating template assets.
- Added populated project documentation:
  - `GAME_DESIGN.md`
  - `STORY.md`
  - `TECHNICAL_DESIGN.md`
  - `ART_BIBLE.md`
  - `RULES.md`
  - `TASKS.md`
  - `CHANGELOG.md`
- Recorded the initial Unity, render pipeline, input, package, scene, and repository audit.
- Defined architecture, naming, content-ID, save/state, testing, accessibility, source-control, and definition-of-done rules.
- Added an ordered full-game backlog from project health through all three endings and Windows release.
- Added `Tools/Verify-Unity.ps1` for exact-version Unity batch compilation plus EditMode/PlayMode execution, ignored logs, NUnit result validation, and non-zero failure reporting.
- Added runtime, input, player, interaction, editor, EditMode test, and PlayMode test assembly boundaries.
- Added authoritative GameMode state and typed change events for Gameplay, Inspect, Dialogue, Terminal, EvidenceBoard, Memory, and Paused.
- Added schema-versioned `GameState` and serializable snapshots for current case/location/checkpoint, story flags, evidence, memories, and completed deductions.
- Added explicit New Input System reading for Move, Interact, and Pause with a pause-mode handler.
- Added configurable Rigidbody2D horizontal movement that immediately stops outside Gameplay mode.
- Added `IInteractable`, deterministic priority/distance/tie selection, bounded nearby detection, active-target notifications, and Gameplay-only dispatch.
- Added `SCN_Test_GameplayFoundation` and its editor builder for manual player, interaction, and GameMode verification.
- Added stable authored-content IDs, lowercase ASCII validation, path-aware duplicate diagnostics, an explicit runtime content catalog, and an editor validation command.
- Added ScriptableObject data for evidence, cases, characters, locations, inspections, deductions, dialogue, terminal entries, and timed memory presentation.
- Added duplicate-safe `EvidenceService`, pure `DeductionService`, pure branching `DialogueRunner`, and `MemoryService`, all integrated through the existing `GameState` APIs.
- Added production Inspect, evidence collection, evidence notification, Evidence Board, Dialogue, Terminal, Memory, interaction-prompt, and gated scene-transition components and uGUI presentation.
- Added `SCN_Bootstrap` with the explicit long-lived composition root and asynchronous `SceneLoader`, plus stable spawn-point installation for scene-local systems.
- Added production greybox scenes `SCN_ErenApartment` and `SCN_MertApartment`, connected through the authored dispatch flag while preserving the engineering scene.
- Added opening content for the 03:17 dispatch, neurological medication, environmental foreshadowing, Mert photograph/inscription, 02:51 terminal access, 02:36 death time, postmortem-terminal deduction, optional damaged recording, and first photograph-triggered memory glitch.
- Added the idempotent **Rebuild Authored Opening Content and Scenes** editor command; repeated batch runs rebuild 19 data assets and 3 production scenes successfully without duplicate content IDs.
- Expanded automated coverage to 89 EditMode and 12 PlayMode tests (101 total), including save migration/recovery/relaunch, content/build/visual validation, checkpoints, objectives, journal unlocks, interrogation logic, Chapter 1 happy path/completion, Main Menu state, settings separation, input-device bindings, production-scene horizontal-plane stability, modal blocking/recovery, importer isolation, final-art preservation, and interrupt-safe unscaled UI motion.
- Added the schema-2 `GameState` fields required for active/completed objectives, dialogue/terminal progression, and completed chapter boundaries.
- Added a versioned local JSON `SaveManager`, injected atomic Windows file storage with backup creation, explicit missing/corrupt/unsupported-schema outcomes, New Game/Save/Load/Continue/delete commands, and deterministic round-trip tests.
- Added five authored Chapter 1 checkpoints with safe location/spawn restoration and checkpoint-boundary saves.
- Added the production-functional Main Menu (`New Game`, validated `Continue`, `Settings`, `Quit`) and Pause Menu (`Resume`, `Investigation Journal`, `Settings`, `Main Menu`).
- Added separately persisted settings for master/music/SFX volume, fullscreen/windowed, resolution, and dialogue text speed.
- Added data-driven Chapter 1 objectives, unobtrusive HUD presentation, milestone-driven progression, and the explicit `chapter.01` completion boundary.
- Added a state-projected investigation journal with Cases, People, collected Evidence, Questions, and a concise contradiction-focused Timeline.
- Expanded Mert's apartment with atmospheric/lore interactions and evidence for the damaged implant, door status, 02:29 memory deletion, last call, and Eren's missing call history.
- Expanded the evidence board into a three-step Chapter 1 deduction chain with prerequisite deductions and restrained retry feedback.
- Added a distinct interrogation mode, authored claims, evidence presentation, retry-safe contradiction logic, persistent branch unlocks, and a reusable controller/panel/interactable layer.
- Added optional dialogue typewriter reveal, instant reveal/skip, current-conversation history, persisted node progress, and keyboard/gamepad focus behavior.
- Expanded Chapter 1 terminal content across Logs, Files, Mail, and Security with gated entries and stable read progress.
- Added the Chapter 1 ending sequence revealing Mert's attempted call to Eren, the missing corresponding device record, and `NLP-0417` without exposing later canon.
- Expanded the photograph memory glitch with the laboratory, alarm, Mert, Eren, two indistinct figures, and “Bunu başlatırsak geri dönüşü yok.”
- Added nullable authored audio hooks for rain, electrical hum, traffic, terminal/UI feedback, evidence, memory glitch, and chapter-end sting; no binary audio was fabricated.
- Extended the idempotent opening builder to regenerate the Main Menu, Chapter 1 assets, checkpoints, objectives, journal/timeline, audio hooks, UI wiring, scenes, and Build Settings.
- Added deterministic Chapter 1 happy-path, chapter boundary, save corruption/schema, checkpoint, objective, journal, interrogation, Main Menu availability, settings, and input-device coverage.

### Changed

- Corrected every animated sheet and reusable atlas specification to state per-cell dimensions, frame/cell count, columns, rows, total dimensions, playback timing, and Unity mirroring policy; Eren idle is now the intended 2× 32×64 layout in a 64×64 total sheet.
- Classified the full manifest as final raster art, reusable texture/mask/atlas, or Unity-procedural visual, and removed giant full-screen rain, reflection, and chapter-sting animation sheets from the external-generation burden.
- Made the first Eren and Eren-apartment production batches approval-gated and dependency-explicit without changing the locked cyber-noir direction.
- Rebuilt `SCN_VisualStylePreview` around the existing 1920×1080, 0.5-match CanvasScaler with explicit 96×72 safe margins and edge-correct pivots so essential content remains in view at supported 16:9 and 16:10 resolutions.
- Extended the opening scene builder to create visual-root anchors and manifest-linked art slots for Main Menu, Eren Apartment, Mert Apartment, Eren, and current Chapter 1 props, while preserving final-art references across structural regeneration.
- Expanded the art bible and technical/rules documentation into the authoritative Phase 5A production standard; procedural preview assets are explicitly non-final and Chapter 2/full UI replacement remain out of scope.

- Fixed production player physics in both apartment scenes: horizontal-only exploration now uses a Dynamic Rigidbody2D with zero gravity, frozen Y/rotation, and controller-enforced vertical-velocity cleanup instead of relying on greybox floor coverage.
- Added a production-scene PlayMode regression that reproduces the former Eren fall, then verifies idle Y stability and horizontal movement in both Eren and Mert apartments.
- Upgraded the save envelope from schema 1 to schema 2 while retaining explicit compatibility through migration; settings remain in an independent file and are unaffected by New Game/save deletion.
- Normalized loaded modal modes to Gameplay and added safe fallback when a persisted checkpoint is missing or unusable.
- Hardened modal owners so unexpected disable/scene teardown cannot strand gameplay in a blocked mode.
- Rebuilt production scenes with one AudioListener per scene and consistent 1920×1080 CanvasScaler matching; locked world art to 480×270, 16 PPU, and a 16-pixel grid.
- Set the Windows product name to `Null Pointer: Anılar Silinmeden Önce` without inventing company or application-identifier branding.

- Aligned the direct URP manifest request from `17.6.0` to Unity 6000.5.4f1's built-in `17.5.0`; the lock file already resolved `17.5.0` and no unrelated package changed.
- Replaced the generic template gameplay actions with Move, Interact, and Pause while preserving Unity UI navigation actions.
- Mode-gated Move and Interact at the input adapter while keeping Pause available for modal close; modal owners now control their own safe cancel paths.
- Changed production startup to Bootstrap → Main Menu → Eren Apartment → Mert Apartment while keeping test scenes excluded.
- Changed terminal entries to support story-gated layered disclosure and typed read events, and deductions to support prerequisite conclusions.
- Changed gamepad UI Cancel to route through modal close behavior without enabling gameplay interaction input.

### Notes

- Save-slot UI/request queuing, terminal credentials/search, memory reconstruction puzzles, final UI/art/audio, voice acting, and Chapter 2+ remain intentionally unimplemented.
- Chapter 1 is a coherent production-functional greybox with placeholder geometry and restrained uGUI visuals; it is not final art or final-duration content.
- No package was installed or upgraded, and no commit was created automatically.
