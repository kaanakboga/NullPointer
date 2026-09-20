# NULL POINTER: ANILAR SİLİNMEDEN ÖNCE — Production Backlog

## How to Use This Backlog

Tasks are ordered by phase and then by row. A later task may be prepared early, but work should not bypass its listed prerequisites. Each task is intended to be small enough to implement, verify, and hand off independently. Split a task further if investigation reveals materially separate risks.

Status values: `DONE`, `NEXT`, `BLOCKED`, `IN PROGRESS`, `NOT STARTED`.

The backlog covers the complete game target. It is not a promise that every implementation detail is known before vertical-slice learning. Re-estimate and refine without silently changing the product pillars or story canon.

## Phase 0 — Foundation and Project Health

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-CORE-001 | DONE | Audit the initial repository; create the durable folder structure and populated design, story, technical, art, rules, backlog, and changelog documents. |
| NP-CORE-002 | DONE | Align the URP manifest request with Unity 6000.5.4f1's supported built-in resolution (`17.5.0`) through a reviewed Package Manager change, confirm a clean import/compile, and make no unrelated upgrades. |
| NP-CORE-003 | DONE | Add minimal runtime, EditMode test, and PlayMode test assembly definitions; verify references compile and an intentional smoke test runs in each test mode. |
| NP-CORE-004 | NOT STARTED | Implement and test the stable content-ID format/value rules, including empty, malformed, and duplicate diagnostics. |
| NP-CORE-005 | NOT STARTED | Implement a content registry that indexes authored assets by stable ID and reports missing/duplicate entries with asset context. |
| NP-CORE-006 | NOT STARTED | Implement typed story flags plus pure condition/effect evaluation with EditMode tests; no UI or scene dependency. |
| NP-CORE-007 | IN PROGRESS | Define the versioned `GameState`/session model for chapter, checkpoint, evidence, deduction, dialogue, terminal, memory, and ending state; test invariant-preserving transitions. Schema 1 foundations for case/location/checkpoint, flags, evidence, memories, and deductions are complete; feature-specific state remains. |
| NP-CORE-008 | NOT STARTED | Create `SCN_Bootstrap` and a small composition root that initializes only required application-lifetime services and routes deterministically to a test destination. |
| NP-CORE-009 | NOT STARTED | Implement the scene-loading contract with guarded single-load behavior, transition hooks, failure reporting, and a test adapter. |
| NP-CORE-010 | NOT STARTED | Add project-wide editor validation entry points for stable IDs and required content references; validator output must identify the asset and repair action. |
| NP-CORE-011 | NOT STARTED | Add development-only state inspection and safe story-jump hooks that cannot enter release builds. |
| NP-CORE-013 | DONE | Implement the authoritative, testable GameMode service/controller and typed transitions for Gameplay, Inspect, Dialogue, Terminal, EvidenceBoard, Memory, and Paused. |
| NP-CORE-014 | DONE | Add reusable exact-version Unity batch compile/EditMode/PlayMode verification with ignored logs, NUnit result validation, and non-zero failure behavior. |

## Phase 1 — Controllable Investigation Space

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-PLAYER-001 | DONE | Replace template combat-oriented input actions with reviewed Gameplay and UI maps for keyboard/mouse and gamepad; preserve generated-asset workflow consistently. |
| NP-PLAYER-002 | DONE | Implement testable movement intent, speed configuration, and movement locking rules without animation or interaction coupling. |
| NP-PLAYER-003 | DONE | Implement the 2D player controller with collision-safe movement and predictable enable/disable lifecycle. |
| NP-PLAYER-004 | NOT STARTED | Add player facing and locomotion animation presentation using placeholder art without changing movement authority. |
| NP-PLAYER-005 | NOT STARTED | Implement stable spawn-point selection and restoration by validated spawn ID. |
| NP-PLAYER-006 | NOT STARTED | Configure a pixel-stable follow camera prototype and verify movement at target integer scales and common aspect ratios. |
| NP-PLAYER-007 | IN PROGRESS | Add PlayMode coverage for movement, input-map switching, collision, pause lock, and spawn behavior. Movement and immediate GameMode blocking are covered; input-map, collision-edge, and spawn coverage remain. |
| NP-INT-001 | IN PROGRESS | Define focused interaction contracts and result types for inspectable, pickup, character, terminal, door, and transition composition. The neutral `IInteractable` availability/priority/transform/invoke contract is complete; result and specialized composition contracts remain. |
| NP-INT-002 | DONE | Implement deterministic nearby-target discovery and focus scoring with EditMode tests for tie and invalid-target cases. |
| NP-INT-003 | DONE | Implement the player interactor lifecycle, ensuring disabled/blocking UI states cannot dispatch interactions. |
| NP-INT-004 | NOT STARTED | Build a reusable interaction prompt presenter with keyboard/gamepad glyph fallback and non-color focus cues. |
| NP-INT-005 | NOT STARTED | Implement a data-authored inspect interaction with first/repeat responses and optional validated effects. |
| NP-INT-006 | NOT STARTED | Implement door and scene-transition interactables through the shared loader and spawn contracts. |
| NP-INT-007 | IN PROGRESS | Create `SCN_Test_Interaction` with placeholder player, collision, inspectable, door, and overlapping focus targets; add a representative PlayMode flow test. `SCN_Test_GameplayFoundation` covers player, collision, one interaction probe, and mode blocking; specialized/overlap flow remains. |

