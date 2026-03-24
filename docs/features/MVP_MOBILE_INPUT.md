# 📱 Feature Spec: Mobile Touch Input
### Phase 1 — MVP Polish | Priority: 🔴 Critical
### Estimated Time: 60–90 minutes | Target MVP Gain: +5%

---

## PROBLEM

Whiskeyville currently relies on:
- `KeyCode` hotkeys (1–6, Q/W/E, F1–F10, ESC)
- Mouse click for building placement and harvest
- Mouse scroll for camera zoom

**None of this works on a touchscreen device.** Given the game is a casual town-builder,
mobile is likely the primary target platform. This is the last major blocking gap before
the MVP can be distributed to real players.

---

## SOLUTION

Replace all direct `Input.GetKey` / `Input.GetMouseButton` calls with Unity's **Input System
package** (new input system), which handles both mouse/keyboard and touch natively through
a single API surface.

```
  DESKTOP                          MOBILE
  ─────────────────────────────    ─────────────────────────────
  Mouse click on tile          ←→  Tap on tile
  Scroll wheel to zoom         ←→  Pinch to zoom
  Middle mouse to pan          ←→  Two-finger drag to pan
  Hotkeys 1–6                  ←→  HUD build panel buttons (✅ already exists)
  ESC                          ←→  Back button / HUD pause button
  F1–F10 (dev tools)           ←→  Debug panel (editor only)
```

---

## MIGRATION APPROACH

### Step 1: Enable Unity Input System
- Project Settings → Player → Active Input Handling: **Both** (safe transition)
- Install `com.unity.inputsystem` package

### Step 2: Create Input Action Asset (`WhiskeyvilleInput.inputactions`)
```
  Map: Gameplay
    Tap / Click       → performed when pointer is pressed
    Pan (drag)        → delta when pointer moves while held
    Zoom (pinch)      → two-touch distance delta
    Pause             → ESC or Back button

  Map: UI
    Submit
    Cancel
    Navigate
```

### Step 3: Replace Per-Script `Input.GetKey` Calls

| Script | Current Code | New Code |
|--------|-------------|----------|
| `BuildingSelector.cs` | `Input.GetKeyDown(KeyCode.Alpha1)` | `_input.Gameplay.Tap` event |
| `CameraController.cs` | `Input.mouseScrollDelta`, middle drag | Pinch + two-finger drag |
| `UIManager.cs` | `Input.GetKeyDown(KeyCode.Escape)` | `_input.Gameplay.Pause` event |
| `DevTools.cs` | `Input.GetKeyDown(KeyCode.F1)` | Keep as-is (desktop/editor only) |
| `BuildingPlacementManager.cs` | `Input.GetMouseButtonDown(0)` | `_input.Gameplay.Tap` |

### Step 4: Touch-Friendly UI Tweaks
- Minimum touch target size: **44×44 dp** (Apple HIG standard)
- Build panel buttons: increase hit area
- Ensure no UI panel requires hover state

---

## CAMERA TOUCH CONTROLS

```csharp
// CameraController.cs additions

private void HandleTouchPan()
{
    if (Touchscreen.current.touches.Count == 2)
    {
        // Two-finger pan
        var delta = Touchscreen.current.touches[0].delta.ReadValue();
        transform.Translate(-delta * panSpeed * Time.deltaTime);
    }
}

private void HandlePinchZoom()
{
    if (Touchscreen.current.touches.Count == 2)
    {
        var t0 = Touchscreen.current.touches[0].position.ReadValue();
        var t1 = Touchscreen.current.touches[1].position.ReadValue();
        float currentDist = Vector2.Distance(t0, t1);
        // compare to previous frame distance → zoom in/out
    }
}
```

---

## ACCEPTANCE CRITERIA

- [ ] Tap on tile places selected building (same as mouse click)
- [ ] Tap on crop when grown harvests it
- [ ] Tap on tree with Axe tool equipped chops it
- [ ] Tap on Saloon opens Sell Panel
- [ ] Two-finger drag pans the camera
- [ ] Pinch-to-zoom works (no scroll wheel required)
- [ ] All HUD buttons respond to tap (already button-based, minimal work)
- [ ] ESC / Back button opens pause menu
- [ ] Game runs in landscape orientation on mobile
- [ ] No hotkey knowledge required to play full game loop on touch device

---

## ANDROID / IOS BUILD NOTES

| Setting | Value |
|---------|-------|
| Orientation | Landscape |
| Target API (Android) | 26+ |
| Min iOS version | 14.0 |
| Scripting Backend | IL2CPP |
| Texture compression | ASTC |

---

## HOW IT CONNECTS TO THE FUTURE

```
  Mobile Input (Phase 1)
       │
       ├── Phase 2: Touch gesture for inspecting buildings (long press)
       ├── Phase 3: Haptic feedback on harvest/sell
       ├── Phase 3: Push notifications ("Your Rickhouse is full!")
       └── Phase 4: Cross-platform cloud save (mobile ↔ desktop)
```

---

## ESTIMATED TIMELINE

| Task | Time |
|------|------|
| Install Input System package | 5 min |
| Create input action asset | 10 min |
| Replace `BuildingPlacementManager` click | 10 min |
| Replace `UIManager` ESC | 5 min |
| Touch pan + pinch zoom in `CameraController` | 30 min |
| Replace `BuildingSelector` hotkeys | 10 min |
| Test full loop on device/emulator | 20 min |
| **Total** | **~90 min** |
