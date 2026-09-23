# Null Pointer — Phase 5A Visual Audit

## Scope and Method

This audit records the production-facing state inspected at the start of Phase 5A. It distinguishes functional presentation from final visual quality. No gameplay judgment is implied by a `REBUILD` or `REPLACE_WITH_ART` classification, and structural objects required by interaction, collision, focus, saves, or progression must remain until their presentation is safely replaced.

Classification meanings:

- `KEEP`: production-suitable implementation or invisible structure.
- `RESTYLE`: retain behavior/layout ownership and replace visual treatment.
- `REBUILD`: presentation structure needs a focused later pass; domain behavior remains.
- `REPLACE_WITH_ART`: an external raster deliverable from `ART_ASSET_MANIFEST.md` is required.
- `REMOVE_FROM_PRODUCTION`: developer/greybox presentation must not ship, but its underlying gameplay object may remain.

## Production-Facing Audit

| Area | Current implementation | Placeholder evidence | Final target | Unity/procedural work | External raster work | Class | Priority | Dependencies |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `SCN_MainMenu` | Screen-space uGUI, solid near-black field, one cyan horizon strip, legacy runtime font, four color-tinted buttons, Settings modal | Flat rectangles, no layered city, rain, haze, motif, or authored title treatment | Multi-plane rain-soaked city/window composition with quiet parallax, strong title lockup, and restrained system noise | Theme binding, cyber-noir controls, parallax hooks, screen overlay, focus motion | `NP-MENU-*`, title-support art, reusable UI family | `REPLACE_WITH_ART` + `RESTYLE` | P0 | Approved menu layers, final title typography, reduced-FX policy |
| `SCN_ErenApartment` | Orthographic greybox room, flat backdrop/floor, solid-color interactables, world-space debug-like location heading | No architecture, window, furnishings, personal history, rain, practical lights, or silhouette depth | 03:17 lived-in lonely apartment that establishes Eren through objects and motivated cyan/amber light | Pixel camera validation, layer composition, 2D lights, rain/haze, art-slot wiring | `NP-ENV-EREN-*`, `NP-PROP-EREN-*` | `REPLACE_WITH_ART` | P0 | Eren reference, approved room perspective, camera/pixel spike |
| `SCN_MertApartment` | Orthographic greybox with colored blocks for rain window, workstation, room mass, evidence, props, and evidence board | Evidence reads as colored pickups; no clinical disturbance, workstation detail, or forensic composition | Cold technical crime scene, controlled red accent, disturbed but authored clutter, evidence readable through framing | Layer composition, light rig, subtle screen animation/rain, interaction readability pass | `NP-ENV-MERT-*`, `NP-PROP-MERT-*`, evidence thumbnails | `REPLACE_WITH_ART` | P0 | Approved evidence silhouettes, Mert environment sheet, camera/pixel spike |
| Pause Menu | Functional centered uGUI panel and buttons; deterministic focus | Solid frame, default rectangular control family, no hierarchy details | Quiet high-contrast interruption layer that preserves scene context | Theme, modal transition, shared button visuals, focus restore animation | UI frame/control atlas | `RESTYLE` | P1 | Phase 5B shared UI rollout |
| Settings | Functional sliders, toggle, dropdown, Apply/Back in Main and Pause contexts | Procedural bars/handles, generic dropdown/template, legacy font | Legible technical settings panel with clear state and controller focus | Theme binding, styled slider/toggle/dropdown, motion, long-string testing | Slider, scrollbar, toggle, tab, focus assets | `REBUILD` | P1 | Shared control prefabs, accessibility settings |
| Inspect | Functional title, description, evidence state, Collect/Close | Large flat modal, no inspected-object image/frame language | Forensic close-up with object art, metadata, calm readable copy | Theme, modal entrance, optional image slot, focus motion | Inspect frame and per-object close-up when authored | `RESTYLE` | P1 | Inspect close-up art contract |
| Dialogue | Functional speaker/body/history/typewriter/choices | No portraits, speaker staging, or final dialogue frame | Stable cinematic lower-third with speaker identity and restrained choices | Theme, reveal timing, portrait slots, choice-button style | Dialogue frame, portraits | `RESTYLE` | P1 | Portrait pipeline, camera staging later |
| Terminal | Functional categorized entries/body/status/close | Flat green-blue rectangles; no diegetic structure or iconography | Mnemosyne workstation language: clinical, bounded, physical, readable | Theme, categorized navigation hierarchy, scanline/noise restraint | Terminal decorative graphics, icons, frame atlas | `REBUILD` | P1 | Terminal feature completion and UI atlas |
| Evidence Board | Functional deterministic selection, attempt/reset, solved text | Button list rather than visual evidence cards/connections; colored block in world | Investigative working surface with evidence cards and authored connection language, not free physics | Theme, card prefab, connection renderer, focus/navigation | Evidence card family, thumbnails, separators | `REBUILD` | P1 | Evidence relation model and final UI assets |
| Journal | Functional Cases/People/Evidence/Questions/Timeline projection | Flat full-screen frame and button column | Dense but calm case notebook with tabs, metadata, stable hierarchy | Theme, tabs, scrolling, selection/focus, layout at 16:10 | Journal tabs, panel/separator/icon atlas | `REBUILD` | P1 | Remaining journal content fields |
| Objective UI | Functional upper HUD text | Generic flat rectangle, developer-like `AMAÇ //` treatment | Brief unobtrusive case-task reveal that recedes after update | Theme, authored reveal/hold/fade timing | Optional objective marker/frame from UI atlas | `RESTYLE` | P1 | Notification queue |
| Evidence notification | Functional single message controller | Solid panel; no queue or final acquired-state language | Restrained evidence registration toast with icon and metadata | Theme, reveal/slide, queue later | Evidence icon/card accent | `RESTYLE` | P1 | Notification queue and icon atlas |
| Memory presentation | Timed full-screen color overlay, text beats, audio hook, skip | Empty black/color field; no authored distortion textures or accessibility profile | Signature authored distortion grammar with protected readable text and default-off normal state | Phase 5A distortion controller/profile; later URP/shader refinement and accessibility profiles | `NP-FX-MEM-*`, memory fragment art | `REBUILD` | P0 | Reduced-glitch settings, final texture tests |
| Interaction prompt | Functional text fallback from active target | Plain solid strip, no device glyph or visual hierarchy | Small stable world/HUD prompt with glyph fallback and non-color focus cue | Theme, focus/reveal motion, device-change hook | Controller glyph placeholders, prompt accent | `RESTYLE` | P1 | Device glyph switching |
| Player visual | Built-in UI sprite scaled into a cyan rectangle; collider and movement are correct | No human silhouette, facing, idle, walk, or reaction art | Recognizable non-military investigator silhouette with restrained locomotion | Animator/facing/pixel camera after approved sprite set | `NP-CHR-EREN-*` | `REMOVE_FROM_PRODUCTION` + `REPLACE_WITH_ART` | P0 | Eren master reference and animation sheets |
| Camera | Orthographic size 4.8, static framing, no Pixel Perfect Camera | Not yet validated against 480×270 reference or integer movement | Pixel-stable side-view composition with safe 16:9/16:10 framing and authored staging hooks | Pixel Perfect Camera spike, crop/letterbox policy, integer-scale tests | None | `REBUILD` | P0 | Representative Eren/environment sprites |
| Lighting | Camera solid color; no authored scene lights in production apartments | Colored sprite rectangles fake illumination | Motivated URP 2D ambient/practical/screen light with controlled shadow groups | Global/point/freeform 2D light rigs and blend-style budget | Emissive/glow companion sprites only where specified | `REBUILD` | P0 | Final environment layers and performance pass |
| Post-processing | No production camera volume stack | Memory uses uGUI color only | Normal play remains clean; restrained bloom/vignette/grain; corruption temporarily overrides through authored profiles | URP Volume profile hooks, accessibility intensity controls | Noise/mask textures | `REBUILD` | P1 | Memory FX validation and settings |

