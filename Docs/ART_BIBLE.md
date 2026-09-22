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

## Format and Pixel Discipline

- Production reference canvas: **480×270**, scaling cleanly to 1920×1080 at 4×.
- Compose essential gameplay for 16:9 while testing wider Windows aspect ratios.
- World art uses **16 pixels per unit** and a **16×16 source-pixel construction grid**. Do not mix arbitrary world sprite scales.
- Adult exploration characters target **48–64 source pixels tall** and approximately **24–40 pixels wide**, with exceptions documented against the same grid.
- World pixel art under `Assets/Art` imports with Point filtering, mipmaps disabled, NPOT scaling disabled, and uncompressed texture data. UI sprites use 100 PPU with the same filtering/compression discipline.
- Avoid subpixel shimmer. Camera motion and sprite placement must be tested with the chosen pixel-perfect strategy.
- Rotation and non-integer scaling of pixel sprites are exceptional effects, not routine layout tools.
- UI text is optimized for legibility and may use higher-resolution raster/vector rendering rather than pretending to share the world sprite grid.

The 480×270 reference canvas, 16 PPU world scale, and 16-pixel grid are the production baseline. The Phase 5 art spike validates camera/pixel-perfect behavior and style execution against this locked technical baseline; it does not reopen arbitrary scale selection.

### Sorting and Normal Maps

World renderers use this back-to-front sorting-layer order: `Default`, `Background`, `Environment`, `PropsBack`, `Characters`, `PropsFront`, `Effects`, `Foreground`, `WorldUI`. New layers require a documented rendering need. Normal maps are opt-in and use the `_Normal` filename suffix so Unity imports them as normal maps; they must support a deliberate URP 2D lighting decision rather than decorate every sprite.

## Core Palette

The palette is directional, not a hard global index. Individual locations may shift hue while preserving hierarchy.

| Role | Suggested color | Usage |
| --- | --- | --- |
| Near black | `#080A12` | Deep background, letterbox, silhouette separation |
| Ink blue | `#101729` | Primary shadow plane |
| Bruised navy | `#18243D` | Architecture and secondary shadow |
| Night purple | `#30264F` | Memory-adjacent shadows, fabric, reflected light |
| Dirty cyan | `#4F9A9C` | Technology, investigation guidance, cool practicals |
| Pale cyan | `#9AC8C5` | High-value cool highlights and readable details |
| Ash | `#A7A3A8` | Neutral text/metal/paper where appropriate |
| Amber | `#D18A3D` | Human warmth, caution, old practical lights |
| Restrained red | `#B83A4B` | Danger, invasive memory state, irreversible emphasis |
| Bone | `#DDD6C8` | Rare brightest highlights and physical documents |

Rules:

- Red and amber are accents; sustained large fields reduce their narrative power.
- Dirty cyan usually signals systems or investigative affordance, not universal interactivity.
- Never encode evidence categories or success/failure by color alone.
- Reserve pure white and fully saturated primaries for exceptional moments.

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
