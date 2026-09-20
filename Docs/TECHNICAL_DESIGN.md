# NULL POINTER: ANILAR SİLİNMEDEN ÖNCE — Technical Design

## Purpose and Status

This document defines the target technical architecture. It is a living design, not evidence that the listed systems are already implemented. Current implementation status is tracked in `TASKS.md` and changes are recorded in `CHANGELOG.md`.

## Audited Project Baseline — 2026-09-20

| Area | Observed state |
| --- | --- |
| Unity editor | `6000.5.4f1` (`d550df8bd089`) |
| Project template | Universal 2D |
| Render pipeline | URP asset assigned for all quality levels; default renderer references `Renderer2D.asset` |
| Color space | Linear |
| Input handling | New Input System only (`activeInputHandler: 1`) |
| Input actions | Production foundation `Gameplay` map (Move, Interact, Pause) plus retained Unity `UI` navigation map |
| Build scenes | Only `Assets/Scenes/SampleScene.unity`, enabled |
| Resolution baseline | 1920×1080 project default |
| Test support | Unity Test Framework configured with EditMode and PlayMode assemblies plus command-line verification |
| Existing game code | Core mode/state, input, horizontal player movement, and interaction foundation |
| Existing authored content | None observed beyond template scene/settings |

### Direct Package Manifest

The project currently declares the following non-module packages. No packages were added during the foundation phase.

| Package | Manifest version |
| --- | --- |
| 2D Animation | `15.1.0` |
| 2D Aseprite Importer | `5.0.3` |
| 2D PSD Importer | `14.0.3` |
| 2D Sprite | `1.0.0` |
| 2D SpriteShape | `15.0.3` |
| 2D Tilemap | `1.0.0` |
| 2D Tilemap Extras | `8.0.3` |
| 2D Tooling | `3.0.1` |
| Version Control | `2.12.4` |
| JetBrains Rider Editor | `3.0.38` |
| Visual Studio Editor | `2.0.26` |
| Input System | `1.19.0` |
| Multiplayer Center | `1.0.1` |
| Universal Render Pipeline | `17.5.0` |
| Test Framework | `1.7.0` |
| Timeline | `1.8.12` |
| Unity UI (uGUI) | `2.5.0` |
| Visual Scripting | `1.9.11` |

The manifest also directly declares these built-in engine modules at `1.0.0`: Accessibility, Adaptive Performance, AI, Android JNI, Animation, Asset Bundle, Audio, Cloth, Director, Image Conversion, IMGUI, JSON Serialize, Particle System, Physics, Physics 2D, Physics Core 2D, Screen Capture, Terrain, Terrain Physics, Tilemap, UI, UI Elements, Umbra, Unity Analytics, Unity Web Request, Unity Web Request Asset Bundle, Unity Web Request Audio, Unity Web Request Texture, Unity Web Request WWW, Vector Graphics, Vehicles, Video, Wind, and XR.

The manifest and lock file now both record Unity 6000.5.4f1's built-in `com.unity.render-pipelines.universal` `17.5.0`. No unrelated package version changed during alignment.

## Unity Editor and Command-Line Verification

The installed editor was discovered from the machine rather than assumed. Verified workstation path:

`C:\Program Files\Unity\Hub\Editor\6000.5.4f1\Editor\Unity.exe`

File product version: `6000.5.4f1_d550df8bd089`.