## Placeholder Inventory and Replacement Contract

`May hide now` is deliberately conservative. `No` means the placeholder still communicates gameplay or anchors collision/focus and remains until the replacement is validated.

| Scene | GameObject / UI object | Current role | Manifest replacement ID | May hide now | Must remain until art exists |
| --- | --- | --- | --- | --- | --- |
| Main Menu | `Noir Backdrop` | Full-screen structural background and controller owner | `NP-MENU-BG-FAR-001` | No | Yes |
| Main Menu | `Cyan Horizon` | Minimal horizon/depth cue | `NP-MENU-BG-MID-001` | Yes | No |
| Main Menu | `Near Architecture Art` | Explicit manual art slot | `NP-MENU-BG-NEAR-001` | Already transparent | No |
| Main Menu | `Fog Haze Art` | Explicit manual art slot | `NP-MENU-FX-FOG-001` | Already transparent | No |
| Main Menu | `Rain Foreground Art` | Explicit manual art slot | `NP-MENU-FX-RAIN-001` | Already transparent | No |
| Main Menu | `Main Menu` buttons | Menu commands/focus/navigation | `NP-UI-BUTTON-PARTS-001`, `NP-UI-FOCUS-001` | No | Yes |
| Eren Apartment | `Backdrop` | Room color field | `NP-ENV-EREN-ARCH-001` | No | Yes |
| Eren Apartment | `Floor` | Visual floor plus collider | `NP-ENV-EREN-ROOM-001` | No | Yes |
| Eren Apartment | `Player` | Movement/collision/interactor plus rectangle renderer | `NP-CHR-EREN-IDLE-001` | No | Yes |
| Eren Apartment | `Neurological Medication` | Inspect target | `NP-PROP-EREN-MEDICATION-001` | No | Yes |
| Eren Apartment | `Cut Photograph` | Inspect/foreshadow target | `NP-PROP-EREN-CUTPHOTO-001` | No | Yes |
| Eren Apartment | `Dispatch Terminal` | Terminal target | `NP-PROP-EREN-WORKSTATION-001` | No | Yes |
| Eren Apartment | `Exit to Mert Apartment` | Scene transition/collider | `NP-PROP-EREN-DOOR-001` | No | Yes |
| Mert Apartment | `Backdrop` | Room color field | `NP-ENV-MERT-ARCH-001` | No | Yes |
| Mert Apartment | `Floor` | Visual floor plus collider | `NP-ENV-MERT-ROOM-001` | No | Yes |
| Mert Apartment | `Player` | Movement/collision/interactor plus rectangle renderer | `NP-CHR-EREN-IDLE-001` | No | Yes |
| Mert Apartment | `Rain Window` | Window/rain block | `NP-ENV-MERT-WINDOW-001` | No | Yes |
| Mert Apartment | `Workstation Pool` | Workstation mass | `NP-PROP-MERT-WORKSTATION-001` | No | Yes |
| Mert Apartment | `Locked Interior` | Interior architecture mass | `NP-ENV-MERT-ARCH-001` | No | Yes |
| Mert Apartment | `Mert Photograph` | Evidence/memory target | `NP-PROP-MERT-PHOTO-001` | No | Yes |
| Mert Apartment | `Mert Terminal` | Terminal target | `NP-PROP-MERT-TERMINAL-001` | No | Yes |
| Mert Apartment | `Biometric Death Time` | Evidence target | `NP-PROP-MERT-DEATH-TIME-001` | No | Yes |
| Mert Apartment | `Damaged Memory Implant` | Evidence target | `NP-PROP-MERT-IMPLANT-001` | No | Yes |
| Mert Apartment | `Internal Door Latch` | Evidence target | `NP-PROP-MERT-DOOR-001` | No | Yes |
| Mert Apartment | `Unfinished Coffee` | Atmospheric inspect target | `NP-PROP-MERT-COFFEE-001` | No | Yes |
| Mert Apartment | `Personal Notes` | Lore inspect target | `NP-PROP-MERT-NOTES-001` | No | Yes |
| Mert Apartment | `Medical Calibrator` | Lore inspect target | `NP-PROP-MERT-MEDICAL-001` | No | Yes |
| Mert Apartment | `Eren Device History` | Evidence target | `NP-PROP-MERT-EREN-DEVICE-001` | No | Yes |
| Mert Apartment | `Evidence Board` | Deduction entry target | `NP-PROP-MERT-EVIDENCE-BOARD-001` | No | Yes |
| Mert Apartment | `Damaged Recorder` | Dialogue target | `NP-PROP-MERT-RECORDER-001` | No | Yes |
| Both apartments | `Location Title` | Developer-like world scene label | `NP-CHAPTER-0317-001` only where narratively appropriate | Yes after replacement | No |
| Both apartments | All modal frames/buttons/sliders | Functional uGUI presentation | `NP-UI-*` family | No | Yes |

## Phase 5A Decision

Gameplay/domain presentation remains untouched. The safe next propagation boundary is Phase 5B: validate the pixel camera with the first approved Eren sheet, then replace only the Main Menu and Eren Apartment visual slots while rolling the shared theme/button/motion components into one production screen at a time.