## Phase 2 — Evidence, Cases, and First Vertical Slice

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-EVIDENCE-001 | NOT STARTED | Define `EvidenceData`, evidence categories, relations, and editor validation; keep mutable discovery data out of assets. |
| NP-EVIDENCE-002 | NOT STARTED | Implement evidence discovery/update state with idempotent behavior, new/read markers, annotations, and typed notifications. |
| NP-EVIDENCE-003 | NOT STARTED | Implement evidence pickup/inspection integration and verify that repeat interaction cannot duplicate state or effects. |
| NP-EVIDENCE-004 | NOT STARTED | Define `CaseData` and objective data with explicit prerequisite and completion rules. |
| NP-EVIDENCE-005 | NOT STARTED | Implement case/objective progression and journal projections with tests for legal and illegal transitions. |
| NP-EVIDENCE-006 | NOT STARTED | Build the evidence database list/detail UI with filters, relations, unread state, and full controller navigation. |
| NP-EVIDENCE-007 | NOT STARTED | Define `DeductionData` with evidence/annotation requirements, optional alternatives, and validated completion effects. |
| NP-EVIDENCE-008 | NOT STARTED | Implement pure deduction validation, solve state, and useful incorrect/incomplete feedback without consuming evidence. |
| NP-EVIDENCE-009 | NOT STARTED | Build an evidence-board working-hypothesis UI with deterministic selection/connection behavior and reset. |
| NP-EVIDENCE-010 | NOT STARTED | Integrate solved deductions with flags, objectives, dialogue conditions, and permanent notebook records. |
| NP-UI-001 | NOT STARTED | Establish the production UI technology and shared screen/focus/navigation conventions through a documented spike. |
| NP-UI-002 | NOT STARTED | Build the notebook shell with Objectives, Evidence, Deductions, People, and Locations tabs backed by projections of canonical state. |
| NP-UI-003 | NOT STARTED | Implement toast/notification queuing for evidence, objective, and deduction updates without obscuring critical text. |
| NP-CORE-012 | NOT STARTED | Assemble a greybox vertical slice: enter one room, inspect clues, collect evidence, solve one deduction, update an objective, and exit through a gated transition. |
| NP-QA-001 | NOT STARTED | Add an automated vertical-slice happy-path test plus a manual keyboard/gamepad checklist; close all progression blockers before expanding scope. |