Reusable verification entry point:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Tools\Verify-Unity.ps1 -Mode All
```

Supported modes are `All`, `Compile`, `EditMode`, and `PlayMode`. The script reads the required version from `ProjectSettings/ProjectVersion.txt`, accepts an explicit `-UnityPath`, then checks `UNITY_PATH` and versioned Unity Hub install locations. It validates the executable's product version before launch, writes logs and NUnit XML under ignored `Logs/Verification`, scans compile logs for known fatal/compiler signatures, validates that tests were actually discovered, and exits non-zero on failure. Machine-specific editor paths are not embedded in runtime code.

Unity does not permit batch mode to open a project already owned by another editor. The verifier warns when `Temp/UnityLockfile` exists and then lets Unity fail safely rather than terminating the active editor. The current workspace verification compiles the project and runs both test modes directly against the production scenes and assets; the latest clean totals are recorded in the Testing Strategy section.

## Architectural Goals

- Keep authored content separate from mutable playthrough state.
- Keep domain rules testable without loading scenes.
- Express dependencies explicitly through constructors for pure C# objects and serialized references or composition-root wiring for Unity components.
- Prefer small components with one reason to change.
- Keep global lifetime limited to services that must survive scene changes.
- Make save/load a supported boundary from the first persistent system.
- Make invalid content fail loudly in editor validation, not silently at runtime.
- Preserve deterministic story progression independent of scene object activation.

## Proposed Runtime Layers

### Content Layer

Immutable authored `ScriptableObject` assets and referenced media:

- `EvidenceData`
- `CharacterData`
- `DialogueData`
- `CaseData`
- `MemoryData`
- `DeductionData`
- `LocationData`

Content assets use stable string IDs, editor validation, and references to other content assets where cycles are controlled. They do not store per-save mutable state.

### Domain Layer

Pure C# logic where practical:

- story flag store and typed conditions;
- evidence discovery and annotation state;
- deduction evaluation;
- objective/case progression;
- dialogue condition/effect evaluation;
- save model validation and migration;
- ending eligibility.

This layer must not query active scenes, `FindObjectOfType`, frame time, or UI directly.

### Presentation Layer

Unity components translate domain state into scene behavior, animation, audio, and UI. Presentation may subscribe to domain events but does not own canonical progression state.

### Infrastructure Layer

Adapters for scene loading, file storage, input, audio playback, localization-ready text resolution, time, and platform services. Domain code depends on narrow interfaces, not concrete Unity APIs.

## Lifetime and Composition

### Bootstrap Scene

`SCN_Bootstrap` now owns the small `GameApplication` composition root. It constructs the current `GameState`, `EvidenceService`, `DeductionService`, and `MemoryService`; owns the authoritative `GameModeController`, input reader, pause handler, and `SceneLoader`; then asynchronously routes to the authored starting `LocationData`. This root is the sole `DontDestroyOnLoad` object because session state and transition ownership must survive single-scene loads. It contains no location presentation.

`GameApplication` listens only at scene-load boundaries, finds the single explicit `OpeningSceneInstaller` among the loaded scene roots, and injects application services into scene-local controllers. Arbitrary gameplay scripts do not locate or access a global singleton. Save, audio, settings, and Main Menu services remain future work and are not stubbed into this root.

### Scene-Scoped Composition

Gameplay scenes own their local interactables, navigation, cameras, ambience emitters, and presenters. A scene context receives explicit references to required application services. Scene objects must unregister listeners on teardown.

### Avoided Patterns

- service location from arbitrary scripts;
- multiple authoritative stores for the same state;
- static mutable gameplay state;
- broad event buses carrying untyped strings;
- `DontDestroyOnLoad` on content or scene presentation objects;
- hard-coded scene names, evidence IDs, or dialogue text inside behaviors.

## State Model

`GameState` is the authoritative mutable state for a playthrough. It should be serializable independently of Unity scene instances and contain versioned records for:

- playthrough metadata and current chapter/location/checkpoint;
- story flags and important choices;
- objectives/cases;
- discovered evidence and annotations;
- solved deductions;
- dialogue history and relationship values;
- credentials and terminal state;
- memory fragments and puzzle progress;
- player spawn/orientation at safe checkpoints;
- ending state;
- cumulative playtime.

Transient presentation state—open panels, hover target, current animation frame, active audio source—is not canonical save data.

### Implemented State Foundation

Schema version 1 currently persists current case, location, checkpoint, story flags, collected evidence IDs, unlocked memory IDs, and completed deduction IDs. `GameState` exposes explicit mutation/query APIs and duplicate-safe ordinal sets. `GameStateSnapshot` is a serializable DTO; snapshot collections are sorted for deterministic output, and restore validates the schema while normalizing duplicates. Save-file I/O and migration orchestration remain intentionally unimplemented.

`GameModeService` is the single state owner behind a scene-wirable `GameModeController`. It exposes the current mode and a typed previous/current change event for Gameplay, Inspect, Dialogue, Terminal, EvidenceBoard, Memory, and Paused. Consumers receive the controller/service explicitly; there is no static singleton or scene lookup.

## Stable IDs and Content Registry

- Use namespaced lowercase IDs such as `evidence.mert.photo_0417` and `flag.ch01.mert_terminal_opened`.
- IDs are authored fields, not asset names or Unity GUIDs.
- IDs must be unique within a validated content registry.
- Renaming an asset must not alter its ID.
- Once a released save can reference an ID, removal requires a migration or compatibility alias.
- Runtime lookup reports missing and duplicate IDs with actionable context.

`AuthoredContentAsset` implements the stable-ID contract for evidence, characters, dialogues, cases, locations, memories, deductions, inspections, and terminals. `ContentIdValidator` enforces nonblank lowercase dot-separated ASCII IDs and reports malformed and duplicate entries. The editor command **Null Pointer → Content → Validate Stable Content IDs** scans authored ScriptableObjects and includes both asset name and path in diagnostics. `ContentCatalog` is an explicit runtime catalog—there is no reflection or AssetDatabase scan in a player—and feature services build ordinal dictionaries from their typed catalog arrays.

Opening IDs use immutable semantic keys such as `evidence.mert.photo`, `evidence.mert.terminal_log_0251`, and `deduction.mert.postmortem_terminal`. Uppercase brief labels such as `EV_MERT_PHOTO` are asset filenames/editor handles, not runtime IDs.

## Story Conditions and Effects

Conditions and effects should be typed data, not free-form expression strings. Initial condition types may include:

- flag set/unset;
- evidence discovered/annotated;
- deduction solved;
- objective state;
- dialogue choice made;
- relationship threshold;
- chapter/location state.

Effects may set state only through domain APIs that validate transitions and emit a specific change notification. Complex sequences should be composed from small effects rather than embedded callbacks.

## System Boundaries

### Input

The generic template gameplay actions have been replaced with:

- `Gameplay`: Move, Interact, Pause;
- `UI`: Navigate, Submit, Cancel, Point, Click, Scroll;
- future context-specific actions only when Inspect, Evidence Board, or Memory requirements prove they are necessary.

Keyboard and common gamepad bindings are present for all Gameplay actions. `GameplayInputReader` resolves the centralized action names and exposes `IGameplayInputSource`, allowing movement/interaction tests to provide input without synthesizing hardware events. The existing asset does not generate a C# wrapper.

The persistent input reader mode-gates Move and Interact actions: they are enabled only in Gameplay. Pause remains enabled so the owning modal can close through the same device-agnostic input contract. `PauseInputHandler` handles only Gameplay/Paused; Inspect, Dialogue, Terminal, Evidence Board, and Memory own their cancel path.

### Interaction

`PlayerInteractionDetector` performs a bounded, reused-buffer 2D overlap query only while Gameplay mode is active. Candidates expose availability, priority, and an interaction transform through `IInteractable`. Selection orders by priority, squared distance, then a stable per-instance tie key; unavailable and duplicate targets are ignored. The active target is readable and raises a specific change event. Input dispatch is blocked in every non-Gameplay mode. Optional `IInteractionPromptSource` text feeds the reusable prompt UI. Inspect, evidence, dialogue, terminal, evidence-board, memory, and scene-transition interactables compose on this neutral contract without adding story-specific knowledge to the player controller.

### Dialogue

`DialogueData` owns authored nodes, speakers, text, next links, choices, typed conditions, and typed actions. The pure `DialogueRunner` filters choices against `GameState`, follows branches, emits typed actions, and terminates safely with a diagnostic when a next node is invalid. `DialogueController` owns Dialogue mode, UI focus/cancel, flag and evidence effects, and exposes memory/scene-transition actions through a narrow event for later composition. Dialogue history, resume/checkpoint policy, text reveal, and full graph validation remain future work.

### Evidence and Deduction

`EvidenceData` contains stable presentation/category/case/critical/icon fields while `EvidenceService` owns catalog lookup, duplicate-safe collection, queries, ordered UI projections, and a typed collection event over `GameState`. Environmental inspection and evidence collection are separate paths; evidence-bearing inspection may invoke a narrow follow-up such as a memory trigger only after first collection.

`DeductionData` declares required evidence IDs, authored result text, optional resulting evidence, and story flags. The pure `DeductionService` reports missing requirements, refuses duplicate completion, commits through `GameState`, and emits a typed completion event. The baseline evidence board selects collected evidence deterministically, attempts exact authored combinations, resets safely, and projects completed deductions from canonical state. It intentionally omits final drag/drop and red-string art.

### Memory

`MemoryData` currently defines timed presentation beats with text, overlay color, duration, and optional audio hooks. `MemoryService` records a stable unlocked ID exactly once. `MemoryController` owns Memory mode, real-time sequencing, skip/cancel, overlay/audio presentation, and return to Gameplay. The presentation boundary can later be replaced by Timeline without changing unlock state. Reconstruction rules and accessibility intensity profiles remain deliberately unimplemented.

### Terminal

`TerminalData` contains a menu title and bounded authored entries categorized as Mail, Logs, Files, Security, Search, or Archive. Entries may add one evidence ID and/or story flag through domain APIs. `TerminalController` owns Terminal mode, controller/keyboard UI focus, entry viewing, evidence integration, and safe close. It never accesses host files or executes terminal text. Credentials, search, archives, and richer session persistence remain future work.

### Audio

Use mixer groups for Master, Music, Ambience, SFX, and Voice. A music/ambience controller handles transitions; pooled one-shot playback handles frequent SFX. User volume settings map to mixer parameters and persist outside individual saves.

### Scene Loading

All production transitions use `SceneLoader`. It rejects concurrent loads, raises typed start/completion hooks for later fade presentation, asynchronously loads the scene named by validated `LocationData`, updates `GameState.LocationId`, and forwards a stable spawn-point ID to the scene installer. `SceneTransitionInteractable` may require a story flag and never couples transition logic to the player controller. Explicit error UI and a concrete fade presenter remain future work.

## Save and Settings Design

- Use a versioned plain data model; never serialize live `MonoBehaviour` or `ScriptableObject` instances.
- Store stable content IDs and primitives.
- Write to a temporary file, validate, then atomically replace the primary file where the platform permits.
- Retain one recoverable backup per slot.
- Include schema version, game version, timestamp, playtime, checkpoint label, and integrity data.
- Migrations are sequential and covered by fixtures from earlier schemas.
- Save only at declared safe boundaries; UI must not claim success until durable write completes.
- Global settings and per-playthrough state are separate files.
- Windows storage uses Unity's persistent data path through an injected storage adapter.

## UI Strategy

The opening vertical slice uses uGUI consistently for runtime HUD and modal screens. This is now the baseline for the implemented Inspect, Dialogue, Terminal, Evidence Board, Memory, interaction-prompt, and notification features; a later production UI review may refine shared styling and accessibility without moving domain logic into views.

All menus require:

- keyboard, mouse, and gamepad navigation;
- deterministic initial focus and restored focus after modals;
- safe-area and aspect-ratio behavior;
- presentation separated from domain commands;
- localization-ready layout;
- accessibility settings applied live where feasible.

## Scene Plan

| Folder | Responsibility |
| --- | --- |
| `Assets/Scenes/Bootstrap` | Application startup and persistent composition |
| `Assets/Scenes/MainMenu` | Front end and save selection |
| `Assets/Scenes/Gameplay` | Production locations and narrative sequences |
| `Assets/Scenes/Test` | Focused developer/test harness scenes |

Build Settings now start with `SCN_Bootstrap`, followed by `SCN_ErenApartment` and `SCN_MertApartment`. The template `SampleScene` remains untouched but is no longer enabled for production startup. `Assets/Scenes/Test/SCN_Test_GameplayFoundation.unity` remains an editor-only engineering harness and is intentionally excluded from Build Settings.

The explicit editor command **Null Pointer → Opening → Rebuild Authored Opening Content and Scenes** creates or updates the opening ScriptableObjects, registry, Bootstrap, both location scenes, and Build Settings. It is idempotent, preserves stable asset GUIDs, validates content IDs after generation, and refuses to discard an unsaved untitled scene during interactive use. Batch automation may replace only Unity's empty startup scene.

## Assembly Boundaries

Current boundaries:

- `NullPointer.Core` — game modes and serializable session state;
- `NullPointer.Content` — stable authored-content contracts, validator, and shared character/case/location assets;
- `NullPointer.Input` — Input System adapter and pause-mode input;
- `NullPointer.Player` — Rigidbody2D horizontal player movement;
- `NullPointer.Interaction` — interaction contracts, selection, detection, and dispatch;
- `NullPointer.Evidence`, `NullPointer.Deduction`, `NullPointer.Inspect`, `NullPointer.Dialogue`, `NullPointer.Terminal`, and `NullPointer.Memory` — feature data, domain/application logic, interactables, and feature-owned presentation;
- `NullPointer.SceneFlow` — location loading, spawn, and transition contracts;
- `NullPointer.UI` — shared prompt and evidence notification presentation;
- `NullPointer.Runtime` — Bootstrap composition and explicit scene installation;
- `NullPointer.Editor` — content validation plus idempotent engineering/production scene generation;
- `NullPointer.Tests.EditMode` — pure rules, input-asset configuration, and engineering-scene validation;
- `NullPointer.Tests.PlayMode` — movement/mode blocking and physics interaction integration.

Dependencies point toward Core, and Player/Interaction share input only through the input contract. Continue to add feature assemblies only when a compile/dependency boundary justifies them.

## Testing Strategy

### EditMode

- stable ID validation and content registry errors;
- story conditions/effects and flag transitions;
- evidence/deduction rules;
- dialogue branching;
- memory puzzle logic;
- save round trips, corruption handling, and migrations;
- ending eligibility.

### PlayMode

- player movement and input enable/disable lifecycle;
- focus selection and interaction dispatch;
- scene transition/spawn behavior;
- UI navigation and modal focus;
- checkpoint restoration;
- representative end-to-end vertical slice.

### Manual / Build QA

- Windows clean-machine launch;
- keyboard/mouse and common controller passes;
- save compatibility and forced-close recovery;
- supported resolutions/aspect ratios;
- accessibility effect limits;
- complete critical path and all endings.

Tests must assert meaningful behavior. Scene or visual tests are added only when they protect a real regression risk.

Current automated coverage includes mode transitions/events, duplicate-safe state and snapshots, input-map requirements, deterministic interaction selection, stable ID format/duplicates, evidence collection, deduction requirements/persistence, dialogue branching and invalid links, memory unlocks, authored content/production scene contracts, modal movement blocking, Inspect close flow, interaction dispatch, and Bootstrap startup. Latest verified result: 33 EditMode and 8 PlayMode tests passed.

## Validation and Observability

- Content assets implement editor validation for required references and ID format.
- A project-wide validator reports duplicate IDs, broken references, unreachable required content, and invalid condition targets.
- Runtime logs use clear categories and avoid leaking full save contents.
- Development builds may expose a read-only state inspector and story jump tools; release builds exclude them.

## Performance and Platform Targets

Specific minimum hardware will be set before content-complete QA. Until then:

- target stable 60 fps at 1920×1080 on the eventual minimum Windows specification;
- avoid per-frame allocations in recurring gameplay/UI paths;
- pool burst effects and common audio sources when profiling proves useful;
- prefer atlas-friendly sprite import settings and bounded texture sizes;
- profile representative content in player builds, not only the Editor;
- async or staged-load heavy locations while keeping save transitions safe.

## Security and Data Safety

- Treat save files as untrusted input: bound collections and strings, validate enums/IDs, and fail safely.
- Do not execute terminal text or deserialize arbitrary types.
- Do not place secrets or service credentials in the client project.
- External telemetry or crash reporting requires a separate privacy and package review.

## Current Foundation Limit

This phase establishes a playable authored opening foundation, not the complete vertical slice. SaveManager, objective/case progression, dialogue history, terminal credentials/search, memory reconstruction puzzles, final evidence-board interaction art, final localization, full accessibility options, final audiovisual assets, and production fades remain outside scope. The Bootstrap session is deliberately in-memory only.
