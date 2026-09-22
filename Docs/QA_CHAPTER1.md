# Chapter 1 Production QA Checklist

Use this checklist against the exact Windows Development artifact produced by `Tools/Build-Windows.ps1`. Record date, commit/worktree state, Unity version, artifact path/hash, Windows version, display, keyboard layout, and controller model. Do not count automated coverage as a manual player result.

## Automated Gate — Completed in Repository Verification

- Exact Unity 6000.5.4f1 compile succeeds.
- 74 EditMode and 11 PlayMode tests pass, including production-scene idle Y stability and horizontal movement in both apartments.
- Production content validation passes for Bootstrap, Main Menu, Eren Apartment, and Mert Apartment.
- Build Settings contain those four scenes in that order and no engineering/test scene.
- Schema-1 migration, payload integrity, corrupt/missing primary recovery, backup preservation, settings/save separation, and fresh-manager disk relaunch are covered.
- Production scenes structurally contain the expected EventSystem, AudioListener, and 1920×1080 CanvasScaler configuration.

## Clean Launch and Exit

- [ ] Start with no Null Pointer save or settings files in a dedicated clean Windows user profile.
- [ ] Launch `NullPointer.exe`; Main Menu appears with no error dialog or exception in the player log.
- [ ] Continue is disabled when no valid save exists.
- [ ] Quit from Main Menu closes the process cleanly.
- [ ] Relaunch twice and confirm no first-run-only failure.

## Keyboard and Mouse Critical Path

- [ ] New Game starts Eren Apartment at the authored spawn.
- [ ] A/D, arrows, and the configured horizontal bindings move; no jump/combat action exists.
- [ ] Interact discovers and invokes the deterministic nearby target.
- [ ] Escape opens Pause and immediately stops horizontal movement.
- [ ] Journal and Settings close back to Pause; a single Escape never leaks through and unpauses twice.
- [ ] Inspect, Dialogue, Terminal, Evidence Board, Memory, and Interrogation block movement and close safely.
- [ ] Complete the 03:17 dispatch, enter Mert Apartment, collect mandatory evidence, complete the deduction chain, and reach the Chapter 1 ending hook without developer shortcuts.
- [ ] Optional interactions and damaged recording can be revisited without duplicate progression.

## Gamepad Critical Path

- [ ] Left stick moves; South/Submit interacts and confirms; Start opens Pause; East/Cancel closes the active modal.
- [ ] D-pad/left stick reaches every visible menu control with an obvious non-color-only focus state.
- [ ] Initial focus is valid on Main Menu, Pause, Settings, Dialogue choices, Journal, Terminal, Evidence Board, and Interrogation.
- [ ] Switching between mouse/keyboard and gamepad does not double-submit or lose focus.
- [ ] Disconnect/reconnect during Gameplay and Pause does not strand input.
- [ ] Complete the Chapter 1 critical path with the controller only.

## Save, Backup, and Relaunch

- [ ] Quit after each declared checkpoint: Eren start, dispatch complete, Mert entrance, critical-evidence milestone, first deduction, and Chapter 1 completion.
- [ ] Relaunch and Continue restores the correct location/spawn/objectives/evidence/deductions and always resumes in Gameplay mode.
- [ ] Force-close only after a confirmed checkpoint write; the previous valid save still loads.
- [ ] With a copied corrupt primary and valid `.bak`, Continue reports recovery and restores the primary without destroying the backup.
- [ ] New Game/delete affects story state only; all settings remain unchanged.

## Settings and Display

- [ ] Master, Music, SFX, fullscreen/windowed, resolution, and text speed apply and persist across process restart.
- [ ] Verify windowed/fullscreen transitions and minimize/restore at 1920×1080.
- [ ] Verify UI/focus/readability at 2560×1440.
- [ ] Verify UI/focus/readability at a supported 16:10 mode (for example 1920×1200).
- [ ] Long Turkish text remains readable with no clipped buttons, dialogue, journal, terminal, or evidence text.
- [ ] Movement/interactions remain aligned with camera/colliders after every display change.

## Failure and Softlock Sweep

- [ ] Repeatedly open/close each modal and transition between scenes; movement and interaction always return exactly once.
- [ ] Cancel during typewriter reveal reveals before advancing and never skips two nodes.
- [ ] Incorrect evidence/deduction/interrogation choices give feedback without consuming evidence or blocking retry.
- [ ] Required terminal entries and the Eren→Mert transition cannot be bypassed before their gates and cannot become unavailable afterward.
- [ ] No scene contains duplicate audio, input, EventSystem, or application ownership at runtime.
- [ ] Player log contains no unhandled exception, missing-script error, failed scene load, or save write failure.

## Result Record

| Field | Value |
| --- | --- |
| Date / tester | |
| Git status / revision | |
| Artifact path / SHA-256 | |
| Windows / GPU / display | |
| Keyboard / controller | |
| Player log | |
| Result | NOT RUN |
| Blocking defects | |
