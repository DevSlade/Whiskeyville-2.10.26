# 🔬 Research System — Research Lab, Tree & Upgrades
### Phase 2 Feature Spec | Priority: Post-MVP
### Target: Q2 2026
### Requires Building: Research Lab

---

## CONCEPT

The Research System gives players a way to **permanently improve** their distillery
through knowledge and expertise rather than just production throughput. Unlike Cash
(which buys buildings) or Fame (which unlocks content tiers), **Research Points** are
earned by producing batches and are spent in the Research Lab to unlock active upgrades
across five research categories.

```
  Cash     → Buy buildings (infrastructure)
  Fame     → Unlock new content tiers
  Research → Permanently improve how everything works
```

The Research Lab is a Phase 2 building that becomes available at 100 Fame. Once placed,
it opens the Research Panel where players can browse and purchase research upgrades.

---

## THE RESEARCH LAB BUILDING

| Property | Value |
|----------|-------|
| Building Name | Research Lab |
| Cost | $500 Cash |
| Unlock Requirement | 100 Fame |
| Size | 2×2 tiles |
| Function | Grants Research Points over time; opens Research Panel on click |
| Research Points Rate | 1 RP every 60 seconds (passive, while Lab is placed) |
| Prefab | ResearchLab_Prefab |
| Popup Color | Purple (RGBA: 0.58, 0.22, 0.85, 1) |

**Research Points (RP)** are also earned by:
- Completing each full production cycle in a Rickhouse: +1 RP
- Selling a full batch via Saloon: +1 RP per 5 units sold (rounded down)
- Achieving a Critical Batch: +3 RP

---

## RESEARCH CATEGORIES

Research is organized into five trees, each mapping to a key game system:

| Category | Icon | Focus | Max Nodes | Total RP Cost |
|----------|------|-------|-----------|--------------|
| Whiskey Mastery | ⭐ | Quality, flavor, batch size | 7 | 110 RP |
| Bottle Crafting | 🍾 | Glass types, label bonuses, Meme Score | 6 | 90 RP |
| Local Marketing | 📍 | Campaign tiers, ROI, loyalty | 6 | 90 RP |
| Financial Outlook | 📊 | Ledger, bottlenecks, projections | 5 | 63 RP |
| Retention & Growth | 🔁 | Session summary, dailies, social | 6 | 63 RP |

---

## RESEARCH TREES

### ⭐ Whiskey Mastery Tree

| Node | Cost (RP) | Effect | Prerequisite |
|------|-----------|--------|-------------|
| Quality Basics | 5 | Enables quality calculation (all whiskey now rated 1–5★) | None |
| Temperature Control | 8 | Unlocks Hot/Warm/Cold toggle at Still | None |
| Flavor Mastery | 12 | Flavor sell price bonuses increased by 50% | Quality Basics |
| Double Batch | 15 | Rickhouse can produce 2 units per cycle (uses 2× inputs) | Quality Basics |
| Master Blender | 20 | +1 quality on all future batches | Flavor Mastery |
| Apple Orchard | 25 | Unlocks Apple Orchard crop building (Sweet-Apple flavor) | Flavor Mastery |
| Triple Batch | 30 | Rickhouse can produce 3 units per cycle (uses 3× inputs) | Double Batch |

### 🍾 Bottle Crafting Tree

| Node | Cost (RP) | Effect | Prerequisite |
|------|-----------|--------|-------------|
| Label Basics | 5 | Unlocks label color options in Bottle Editor | None |
| Glass Variety | 10 | Unlocks Hip Flask and Ceramic Jug glass types | None |
| Emblem Library | 12 | Unlocks 3 additional emblems (Wheat Sheaf, Oak Barrel, Running Fox) | Label Basics |
| Premium Glass | 20 | Unlocks Tall Decanter and Personalized Crock | Glass Variety |
| Meme Boost | 15 | All bottle customization Meme Score bonuses +50% | Label Basics + Emblem Library |
| Crystal Tier | 28 | Unlocks Crystal Decanter (requires 1000 Fame also) | Premium Glass + Meme Boost |

### 📍 Local Marketing Tree

| Node | Cost (RP) | Effect | Prerequisite |
|------|-----------|--------|-------------|
| Marketing Basics | 5 | Unlocks Tier 1 marketing campaigns | None |
| Analytics | 8 | ROI calculator in Marketing Panel shows tighter estimates | Marketing Basics |
| Loyal Regulars | 12 | Past tourists have 20% chance to return without a campaign | Marketing Basics |
| Tier 2 Reach | 15 | Unlocks Tier 2 marketing campaigns | Marketing Basics |
| Campaign Stacking | 20 | Campaign spawn rate cap raised from 300% to 500% | Tier 2 Reach |
| Tier 3 Regional | 30 | Unlocks Tier 3 marketing campaigns (Billboard, Festival, Influencer) | Tier 2 Reach |

### 📊 Financial Outlook Tree

