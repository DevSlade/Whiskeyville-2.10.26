# 🎓 Feature Spec: Tutorial & Onboarding
### Phase 1 — MVP Polish | Priority: 🔴 Critical
### Estimated Time: 45–60 minutes | Target MVP Gain: +4%

---

## PROBLEM

A new player who downloads Whiskeyville has **no idea what to do.** The production chain is
non-obvious. The build panel isn't self-explanatory. There's no prompt to plant, harvest,
or sell. Without a tutorial, the first session ends in confusion.

> "A new player cannot: Select buildings (hotkeys hidden), Sell products (F6 hidden),
> Save/Load (F7/F8 hidden), Understand what to do (no tutorial)."
> — Whiskeyville Bulletin, Jan 14, 2026

---

## SOLUTION

A **lightweight, non-blocking tooltip tutorial** triggered on first launch. No full cutscenes.
No unskippable walls of text. Just directional arrows and a single-sentence prompt that
guides the player through their first whiskey production cycle.

```
  ┌──────────────────────────────────────────────────────────────────┐
  │  TUTORIAL FLOW (first run only)                                  │
  │                                                                  │
  │  Step 1: "Tap the Build button to open your buildings"           │
  │            ▼ arrow points to HUD build button                    │
  │            [PLAYER TAPS BUILD BUTTON] → advance                  │
  │                                                                  │
  │  Step 2: "Select a Field to plant corn"                          │
  │            ▼ arrow points to Field button in build panel         │
  │            [PLAYER SELECTS FIELD] → advance                      │
  │                                                                  │
  │  Step 3: "Tap a dirt tile to place your field"                   │
  │            ▼ arrow pulses over center tiles                      │
  │            [PLAYER PLACES FIELD] → advance                       │
  │                                                                  │
  │  Step 4: "Wait for corn to grow, then tap to harvest"            │
  │            ▼ arrow points to field                               │
  │            [PLAYER HARVESTS CORN] → advance                      │
  │                                                                  │
  │  Step 5: "Build a Mash Tun to turn corn into mash"               │
  │            [PLAYER PLACES MASH TUN] → advance                    │
  │                                                                  │
  │  Step 6: "Keep building! Mash → Still → Cooperage → Rickhouse"  │
  │            Show chain diagram (static image or icons)            │
  │            [TAP TO DISMISS] → Tutorial complete ✅               │
  │                                                                  │
  │  Step 7: "Open the Saloon to sell your aged whiskey for cash!"   │
  │            [PLAYER TAPS SALOON] → tutorial ends                  │
  └──────────────────────────────────────────────────────────────────┘
```

---

## SCRIPT TO CREATE: `TutorialManager.cs`

```csharp
// TUTORIALMANAGER.CS
// PURPOSE:   Drives first-run tutorial steps. Detects player actions via events.
// DEPENDS:   UIManager, BuildingSelector, BuildingPlacementManager, CropBehavior
// FIRST RUN: Detected via PlayerPrefs.GetInt("TutorialComplete", 0)

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialStep[] steps;
    private int _currentStep = 0;

    private void Start()
    {
        if (PlayerPrefs.GetInt("TutorialComplete", 0) == 1) return;
        ShowStep(0);
    }

    private void ShowStep(int index) { ... } // show overlay, arrow, text
    public void AdvanceStep() { ... }        // called by game events
    private void Complete() { PlayerPrefs.SetInt("TutorialComplete", 1); }
}

[System.Serializable]
public class TutorialStep
{
    public string promptText;
    public RectTransform arrowTarget; // points toward this UI element
    public TutorialTrigger completionTrigger; // what event advances the step
}

public enum TutorialTrigger
{
    BuildPanelOpened,
    BuildingSelected,
    BuildingPlaced,
    CropHarvested,
    ItemProduced,
    SaloonOpened,
    Dismissed
}
```

---

## TUTORIAL UI COMPONENTS

### Tooltip Overlay
```
  ┌─────────────────────────────────┐
  │  💬  "Tap a tile to place       │
  │       your field here"          │
  │                         [SKIP]  │
  └─────────────────────────────────┘
       ▼ (animated arrow pointing to grid)
```

- Semi-transparent dark background behind the prompt
- White rounded rectangle bubble with prompt text
- Animated pulsing arrow pointing at the target element
- SKIP button in corner to dismiss tutorial entirely

### Production Chain Diagram (Step 6)
A static icon-based visual showing the full chain:
```
  🌽 → 🍺 → 🥃 → 🪣 → 🏚️ → 🍾 → 💰
 Field  Mash  Still  Coop  Rick  Saloon Cash
```
This should be a UI texture/sprite asset so it renders cleanly on all screen sizes.

---

## IMPLEMENTATION DETAILS

### First-Run Detection
```csharp
// TutorialManager.Start()
bool isFirstRun = PlayerPrefs.GetInt("TutorialComplete", 0) == 0;
if (!isFirstRun) return; // skip tutorial entirely
```

### Event Wiring
Wire into existing events so tutorial advances automatically:
- `BuildingSelector.OnBuildingSelected` → advance step after selecting Field
- `BuildingPlacementManager.OnBuildingPlaced` → advance after placement
- `CropBehavior.OnHarvested` → advance after first harvest
- `SellManager.OnSold` → show "You did it!" completion

---

## SKIP / REPLAY OPTIONS

| Action | How |
|--------|-----|
| Skip tutorial | Tap SKIP at any step → `PlayerPrefs.SetInt("TutorialComplete", 1)` |
| Replay tutorial | Settings panel → "Replay Tutorial" → `PlayerPrefs.SetInt("TutorialComplete", 0)` |

---

## ACCEPTANCE CRITERIA

- [ ] New player is greeted with tutorial step 1 on first launch
- [ ] Tutorial advances automatically when player completes each action
- [ ] Tutorial is skippable at any step
- [ ] Tutorial does NOT appear on subsequent launches after completion
- [ ] Tutorial can be replayed from settings
- [ ] All tutorial prompts are readable on mobile (min 16pt font)
- [ ] Arrows correctly point at target UI elements on all screen sizes
- [ ] Production chain diagram is shown before tutorial closes

---

## HOW IT CONNECTS TO THE FUTURE

```
  Tutorial (Phase 1)
       │
       ├── Phase 2: Context-sensitive tips ("You have 10 corn — build a Mash Tun!")
       ├── Phase 3: Quest system uses same step/trigger framework
       ├── Phase 3: NPC dialogue uses same tooltip overlay component
       └── Phase 4: Prestige mode tutorial variation ("Welcome back, legend")
```

---

## ESTIMATED TIMELINE

| Task | Time |
|------|------|
| `TutorialManager.cs` | 20 min |
| Tooltip overlay prefab + arrow animation | 15 min |
| Wire step triggers into existing scripts | 10 min |
| Production chain diagram (simple icons) | 10 min |
| Test full tutorial flow | 5 min |
| **Total** | **~60 min** |
