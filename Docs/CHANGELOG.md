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

### Known Issues

- `Packages/manifest.json` requests `com.unity.render-pipelines.universal` `17.6.0`, while Unity 6000.5.4f1 warns that it resolves its built-in `17.5.0` package, also recorded by the lock file. This was present at foundation audit and was intentionally not changed without a package-resolution task.

### Notes

- No gameplay systems, scenes, packages, or commits were created in the foundation documentation pass.