| Node | Cost (RP) | Effect | Prerequisite |
|------|-----------|--------|-------------|
| Ledger Basics | 5 | Unlocks Ledger Panel (L key); shows current rates + balance | None |
| Bottleneck Alerts | 8 | Buildings display red/orange when input-starved | Ledger Basics |
| Projections | 12 | 30-min cash/resource projections shown in Ledger | Ledger Basics |
| ROI Display | 15 | Build Panel shows break-even estimate for new buildings | Bottleneck Alerts |
| Full Outlook | 23 | 7-day + 30-day scenario projections in Ledger | Projections + ROI Display |

### 🔁 Retention & Growth Tree

| Node | Cost (RP) | Effect | Prerequisite |
|------|-----------|--------|-------------|
| Session Summary | 3 | End-of-session summary card on pause/quit | None |
| Daily Bonuses | 5 | Activates daily barrel streak system | None |
| Narrative Postcards | 8 | Town story postcards appear every 5 in-game days | None |
| Weekly Events | 12 | Unlocks weekly challenge event schedule | Daily Bonuses |
| Offline Retention | 15 | Enables push notification for full inventory (mobile) | None |
| Social Export | 20 | Enables shareable label screenshot export | Narrative Postcards |

---

## RESEARCH PANEL UI

### Layout

```
  ╔════════════════════════════════════════════╗
  ║  🔬 Research Lab                            ║
  ║  Research Points: 47 RP                     ║
  ╠══════════════════╦═════════════════════════╣
  ║  CATEGORIES      ║  TREE VIEW               ║
  ║                  ║                          ║
  ║  ⭐ Whiskey     ║  [Quality Basics ✅]      ║
  ║  🍾 Bottle      ║       │                   ║
  ║  📍 Marketing   ║  [Flavor Mastery ✅]      ║
  ║  📊 Financial   ║       │                   ║
  ║  🔁 Retention   ║  [Master Blender]         ║
  ║                  ║  Cost: 20 RP  [Purchase] ║
  ║                  ║       │                   ║
  ║                  ║  [Apple Orchard 🔒]       ║
  ║                  ║  Cost: 25 RP              ║
  ╚══════════════════╩═════════════════════════╝
```

### Behavior
- **Categories**: Clicking a category tab shows its research tree on the right
- **Completed nodes**: Shown with ✅ check mark, grayed out
- **Available nodes**: Highlighted, cost shown, [Purchase] button active
- **Locked nodes**: 🔒 shown with prerequisite listed; [Purchase] button disabled
- **Hover tooltips**: Describe the exact effect of each research node
- **Research Points counter**: Updates live in panel header

---

## RESEARCH POINTS — EARN RATES

| Activity | RP Earned |
|----------|----------|
| Research Lab passive (per 60s) | +1 RP |
| Rickhouse cycle completed | +1 RP |
| Sell 5 Aged Whiskey (Saloon) | +1 RP |
| Critical Batch triggered | +3 RP |
| Tourist purchases (per 3 tourists) | +1 RP |
| Daily bonus (with Daily Bonuses research) | +5 RP |
| Weekly event completed | +10 RP |

---

## IMPLEMENTATION

### Scripts Required

- **ResearchData.cs** — ScriptableObject defining a single research node:
  `nodeId, nodeName, category, cost, description, effectType, effectValue, prerequisiteNodeId`
- **ResearchManager.cs** — Singleton:
  - Tracks purchased nodes in `HashSet<string> _unlockedNodes`
  - Tracks Research Points balance
  - `CanPurchase(string nodeId)` → bool
  - `PurchaseResearch(string nodeId)` → bool
  - `HasResearch(string nodeId)` → bool (queried by other systems)
  - `GetResearchMultiplier(string effectKey)` → float
  - Fires `OnResearchUnlocked(string nodeId)` event

### Save Data

Add to `GameData.cs`:
```csharp
// Research
public int researchPoints;
public List<string> unlockedResearch;
```

### New BuildingData Asset Required

`ResearchLabData.asset`:
```yaml
buildingName: ResearchLab
cost: 500
isCrop: false
outputResource: ResearchPoints
outputAmount: 1
productionInterval: 60
requiresInput: false
popupColor: Purple (RGBA: 0.58, 0.22, 0.85, 1)
```

---

## INTEGRATION POINTS

- **WhiskeyPropertyManager.cs** — Queries `ResearchManager.HasResearch("quality_basics")` before applying quality; reads `GetResearchMultiplier("flavor_bonus")`
- **BottleCustomizationManager.cs** — Queries research for unlocked glass types and Meme Score multipliers
- **SellManager.cs** — Queries research for sell price research multipliers
- **MarketingManager.cs** (Phase 2) — Queries research tier for available campaign types
- **LedgerManager.cs** (Phase 2) — Queries research for Ledger Panel availability
- **UIManager.cs** — `ToggleResearchPanel()` method; opens when Research Lab clicked
- **BuildingBehavior.cs** — `ResearchLab` building name triggers `UIManager.ToggleResearchPanel()` in OnMouseDown
- **SaveManager.cs** — Persists `researchPoints` and `unlockedResearch` list in GameData

---

## PRESTIGE INTERACTION

During Prestige reset (Phase 4):
- Research Points: Reset to 0
- Unlocked Research: All lost (incentivizes different research paths on repeat runs)
- **Exception**: One selected research node can be carried over (Prestige perk)
