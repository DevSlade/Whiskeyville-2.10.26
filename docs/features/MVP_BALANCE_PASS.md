# ⚖️ Feature Spec: Balance Pass (Economy Tuning)
### Phase 1 — MVP Polish | Priority: 🟡 Medium
### Estimated Time: 20–30 minutes | Target MVP Gain: +1%

---

## PROBLEM

The game's economy was set up for development testing, not player enjoyment. Production intervals
and cash values are likely either too fast (no sense of accomplishment), too slow (boring
waiting), or unbalanced relative to building costs.

A balanced economy should create a **satisfying loop rhythm**: plant → wait → harvest →
watch production chain fire → sell → immediately have enough to expand.

---

## CURRENT VALUES (as of Feb 18, 2026)

| Building | Cost | Input | Output | Interval | Notes |
|---------|------|-------|--------|----------|-------|
| Field | 25 Cash | — | 1 Corn | 4 stages (~20s?) | Estimate |
| Mash Tun | 75 Cash | 1 Corn | 1 Mash | 5s | Per `BuildingBehavior` default |
| Still | 150 Cash | 1 Mash | 1 Whiskey | 5s | Per `BuildingBehavior` default |
| Cooperage | — | 1 Wood | 1 Barrel | 5s | Estimate |
| Rickhouse | 250 Cash | 1 Whiskey + 1 Barrel | 1 Aged Whiskey | 5s | Estimate |
| Saloon | — | 1 Aged Whiskey | 50 Cash | On-click | Manual sell |

---

## TARGET ECONOMY FEEL

The player starts with **200 Cash**. The session goal for the first 5 minutes should be:
1. Place a Field (25 Cash left: 175)
2. Place a Mash Tun (100 Cash left)
3. First corn harvest → first mash → place a Still (50 Cash left after a few sells)
4. Feel the dopamine of the chain firing and cash climbing
5. Save up for Cooperage + Rickhouse in 10–15 minutes of play

**Tuning target:** Player should be able to place all 6 buildings within 20 minutes of first play.

---

## PROPOSED TUNED VALUES

| Building | Cost | Sell Value | Production Time | Ratio |
|---------|------|-----------|-----------------|-------|
| Field | 25 Cash | — | 15s per crop growth stage (60s full cycle) | — |
| Mash Tun | 75 Cash | — | 8s per unit | 3:1 crop-to-mash ratio intent |
| Still | 150 Cash | — | 10s per unit | — |
| Cooperage | 100 Cash | — | 6s per barrel | — |
| Rickhouse | 250 Cash | — | 30s per aged whiskey | Aging takes time — feels premium |
| Saloon | 50 Cash | 50 Cash / Aged | On-click | — |

**Net: One full Corn → Aged Whiskey cycle ≈ ~2 minutes.**
**With 3 Fields, 2 Mash Tuns, 1 Still, 1 Cooperage, 1 Rickhouse = ~1 Aged Whiskey / 45 seconds.**

---

## BALANCE LEVERS (per `BuildingData.asset`)

All values are in ScriptableObjects — no code changes required for tuning:

```
  BuildingData fields:
  ├── buildingCost      → Cash to place
  ├── productionInterval → Seconds between producing
  ├── outputAmount       → Units produced per tick
  └── inputAmount        → Units consumed per tick
```

**Change these in the Inspector, not in code.**

---

## CASH FLOW SIMULATION

```
  STARTING CASH: 200

  T=0:   Place Field (25)         → 175 Cash
  T=60:  Harvest 1 Corn
  T=68:  Mash Tun produces 1 Mash (requires Mash Tun placed)
         Place Mash Tun (75)      → 100 Cash
  T=78:  Still produces 1 Whiskey (requires Still placed)
         Place Still (150)        → deficit — need first sells first!

  FIRST SELL PATH:
  T=0:   Start with DevTools to add Corn in first run (tutorial path)
  OR: Starting cash = 500 (tutorial-only value, then normalize)
  OR: Reduce Still cost to 75 Cash for first purchase
```

**Recommendation:** Raise starting cash to **300** for a less frustrating opening.
Or: Give player **50 Corn + 1 free Field** placed at game start as a tutorial gift.

---

## ACCEPTANCE CRITERIA

- [ ] A new player can place their first Mash Tun within 2 minutes of starting
- [ ] A new player can complete the full chain in one ~20-minute session
- [ ] No building feels "unreachable" given normal play rhythm
- [ ] Selling Aged Whiskey feels meaningful (not pennies, not game-breaking)
- [ ] Production chain does not bottleneck — Mash supply keeps up with Still demand

---

## QoL BALANCE FEATURES

These make balance feel better without changing numbers:
- [ ] **Overflow indicator** — "Mash is capping!" if inventory hits 999
- [ ] **Production efficiency hint** — "Your Mash Tun is idle (no Corn)" in tooltip
- [ ] **Time-to-fill estimate** — "Next batch: ~30s" under building

---

## HOW IT CONNECTS TO THE FUTURE

```
  Balance Pass (Phase 1)
       │
       ├── Phase 2: Multiple whiskey quality tiers → higher sell prices
       ├── Phase 2: Crop variety → different production ratios
       ├── Phase 3: Seasonal demand spikes (Sell for 2× during Harvest Festival)
       └── Phase 4: Prestige multipliers on production speed/value
```

---

## ESTIMATED TIMELINE

| Task | Time |
|------|------|
| Spreadsheet simulation of cash flow | 10 min |
| Adjust values in BuildingData assets (Inspector) | 10 min |
| Playtest full loop twice with new values | 10 min |
| **Total** | **~30 min** |
