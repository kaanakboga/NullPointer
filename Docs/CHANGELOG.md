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
- Added 10 EditMode and 2 PlayMode tests covering state, modes, input configuration, target selection, scene composition, movement blocking, and interaction dispatch.

### Changed

- Aligned the direct URP manifest request from `17.6.0` to Unity 6000.5.4f1's built-in `17.5.0`; the lock file already resolved `17.5.0` and no unrelated package changed.
- Replaced the generic template gameplay actions with Move, Interact, and Pause while preserving Unity UI navigation actions.

### Notes

- Save-file I/O, Inspect, Evidence, Dialogue, Terminal, Memory, final content, and story systems remain intentionally unimplemented.
- No package was installed or upgraded, and no commit was created automatically.
