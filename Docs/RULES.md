# NULL POINTER: ANILAR SİLİNMEDEN ÖNCE — Project Rules

## Authority

These rules apply to all repository work. When documents disagree, explicit task instructions take priority, then this file, then `TECHNICAL_DESIGN.md`, `GAME_DESIGN.md`, `STORY.md`, and `ART_BIBLE.md` within their respective subjects. Update the affected documents when an approved decision changes a rule.

## Engine and Repository

- Use Unity **6000.5.4f1**. Do not upgrade or downgrade without an explicit migration task.
- Target Windows and preserve the Universal 2D / URP 2D setup.
- Inspect `git status` before significant changes and again before handoff.
- Preserve user work and unrelated changes. Never rewrite history, force-push, or run destructive Git commands.
- Do not create commits unless explicitly requested.
- Do not commit `Library`, `Temp`, `Logs`, `Obj`, `UserSettings`, IDE state, generated solution/project files, or player builds.
- Commit Unity `.meta` files with their assets. Move/rename assets with their `.meta` through Unity or a GUID-safe workflow.
- Never invent or casually edit an existing `.meta` GUID.
- Do not add packages without a concrete requirement, compatibility check, license review, and explicit task scope.
- The project must remain openable after each completed task.

## Folder Ownership

| Path | Purpose |
| --- | --- |
| `Assets/Art` | Production visual assets grouped by Characters, Environments, UI, Effects, Icons |
| `Assets/Audio` | Music, Ambience, SFX, Voice |
| `Assets/Data` | Authored gameplay `ScriptableObject` assets |
| `Assets/Prefabs` | Reusable Unity prefabs grouped by role |
| `Assets/Scenes` | Bootstrap, MainMenu, Gameplay, Test scenes |
| `Assets/Scripts` | Runtime/editor source grouped by feature |
| `Assets/Settings` | Unity/template render and input settings |
| `Assets/Tests` | EditMode and PlayMode tests/fixtures |
| `Docs` | Design, technical, production, and change records |

Do not create catch-all `Misc`, `Common`, or `Helpers` folders. A genuinely cross-cutting type belongs in `Core` and must have a clear domain-neutral purpose.

## Naming Conventions

### C#

- Root namespace: `NullPointer`.
- Feature namespaces: `NullPointer.Core`, `NullPointer.Player`, `NullPointer.Interaction`, and equivalent folder-aligned names.
- Types, methods, properties, events, and public fields: `PascalCase`.
- Interfaces: `I` + `PascalCase` (`ISaveStorage`).
- Private instance fields: `_camelCase`.
- Parameters and local variables: `camelCase`.
- Constants: `PascalCase`.
- Boolean names describe a true state (`IsDiscovered`, `CanInteract`, `HasCheckpoint`).
- Event names describe what happened (`EvidenceDiscovered`) and event handlers use `OnEvidenceDiscovered`.
- Async methods end in `Async`; cancellation is accepted and propagated for operations that can outlive a frame or scene.
- One primary public type per file; filename matches that type.
- Avoid abbreviations except established domain terms such as ID, UI, SFX, or NLP.

### Unity Assets

Use descriptive `PascalCase` names and a type prefix where it improves searchability:

- scenes: `SCN_Bootstrap`, `SCN_MainMenu`, `SCN_CH01_MertApartment`;
- prefabs: `PF_Player`, `PF_InteractPrompt`, `PF_Terminal`;
- data assets: `EV_MertPhotograph`, `CHAR_ErenVardar`, `DLG_CH01_FirstInterview`;
- sprites/textures: `CHR_Eren_Idle`, `ENV_MertApartment_Walls`, `UI_Notebook_TabEvidence`;
- audio: `MUS_CH01_Investigation`, `AMB_MertApartment_Rain`, `SFX_Evidence_Acquired`;
- materials/shaders: `MAT_MemoryGhost`, `SHD_GlitchBlock`;
- animation clips/controllers: `ANIM_Eren_Walk`, `AC_Eren`.

