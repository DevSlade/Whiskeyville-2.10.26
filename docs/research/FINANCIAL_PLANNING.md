# 📊 Financial Planning — Outlook & Potential Outcomes for Whiskeyville
### Research Folder | Internal Design Reference
### Updated: March 2026

---

## PURPOSE

This document serves two functions:

1. **In-game financial mechanics** — giving the player tools to plan and project their
   distillery's cash flow, production efficiency, and growth trajectory
2. **Developer economic balance reference** — the tuning targets that keep the economy
   feeling fair and progressively rewarding

---

## PART A: IN-GAME FINANCIAL PLANNING TOOLS

### The Ledger Panel

A new **Ledger Panel** (accessible via HUD "Book" icon or L key) gives the player a
real-time financial dashboard:

```
  ╔══════════════════════════════════════════╗
  ║  🥃 Whiskeyville Distillery Ledger       ║
  ╠══════════════════════════════════════════╣
  ║  CURRENT BALANCE                         ║
  ║  Cash:              $3,450               ║
  ║  Aged Whiskey:      12 bottles           ║
  ║  Projected sale value: $600              ║
  ║  Estimated net worth: $4,050             ║
  ╠══════════════════════════════════════════╣
  ║  PRODUCTION RATE (last 5 min average)    ║
  ║  Corn/min:          2.4                  ║
  ║  Mash/min:          1.1                  ║
  ║  Whiskey/min:       0.5                  ║
  ║  Aged Whiskey/min:  0.4                  ║
  ║  Cash/min (selling): $20/min             ║
  ╠══════════════════════════════════════════╣
  ║  BOTTLENECK ALERT                        ║
  ║  ⚠ Rickhouse is input-starved            ║
  ║  (Whiskey arrives 40% slower than cap)   ║
  ╠══════════════════════════════════════════╣
  ║  30-MIN PROJECTION (if no changes)       ║
  ║  Estimated cash: $4,050 → $4,650         ║
  ║  Aged Whiskey: 12 → 24 bottles           ║
  ║  Next milestone: 18 Fame needed          ║
  ╚══════════════════════════════════════════╝
```

### Bottleneck Detection

The system tracks each resource's production rate and flags when a building in the chain
is receiving inputs slower than its rated capacity. This teaches players supply-chain
optimization without lecturing them:

| Status | Condition | Display |
|--------|-----------|---------|
| ✅ Optimal | Input rate ≥ 90% of building capacity | Green indicator |
| ⚠ Starved   | Input rate 50–89% of capacity         | Orange warning |
| ❌ Bottleneck | Input rate < 50% of capacity         | Red alert + suggestion tooltip |

Tooltip example: "Your Still is only running at 45% capacity. Try adding another Mash Tun."

---

## PART B: POTENTIAL OUTCOMES — PRODUCTION SCENARIOS

### Scenario A: Minimal Setup (3 Buildings)
*Player has: 1 Field + 1 Mash Tun + 1 Still*

| Metric | Value |
|--------|-------|
| Corn/min | 0.2 (harvest every 5s, 1 field) |
| Mash/min | 0.08 (needs 2 corn, 8s cycle; corn bottleneck) |
| Whiskey/min | 0.04 (needs 2 mash, 10s cycle) |
| Cash/min | ~$0 (no Rickhouse or Saloon yet) |
| Time to complete chain | ~25 min to place Rickhouse + Saloon |

**Bottleneck**: Field is severely undersupplied. Player needs 2–3 fields minimum.

---

### Scenario B: Balanced Starter (6 Buildings)
*Player has: 3 Fields + 1 Mash Tun + 1 Still + 1 Cooperage + 1 Rickhouse + 1 Saloon*

| Metric | Value |
|--------|-------|
| Corn/min | 0.6 (3 fields × 0.2/min) |
| Mash/min | 0.19 (Mash Tun runs at ~63% capacity) |
| Whiskey/min | 0.10 |
| Barrel/min | 0.17 (Cooperage: 2 Wood → 1 Barrel per 5s = depends on Wood supply) |
| Aged Whiskey/min | ~0.08 (bottleneck: Whiskey arrival rate) |
| Cash/min (sell all every 5 min) | ~$20/min |
| Fame/min | ~0.08 fame/min |

**Time to 100 Fame**: ~20 hours of play (with some idle time)

---

### Scenario C: Optimized Chain (12+ Buildings)
*Player has: 6 Fields + 2 Mash Tuns + 2 Stills + 2 Cooperages + 3 Rickhouses + 1 Saloon*

