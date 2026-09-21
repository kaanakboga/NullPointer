# NULL POINTER: ANILAR SİLİNMEDEN ÖNCE — Game Design

## Document Purpose

This document defines the player experience, scope, gameplay pillars, and high-level system requirements for the commercial Windows release. It is the design authority for how the game should feel. Narrative canon belongs in `STORY.md`; implementation decisions belong in `TECHNICAL_DESIGN.md`; production rules belong in `RULES.md`.

## Product Summary

| Field | Definition |
| --- | --- |
| Title | **NULL POINTER: ANILAR SİLİNMEDEN ÖNCE** |
| Genre | 2D narrative detective game / psychological mystery / puzzle thriller |
| Platform | Windows |
| Engine | Unity 6.5, exact editor `6000.5.4f1`, Universal 2D / URP 2D |
| Camera | 2D side-on or staged-room investigation presentation, chosen per location |
| First-playthrough target | Approximately 4–5 hours |
| Core progression | Information, evidence, deductions, dialogue, memories, and discoveries |
| Combat | None |
| Tone | Cyber-noir, intimate, unsettling, restrained |

## Player Fantasy

The player is a methodical investigator who learns that the most unreliable crime scene is their own memory. Progress comes from observing carefully, connecting facts, testing testimony, and deciding what identity means when memory can be edited or transplanted.

## Design Pillars

### 1. Information Is Progress

Knowledge replaces conventional power growth. New evidence, verified contradictions, reconstructed memories, credentials, and story flags open paths. The game must make the player understand why an option became available.

### 2. Every Discovery Has Context

Evidence is not generic currency. Every item has a source, description, relevance, and relationship to people, locations, cases, or deductions. Important discoveries should change at least one of: the player's hypothesis, an available interaction, a dialogue branch, or the emotional meaning of an earlier scene.

### 3. Memory Is Mechanic and Theme

Memory corruption is not only a visual filter. Glitches communicate unreliable state, reconstruction puzzles ask the player to impose an order on partial truth, and later revelations reframe earlier interactions without invalidating fair clues.

### 4. Tension Without Combat

Threat is created through pacing, surveillance, incomplete information, time-sensitive presentation, social pressure, and unstable perception. Failure should usually cost information, trust, or certainty rather than health.

### 5. Player Conclusions Matter

The evidence board and deduction systems let the player express a theory. The finale's RELEASE, PURGE, and REMAIN choices are conclusions reached through play, not disconnected menu buttons.

## Core Loop

1. Enter a location with an explicit investigative question.
2. Explore, inspect, and speak to relevant characters.
3. Collect evidence and record observations automatically in the notebook.
4. Compare testimony, terminal records, physical clues, and memory fragments.
5. Form or unlock deductions by connecting supported evidence.
6. Use new knowledge to open dialogue, interrogation, terminal, or location paths.
7. Resolve the location's immediate question while introducing a deeper uncertainty.

The loop should alternate quiet observation, focused reasoning, and short high-intensity memory or interrogation sequences.

## Player Experience Principles

- Interaction targets must be readable without covering the scene in icons.
- Collected evidence must remain reviewable; the player should not need external notes.
- Required deductions must be logically supported by information already available.
- Optional observations reward attention with characterization, foreshadowing, or alternate routes.
- A blocked path must communicate what category of information is missing without giving away the answer.
- Dialogue choices should express intent and investigative strategy; misleading paraphrases are not allowed.
- Glitch effects must never compromise accessibility-critical text or controls.
- Save and checkpoint behavior must be predictable and visible.

## Gameplay Systems

### Movement and Navigation

- Keyboard and gamepad support are first-class requirements.
- Movement is responsive and deliberately paced for investigation spaces.
- The player cannot attack; action mappings and animations must reflect the non-combat design.
- Location boundaries, transitions, and interactable staging must preserve pixel clarity.

### Interaction and Inspection

- A shared interaction contract supports inspectables, pickups, characters, terminals, doors, and scene transitions.
- Focus selection must be stable and deterministic when targets overlap.
- Inspection may reveal text, close-up visuals, evidence, a story flag, or a follow-up action.
- Repeat inspections should have concise resolved-state text where appropriate.

### Evidence Database

Each evidence entry requires a stable ID and authored presentation data. Runtime state tracks discovery, review state, and any unlocked annotations. Evidence may be physical, testimonial, digital, mnemonic, or deductive.

Minimum player-facing fields:

- title and icon/thumbnail;
- concise description;
- source and time/location context;
- category;
- discovered annotations;
- related characters, locations, and case threads;
- whether the entry is new or updated.

### Evidence Board and Deduction

- The board shows relevant evidence without becoming an unrestricted physics sandbox.
- Connections are validated by authored deduction rules.
- Incorrect hypotheses receive useful feedback and do not permanently consume evidence.
- Required deductions may unlock flags; optional deductions add context, dialogue leverage, or ending nuance.
- Solved deductions remain legible as a record of the player's reasoning.

### Dialogue and Interrogation

- Dialogue is data-authored and supports conditions, choices, effects, localization-ready text, and interruption/resume rules.
- Conditional branches can read evidence, deductions, relationship state, chapter progress, and story flags.
- Interrogation is a focused variant: the player selects a claim or evidence item to challenge a statement.
- Contradictions must be grounded in collected information.
- Important choices and irreversible exits require clear presentation.

### Terminal and Database

- Terminals provide diegetic access to files, messages, personnel records, logs, and restricted systems.
- Credentials and discovered identifiers can unlock content.
- Search and navigation stay intentionally scoped; content is authored rather than simulated without bounds.
- Terminal discoveries feed the same evidence and story-state systems as world interactions.