Do not rename imported assets merely to change capitalization if that risks GUID or case-sensitive path churn; use a deliberate Unity-safe rename task.

### Stable Content IDs

- Format: lowercase dot-separated ASCII identifiers, for example `evidence.mert.photo_0417`.
- Start with a domain: `character`, `evidence`, `dialogue`, `case`, `location`, `memory`, `deduction`, `flag`, `objective`, `terminal`, or `ending`.
- IDs are unique, immutable once released, and independent of display text, file path, asset name, or Unity GUID.
- Turkish display text may contain full Turkish characters; stable IDs remain ASCII for tooling safety.
- Never use enum ordinals, list indices, or scene hierarchy paths as persistent IDs.

### Git Branches and Commits

- Default branch prefix when a branch is requested: `codex/`.
- Suggested commit format when commits are explicitly requested: `<type>(<area>): <imperative summary>`.
- Keep generated asset reimports separate from authored logic where practical.

## C# and Unity Code Rules

- Prefer composition over inheritance. Inheritance is reserved for genuine substitutable behavior.
- `MonoBehaviour` components coordinate Unity lifecycle and presentation; domain rules belong in pure C# types where practical.
- Use `ScriptableObject` for immutable authored content/configuration, not mutable per-save state.
- Use `[SerializeField] private` instead of public mutable fields.
- Validate serialized dependencies in `OnValidate` or dedicated validators when useful; fail with actionable messages.
- Do not use `GameObject.Find`, string-based `SendMessage`, or repeated scene-wide searches in production paths.
- Avoid hidden singleton access. Global lifetime services are wired by the Bootstrap composition root.
- Do not add `DontDestroyOnLoad` without documenting why scene lifetime is insufficient.
- Subscribe and unsubscribe symmetrically; do not leave scene objects referenced by long-lived publishers.
- Avoid work and allocation in `Update` unless it is actually frame-driven. Cache component references used repeatedly.
- Use coroutines for frame sequencing, not as an implicit domain state machine. Use explicit state for multi-step gameplay flows.
- Do not hard-code dialogue, evidence, story flags, or scene progression in presentation scripts.
- Do not catch exceptions only to suppress them. Add context or recover at an owned boundary.
- Logs must identify the subsystem and actionable context; remove noisy per-frame logs.
- Avoid premature generic frameworks. Extract an abstraction after the boundary is understood or where tests/infrastructure require it.

## Dependency Rules

- Dependencies point inward: presentation → domain contracts; infrastructure implements domain/application contracts.
- Feature code may depend on `Core`; `Core` must not depend on feature presentation code.
- UI reads projections/view models and submits commands; it does not mutate `GameState` fields directly.
- Scene names, mixer parameter names, and other external keys are centralized or asset-referenced and validated.
- Cross-feature communication uses narrow interfaces or specific typed events, not a universal untyped event bus.
- Circular assembly references are prohibited.

## Content Authoring Rules

- Every authored content asset has a stable ID and concise editor-facing description.
- Required references, condition targets, and effect targets must be validated before a build.
- Display text remains localization-ready even while Turkish is the initial authored language.
- Conditions and effects are typed data. Do not store arbitrary executable expressions in content.
- Canon changes are recorded in `STORY.md` before dependent dialogue or scene implementation.
- Critical-path information is available through at least one mandatory route; optional routes may reinforce it.
- Evidence descriptions distinguish observed fact from Eren's inference.
- A deduction cannot require evidence that becomes unavailable before the deduction is solvable.
- Irreversible choices are explicit and checkpointed.

## Scene and Prefab Rules