| Metric | Value |
|--------|-------|
| Corn/min | 1.2 |
| Mash/min | 0.38 (2 Mash Tuns running at ~63% capacity) |
| Whiskey/min | 0.20 |
| Barrel/min | 0.34 (2 Cooperages) |
| Aged Whiskey/min | 0.18 (3 Rickhouses, Whiskey bottleneck) |
| Cash/min | ~$54/min |
| Fame/min | ~0.18 fame/min |

**Time to 1000 Fame**: ~93 hours of play (with active selling)
**Time to 1000 Fame** (with Marketing campaigns): ~55 hours

---

### Scenario D: Late-Game Power Setup (Phase 2+)
*Assumes: multiple crop types, Tasting Room, Export Dock (Phase 2 buildings), Prestige bonus ×1.25*

| Metric | Estimated Value |
|--------|----------------|
| Aged Whiskey/min | 0.5–0.8 |
| Cash/min | $150–250/min |
| Fame/min | 0.5–1.0 fame/min |
| Time to Prestige (5000 Fame) | 15–20 hours (with Late Game optimizations) |

---

## PART C: DEVELOPER BALANCE TARGETS

### Economy Balance Goals

| Goal | Target |
|------|--------|
| Player can place all 6 MVP buildings within 20 min | Starting cash: $300, building costs balanced |
| Full Corn→Aged Whiskey cycle in ~2 minutes | Adjust production intervals per balance pass |
| First "feel good sell" happens in session 1 | Ensure AgedWhiskey available within 10 min |
| Cash never feels like an impenetrable wall | Adjacent building always within 3–5 min of farming |

### Building Cost vs. Production ROI

| Building | Cost | Revenue Impact | Break-Even (at $50/bottle) |
|----------|------|---------------|--------------------------|
| Field | $50 | +0.2 Corn/min | ~2 min (provides chain starter) |
| Mash Tun | $75 | Chain enabler | ~5 min chain contribution |
| Still | $150 | Chain enabler | ~8 min chain contribution |
| Cooperage | $100 | Chain enabler | ~6 min chain contribution |
| Rickhouse | $250 | +0.05 AW/min → +$2.5/min | ~100 min break-even |
| Saloon | $300 | Unlocks selling | Immediate ROI on first sell |

**Finding**: Rickhouse has the longest break-even. Consider reducing cost to $200 or
increasing AgedWhiskey production rate slightly for better early-game feel.

---

## PART D: FINANCIAL PROJECTIONS BY PLAY-STYLE

### Casual Player (20 min/day)
- **Month 1**: Completes MVP chain, earns ~500 Fame, unlocks Tasting Room
- **Month 3**: Reaches Prestige-ready state (5000 Fame), first reset
- **Month 6**: 2–3 Prestige completions, late-game buildings unlocked

### Moderate Player (1 hr/day)
- **Week 1**: Full optimized chain, 250 Fame, first tourist type unlocked
- **Month 1**: First Prestige, ~3000 Fame milestone on second run
- **Month 3**: Leaderboard competitive, late-game content explored

### Hardcore Player (3+ hr/day)
- **Day 3**: First Prestige possible
- **Week 2**: All Phase 2 content unlocked
- **Month 1**: Awaiting Phase 3 content (tourists, events)

---

## RESEARCH UPGRADES (ResearchManager)

Unlock via Financial Planning research node in the Research Lab.

| Research Node | Cost | Effect | Prerequisite |
|--------------|------|--------|-------------|
| Ledger Basics | 5 Research Pts | Unlocks Ledger Panel (L key) | None |
| Bottleneck Alerts | 8 Research Pts | Red/orange building indicators when starved | Ledger Basics |
| Projections | 12 Research Pts | 30-min cash/resource projections in Ledger | Ledger Basics |
| ROI Display | 15 Research Pts | Break-even estimator for new buildings in Build Panel | Bottleneck Alerts |
| Full Outlook | 25 Research Pts | 7-day and 30-day outlook with multiple scenarios | Projections + ROI Display |

---

## INTEGRATION POINTS

- **InventoryManager.cs** — Reads resource rates for Ledger calculations
- **BuildingBehavior.cs** — Reports actual production rate per building
- **SellManager.cs** — Reports sell history for cash/min calculation
- **UIManager.cs** — `ToggleLedgerPanel()` method
- **SaveManager.cs / GameData.cs** — Persist Ledger visibility preference
- **ResearchManager.cs** — Gate Ledger Panel behind Financial Planning research node
