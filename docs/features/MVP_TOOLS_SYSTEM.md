# 🪓 Feature Spec: Tool System (Axe / Hoe / Demolish)
### Phase 1 — MVP Polish | Priority: 🔴 Critical
### Estimated Time: 30–45 minutes | Target MVP Gain: +5%

---

## PROBLEM

Currently the player interaction model is "click anything and something happens." This is
confusing and unintentional. A player clicking a tree doesn't know if they're chopping it or
selecting it. Buildings respond to clicks ambiguously. There is no sense of "I am a player
with tools in a world."

**Current pain:**
- Trees: click → ambiguous (harvest? select?)
- Tiles: click → place selected building (no confirmation of intent)
- Buildings: no way to demolish them

---

## SOLUTION

A three-tool selector that defines **what happens when the player clicks**. The selected tool
is always visible in the HUD. Interaction model becomes intentional.

```
  ┌──────────────────────────────────────────────────────┐
  │  TOOLBAR (bottom-left HUD)                           │
  │                                                      │
  │  [ 🪓 Axe ]  [ 🌾 Hoe ]  [ 💥 Demolish ]           │
  │      ▲            │              │                   │
  │   selected     chops tree     removes building       │
  │   (default)   tills ground    (with confirm)         │
  └──────────────────────────────────────────────────────┘
```

---

## TOOL DEFINITIONS

### 🪓 Axe Tool
- **Action:** Click a tile with a tree → chop it → add `Wood` to inventory
- **Feedback:** Tree sprite swings/fades. `ProductionPopup` fires "+1 Wood"
- **Invalid click:** Clicking non-tree tile shows "Nothing to chop here" (brief flash)
- **Hotkey:** `Q` or `1` (configurable)

### 🌾 Hoe Tool
- **Action:** Click an empty dirt/grass tile → till it → marks tile as `Tilled`
- **Purpose:** Only tilled tiles accept the `Field` (crop) building placement
- **Feedback:** Dirt tile changes sprite to tilled texture
- **Hotkey:** `W` or `2` (configurable)

### 💥 Demolish Tool
- **Action:** Click a placed building → show confirm prompt → removes building
- **Confirm UX:** 3-second countdown with "Tap again to confirm" overlay
  - OR: Show confirmation panel with "Demolish / Cancel"
- **Refund:** Partial refund (50% of building cost) added to Cash
- **Hotkey:** `E` or `3` (configurable)

---

## SCRIPT TO CREATE: `ToolSelector.cs`

```csharp
// TOOLSELECTOR.CS
// PURPOSE:  Manages active tool selection. Tool defines what happens on tile click.
// ATTACHED: GameManager or persistent object
// DEPENDS:  BuildingPlacementManager, TreeBehavior, GridManager, InventoryManager

public enum PlayerTool { Build, Axe, Hoe, Demolish }

public class ToolSelector : MonoBehaviour
{
    public static ToolSelector Instance { get; private set; }

    public PlayerTool CurrentTool { get; private set; } = PlayerTool.Build;

    public void SelectTool(PlayerTool tool) { ... }
    private void HandleHotkeyInput() { ... } // Q/W/E
    // Fire OnToolChanged event for UI to react
    public event Action<PlayerTool> OnToolChanged;
}
```

### Integration Points
| Script | Change Needed |
|--------|--------------|
| `BuildingPlacementManager.cs` | Check `ToolSelector.CurrentTool == Build` before placing |
| `TreeBehavior.cs` | Only chop when `ToolSelector.CurrentTool == Axe` |
| `TileBehavior.cs` | Add `Tilled` state; only till when `CurrentTool == Hoe` |
| `BuildingBehavior.cs` | Add `Demolish()` method; only when `CurrentTool == Demolish` |

### New UI: `ToolbarUI.cs`
- 3 buttons, highlights active tool
- Updates on `ToolSelector.OnToolChanged`
- Positioned bottom-left, always visible

---

## ACCEPTANCE CRITERIA

- [ ] Player can select Axe tool (Q hotkey or HUD button)
- [ ] Clicking a tree with Axe equipped removes the tree and adds +1 Wood
- [ ] Clicking a non-tree with Axe shows brief "nothing to chop" feedback
- [ ] Player can select Hoe tool (W hotkey or HUD button)
- [ ] Clicking a tile with Hoe tills it (changes sprite, marks as tilled)
- [ ] Fields can only be placed on tilled tiles
- [ ] Player can select Demolish tool (E hotkey or HUD button)
- [ ] Clicking a building with Demolish prompts for confirmation
- [ ] Confirmed demolish removes building, refunds 50% cost, frees tile
- [ ] Active tool is always visually indicated in the toolbar
- [ ] Build mode (default) is unchanged from current behavior

---

## HOW IT CONNECTS TO THE FUTURE

```
  ToolSelector (Phase 1)
       │
       ├── Phase 2: Add "Upgrade" tool → click building to level it up
       ├── Phase 2: Add "Inspect" tool → click anything to see stats/tooltip
       ├── Phase 3: Tool durability / charges (premium tool upgrades?)
       └── Phase 5: Tools become animated characters (farm hand NPCs?)
```

---

## ESTIMATED TIMELINE

| Task | Time |
|------|------|
| `ToolSelector.cs` | 15 min |
| `ToolbarUI.cs` | 10 min |
| Integrate with `BuildingPlacementManager` | 5 min |
| Integrate with `TreeBehavior` | 5 min |
| Demolish confirm UI | 10 min |
| **Total** | **~45 min** |
