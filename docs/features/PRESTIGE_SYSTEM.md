# 🏆 Feature Spec: Prestige System (New Game Plus)
### Phase 4 — Prestige & Meta | Priority: Post-MVP
### Target: Q4 2026

---

## CONCEPT

Prestige is a **voluntary reset** of your town. You give up everything you've built in
exchange for permanent bonuses that make your next run faster, deeper, and more powerful.
It's the long-term replayability loop.

```
  WITHOUT PRESTIGE:  I've built everything. I've sold 10,000 whiskeys. Now what?
  WITH PRESTIGE:     I reset. I'm legendary. My second town is twice as good.
                     I get to experience the whole game again — but better.
```

---

## PRESTIGE LOOP

```
  Player reaches 5,000 Fame
          │
          ▼
  "PRESTIGE AVAILABLE" notification fires
          │
          ▼
  Player chooses: [PRESTIGE NOW] or [KEEP PLAYING]
          │
          ▼
  Prestige Reset:
  - All buildings removed
  - Cash reset to 200
  - Resources reset to 0
  - Fame reset to 0
  - KEPT: Prestige Points, Prestige Perks, Prestige Trophy decorations
          │
          ▼
  Player starts new run with selected Prestige Perks
  (Production is faster, costs are lower, tourists arrive sooner...)
          │
          ▼
  Reach 5,000 Fame again (faster this time) → Prestige again
          │
          ▼
  Repeat — each run shorter and more optimized
```

---

## PRESTIGE POINTS (PP)

Each prestige earns **Prestige Points** based on performance:

| Achievement | PP Earned |
|------------|----------|
| Completed in under 7 days | +5 PP |
| Reached 10,000 Cash | +3 PP |
| Hosted 100+ tourists | +3 PP |
| Completed all NPC quests | +5 PP |
| Base prestige completion | +10 PP |

---

## PRESTIGE PERK TREE

Spend PP to buy permanent bonuses that carry across ALL future runs:

```
  ┌─────────────────────────────────────────────────────────────────┐
  │                    PRESTIGE PERK TREE                           │
  │                                                                 │
  │  TIER 1 (1 PP each):                                            │
  │  [+10% production speed]  [Start with 300 Cash]                 │
  │  [Tourists arrive at 100 Fame]  [Trees respawn 2× faster]       │
  │                                                                 │
  │  TIER 2 (3 PP each, requires 2 Tier 1 perks):                   │
  │  [+25% sell price]  [Rickhouse capacity +5]                     │
  │  [Unlocks carry over to new run]  [NPC quest rewards ×2]        │
  │                                                                 │
  │  TIER 3 (5 PP each, requires 3 Tier 2 perks):                   │
  │  [Start with 1 free building]  [+50% Fame from all sources]     │
  │  [Celebrity tourist from day 1]  [Double crop yield]            │
  │                                                                 │
  │  LEGENDARY (10 PP, requires all Tier 3):                        │
  │  [New crop: Moonshine Corn — 5× value]                          │
  │  [The "Black Label" title — shows in town name]                  │
  └─────────────────────────────────────────────────────────────────┘
```

---

## PRESTIGE TROPHIES

Decorations earned from prestige runs that persist across resets. Show off your history:

| Trophy | Earned From | Notes |
|--------|------------|-------|
| Gold Barrel | First prestige | Permanent decoration |
| Aged Oak Plaque | 5,000 Fame run | Shows run number |
| Legend's Portrait | 3rd prestige | NPC portrait on wall |
| Black Label Sign | Legendary perk | Glows at night |

---

## PRESTIGE CONFIRM UX

Before resetting, show the player what they're trading for what:

```
  ┌────────────────────────────────────────────────────────────────┐
  │  🏆  PRESTIGE AVAILABLE                                        │
  │                                                                │
  │  You've built a legend. Are you ready to start over?          │
  │                                                                │
  │  YOU WILL LOSE:                      YOU WILL KEEP:           │
  │  ✗ All buildings                     ✓ Your trophies          │
  │  ✗ All cash & resources              ✓ Your Prestige Points   │
  │  ✗ Current Fame total                ✓ Your perks (equipped)  │
  │                                      ✓ This memory            │
  │                                                                │
  │  You'll earn: 10 PP + bonuses                                  │
  │                                                                │
  │         [ START PRESTIGE RUN ]    [ NOT YET ]                 │
  └────────────────────────────────────────────────────────────────┘
```

---

## TECHNICAL IMPLEMENTATION

### `PrestigeManager.cs` (New)
```csharp
public class PrestigeManager : MonoBehaviour
{
    public static PrestigeManager Instance { get; private set; }

    public int PrestigeLevel { get; private set; }
    public int PrestigePoints { get; private set; }
    public List<string> UnlockedPerks { get; private set; }

    public void TriggerPrestige()
    {
        // 1. Calculate PP earned
        // 2. Save prestige data (survives reset)
        // 3. Clear GameData (buildings, resources, fame)
        // 4. Apply equipped perk modifiers to BuildingData / SellManager / etc.
        // 5. Load GameScene fresh
    }
}
```

### `GameData.cs` Extension
```csharp
// These fields persist through prestige reset:
public int prestigeLevel;
public int prestigePoints;
public List<string> equippedPerks;
public List<string> earnedTrophies;
```

---

## ACCEPTANCE CRITERIA

- [ ] Prestige option appears at 5,000 Fame
- [ ] Prestige confirm screen shows what is lost vs. kept
- [ ] After prestige, town resets but trophies / PP / perks persist
- [ ] PP correctly calculated based on run performance
- [ ] Prestige perk tree UI shows all perks, cost, and which are unlocked
- [ ] Equipped perks apply correctly in new run
- [ ] Prestige trophy decorations appear in new run

---

## HOW IT CONNECTS TO THE FUTURE

```
  Prestige System
       │
       ├── Leaderboards: Prestige level = ranked score
       ├── Social: "I'm on Prestige 7" as bragging right
       ├── Events: Prestige players get early access to events
       └── Live Service: Season Pass tracks prestige progress
```
