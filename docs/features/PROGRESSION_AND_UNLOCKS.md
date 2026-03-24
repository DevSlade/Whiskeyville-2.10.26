# 🔓 Feature Spec: Progression & Unlocks
### Phase 2 — Content Expansion | Priority: Post-MVP
### Target: Q2 2026

---

## CONCEPT

Progression is the answer to "why keep playing?" The player already knows **how** to play.
Unlocks give them something to work **toward**. A visible unlock tree shows the road ahead
and makes every Fame milestone feel like forward momentum.

```
  WITHOUT PROGRESSION:   I built everything on day 1. Now what?
  WITH PROGRESSION:      I need 500 more Fame to unlock the Export Dock.
                         That's my goal today.
```

---

## UNLOCK TREE

```
                              ┌──────────────────┐
                              │  START (0 Fame)  │
                              │  6 base buildings│
                              └────────┬─────────┘
                                       │
               ┌───────────────────────┼───────────────────────┐
               │                       │                       │
        ┌──────▼──────┐        ┌───────▼──────┐       ┌───────▼──────┐
        │  50 Fame    │        │  100 Fame    │       │  250 Fame    │
        │  Wooden     │        │  Tasting     │       │  Enthusiast  │
        │  Fence Dec  │        │  Room Bldg   │       │  Tourists    │
        └──────┬──────┘        └───────┬──────┘       └───────┬──────┘
               │                       │                       │
        ┌──────▼──────┐        ┌───────▼──────┐       ┌───────▼──────┐
        │  500 Fame   │        │  750 Fame    │       │ 1,000 Fame   │
        │  Barrel Art │        │  Wheat Crop  │       │  Critic      │
        │  Wall Dec   │        │  (new crop)  │       │  Tourists    │
        └──────┬──────┘        └───────┬──────┘       └───────┬──────┘
               │                       │                       │
        ┌──────▼──────┐        ┌───────▼──────┐       ┌───────▼──────┐
        │ 2,500 Fame  │        │ 3,000 Fame   │       │ 5,000 Fame   │
        │  Export     │        │  Rye Crop    │       │  PRESTIGE    │
        │  Dock Bldg  │        │ (premium $)  │       │  AVAILABLE   │
        └─────────────┘        └──────────────┘       └──────────────┘
```

---

## UNLOCK CATEGORIES

### Buildings (New Production Buildings)

| Building | Fame | Function |
|---------|------|----------|
| Tasting Room | 100 | Converts Fame → premium sell bonus |
| Export Dock | 2,500 | Bulk sell: 10 Aged Whiskey → 600 Cash (vs 500) |
| Bottling Line | 1,500 | Auto-bottles Aged Whiskey for bonus Fame per sale |
| Smokehouse | 800 | Produces smoked mash → premium whiskey tier |

### Crop Varieties (New Resources)

| Crop | Fame | Produces | Notes |
|------|------|---------|-------|
| Wheat | 750 | Wheat Mash | Different flavor profile, higher sell value |
| Rye | 3,000 | Rye Mash | Rare, double sell price |
| Barley | 5,000 | Barley Mash | Prestige ingredient |

### Decorations (See DECORATIONS doc)

### Tourist Types (See TOURISTS doc)

---

## `UnlockManager.cs` (New Script)

```csharp
// UNLOCKMANAGER.CS
// PURPOSE:  Tracks which unlocks are available. Fires events on new unlocks.
// DEPENDS:  FameMilestoneManager, BuildingDatabase

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }

    [SerializeField] private UnlockDefinition[] _allUnlocks;
    private HashSet<string> _unlockedItems = new HashSet<string>();

    // Called by FameMilestoneManager when Fame threshold crossed
    public void CheckUnlocksForFame(int currentFame) { ... }

    public bool IsUnlocked(string unlockId) => _unlockedItems.Contains(unlockId);

    public event Action<UnlockDefinition> OnNewUnlock;
}

[System.Serializable]
public class UnlockDefinition
{
    public string unlockId;
    public string displayName;
    public UnlockType type;    // Building / Crop / Decoration / Tourist
    public int fameCost;
    public Sprite iconSprite;
    public string description;
}
```

---

## UNLOCK NOTIFICATION UI

When a new unlock triggers:
```
  ┌──────────────────────────────────────────────────────┐
  │  🔓  NEW UNLOCK!                                     │
  │                                                      │
  │  ⭐ 100 Fame reached!                               │
  │                                                      │
  │  Tasting Room is now available to build.             │
  │  [Open Build Menu]         [Dismiss]                 │
  └──────────────────────────────────────────────────────┘
```

---

## BUILD PANEL INTEGRATION

The Build Panel should show locked buildings as **grayed-out ghost entries** with a
Fame requirement badge, so players always know what's coming:

```
  Build Panel:
  ┌────────────────────────────────────┐
  │  [ Field $25 ]   [ Mash Tun $75 ] │
  │  [ Still $150 ]  [ Cooperage ]    │
  │  [ Rickhouse ]   [ Saloon ]       │
  │  ─────── Locked ────────          │
  │  [ ⭐100 Tasting Room ]  (grayed) │
  │  [ ⭐500 Export Dock  ]  (grayed) │
  └────────────────────────────────────┘
```

---

## SAVE/LOAD INTEGRATION

Unlocked items stored in `GameData.cs`:
```csharp
public List<string> unlockedItems = new List<string>();
```

---

## ACCEPTANCE CRITERIA

- [ ] `UnlockManager` tracks unlocked state correctly
- [ ] Fame milestones fire `OnNewUnlock` events
- [ ] Unlock notification appears when new item unlocked
- [ ] Locked buildings appear grayed in Build Panel with Fame requirement
- [ ] Unlocked buildings appear in Build Panel immediately
- [ ] Unlock state persists across save/load

---

## HOW IT CONNECTS TO THE FUTURE

```
  Progression & Unlocks
       │
       ├── Fame: Fame is the currency that drives unlock tree
       ├── Prestige: Second run starts with some unlocks pre-unlocked (bonus)
       ├── Events: Some unlocks only available during specific events
       ├── Multiple Crops: New crops gate through this unlock system
       └── Premium: Some cosmetic unlocks purchasable with premium currency
```
