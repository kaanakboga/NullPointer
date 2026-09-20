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

Unity does not permit batch mode to open a project already owned by another editor. The verifier warns when `Temp/UnityLockfile` exists and then lets Unity fail safely rather than terminating the active editor. The 2026-09-20 verification run therefore used an isolated copy of the identical source/configuration set while the workspace editor remained open: compile passed, EditMode passed 10/10, and PlayMode passed 2/2.

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

A minimal Bootstrap scene will create the application composition root, initialize long-lived services, validate startup, and route to Main Menu or a requested test scene. It must contain only objects that genuinely require application lifetime.

Likely long-lived responsibilities:

- `GameState` owner / current session;
- `SaveManager` and storage adapter;
- `AudioManager` or audio service root;
- `SceneLoader` with transition control;
- settings service.

`GameManager` is not mandatory. If introduced, it coordinates lifecycle only and must not absorb system-specific logic.

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

### Interaction

`PlayerInteractionDetector` performs a bounded, reused-buffer 2D overlap query only while Gameplay mode is active. Candidates expose availability, priority, and an interaction transform through `IInteractable`. Selection orders by priority, squared distance, then a stable per-instance tie key; unavailable and duplicate targets are ignored. The active target is readable and raises a specific change event. Input dispatch is blocked in every non-Gameplay mode. Evidence, dialogue, terminal, door, inspect, and memory implementations remain deliberately absent.

### Dialogue

The dialogue runner reads authored graphs or node data, evaluates conditions through the domain layer, applies validated effects, and emits presentation events. Text, speaker identity, portraits, choices, and timing data remain authored. Save/checkpoint rules define whether a conversation resumes at a node or restarts at a safe boundary.

### Evidence and Deduction

Evidence discovery writes to `GameState`; UI views read projections of that state. Deduction definitions declare required evidence/annotations and resulting effects. The board manipulates a working hypothesis; only a validated solve commits progression.

### Memory

Memory definitions describe fragment content, ordering/relationship rules, presentation profile, completion effects, and restart behavior. Puzzle logic is testable separately from glitch rendering. Accessibility settings scale effects without changing the logical puzzle.

### Terminal

Terminal content is authored and queryable through a scoped virtual database. Authentication and discovered queries are state-backed. The terminal UI never reads real files or executes arbitrary commands.

### Audio

Use mixer groups for Master, Music, Ambience, SFX, and Voice. A music/ambience controller handles transitions; pooled one-shot playback handles frequent SFX. User volume settings map to mixer parameters and persist outside individual saves.

### Scene Loading

All production transitions go through one loader abstraction that can show progress, fade safely, choose spawn points, and avoid double-load requests. Scene references should be validated assets or centralized keys rather than scattered strings.

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

Use one UI technology consistently per feature; do not mix uGUI and UI Toolkit inside a single screen without a concrete need. A later UI spike will choose the production standard based on pixel-art rendering, gamepad navigation, text styling, and authoring workflow.

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

The template `SampleScene` remains untouched and is still the only Build Settings scene. `Assets/Scenes/Test/SCN_Test_GameplayFoundation.unity` is an editor-only engineering harness and is intentionally excluded from release Build Settings. It contains a Rigidbody2D player, floor collision, visible interaction probe, current-mode display, and keyboard/gamepad instructions.

## Assembly Boundaries

Current boundaries:

- `NullPointer.Core` — game modes and serializable session state;
- `NullPointer.Input` — Input System adapter and pause-mode input;
- `NullPointer.Player` — Rigidbody2D horizontal player movement;
- `NullPointer.Interaction` — interaction contracts, selection, detection, and dispatch;
- `NullPointer.Editor` — engineering scene generation;
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

Current automated coverage includes mode transitions/events, duplicate-safe state and snapshot round trips, required input actions/UI preservation, deterministic interaction selection, engineering-scene composition, Rigidbody2D movement blocking, and interaction dispatch blocking. Latest verified result: 10 EditMode and 2 PlayMode tests passed.

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

The persistent Bootstrap composition root is not implemented yet. The engineering scene wires one explicit `GameModeController` and one input reader for validation; production scene lifetime and startup routing remain NP-CORE-008.
