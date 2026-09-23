# NULL POINTER: ANILAR SİLİNMEDEN ÖNCE — Art Bible

## Visual Thesis

The world is a readable neo-noir pixel-art city where memory technology feels clinical, intimate, and invasive. Darkness should shape composition rather than hide information. Glitches are meaningful signs of corruption, not constant decoration.

## Art Pillars

### Controlled Darkness

Large dark masses, deliberate pools of light, and clear silhouettes create noir tension. Interactable information remains readable through value, framing, motion, or restrained highlight—not by flattening every scene into equal brightness.

### Dirty Technology

Mnemosyne Systems combines precise medical interfaces with worn public infrastructure. Screens, cables, scanners, and memory equipment should feel used and physically embedded rather than holographic by default.

### Human Detail Over Spectacle

Faces, posture, personal objects, photographs, and room history carry narrative weight. Environments should imply lives and institutional pressure without excessive prop noise.

### Corruption Has Grammar

Memory distortion uses a limited vocabulary: displaced pixel blocks, channel separation, frame discontinuity, data omission, temporal echo, and controlled palette intrusion. Each effect maps to a narrative condition and has an accessibility-reduced variant.

## Phase 5A Authority

The conventions below are locked production defaults as of Phase 5A. A later asset may vary only when its manifest row records the exception and visual QA approves it. Procedural preview graphics demonstrate the system; they are not final raster art.

## Format and Pixel Discipline

- World reference canvas: **480×270**, scaling cleanly to 1920×1080 at 4× and 2560×1440 only through the approved pixel-camera strategy.
- UI reference canvas: **1920×1080** with `CanvasScaler` match at `0.5`; validate 2560×1440 and representative 1920×1200 (16:10).
- Compose essential gameplay for 16:9. At 16:10, extend/crop authored edge composition rather than stretching world art.
- World art uses **16 pixels per unit** and a **16×16 source-pixel construction grid**. Do not mix arbitrary world sprite scales.
- Eren's production gameplay frame is **32×64 source pixels**. Other adult exploration characters target **48–64 source pixels tall** and approximately **24–40 pixels wide**, documented against the same grid.
- Dialogue portraits target **512×512**; evidence thumbnails target **384×256**; full environment layers target **480×270** (or **960×270** only for authored horizontal parallax).
- World pixel art under `Assets/Art` imports with Point filtering, mipmaps disabled, NPOT scaling disabled, and uncompressed texture data. UI sprites use 100 PPU with the same filtering/compression discipline.
- Avoid subpixel shimmer. Camera motion and sprite placement must be tested with the chosen pixel-perfect strategy.
- Rotation and non-integer scaling of pixel sprites are exceptional effects, not routine layout tools.
- UI text is optimized for legibility and may use higher-resolution raster/vector rendering rather than pretending to share the world sprite grid.

The 480×270 reference canvas, 16 PPU world scale, and 16-pixel grid are the production baseline. The Phase 5 art spike validates camera/pixel-perfect behavior and style execution against this locked technical baseline; it does not reopen arbitrary scale selection.

### Sorting and Normal Maps

World renderers use this stable back-to-front sorting-layer order: `Default`, `Background`, `Environment`, `PropsBack`, `Characters`, `PropsFront`, `Effects`, `Foreground`, `WorldUI`. `Default` is a migration/structural fallback, not a destination for new final art. Conceptual `BackgroundFar`, `EnvironmentBack`, `Gameplay`, `EnvironmentFront`, and `WorldFX` depth is expressed by these existing names plus the order bands below so current scenes are not arbitrarily broken.

| Layer | Orders | Purpose |
| --- | ---: | --- |
| Background | -3000…-2001 | Far city, sky, deepest parallax |
| Environment | -2000…-1001 | Architecture, floor/walls, fixed windows |
| PropsBack | -1000…-101 | Furniture, rear props, rear cables |
| Characters | -100…99 | Player/cast; Eren defaults to 0–19 |
| PropsFront | 100…999 | Handled evidence/props and near room detail |
| Effects | 1000…1999 | Screen glows, rain, particles, world distortion |
| Foreground | 2000…2999 | Noir framing silhouettes; never critical occlusion |
| WorldUI | 3000…3999 | Rare diegetic world labels/prompts only |

Normal maps are opt-in and use the `_Normal` filename suffix so Unity imports them as normal maps; they must support a deliberate URP 2D lighting decision rather than decorate every sprite.

### Import, Pivot, Atlas, and Alpha Rules