## Phase 3 — Save, Checkpoints, Menus, and Settings

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-SAVE-001 | NOT STARTED | Define the versioned save DTO, slot metadata, and mapping to/from `GameState`; exclude live Unity object references. |
| NP-SAVE-002 | NOT STARTED | Implement an injected Windows file-storage adapter with bounded paths and test in-memory storage. |
| NP-SAVE-003 | NOT STARTED | Implement serialize/validate/deserialize round trips with malformed and unknown-content-ID handling. |
| NP-SAVE-004 | NOT STARTED | Implement atomic slot writes and one backup; simulate interrupted/corrupt writes and verify last valid data survives. |
| NP-SAVE-005 | NOT STARTED | Implement save schema migration pipeline and fixtures beginning with schema version 1. |
| NP-SAVE-006 | NOT STARTED | Implement checkpoint definitions and safe-boundary capture/restore, including location spawn and objective state. |
| NP-SAVE-007 | NOT STARTED | Add autosave feedback, write serialization/queuing, and input-safe error handling. |
| NP-SAVE-008 | NOT STARTED | Implement manual save/load/delete slot commands with sequence restrictions and destructive-action confirmation. |
| NP-SAVE-009 | NOT STARTED | Add EditMode and PlayMode coverage for round trip, backup recovery, migration, checkpoint restore, and repeated rapid save requests. |
| NP-UI-004 | NOT STARTED | Build Main Menu with New Game, Continue, Load, Settings, Credits, and Quit state handling. |
| NP-UI-005 | NOT STARTED | Build Pause Menu with Resume, Notebook, Save/Load availability, Settings, Main Menu, and Quit confirmation. |
| NP-UI-006 | NOT STARTED | Implement the save-slot UI with timestamp, playtime, location/chapter label, version compatibility, empty/corrupt state, and controller navigation. |
| NP-UI-007 | NOT STARTED | Implement global settings data/storage for audio, display, controls, text, and accessibility separately from save slots. |
| NP-UI-008 | NOT STARTED | Build settings screens with apply/revert semantics for display changes and live preview for safe presentation/audio settings. |
| NP-UI-009 | NOT STARTED | Implement input rebinding UI with conflict detection, reset defaults, cancellation, and persistence for keyboard/gamepad. |

## Phase 4 — Dialogue and Interrogation

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-DIALOGUE-001 | NOT STARTED | Define `CharacterData` and speaker presentation fields with stable IDs and required-reference validation. |
| NP-DIALOGUE-002 | NOT STARTED | Define dialogue conversation/node/line/choice data with conditions, effects, next links, and editor validation for broken/unreachable required nodes. |
| NP-DIALOGUE-003 | NOT STARTED | Implement the pure dialogue runner for lines, branches, choices, conditions, effects, and completion; cover branch behavior in EditMode tests. |
| NP-DIALOGUE-004 | NOT STARTED | Implement dialogue history and important-choice state for conditional follow-ups and save/load. |
| NP-DIALOGUE-005 | NOT STARTED | Build the dialogue presentation screen with speaker, text reveal, instant-complete, continue, choices, and controller focus. |
| NP-DIALOGUE-006 | NOT STARTED | Add conversation entry/exit integration that owns input maps, player movement lock, camera staging hooks, and safe interruption cleanup. |
| NP-DIALOGUE-007 | NOT STARTED | Add evidence/topic selection within authored conversation nodes without exposing unavailable evidence. |
| NP-DIALOGUE-008 | NOT STARTED | Define interrogation statements, contradiction prompts, accepted evidence, failure feedback, and completion effects. |
| NP-DIALOGUE-009 | NOT STARTED | Implement interrogation logic and presentation with retry-safe state and fair contradiction feedback. |
| NP-DIALOGUE-010 | NOT STARTED | Implement relationship/trust variables only for authored uses, with bounded transitions and visible narrative consequences. |
| NP-DIALOGUE-011 | NOT STARTED | Create a test conversation containing conditions, a persistent choice, evidence challenge, and interruption/checkpoint boundary; add end-to-end coverage. |

## Phase 5 — Terminal and Database Investigation

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-TERMINAL-001 | NOT STARTED | Define authored terminal, account/credential, directory, message, file, record, and query data with stable IDs. |
| NP-TERMINAL-002 | NOT STARTED | Implement virtual database navigation/search and access checks as pure bounded logic; never access or execute host files. |
| NP-TERMINAL-003 | NOT STARTED | Implement terminal session state for discovered files, queries, credentials, and story effects with save support. |
| NP-TERMINAL-004 | NOT STARTED | Build diegetic terminal UI for login, navigation, search, file reading, locked results, history, and exit with keyboard/gamepad support. |
| NP-TERMINAL-005 | NOT STARTED | Integrate terminal discoveries with evidence, objectives, flags, and notifications through domain APIs. |
| NP-TERMINAL-006 | NOT STARTED | Add a restricted Mnemosyne database test dataset with one credential gate, one searchable identifier, and one evidence-producing record. |
| NP-TERMINAL-007 | NOT STARTED | Test access control, bounded search, state persistence, duplicate discovery, UI focus restoration, and interaction exit. |

