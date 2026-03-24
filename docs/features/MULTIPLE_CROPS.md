# 🌽 Feature Spec: Multiple Crop Types
### Phase 2 — Content Expansion | Priority: Post-MVP
### Target: Q2 2026

---

## CONCEPT

Currently Whiskeyville supports one crop: **Corn**. Corn → Mash → Whiskey → Aged Whiskey.
Multiple crop types add **recipe variety** — different inputs produce different outputs with
different sell prices and flavor profiles. This expands the production chain horizontally,
giving experienced players more to manage and optimize.

```
  Current:   Corn → Corn Mash → Whiskey → Aged Whiskey ($50)
             
  Expanded:  Corn   → Corn Mash   → Bourbon    → Aged Bourbon   ($50)
             Wheat  → Wheat Mash  → Wheat Whiskey → Aged ($75)
             Rye    → Rye Mash    → Rye Whiskey   → Aged ($120)
             Barley → Barley Mash → Single Malt    → Aged ($200)
```

---

## CROP ROSTER

| Crop | Unlock | Growth Time | Resource | Notes |
|------|--------|-------------|---------|-------|
| **Corn** | Default (0 Fame) | 60s | Corn | The classic. Reliable. |
| **Wheat** | 750 Fame | 45s | Wheat | Grows fast, mild flavor |
| **Rye** | 3,000 Fame | 90s | Rye | Slow. Premium price. |
| **Barley** | 5,000 Fame | 120s | Barley | Very slow. Elite price. |

---

## PRODUCTION CHAIN EXPANSION

Each crop needs its own Mash Tun recipe (same building, different input):

```
  CORN MASH TUN:    Corn (1)   → Corn Mash (1)
  WHEAT MASH TUN:   Wheat (1)  → Wheat Mash (1)     ← same Mash Tun building,
  RYE MASH TUN:     Rye (1)    → Rye Mash (1)           different recipe config
  BARLEY MASH TUN:  Barley (1) → Barley Mash (1)
```

The Still, Cooperage, and Rickhouse accept **any mash type** as input — the output just
has a different name and sell value.

**Option A:** Single Mash Tun with a recipe picker (simpler code)
**Option B:** Separate Mash Tun building per crop type (more buildings, more decisions)

_Recommendation: Option A for Phase 2, Option B as luxury expansion later._

---

## SELL PRICE TABLE (Updated)

| Product | Aged Whiskey Variant | Sell Price |
|---------|---------------------|-----------|
| Aged Bourbon (from Corn) | Default | $50 |
| Aged Wheat Whiskey | 750 Fame unlock | $75 |
| Aged Rye Whiskey | 3,000 Fame unlock | $120 |
| Aged Single Malt (Barley) | 5,000 Fame unlock | $200 |

---

## TECHNICAL CHANGES

### `CropBehavior.cs` Extension
```csharp
// Add crop type to CropBehavior:
[SerializeField] private CropType _cropType = CropType.Corn;

public enum CropType { Corn, Wheat, Rye, Barley }

// OnHarvest() checks _cropType and adds corresponding resource:
// CropType.Corn → InventoryManager.Add("Corn", 1)
// CropType.Wheat → InventoryManager.Add("Wheat", 1)
```

### `BuildingData.cs` — Recipe Support
```csharp
// Mash Tun can have recipe configuration:
[Header("Recipe (Mash Tun variant)")]
public string recipeInput = "Corn";   // matches InventoryManager resource key
public string recipeOutput = "CornMash";
```

### `InventoryManager.cs` — New Resources
```csharp
public const string RESOURCE_WHEAT        = "Wheat";
public const string RESOURCE_RYE          = "Rye";
public const string RESOURCE_BARLEY       = "Barley";
public const string RESOURCE_WHEAT_MASH   = "WheatMash";
public const string RESOURCE_RYE_MASH     = "RyeMash";
public const string RESOURCE_BARLEY_MASH  = "BarleyMash";
```

### New `BuildingData.asset` Files
- `MashTunWheat.asset`
- `MashTunRye.asset`
- `MashTunBarley.asset`

---

## FIELD SELECTION UX

When placing a Field, a **Crop Picker** popup appears:
```
  ┌──────────────────────────────────────────────────────────┐
  │  🌽  Select Crop Type                                    │
  │                                                          │
  │  [🌽 Corn   $25 tile]  [🌾 Wheat  $40 tile]             │
  │  [🌿 Rye    $75 tile]  [🌱 Barley $120 tile]            │
  │                                                          │
  │  🌿 Rye — Locked (3,000 Fame required)                   │
  └──────────────────────────────────────────────────────────┘
```

---

## SPRITE REQUIREMENTS

Each crop needs 4 growth stage sprites:
- Corn: ✅ Already exists
- Wheat: 4 new sprites
- Rye: 4 new sprites
- Barley: 4 new sprites

Total: 12 new sprite assets

---

## ACCEPTANCE CRITERIA

- [ ] Wheat unlocks at 750 Fame and can be grown
- [ ] Wheat produces Wheat Mash → Wheat Whiskey → Aged (sells for $75)
- [ ] Rye unlocks at 3,000 Fame
- [ ] Barley unlocks at 5,000 Fame
- [ ] Crop picker appears when placing a Field
- [ ] Locked crops shown with Fame requirement
- [ ] All crop resources saved/loaded correctly
- [ ] New Mash Tun variants available in Build Panel when corresponding crop unlocked

---

## HOW IT CONNECTS TO THE FUTURE

```
  Multiple Crops
       │
       ├── Events: "Rye Harvest Festival" — rye price triples for 2 days
       ├── Tourists: Enthusiasts specifically seek out Barley Single Malt
       ├── Prestige: Start with Wheat unlocked on 2nd run
       └── Premium: Exclusive crop type (Black Corn, aged to Black Label Whiskey)
```