- `Characters`, `Environments`, and world `Effects`: Sprite, 16 PPU, Point, uncompressed, mipmaps off, NPOT scaling off.
- `UI` and `Icons`: Sprite, 100 PPU, Point, uncompressed, mipmaps off, NPOT scaling off.
- Multiple-sprite import is allowed only in an explicit `SpriteSheets` or `Atlases` folder. Other production textures import as Single.
- `Source` and `Reference` folders are excluded from automatic production-sprite rules.
- Grounded actors/props use bottom-center pivots; wall/overlay layers use center; tile pieces use the documented corner/edge pivot required by their kit. A pivot exception belongs in the manifest.
- Character sheets keep one frame size, baseline, and pivot across every animation. Empty padding is transparent and consistent.
- Atlases group assets with shared material, scale, and lifetime. Do not atlas full-screen backgrounds with frequently changed UI/icons.
- Use straight alpha. Remove matte fringe and hidden colored pixels at transparent edges. Do not pre-bake bloom or soft antialiasing into pixel sprites.
- Sprite names follow `<Prefix>_<Subject>_<Action>_<Variant>`; sheets and atlases append `_Sheet` or `_Atlas`; normal maps append `_Normal`.

### Production Folder Standard

```text
Assets/Art/
  Characters/<Character>/{Portraits,SpriteSheets}
  Environments/<Location>/Props
  UI/{Atlases,Dialogue,Inspect,MainMenu,Themes,Transitions}
  Effects/{Profiles,Screen,World}
  Icons/Evidence
  Reference/
  Source/
```

## Core Palette

This is the exact working semantic palette. Location ramps may interpolate darker/lighter values, but new accents require an art-bible revision. The authored `VT_CyberNoir` theme carries matching UI values.

| Role | Color | Usage |
| --- | --- | --- |
| Near black | `#070A0F` | Deep background, letterbox, silhouette separation |
| Deep navy | `#0C1523` | Primary shadow plane, UI field |
| Charcoal | `#161A20` | Physical structure, disabled surfaces, neutral metal |
| Desaturated blue | `#25364A` | Architecture midtone, rain depth, inactive technical surfaces |
| Restrained violet | `#5D4D82` | Memory-adjacent shadows and corruption support only |
| Dirty cyan | `#4DA3A6` | Technology, investigation guidance, cool practicals |
| Pale cyan | `#9AC8C5` | High-value cool highlights and readable details |
| Warm amber | `#D28A3D` | Human warmth, caution, old practical lights |
| Pale warning red | `#B84A56` | Danger, invasive memory state, irreversible emphasis |
| Off-white text | `#E0E4E5` | Primary text and rare brightest UI value |
| Muted secondary text | `#8796A0` | Metadata, history, inactive secondary copy |
| Bone paper | `#D8D0C0` | Physical documents and photographs, not general UI text |

Rules:

- Red and amber are accents; sustained large fields reduce their narrative power.
- Dirty cyan usually signals systems or investigative affordance, not universal interactivity.
- Never encode evidence categories or success/failure by color alone.
- Reserve pure white and fully saturated primaries for exceptional moments.

## Typography System

No font is downloaded in Phase 5A. `LegacyRuntime.ttf` remains a legal local fallback in the procedural preview and current generated screens; it is not the approved final brand face. Final fonts must have recorded licensing, Turkish glyph coverage (`ÇĞİÖŞÜçğıöşü`), clear numerals, distinguishable `I/İ/1/l`, and readable small sizes.

| Role | Current fallback | Desired final characteristics | Case / tracking / size guidance |
| --- | --- | --- | --- |
| Logo/title | LegacyRuntime Bold | Custom angular grotesk or authored wordmark; distinctive `NULL POINTER` rhythm | Uppercase, generous tracking; 64–96 px at 1080p |
| Chapter title | LegacyRuntime Bold | Condensed cinematic sans, excellent numerals/timecodes | Uppercase, 44–72 px |
| Major heading | LegacyRuntime Bold | Same family as chapter or compatible condensed weight | Uppercase, 34–48 px |
| Panel heading | LegacyRuntime Bold | Compact technical sans | Uppercase, 24–34 px |
| Body text | LegacyRuntime Regular | Humanist/neutral sans with Turkish clarity | Sentence case, 22–30 px, 1.3–1.45 line height |
| Dialogue | LegacyRuntime Regular | Warm readable sans, not monospaced | Sentence case, 26–32 px |
| Terminal | LegacyRuntime Regular | Licensed mono with clear punctuation/zero | Mixed case, 20–27 px |
| Metadata | LegacyRuntime Regular | Compact mono/technical sans | Uppercase where short, 14–20 px, wider tracking |
| Evidence label | LegacyRuntime Bold | Compact sans, strong at card scale | Uppercase or title case, 18–24 px |
| Warning | LegacyRuntime Bold | Same UI family; weight/shape, not red alone | Uppercase, 18–26 px |
| Interaction prompt | LegacyRuntime Bold | Compact UI sans with glyph-aligned metrics | Short sentence/title case, 18–24 px |

