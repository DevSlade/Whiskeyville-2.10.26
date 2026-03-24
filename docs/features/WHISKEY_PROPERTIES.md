# 🥃 Whiskey Properties — Flavor, Quality, Temperature & Quantity
### Phase 2 Feature Spec | Priority: Post-MVP
### Target: Q2 2026

---

## CONCEPT

Every batch of Aged Whiskey in Whiskeyville has **properties** that affect its value,
the types of customers it attracts, its impact on town fame, and which seasonal events
it qualifies for. These properties are set during production based on:

- The **grain type** used (corn, rye, wheat, barley)
- The **barrel quality** (standard, charred, French oak)
- The **aging duration** (regular rickhouse vs. extended aging)
- Any **Research upgrades** the player has purchased

Understanding these properties lets players craft *intentional* whiskey — not just
whatever the chain produces, but a distinct product with a personality.

---

## THE FOUR CORE PROPERTIES

### 1. Flavor Profile

The flavor profile is determined primarily by the grain used in the Mash Tun.

| Grain | Flavor | Notes |
|-------|--------|-------|
| Corn | **Sweet** | Classic bourbon character; broad appeal |
| Rye | **Spicy** | Bold, complex; attracts Critic tourists |
| Wheat | **Smooth** | Approachable; increases sell speed |
| Barley | **Rich** | Deep malt character; premium sell price |

**Apple flavor** is a special sub-flavor unlocked by adding an Apple Orchard building
(Phase 2 crop). Apple mash blended with corn creates a **Sweet-Apple** flavor profile
that is highly appealing to Passerby and Enthusiast tourists.

```
  Flavor determines:
  ✓ Which tourist types preferentially visit
  ✓ Meme Score contribution (rare flavors = higher Meme Score)
  ✓ Seasonal event qualification
  ✓ Sell price modifier (+/−10%)
```

#### Flavor → Tourist Attraction Matrix

| Flavor | Passerby | Enthusiast | Critic | Celebrity | Journalist |
|--------|----------|------------|--------|-----------|------------|
| Sweet | ✅ +20% | ✅ +10% | ➖ | ➖ | ➖ |
| Sweet-Apple | ✅ +40% | ✅ +30% | ➖ | ✅ +10% | ➖ |
| Spicy | ➖ | ✅ +20% | ✅ +40% | ✅ +20% | ✅ +15% |
| Smooth | ✅ +30% | ✅ +20% | ➖ | ✅ +10% | ➖ |
| Rich | ➖ | ✅ +30% | ✅ +50% | ✅ +40% | ✅ +30% |

---

### 2. Quality (1–5 Stars ⭐)

Quality is a 1–5 integer score calculated at production time in the Rickhouse.

| Factor | Quality Contribution |
|--------|---------------------|
| Barrel type (Standard) | +1 base |
| Barrel type (Charred Oak) | +2 base |
| Barrel type (French Oak) | +3 base |
| Aging duration (standard 8s) | No bonus |
| Extended aging (×2 interval) | +1 |
| Critical Batch roll | +1 |
| Research: Master Blender | +1 |

Quality ranges:
- **1★ — Rough Cut**: Sell price −20%, no Critic tourists attracted
- **2★ — House Whiskey**: Sell price ×1 (base, $50)
- **3★ — Small Batch**: Sell price +25% ($62)
- **4★ — Select Reserve**: Sell price +60% ($80), Critic chance +20%
- **5★ — Master Distiller**: Sell price +100% ($100), all tourist types attracted, +3 Fame per sale

```csharp
// Quality calculation (WhiskeyPropertyManager.cs)
int quality = barrelQuality + agingBonus + criticalBonus + researchBonus;
quality = Mathf.Clamp(quality, 1, 5);
```

---

### 3. Temperature Profile (Hot / Warm / Cold)

Temperature represents the **alcohol proof and finish intensity** of the whiskey.

| Temperature | Proof Level | Character | Sell Effect | Town Effect |
|-------------|-------------|-----------|------------|------------|
| **Cold** | Low (70–80 proof) | Smooth, easy-drinking | +10% tourist appeal (broader audience) | More Passerby visits |
| **Warm** | Medium (86–92 proof) | Balanced | No modifier | Balanced tourist mix |
| **Hot** | High (100–120 proof) | Bold, intense | +20% sell price, fewer tourists | Critic/Celebrity tourists only |

**Setting Temperature**: The player sets the temperature profile at the Still via a
toggle on the SelectedBuildingUI (if Still is selected). Changing temperature takes
effect on the next production cycle.

**Seasonal Interactions**:
- Hot whiskey sells at ×1.5 price during Winter Warming event
- Cold whiskey sells at ×1.3 during Summer Harvest event

---

### 4. Quantity / Batch Size

Batch size is the number of Aged Whiskey units produced in a single Rickhouse cycle.

