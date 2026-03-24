# 🌸 Feature Spec: Decorations
### Phase 2 — Content Expansion | Priority: Post-MVP
### Target: Q2 2026

---

## CONCEPT

Decorations are **non-functional cosmetic items** placed on the grid. They don't produce
resources. They express ownership, personality, and pride in the town the player has built.

> "My distillery. My aesthetic."

Decorations are the emotional glue of a town-builder. When players decorate, they are
saying: *I care about this place.* That's the moment the game has them for good.

---

## DECORATION TYPES

### Fences & Paths

| Item | How to Unlock | Effect |
|------|--------------|--------|
| Wooden Fence (segment) | 50 Fame | Visual |
| Stone Path | 100 Cash | Visual |
| Dirt Path | 0 (free) | Visual, shows worn paths |
| White Picket Fence | 500 Fame | Visual |
| Iron Fence | 1,500 Fame | Premium |

### Nature & Landscape

| Item | How to Unlock | Effect |
|------|--------------|--------|
| Flower Patch | 25 Cash | Visual |
| Wildflower Bed | 75 Cash | Visual |
| Rock Cluster | Free (world-gen) | Visual |
| Pond (2×2) | 200 Cash | Visual + tourist attraction (+Fame/day) |
| Old Oak Tree | 1,000 Fame | Visual + ambient flavor |

### Signage & Town Identity

| Item | How to Unlock | Effect |
|------|--------------|--------|
| Town Name Sign | 50 Cash | Displays town name (player input) |
| "WHISKEY" Billboard | 500 Fame | +2 Fame/day (word spreads) |
| Directional Arrow Sign | 100 Cash | Visual, points toward Saloon |
| Chalkboard Special Sign | 200 Cash | Customizable text |

### Furniture & Props

| Item | How to Unlock | Effect |
|------|--------------|--------|
| Bench | 50 Cash | Visual |
| Barrel Stack | 100 Cash | Thematic flavor |
| Lantern Post | 75 Cash | Glows at night (DayNightCycle) |
| Hay Bale | 25 Cash | Visual |
| Wagon Wheel | 200 Fame | Antique aesthetic |
| Whiskey Barrel (single) | 150 Cash | Shows off your product |

---

## PLACEMENT SYSTEM

Decorations use the same `BuildingPlacementManager` flow:
1. Select decoration from a new **Decorations Panel** (or a tab in Build Panel)
2. Click tile → place decoration
3. Decorations can overlap with other decorations (but not buildings/crops)
4. Demolish tool removes decorations (no refund for cheap items, 50% for rare)

### Grid Behavior
- 1×1 tiles only (most items)
- Some items are 2×2 (Pond)
- Decorations don't block pathing (tourists walk through them)
- Decorations render in the correct Y-sort layer

---

## TOWN NAME SYSTEM

The **Town Name Sign** decoration triggers a one-time (or repeatable) text input:
```
  ┌────────────────────────────────────────────────────┐
  │                                                    │
  │  What's your town called?                         │
  │                                                    │
  │  [ _________________________ ]                     │
  │                                                    │
  │               [ CONFIRM ]                         │
  └────────────────────────────────────────────────────┘
```
- Town name saved in `GameData.townName`
- Displayed on the sign sprite (dynamic text mesh)
- Used in tutorial ("Welcome to [Town Name]!")
- Shown on save slot screen
- Could appear in future social/sharing features

---

## PREMIUM DECORATIONS (Phase 4 Monetization Hook)

Some decorations are available for **premium currency** only:
- Animated lanterns (flicker effect)
- Rare flower varieties (imported)
- Historical photos for walls
- Seasonal holiday decorations

**Policy:** All gameplay-adjacent decorations (town sign, fence) earnable via Cash/Fame.
Premium-only = purely cosmetic aesthetic upgrades.

---

## TECHNICAL NOTES

### `DecorationData.cs` (New ScriptableObject)
```csharp
[CreateAssetMenu(menuName = "Whiskeyville/Decoration")]
public class DecorationData : ScriptableObject
{
    public string decorationName;
    public Sprite sprite;
    public int cashCost;
    public int fameCost;
    public bool isLocked;
    public string unlockRequirement; // e.g., "50 Fame"
    public bool isDestroyable;
    public int refundAmount;
}
```

### Save Integration
All placed decorations saved as list in `GameData`:
```csharp
public List<PlacedDecoration> decorations = new List<PlacedDecoration>();

[System.Serializable]
public class PlacedDecoration
{
    public string decorationName;
    public int gridX;
    public int gridY;
}
```

---

## ACCEPTANCE CRITERIA

- [ ] Decorations panel accessible from HUD
- [ ] At least 8 decorations available at launch (Phase 2)
- [ ] Decorations placed on grid at correct sort order
- [ ] Decorations saved and restored correctly
- [ ] Town Name Sign prompts for name input on placement
- [ ] Locked decorations shown with unlock requirements
- [ ] Demolish tool removes decorations

---

## HOW IT CONNECTS TO THE FUTURE

```
  Decorations
       │
       ├── Events: Seasonal decorations during holidays
       ├── Prestige: Rare decorations as prestige trophies
       ├── Social: Town sharing shows off decorations
       ├── Tourists: Some decorations attract specific tourist types
       └── Premium: Animated/exclusive cosmetics as monetization
```