- Production startup flows through the Bootstrap scene after it is implemented.
- Gameplay scenes own local presentation and references; persistent services do not own location objects.
- Prefab overrides are intentional and reviewed; apply or revert accidental overrides before handoff.
- Avoid deep hierarchies and behavior controlled only by sibling order.
- Every production scene has a predictable entry/spawn contract and a safe transition exit.
- Test scenes live under `Assets/Scenes/Test` and are not included in release Build Settings.
- Template assets remain until a task explicitly replaces or removes them.

## Input and UI Rules

- Keyboard/mouse and gamepad are supported for every required action and menu.
- Input maps are enabled/disabled by state; gameplay input cannot leak through blocking UI.
- Never bind progression to a device-specific key in gameplay code.
- Each modal establishes initial focus and restores prior focus when closed.
- A visible state must accompany color-only focus/status cues.
- UI layouts are tested with Turkish text, long strings, 1920×1080, and supported aspect-ratio extremes.
- Accessibility settings must affect all relevant presentation consistently.

## Save and State Rules

- Only the authoritative domain state is saved; Unity objects and authored assets are referenced by stable ID.
- Every save schema has an explicit integer version.
- A schema change includes migration or a documented compatibility break before release.
- Save writes are atomic with validation and backup; failures are surfaced without destroying the last valid save.
- Never overwrite a future-schema primary save. Backup recovery requires full validation, restores the primary atomically, and preserves the known-good backup.
- Every schema upgrade is sequential, has a checked-in legacy fixture, normalizes data through explicit APIs, and is covered by a regression test.
- Checkpoints occur only at known-safe boundaries.
- State transitions use APIs that preserve invariants; no arbitrary dictionary or field writes from presentation code.
- Global settings are stored separately from save slots.

## Testing Rules

- Important pure logic receives focused EditMode tests.
- Unity lifecycle, input integration, scene transition, and UI navigation receive PlayMode coverage where it protects real behavior.
- Tests use Arrange–Act–Assert structure and name the behavior and expected outcome.
- One test should fail for one understandable reason.
- Do not test Unity itself, trivial property accessors, or implementation details with no behavioral value.
- Bug fixes include a regression test when the failure is deterministic and reasonably testable.
- Tests must clean up created files, scenes, objects, and static state.
- A task is not complete with known compiler errors, failing relevant tests, or an unverified changed scene/prefab.
- A production Windows build must pass the full Unity verifier and production content validator; test scenes and editor-only diagnostics are excluded from player builds.
- Pixel-art assets under `Assets/Art` follow `ART_BIBLE.md` import rules. Intentional exceptions are documented and reviewed rather than silently overriding the importer.

## Documentation and Change Tracking

- Update `TASKS.md` when task scope/status changes.
- Update `CHANGELOG.md` under `Unreleased` for user-visible or architecture-significant work.
- Update design documents in the same change when behavior or architecture intentionally changes.
- Comments explain why, constraints, or non-obvious risk; they do not narrate straightforward code.
- Public APIs with non-obvious contracts receive XML documentation.

## Definition of Done

A task is complete only when:

1. The requested behavior and explicit acceptance criteria are met.
2. Existing user work is preserved and unrelated files are untouched.
3. Relevant automated tests pass, or the reason they cannot run is reported.
4. Changed scenes/assets are opened or otherwise validated at an appropriate level.
5. There are no new compiler errors or avoidable warnings.
6. Keyboard/gamepad/accessibility implications are considered where relevant.
7. Save compatibility implications are considered for state changes.
8. `TASKS.md`, `CHANGELOG.md`, and affected design documents are current.
9. `git diff` and `git status` have been reviewed.
10. The handoff states exactly what changed, what was verified, and any remaining risk.

## Prohibited Without Explicit Approval

- Unity editor version changes;
- package installation/removal/upgrades;
- broad asset relocation or deletion;
- render pipeline replacement;
- source-control history rewriting or commits;
- external telemetry, online accounts, or network services;
- changing established story canon or ending structure;
- introducing traditional combat or other scope that conflicts with `GAME_DESIGN.md`.
