# 📍 Local Marketing — In-Game Marketing Mechanics for Whiskeyville
### Research Folder | Internal Design Reference
### Updated: March 2026

---

## CONCEPT

Local Marketing represents the player's active investment in attracting customers to
their distillery. Unlike Memetic Research (passive spread) or Fame (reputation score),
Local Marketing is **paid, targeted, and immediate** — spend money or resources now,
get more customers tonight.

In a real small-town distillery, local marketing means flyers at the hardware store,
a sign on the highway, a discount night, or a partnership with the local diner.
In Whiskeyville, these translate to purchasable **Marketing Campaigns** that boost
tourist traffic for a defined window.

---

## THE MARKETING LOOP

```
  Player has Cash surplus
           │
           ▼
  Opens Marketing Panel (M key / HUD button)
           │
           ▼
  Selects Campaign type + duration
           │
           ▼
  Cash deducted immediately
           │
           ▼
  Campaign active: Tourist spawn rate increases
           │
           ▼
  More tourists → More Aged Whiskey sold → More Cash + Fame
           │
           ▼
  ROI: Did the campaign pay for itself?
```

---

## MARKETING CAMPAIGNS

### Tier 1 — Grassroots (Unlocked: 0 Fame)

| Campaign | Cost | Duration | Effect | Notes |
|----------|------|----------|--------|-------|
| Chalkboard Sign | $50 | 5 min | +20% tourist spawn | Visible sign placed on Saloon |
| Word on the Street | $75 | 8 min | +15% spawn, tourists stay longer (+25% spend time) | NPCs chat longer |
| Flyer Drop | $100 | 10 min | +30% tourist spawn | Paper litter particle effect |

### Tier 2 — Local Reach (Unlocked: 100 Fame)

| Campaign | Cost | Duration | Effect | Notes |
|----------|------|----------|--------|-------|
| Radio Spot | $250 | 15 min | +50% tourist spawn + unlocks Enthusiast type | Faint radio audio |
| Newspaper Ad | $300 | 20 min | +40% spawn + +2 Fame per sale during window | Newspaper prop appears |
| Discount Night | $200 | 10 min | ×2 tourist count but sell price −20% | Volume play |

### Tier 3 — Regional (Unlocked: 500 Fame)

| Campaign | Cost | Duration | Effect | Notes |
|----------|------|----------|--------|-------|
| Highway Billboard | $750 | 30 min | +100% tourist spawn + Critic tourist chance | Billboard appears on map edge |
| Food Festival Booth | $1,000 | 45 min | +150% spawn, all tourists buy 2 units | Major Cash/Fame event |
| Influencer Visit | $1,500 | 60 min | Guaranteed Celebrity tourist + Viral Batch chance | Rare visitor type |

---

## ROI CALCULATOR

Every campaign shows an estimated ROI before purchase based on current sell price,
tourist spend rate, and batch Meme Score. This teaches players to think economically:

```
  Campaign: Radio Spot ($250, 15 min)
  ┌──────────────────────────────────┐
  │ Estimated tourists: 18–24        │
  │ Avg. spend per tourist: 1–2 units│
  │ Current sell price: $50/bottle   │
  │ Estimated revenue: $900–$2,400   │
  │ Net estimated ROI: +$650–$2,150  │
  │ Confidence: Medium               │
  └──────────────────────────────────┘
  [Run Campaign]  [Cancel]
```

Actual results vary based on real-time tourist behavior and Meme Score.

---

## MARKETING EFFECTS ON THE TOWN

Active campaigns have **visible world-space effects** to reinforce immersion:

| Campaign | World Effect |
|----------|-------------|
| Chalkboard Sign | Chalkboard sign prop appears on Saloon exterior |
| Flyer Drop | Paper flyer particles drift across the scene for 10 seconds |
| Radio Spot | Faint country radio music plays (different from background track) |
| Newspaper Ad | Newspaper stack prop spawns near Saloon entrance |
| Highway Billboard | Billboard sprite appears at scene edge |
| Food Festival Booth | Booth decoration spawns outside Saloon, 2× tourist density |

---

## MARKETING SYNERGIES

Campaigns stack additively (up to a cap of 300% spawn rate to avoid performance issues).
Certain combinations unlock bonus effects:

| Combination | Bonus |
|-------------|-------|
| Flyer Drop + Radio Spot | "Double Coverage" — +10% extra spawn on top of both |
| Newspaper Ad + Billboard | "Regional Splash" — Journalist NPC guaranteed within 10 min |
| Discount Night + Food Festival | "Blow-Out Night" — ×3 tourist count, sell price −10% (better than discount alone) |

---

## RESEARCH UPGRADES (ResearchManager)

Unlock via Local Marketing research node in the Research Lab.

| Research Node | Cost | Effect | Prerequisite |
|--------------|------|--------|-------------|
| Marketing Basics | 5 Research Pts | Unlocks Tier 1 campaigns | None |
| Analytics | 8 Research Pts | ROI calculator shows tighter confidence band | Marketing Basics |
| Loyal Regulars | 12 Research Pts | Past tourists have 20% chance to return without a campaign | Marketing Basics |
| Tier 2 Reach | 15 Research Pts | Unlocks Tier 2 campaigns | Marketing Basics |
| Campaign Stacking | 20 Research Pts | Stack cap raised from 300% to 500% | Tier 2 Reach |
| Tier 3 Regional | 30 Research Pts | Unlocks Tier 3 campaigns | Tier 2 Reach |

---

## SAVE & PERSISTENCE

Active campaign state is saved per-session:
- `activeCampaignId` (string) — which campaign is running, if any
- `campaignEndTime` (float) — game time when campaign expires
- `campaignStackCount` (int) — number of currently stacked campaigns

On load, if `campaignEndTime` is in the past, the campaign is silently expired.

---

## UI INTEGRATION

- **HUD Button**: "M" key or Marketing icon in HUD opens Marketing Panel
- **Active Campaign Banner**: Visible below resources HUD when a campaign is active
  - Shows campaign name, time remaining (countdown), and current spawn-rate bonus
- **Campaign History Log**: Last 5 campaigns shown with actual vs. estimated ROI

---

## INTEGRATION POINTS

- **TouristSpawner.cs** (Phase 3) — Reads `MarketingManager.ActiveSpawnMultiplier`
- **SellManager.cs** — Applies `MarketingManager.ActiveSellPriceMultiplier`
- **UIManager.cs** — `ToggleMarketingPanel()` method (add alongside ToggleSellPanel)
- **SaveManager.cs / GameData.cs** — Persist active campaign state
- **ResearchManager.cs** — Checks research tier before allowing campaign purchase