## Phase 6 — Memory Corruption and Reconstruction

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-MEMORY-001 | NOT STARTED | Define `MemoryData`, fragment data, corruption cues, puzzle rule data, completion effects, and validation. |
| NP-MEMORY-002 | NOT STARTED | Implement memory-fragment discovery and reconstruction progress in canonical state with save/load support. |
| NP-MEMORY-003 | NOT STARTED | Implement the first pure reconstruction rule type (ordered sequence) with reset, partial feedback, and tests. |
| NP-MEMORY-004 | NOT STARTED | Add a second authored rule type (relationship/inconsistency identification) only after the first puzzle proves the common contract. |
| NP-MEMORY-005 | NOT STARTED | Build reconstruction UI for fragment inspect/select/place/confirm/reset and full controller navigation. |
| NP-MEMORY-006 | NOT STARTED | Implement a glitch presentation controller with intensity profiles separated from puzzle logic and subtitle/UI protection. |
| NP-MEMORY-007 | NOT STARTED | Implement reduced-glitch, reduced-flashing, and zero-shake behavior and verify semantic cues remain understandable. |
| NP-MEMORY-008 | NOT STARTED | Integrate memory entry/exit, checkpoints, audio snapshots, input maps, and scene/camera presentation. |
| NP-MEMORY-009 | NOT STARTED | Create a representative corrupted-memory sequence that unlocks evidence and changes a later dialogue branch. |
| NP-MEMORY-010 | NOT STARTED | Test correct/incorrect/reset flows, checkpoint restoration, all accessibility profiles, and repeated completion idempotence. |

## Phase 7 — Audio and Presentation Infrastructure

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-AUDIO-001 | NOT STARTED | Create the audio mixer hierarchy for Master, Music, Ambience, SFX, and Voice with stable exposed parameter names. |
| NP-AUDIO-002 | NOT STARTED | Implement settings-to-mixer volume mapping with mute-safe decibel conversion and persistence tests. |
| NP-AUDIO-003 | NOT STARTED | Implement music state transitions and crossfades without overlapping ownership across scenes. |
| NP-AUDIO-004 | NOT STARTED | Implement location ambience layers and snapshot transitions for dialogue, terminal, memory, and pause contexts. |
| NP-AUDIO-005 | NOT STARTED | Implement bounded pooled one-shot SFX playback with priority/duplicate controls where profiling warrants it. |
| NP-AUDIO-006 | NOT STARTED | Implement voice/subtitle timing hooks that support both voiced and unvoiced lines without making voice mandatory. |
| NP-AUDIO-007 | NOT STARTED | Build and verify a representative audio pass for the vertical slice, including focus, evidence, deduction, UI, room tone, and memory cues. |
| NP-UI-010 | NOT STARTED | Implement shared transition/fade/loading presentation with double-submit protection and accessibility-safe timing. |
| NP-UI-011 | NOT STARTED | Implement credits presentation and data source for team, licenses, middleware, and third-party assets. |
| NP-UI-012 | NOT STARTED | Complete UI scaling, aspect-ratio, long Turkish string, keyboard-only, mouse-only, and controller-only passes. |

## Phase 8 — Art Direction Validation and Production Pipeline

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-ART-001 | NOT STARTED | Produce an art spike comparing candidate PPU/character scale and approve the final reference canvas, grid, camera, and import rules. |
| NP-ART-002 | NOT STARTED | Create Unity import presets for environment, character, UI, and effect sprites; verify no filtering/compression/pivot regressions. |
| NP-ART-003 | NOT STARTED | Create approved core palette assets, lighting examples, and grayscale/readability reference. |
| NP-ART-004 | NOT STARTED | Produce final Eren exploration sprite set and restrained locomotion/interaction animations. |
| NP-ART-005 | NOT STARTED | Define and produce the supporting-character sprite/portrait pipeline, then complete cast assets as the narrative roster locks. |
| NP-ART-006 | NOT STARTED | Create the reusable urban/interior environment kit and prop language for investigation spaces. |
| NP-ART-007 | NOT STARTED | Produce final UI frame, controls, focus states, evidence cards, notebook, terminal, memory, and icon families. |
| NP-ART-008 | NOT STARTED | Produce glitch effect assets/shaders matching the defined grammar and all accessibility-reduced variants. |
| NP-ART-009 | NOT STARTED | Complete Prologue and Chapter 1 environment/lighting art and conduct interaction-readability review. |
| NP-ART-010 | NOT STARTED | Complete Chapter 2 and Chapter 3 environment/lighting art and conduct interaction-readability review. |
| NP-ART-011 | NOT STARTED | Complete Chapter 4 and Chapter 5 environment/lighting art and conduct interaction-readability review. |
| NP-ART-012 | NOT STARTED | Run global atlas, texture memory, overdraw, pixel stability, palette consistency, and asset provenance passes. |