## Motion Language

Motion uses fixed authored categories, unscaled time for pause/modal presentation, and interrupt-safe transitions. It never owns `GameMode` or gameplay commands.

| Category | Duration | Use |
| --- | ---: | --- |
| Micro interaction | 120–220 ms; default 160 ms | Hover, focus, press, edge sweep, tiny text offset |
| Panel/modal | 180–320 ms; default 240 ms | Fade/slide/scale entrance and exit |
| Important reveal | 300–650 ms; default 460 ms | Objective/evidence/chapter emphasis |
| Focus pulse | 1200 ms period | Low-amplitude breathing cue; stops immediately on lost focus |
| Memory corruption | Irregular authored beats | Deterministic authored timing; never random duration |

Scale response is restrained (normally 0.985–1.015). Slides are normally 16–36 reference pixels. Rapid reversal resumes from the current visual state; it does not queue stale transitions.

## Lighting Specification

- Normal ambient field: near-black/deep navy (`#070A0F` → `#0C1523`), never pure black across the entire playable plane.
- Practical lights motivate every visible color: cyan from screens/scanners; amber from old domestic fixtures; pale red only from fault/emergency/irreversible states.
- Eren Apartment balances a cold window/screen plane with one small amber human practical. Mert Apartment is colder, more clinical, and may use one controlled red fault accent.
- Characters require a readable silhouette/value separation at the authored movement plane. Add a local rim or background value adjustment before adding universal outlines.
- URP 2D shadows are reserved for architecture/large props that improve composition. Do not enable shadow casters on every small object.
- Bloom is restrained to practical sources and evaluated at integer scale; it must not inflate text or merge pixel clusters.
- Memory override may shift violet/red, vignette, and screen-space distortion temporarily. On completion/skip/disable/scene change, all overrides return to zero.

## VFX Usage Budget

| Effect | Normal use | Prohibited use |
| --- | --- | --- |
| Rain | Window/exterior depth and sparse foreground streaks | Uniform full-screen high-frequency layer over readable UI |
| Dust | Rare amber practical beam or abandoned interior | Constant particle field in every room |
| Haze/fog | Broad depth separation at low opacity | Soft blur that destroys pixel clusters |
| Scanlines | Diegetic terminal/memory boundary, subtle menu texture | Permanent heavy overlay over dialogue/settings |
| Film grain/noise | 0–8% normal presentation, higher only in authored memory | Hiding weak art or making text shimmer |
| Chromatic split | Memory intrusion only | Normal gameplay decoration |
| Glitch/tearing | Specific corruption beat, outside protected text zones | Random continuous glitch or progression cue visible for one frame |
| Particles | Motivated rain, dust, electrical fleck | Simultaneous rain+dust+sparks+fog without composition need |
| Bloom | Screen/practical highlight, low threshold/strength | UI text bloom or evidence pickup glow |
| Vignette | Subtle composition or authored memory shift | Crushing corners until movement/clues disappear |

## Value and Lighting

- Build scenes in three primary value groups: shadow mass, readable midground, focal light.
- Character silhouettes must separate from the immediate background in dialogue and exploration positions.
- Use URP 2D lights deliberately; avoid many overlapping lights with no compositional role.
- Practical sources—monitors, signage, desk lamps, emergency fixtures—motivate color.
- Memory sequences may violate continuity, but their interactive targets remain readable.
- Bloom, chromatic aberration, and vignette must be subtle and evaluated at native integer scale.

## Character Art

- Silhouette, posture, and one or two identifying shapes take priority over micro-detail.
- Idle animation is restrained; avoid constant exaggerated motion in serious scenes.
- Facing and eye-line must support interrogation staging.
- Portraits, if used, preserve the sprite design's proportions and lighting logic.
- Eren's visual presentation may acquire controlled discontinuities as identity destabilizes, but must not telegraph the complete reveal early.
- Mnemosyne personnel share institutional motifs without becoming uniform caricatures.

## Environment Art

Every production location needs:

- a narrative purpose and dominant visual question;
- foreground/midground/background separation;
- a controlled interaction readability pass;
- evidence placement that makes physical sense;
- lighting sources and ambient motion plan;
- a clean collision/navigation overlay;
- an accessibility check with reduced post-processing.

