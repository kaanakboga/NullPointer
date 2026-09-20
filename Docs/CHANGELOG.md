# Changelog

All notable project changes are recorded here. This project uses an `Unreleased` section until a release/versioning policy is established.

## Unreleased

### Added

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
- Expanded automated coverage to 33 EditMode and 8 PlayMode tests for content IDs, duplicate detection, evidence idempotence, deduction requirements/persistence, dialogue branches/invalid links, memory unlock, production asset/scene contracts, modal blocking, Inspect cancel, and Bootstrap startup.

### Changed

- Aligned the direct URP manifest request from `17.6.0` to Unity 6000.5.4f1's built-in `17.5.0`; the lock file already resolved `17.5.0` and no unrelated package changed.
- Replaced the generic template gameplay actions with Move, Interact, and Pause while preserving Unity UI navigation actions.
- Mode-gated Move and Interact at the input adapter while keeping Pause available for modal close; modal owners now control their own safe cancel paths.
- Replaced template Build Settings startup with Bootstrap → Eren Apartment → Mert Apartment and kept all test scenes excluded.

### Notes

- Full SaveManager, objective/notebook progression, interrogation, terminal search/credentials, memory reconstruction puzzles, final UI/art/audio, voice acting, and Chapter 2+ remain intentionally unimplemented.
- The opening is functional production foundation content with placeholder geometry and restrained uGUI visuals; it is not final art or a completed chapter.
- No package was installed or upgraded, and no commit was created automatically.