## Phase 9 — Narrative Preproduction and Content Production

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-STORY-001 | NOT STARTED | Lock the complete cast, the four-person relationship, host-body identity, Mert death truth, Mnemosyne antagonistic structure, and technology constraints in `STORY.md`. |
| NP-STORY-002 | NOT STARTED | Create the master clue/reveal dependency map proving each mandatory deduction and reveal has a fair acquisition route. |
| NP-STORY-003 | NOT STARTED | Create the full chapter/scene/beat outline with location, objective, entry state, exit state, estimated time, and required assets. |
| NP-STORY-004 | NOT STARTED | Define the complete stable-ID catalog for chapters, locations, characters, cases, evidence, deductions, memories, and ending variables. |
| NP-STORY-005 | NOT STARTED | Author and implement the Prologue critical path, optional observations, checkpoint, and chapter handoff. |
| NP-STORY-006 | NOT STARTED | Author and implement Chapter 1 exploration/evidence/objectives. |
| NP-STORY-007 | NOT STARTED | Author and implement Chapter 1 dialogue/interrogation/terminal content and chapter-completion deduction. |
| NP-STORY-008 | NOT STARTED | Author and implement Chapter 2 exploration, photograph discovery, “Dördümüzden biri hatırlamalı.” clue chain, and identity case thread. |
| NP-STORY-009 | NOT STARTED | Author and implement Chapter 2 conversations/interrogations, optional character content, and chapter transition. |
| NP-STORY-010 | NOT STARTED | Author and implement Chapter 3 Project Lazarus terminal/evidence path and restricted-access progression. |
| NP-STORY-011 | NOT STARTED | Author and implement Chapter 3 memory reconstruction, dialogue consequences, and `NLP-0` objective reveal. |
| NP-STORY-012 | NOT STARTED | Author and implement Chapter 4 evidence and memories proving Eren's researcher identity and death three years earlier. |
| NP-STORY-013 | NOT STARTED | Author and implement Chapter 4 host-body revelation, character responses, agency-restoring objective, and chapter transition. |
| NP-STORY-014 | NOT STARTED | Author and implement Chapter 5 route to `NLP-0`, final reconstruction, proof synthesis, and confrontation. |
| NP-STORY-015 | NOT STARTED | Implement RELEASE, PURGE, and REMAIN decision presentation, checkpoints, consequences, and base epilogues. |
| NP-STORY-016 | NOT STARTED | Add authored epilogue variations for approved optional evidence/relationship outcomes without hidden ending eligibility. |
| NP-STORY-017 | NOT STARTED | Complete Turkish copy edit, continuity check, clue fairness review, content warnings, credits text, and localization handoff tables. |
| NP-STORY-018 | NOT STARTED | Lock critical-path timing to approximately 4–5 first-play hours through observed playtests; cut repetition before adding filler. |

## Phase 10 — Full-Game UI, Accessibility, and Polish

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-UI-013 | NOT STARTED | Complete People and Locations notebook content with discovered/unknown states and spoiler-safe updates. |
| NP-UI-014 | NOT STARTED | Implement chapter/title cards, objective presentation, ending choice UI, epilogue flow, and post-ending return behavior. |
| NP-UI-015 | NOT STARTED | Implement subtitle background/size/speaker options and verify every semantic voice cue has text support. |
| NP-UI-016 | NOT STARTED | Implement text speed, instant reveal, glitch intensity, flashing reduction, screen shake, and any approved UI scale options across all systems. |
| NP-UI-017 | NOT STARTED | Perform global focus-order, focus-restore, mouse/controller switching, blocked-input, modal stacking, and cancel-path audit. |
| NP-PLAYER-008 | NOT STARTED | Complete movement/collision/camera feel polish across every production location without scene-specific controller hacks. |
| NP-INT-008 | NOT STARTED | Complete all production interactable states, prompts, repeat text, and resolved-state feedback; remove debug-only guidance. |
| NP-AUDIO-008 | NOT STARTED | Produce/integrate final music, ambience, SFX, and voice assets with loudness consistency, looping, memory profile, and license records. |