Environmental storytelling should be specific: altered labels, missing personal traces, physical backups, repair marks, and institutional cleanup are stronger than generic cyberpunk clutter.

## UI and Iconography

- UI is minimalist, calm, and more stable than the world. Instability is reserved for authored memory events.
- Primary text uses near-white or pale cyan on deep navy, meeting practical contrast needs.
- Red communicates destructive/irreversible or critical corruption states; amber communicates caution/pending review.
- Evidence, notebook, dialogue, terminal, and settings screens share spacing, focus, modal, and typography rules.
- Selected/focused controls use shape, border, movement, or icon changes in addition to color.
- Icons use a consistent grid, stroke weight, and filled/outline convention.
- Evidence thumbnails prioritize recognition at small size; decorative noise is removed.
- Long Turkish strings and localization expansion must be tested early.

## Glitch Language

| Effect | Narrative use | Accessibility-reduced version |
| --- | --- | --- |
| Pixel block displacement | Missing or overwritten spatial information | Fewer, slower blocks outside text |
| RGB/channel separation | Unstable recall or external interference | Low-offset luminance echo |
| Dropped frames / pose echo | Temporal discontinuity | Brief desaturation cue |
| Scanline tear | System-mediated memory access | Static boundary line |
| Palette intrusion in red | Invasive/manual alteration | Restrained border pulse plus audio cue |
| Data omission / black bar | Deliberately removed content | Stable masked region with label/icon |

Constraints:

- No essential clue is readable only during a short glitch frame.
- Avoid high-frequency full-screen flashing.
- Effects never distort subtitles, pause/settings navigation, or safety prompts beyond legibility.
- The player's glitch intensity setting scales presentation, not narrative timing or puzzle logic.

## Animation

- Favor strong key poses and economical loops.
- Interaction anticipation and completion should be readable without slowing routine play.
- UI transitions are short and consistent.
- Memory reconstruction may use stepped or discontinuous motion; ordinary scenes should not.
- Any camera shake has intensity controls and a zero setting.
- Animation clips use `<Prefix>_<Subject>_<Action>_<DirectionOrVariant>` where useful, for example `CHR_Eren_Walk_Right`; sprite source frames append a zero-padded frame number.

## Asset Naming and Delivery

Art assets follow `RULES.md`. Source files belong beside or in a clearly paired source subfolder only when Unity import supports the workflow. Exported assets must document scale, pivot, palette/location, and animation slicing expectations.

Suggested prefixes:

- `CHR_` character sprite/animation assets;
- `ENV_` environment assets;
- `PROP_` props;
- `UI_` interface graphics;
- `ICO_` icons;
- `FX_` effects;
- `PAL_` palettes.

Before final import, verify license/provenance, color mode, transparent edges, filtering, compression, pixels per unit, pivot, atlas compatibility, and naming.

AI-assisted visual output is staged outside final production folders until a human reviews anatomy/perspective, palette, pixel cleanup, tiling/seams, alpha edges, consistency, and rights/provenance. Record the tool/model, date, prompt/reference ownership, license/usage terms, and human edits. Generated images are references or raw material, never automatically approved final assets.

### Builder Ownership and Final-Art Slots

- Scene builders own structural GameObjects, colliders, installers, controllers, layout anchors, and placeholder renderers.
- Human-approved raster references on builder-owned objects live only in a `FinalArtSlot` with a unique stable slot ID and matching manifest asset ID.
- Builders may regenerate structure and placeholders, but must capture and restore the slot's `_finalArt` reference by stable slot ID. They must never infer final ownership from hierarchy names or direct renderer sprites.
- Artists may edit/import final source assets and assign `FinalArtSlot.FinalArt`; they do not hand-edit generated gameplay components or colliders.
- Missing optional art keeps the structural placeholder and must fail gracefully. No builder may hide a required interaction solely because final art is absent.
- Manually composed final-art hierarchies that cannot fit a slot require a separately owned prefab/anchor contract before integration; placing them under disposable generated roots is prohibited.

## Audio-Visual Coordination

Glitches, memory locks, evidence acquisition, and deduction completion each require a paired motion/sound language. Do not use the same sting for discovery, danger, and confirmation. Silence is a compositional tool and should precede major identity revelations where appropriate.

## Art Review Checklist

- Does the focal point read at gameplay zoom and 1080p?
- Is the interactable readable without an icon carpet?
- Are silhouettes clear in grayscale?
- Are red and amber still restrained?
- Does the asset align to the chosen pixel grid and scale?
- Does animation preserve pixel stability?
- Does reduced-glitch/reduced-shake mode remain coherent?
- Are Turkish UI strings legible without clipping?
- Is source ownership and license recorded?
