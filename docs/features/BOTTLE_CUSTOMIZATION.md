# 🍾 Bottle Customization — Label, Glass & Logo Editor
### Phase 2 Feature Spec | Priority: Post-MVP
### Target: Q2 2026
### Requires Building: Bottling House

---

## CONCEPT

Bottle Customization gives the player a creative outlet to design the *identity* of
their whiskey brand. A distillery is not just a production chain — it is a brand, a
story, a label that people remember. Players who invest in their bottle design see
higher sell prices, more tourist interest, and meaningful Meme Score boosts.

The customization suite is accessed via the **Bottling House** building, which must be
constructed before bottle designs can be created. Every Aged Whiskey batch produced
after the Bottling House is placed uses the active bottle design.

---

## THE BOTTLING HOUSE BUILDING

| Property | Value |
|----------|-------|
| Building Name | Bottling House |
| Cost | $400 Cash |
| Unlock Requirement | 100 Fame |
| Size | 2×2 tiles |
| Function | Applies active bottle design to Aged Whiskey output; opens Bottle Editor UI on click |
| Production Effect | No change to production speed; purely cosmetic multiplier on sell price |
| Prefab | BottlingHouse_Prefab |
| Popup Color | Navy Blue (RGBA: 0.18, 0.32, 0.72, 1) |

The Bottling House is a **non-production building** (like the Saloon). Clicking it opens
the **Bottle Customization Panel** instead of a sell panel.

---

## CUSTOMIZATION COMPONENTS

### Component 1: Glass Type

The player selects from unlocked glass shapes. Each shape has distinct visual
appearance and a sell price modifier.

| Glass Type | Unlock | Cost to Equip | Sell Price Modifier | Meme Score Bonus | Notes |
|------------|--------|--------------|--------------------|--------------------|-------|
| Standard Bottle | Default (free) | Free | ×1.00 | +0 | Classic 750ml shape |
| Hip Flask | 100 Fame | $50 | ×1.05 | +5 | Compact; appeals to travelers |
| Tall Decanter | 250 Fame | $100 | ×1.15 | +10 | Elegant; attracts Critics |
| Ceramic Jug | 150 Fame | $75 | ×1.10 | +8 | Rustic; strong Passerby appeal |
| Crystal Decanter | 1000 Fame | $250 | ×1.30 | +20 | Luxury; Celebrity tourist magnet |
| Personalized Crock | 500 Fame | $150 | ×1.20 | +15 | Hand-thrown look; high Meme Score |

---

### Component 2: Label Color & Background

Players pick a **background color** or **texture pattern** for their label.

| Option | Unlock | Meme Score Bonus | Notes |
|--------|--------|-----------------|-------|
| Plain White | Default | +0 | Clean, minimal |
| Kraft Brown | Default | +2 | Rustic paper look |
| Deep Navy | 50 Fame | +4 | Premium feel |
| Emerald Green | 50 Fame | +4 | Eye-catching |
| Crimson Red | 100 Fame | +6 | Bold; western aesthetic |
| Charcoal Black | 250 Fame | +8 | Sophisticated |
| Aged Parchment | 500 Fame | +10 | Artisan look; highest Meme Score |
| Custom Gradient | 750 Fame | +12 | Player picks two colors |

---

### Component 3: Logo / Emblem

A small decorative emblem placed on the label. Chosen from a library of unlockable art assets.

| Emblem | Unlock | Meme Score Bonus | Tourist Attraction |
|--------|--------|-----------------|-------------------|
| No emblem | Default | +0 | — |
| Simple Star | Default | +2 | Passerby +5% |
| Wheat Sheaf | 50 Fame | +4 | Enthusiast +8% |
| Oak Barrel | 100 Fame | +5 | Enthusiast +10% |
| Running Fox | 200 Fame | +6 | Critic +5% |
| Eagle Crest | 500 Fame | +8 | Celebrity +10% |
| Black Label Seal | 1000 Fame | +12 | All tourists +10% |
| Custom Emblem (draw) | Phase 3 | +15 | Depends on Meme Score |

---

### Component 4: Distillery Name & Whiskey Name

Two text fields the player fills in once (names persist until changed):

| Field | Max Characters | Default | Notes |
|-------|---------------|---------|-------|
| Distillery Name | 24 chars | "My Distillery" | Shown at top of label |
| Whiskey Name | 20 chars | "House Whiskey" | Shown in large text on label |
| Vintage Year | 4 digits | Current in-game year | Optional; cosmetic only |
| Tagline | 40 chars | (empty) | Optional; shown at bottom of label |

Naming the distillery and whiskey gives:
- **+5 Meme Score** baseline (having a name vs. default)
- **+5 additional Meme Score** if both Distillery Name and Whiskey Name are custom
- The distillery name appears on the Town Sign decoration (if placed)
- The whiskey name appears in tourist dialogue: "I've heard of [Whiskey Name]!"

---