## Phase 11 — QA, Optimization, and Release

| ID | Status | Task / acceptance outcome |
| --- | --- | --- |
| NP-QA-002 | NOT STARTED | Build a content validator covering duplicate/missing IDs, broken references, invalid conditions/effects, unreachable mandatory dialogue, and invalid build-scene keys. |
| NP-QA-003 | NOT STARTED | Create automated critical-state fixtures for every chapter start, major reveal, and ending decision checkpoint. |
| NP-QA-004 | NOT STARTED | Run complete critical-path keyboard/mouse playthroughs from New Game to each ending with no developer shortcuts. |
| NP-QA-005 | NOT STARTED | Run complete critical-path gamepad playthroughs and document supported controller behavior/disconnect recovery. |
| NP-QA-006 | NOT STARTED | Verify save/load at every declared save point and checkpoint, including quit/relaunch and backup recovery. |
| NP-QA-007 | NOT STARTED | Verify all dialogue branches, interrogation outcomes, deductions, terminal gates, memories, and optional-content re-entry states. |
| NP-QA-008 | NOT STARTED | Run accessibility test matrix with reduced glitch/flashing/shake, subtitle variants, text speeds, and no color-only interpretation. |
| NP-QA-009 | NOT STARTED | Test supported Windows resolutions, window modes, DPI/UI scaling, 16:9, wider aspect ratios, minimize/restore, and display apply/revert. |
| NP-QA-010 | NOT STARTED | Profile representative low/high-content scenes in Development players; fix measured CPU, GPU, allocation, load-time, and memory regressions. |
| NP-QA-011 | NOT STARTED | Run soak tests through repeated scene, dialogue, terminal, memory, save/load, and menu transitions to find leaked objects/listeners. |
| NP-QA-012 | NOT STARTED | Conduct first-time-player clue comprehension, pacing, and ending-choice usability tests; triage findings by severity and design intent. |
| NP-QA-013 | NOT STARTED | Complete regression pass after content lock and maintain a release-blocker list with owner and reproducible steps. |
| NP-QA-014 | NOT STARTED | Verify all third-party asset licenses/provenance, privacy requirements, content warnings, and credits entries. |
| NP-QA-015 | NOT STARTED | Achieve zero known progression blockers, save-loss defects, release-build exceptions, and unhandled missing-content errors. |
| NP-BUILD-001 | NOT STARTED | Define supported Windows versions, minimum/recommended hardware, architecture, graphics API policy, and target performance with evidence. |
| NP-BUILD-002 | NOT STARTED | Configure product/company metadata, application identifier, versioning, icons, cursor, splash behavior, and Windows player settings. |
| NP-BUILD-003 | NOT STARTED | Replace template Build Settings with Bootstrap/Main Menu/production scene flow; exclude all test scenes and validate scene references. |
| NP-BUILD-004 | NOT STARTED | Create a reproducible local/CI Windows build entry point that fails on compiler errors, failing validators, or failing required tests. |
| NP-BUILD-005 | NOT STARTED | Produce and smoke-test Development builds on a clean user profile with no pre-existing save/settings data. |
| NP-BUILD-006 | NOT STARTED | Produce release-candidate builds and verify install/portable layout, first launch, save location, permissions, and uninstall/update expectations. |
| NP-BUILD-007 | NOT STARTED | Verify release logging/crash behavior contains actionable diagnostics without development tools, secrets, or private save content. |
| NP-BUILD-008 | NOT STARTED | Prepare store/package assets, release notes, support/known-issues material, and final credits/licenses bundle for the chosen distributor. |
| NP-BUILD-009 | NOT STARTED | Run final signed-off three-ending release checklist on the exact candidate artifact and record artifact hash/version. |
| NP-BUILD-010 | NOT STARTED | Archive the reproducible release inputs, symbol/debug artifacts as appropriate, schema fixtures, and post-release hotfix procedure. |

## First Recommended Next Task

**NP-CORE-004 — Implement stable content IDs.** Add the validated ID value/format rules and duplicate diagnostics before more authored or persistent state begins depending on raw strings. Keep it pure and cover valid, malformed, empty, and duplicate cases in EditMode.