### Memory Glitches and Reconstruction

- Glitches use controlled visual/audio distortion to mark corruption, intrusion, or unstable recall.
- Reconstruction puzzles arrange fragments, identify inconsistencies, restore sequence, or isolate a planted element.
- The puzzle state is saveable and supports reset/retry.
- Critical narrative content cannot be lost permanently through puzzle failure.
- Effects offer intensity controls and avoid sustained flashing.

### Implemented Chapter 1 Vertical Slice

The production flow now boots through a functional Main Menu and starts at 03:17 in Eren's apartment. New Game establishes the opening checkpoint and objectives; Continue is available only for a valid local save. The player reads the Sector 7 dispatch, may inspect Eren's medication and cut photograph, checks Eren's local call history, and travels to Mert's apartment through the authored transition.

Mert's apartment now contains a deliberately mixed investigation set: atmospheric coffee and medical equipment, lore-bearing notes, and evidence-bearing photograph, death-time record, damaged implant, door latch, terminal records, memory-deletion time, and mismatched call histories. The photograph triggers the fragmented laboratory memory. Terminal information is divided among Logs, Files, Mail, and Security, with the last-contact record withheld until the first timeline contradiction is established.

The Chapter 1 deduction chain moves from postmortem terminal access, to an inconsistent apparent-suicide timeline, to the conclusion that the locked-room appearance cannot be accepted at face value. It ends by revealing Mert's attempted call to Eren, the absence of the corresponding record on Eren's device, and `NLP-0417` as the next lead without naming Project Lazarus or the responsible party.

Objectives, safe checkpoints, local JSON save/continue, the pause menu, the investigation journal, the case timeline, settings, and the Chapter 1 completion boundary are functional. Placeholder geometry and restrained uGUI styling remain temporary. The evidence board still uses button selection rather than final drag/string presentation, the memory remains presentation-only rather than a reconstruction puzzle, and the interrogation layer is production-ready but represented by a controlled claim/evidence fixture because Chapter 1 has no appropriate living suspect scene.

### Notebook / Investigation Journal

The notebook consolidates objectives, case summaries, evidence, deductions, people, locations, and recent discoveries. It is a usability layer over canonical state, not a second state store.

### Save, Checkpoint, and Settings

- Manual save availability is defined per sequence; autosaves occur at safe state boundaries.
- Checkpoints protect the player before irreversible dialogue, puzzle, or ending decisions.
- Save files include versioning, validation, atomic replacement, and a recoverable backup.
- Settings include master/music/ambience/SFX/voice volume, display mode, resolution, VSync/frame limit where appropriate, language-ready text settings, input rebinding, text speed, screen shake, glitch intensity, flashing reduction, and subtitle presentation.

## Structure and Pacing

| Section | Target first-play time | Primary experience |
| --- | ---: | --- |
| Prologue — **03:17** | 15–25 min | Inciting scene, baseline controls, first anomaly |
| Chapter 1 — **The Dead Man** | 45–55 min | Mert Ersoy case, core investigation loop |
| Chapter 2 — **People Who Remember You** | 50–60 min | Testimony conflicts, photograph, identity pressure |
| Chapter 3 — **Lazarus** | 50–60 min | Restricted records, memory technology, conspiracy scope |
| Chapter 4 — **Who Are You?** | 50–60 min | Reconstruction truth, Eren's identity collapse |
| Chapter 5 — **Null Pointer** | 40–55 min | NLP-0, final synthesis, ending choice |

These are production targets, not permission to pad scenes. Optional content may extend playtime but the critical path must remain coherent.

## Progression Model

Progress is represented by explicit, inspectable state:

- chapter and location availability;
- current and completed objectives;
- evidence discovery and annotations;
- solved deductions;
- dialogue node history and important choices;
- character relationship/trust variables where authored;
- credentials and terminal access;
- memory fragments and reconstruction state;
- one-way story milestones;
- ending eligibility and final decision.

No progression-critical fact should exist only in scene object activation state.

## Endings

The finale supports three deliberate decisions:

- **RELEASE** — publish the evidence.
- **PURGE** — destroy the memory infrastructure.
- **REMAIN** — keep the system intact and remain inside it.

All three choices must be understandable from accumulated evidence. Supporting variations may reflect optional discoveries or relationships, but the advertised ending paths cannot depend on opaque hidden scores.

## Accessibility and Usability Baseline

- Full keyboard/mouse and common gamepad navigation for gameplay and UI.
- Rebindable gameplay controls with conflict feedback.
- Subtitles for all voiced or semantically relevant speech.
- Adjustable text speed and an instant-complete option.
- Avoid color-only evidence and puzzle communication.
- Adjustable glitch intensity, screen shake, and flashing.
- Legible UI scaling at 1080p and common Windows aspect ratios.
- Pause during ordinary single-player gameplay and safe handling where pausing is narratively restricted.
- Confirmation for destructive save operations and irreversible ending choices.

## Scope Guardrails

- No traditional combat, weapon, health, loot, or character-level system.
- No procedural narrative required for release.
- No open-world structure; locations are authored and chapter-gated.
- No online or multiplayer dependency.
- No package is added until a proven requirement survives a build-versus-buy review.
- Major systems are built through a representative vertical slice before bulk story production.

## Commercial Quality Bar

Release readiness requires a complete critical path, all three endings, no progression blockers, stable save migration, consistent input navigation, acceptable performance on the agreed minimum PC, content-complete Turkish text with localization-ready storage, licensed source assets, tested Windows builds, and documented known issues.