### Component 5: Label Art Style (Phase 3 — "Requires Building")

The full label painter requires the Bottling House AND a **Label Press** upgrade
building (Phase 3, unlocked at 500 Fame). This is the "editor" the problem statement
references as "requires building."

| Feature | Requirement |
|---------|-------------|
| Predefined label templates | Bottling House only |
| Color pickers for label | Bottling House only |
| Freeform text placement | Bottling House + Label Press |
| Logo stamp tool (press an emblem onto label) | Bottling House + Label Press |
| Import custom image (mobile: camera roll) | Phase 4, Label Press |
| Shareable label screenshot | Phase 3, Label Press |

---

## SELL PRICE CALCULATION WITH CUSTOMIZATION

The final sell price for a bottle is calculated as:

```
  finalPrice = baseSellPrice
             × whiskeySellMultiplier     (from WhiskeyPropertyManager)
             × glassSellMultiplier        (from BottleCustomizationManager)
             × qualitySellMultiplier      (from WhiskeyPropertyManager)
             × eventMultiplier            (from EventManager, if active)
```

**Example**:
```
  Base price: $50
  × Quality 4★ modifier: 1.60  → $80
  × Tall Decanter glass: 1.15  → $92
  × Radio Spot campaign: 1.00  → $92  (no sell price effect, only spawn rate)
  × No active event:     1.00  → $92
  Final price: $92 per bottle
```

---

## MEME SCORE CONTRIBUTION

```
  Meme Score (Customization portion, max 40 pts):
  = glassBonus + labelColorBonus + emblemBonus + nameBonus
  
  This feeds into the global Meme Score alongside WhiskeyPropertyManager inputs
  (see MEMETIC_RESEARCH.md for full Meme Score calculation)
```

---

## BOTTLE CUSTOMIZATION UI

### Panel Layout

```
  ╔═══════════════════════════════════════════╗
  ║  🍾 Bottle Editor                          ║
  ╠════════════════╦══════════════════════════╣
  ║  LIVE PREVIEW  ║  CUSTOMIZE               ║
  ║                ║  ┌─────────────────────┐ ║
  ║  [Bottle       ║  │ Glass Type    ▼     │ ║
  ║   Preview      ║  │ Label Color   ▼     │ ║
  ║   Sprite]      ║  │ Emblem        ▼     │ ║
  ║                ║  │ Distillery: _______  │ ║
  ║  Meme Score:   ║  │ Whiskey:    _______  │ ║
  ║  ████░░ 68/100 ║  │ Tagline:    _______  │ ║
  ║                ║  └─────────────────────┘ ║
  ║  Sell Price:   ║                          ║
  ║  $92/bottle    ║  [Apply Design]          ║
  ╚════════════════╩══════════════════════════╝
```

### Behavior
- **Live Preview**: Updates in real time as options change
- **Meme Score bar**: Shows the customization contribution to Meme Score
- **Sell Price**: Shows projected sell price with active customization
- **Apply Design**: Saves customization, closes panel; next Rickhouse output uses new design

---

## IMPLEMENTATION

### Scripts Required

- **BottleCustomizationData.cs** — Serializable data class:
  `glassTypeIndex, labelColorIndex, emblemIndex, distilleryName, whiskeyName, tagline, vintageYear`
- **BottleCustomizationManager.cs** — Singleton:
  - Stores active `BottleCustomizationData`
  - `GetSellPriceMultiplier()` → float
  - `GetMemeScoreBonus()` → int
  - `OpenEditor()` / `CloseEditor()` for UI
  - `ApplyDesign(BottleCustomizationData)` to save active design

### Save Data

Add to `GameData.cs`:
```csharp
// Bottle Customization
public int glassTypeIndex;
public int labelColorIndex;
public int emblemIndex;
public string distilleryName;
public string whiskeyName;
public string tagline;
public int vintageYear;
```

### New BuildingData Asset Required

`BottlingHouseData.asset`:
```yaml
buildingName: BottlingHouse
cost: 400
isCrop: false
outputResource: ""
outputAmount: 0
productionInterval: 0
requiresInput: false
popupColor: NavyBlue (RGBA: 0.18, 0.32, 0.72, 1)
```

---

## INTEGRATION POINTS

- **SellManager.cs** — Queries `BottleCustomizationManager.GetSellPriceMultiplier()` in `SellOne()` / `SellAll()`
- **MemeticResearchManager.cs** — Queries `BottleCustomizationManager.GetMemeScoreBonus()` for Spread Event calculation
- **UIManager.cs** — `ToggleBottleEditor()` method; opens when Bottling House clicked
- **BuildingBehavior.cs** — `BottlingHouse` building name triggers `UIManager.ToggleBottleEditor()` in OnMouseDown (similar to Saloon → SellPanel)
- **SaveManager.cs** — Persists `BottleCustomizationData` fields in GameData
- **UnlockManager.cs** (Phase 2) — Gates glass types and emblems behind Fame thresholds