- **Standard Batch**: 1 unit (default)
- **Double Batch**: 2 units — requires 2× inputs, same timer (Research upgrade)
- **Triple Batch**: 3 units — requires 3× inputs, 1.5× timer (Research upgrade, late game)

**Effect on Player**:
- Larger batches mean fewer sell events needed to generate cash
- More units available for tourism-based consumption
- Larger batches do not change per-unit quality — each unit has the same property set

**Effect on Town**:
- Large batch availability means the Saloon "never runs out," allowing extended tourist visits
- Triggers the "Abundance" status: tourists spend +20% longer in Saloon when stock > 10 units

---

## WHAT DO THESE PROPERTIES DO? — SUMMARY

### For the Player

| Property | Sell Price Impact | Fame Impact | Strategy Value |
|----------|-----------------|-------------|----------------|
| Flavor (Sweet) | Base | +1/sale | Broadest tourist appeal; reliable income |
| Flavor (Spicy) | +15% | +2/sale | Premium pricing; needs Critic tourists |
| Flavor (Apple) | +10% | +2/sale | High Passerby volume; good for marketing campaigns |
| Flavor (Rich) | +25% | +3/sale | Highest single-unit value |
| Quality 1★ | −20% | +0 | Avoid; only happens if Research is ignored |
| Quality 3★ | +25% | +1/sale | Target for mid-game |
| Quality 5★ | +100% | +3/sale | Late-game goal; requires research investment |
| Temperature Hot | +20% | +1/sale | High-value niche market |
| Temperature Cold | no Δ | no Δ | Volume play; more tourists |
| Batch Size ×2 | no Δ/unit | +Δ/extra unit | Accelerates all economies |

### For the Town

Each batch sold affects the **Town Health** score (a hidden metric that drives cosmetic
changes and NPC dialogue):

| Condition | Town Change |
|-----------|-------------|
| 5★ quality sold ×5 | New "Award-Winning Distillery" banner decoration |
| Apple whiskey sold ×10 | Apple tree decorations appear around Saloon |
| Hot whiskey sold ×20 | "Fire & Spirits" sign appears on Saloon |
| Quality avg. > 3★ sustained for 50 batches | Town upgrades to "Renowned Distillery" status (more tourists baseline) |
| Smooth/Cold ×30 | "Friendly Welcome" sign; Passerby visit rate +10% permanent |

---

## IMPLEMENTATION

### Scripts Required

- **WhiskeyProperties.cs** — Data class (flavor enum, quality int, temperature enum, batchSize int)
- **WhiskeyPropertyManager.cs** — Singleton: manages current batch properties, effects on sell price / fame / tourist attraction; integrates with SellManager and FameMilestoneManager

### Data Flow

```
  Player selects grain type at Mash Tun
           │
           ▼
  WhiskeyPropertyManager records FlavorProfile
           │
           ▼
  Player sets Temperature at Still (toggle)
           │
           ▼
  Rickhouse produces: WhiskeyPropertyManager.CalculateQuality()
           │
           ▼
  AgedWhiskey added with property tag
           │
           ▼
  SellManager queries WhiskeyPropertyManager for sell price multiplier
           │
           ▼
  FameMilestoneManager queries for fame modifier
           │
           ▼
  TouristSpawner (Phase 3) queries for attraction weights
```

### Save Data

Add to `GameData.cs`:
```csharp
// Whiskey Properties
public int currentFlavorProfile;   // enum cast to int
public int currentQuality;
public int currentTemperatureProfile; // enum cast to int
public int currentBatchSize;
```

---

## UI INTEGRATION

- **Still SelectedBuildingUI**: Temperature toggle (Cold / Warm / Hot)
- **Mash Tun SelectedBuildingUI**: Recipe selector (grain type, if multiple crops unlocked)
- **Saloon / Sell Panel**: Show active batch properties before selling:
  ```
    Current Batch: 3★ Small Batch | Spicy | Warm
    Sell Price: $62/bottle (+25%)
    Expected Fame: +2 per sale
  ```
- **HUD Indicator**: Small flavor/quality badge near AgedWhiskey counter

---

## RESEARCH UPGRADES

| Research Node | Cost | Effect | Prerequisite |
|--------------|------|--------|-------------|
| Quality Basics | 5 Research Pts | Enables quality calculation (2★ default without research) | None |
| Temperature Control | 8 Research Pts | Unlocks temperature toggle at Still | None |
| Flavor Mastery | 12 Research Pts | Flavor bonuses increased by 50% | Quality Basics |
| Double Batch | 15 Research Pts | Unlocks ×2 batch size at Rickhouse | Quality Basics |
| Master Blender | 20 Research Pts | +1 quality on all future batches | Flavor Mastery |
| Triple Batch | 30 Research Pts | Unlocks ×3 batch size at Rickhouse | Double Batch |
| Apple Orchard | 25 Research Pts | Unlocks Apple Orchard crop building | Flavor Mastery |
