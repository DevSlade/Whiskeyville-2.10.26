# 🔁 Retention Strategies — Keeping Players Coming Back to Whiskeyville
### Research Folder | Internal Design Reference
### Updated: March 2026

---

## CONCEPT

Retention is about giving players a *reason* to return to the game after closing it.
Day-1 retention measures whether first-time players come back the next day; Day-7 and
Day-30 retention measure whether the game has become a habit.

Whiskeyville's production-chain structure is naturally retention-friendly: things keep
happening while the player is away (or almost happen — see Offline Progress). The goal
of this document is to codify the **hooks** that pull players back in.

---

## RETENTION LIFECYCLE

```
  Day 0  → Tutorial + first production cycle = hook
  Day 1  → Offline gains waiting + first building unlock = reward
  Day 3  → First Fame milestone + new building type = progression step
  Day 7  → Saloon upgraded / first tourist visit = social signal
  Day 14 → Seasonal event notification = FOMO pull
  Day 30 → Prestige system becomes relevant = identity investment
```

---

## HOOK FRAMEWORK: THE "JUST ONE MORE" ENGINE

Each session should end with the player knowing *exactly* what they are working toward,
so the next session starts with clear intent.

### End-of-Session Summary

When the player pauses or manually saves, show a **Session Summary** card:

```
  ┌─────────────────────────────────────────┐
  │  🥃 Today's Haul                         │
  │  ─────────────────────────────────────  │
  │  Cash earned:      +$1,250              │
  │  Fame gained:      +18 ★               │
  │  Batches produced: 12                   │
  │  ─────────────────────────────────────  │
  │  Next milestone:   100 Fame (82 / 100)  │
  │  Unlock preview:   Tasting Room 🔒      │
  └─────────────────────────────────────────┘
  [Keep Playing]  [Save & Quit]
```

The key element is the **Next milestone** line — always tells the player what is waiting
for them when they return.

---

## DAILY RETURNS

### Morning Bonus
First session of each calendar day grants a **Daily Barrel** — a free Barrel in inventory.
This is modest enough not to break balance but meaningful enough to feel like a gift.

| Streak | Daily Bonus |
|--------|------------|
| 1 day  | 1 Barrel |
| 3 days | 1 Barrel + 25 Cash |
| 7 days | 1 Barrel + 100 Cash + 5 Fame |
| 14 days| 1 Barrel + 250 Cash + 10 Fame + decoration (Vintage Clock) |
| 30 days| Prestige Emblem decoration (Black Label Trophy) |

### Offline Production Reminder
- **Push notification** (mobile): "Your rickhouse is full — come collect your aged whiskey!"
- Fired when AgedWhiskey inventory hits the building cap (if cap system is added)
- Not spam: maximum 1 notification per 4-hour period

---

## WEEKLY EVENTS (Retention Pull)

Weekly events give players a reason to return within a specific 3-day window.
Missing an event means missing the cosmetic reward (not a functional penalty).

| Event | Timing | Activity | Reward |
|-------|--------|---------|--------|
| Flash Sale Weekend | Random Friday–Sunday | Sell Whiskey for ×1.5 price | Weekend Badge decoration |
| Barrel Rush | Monday 48h | Produce 10 Barrels | Copper Barrel decoration |
| Town Tasting | Wednesday 24h | Serve 5 tourists | Signature Glass decoration |
| Grain Glut | Thursday 72h | Harvest 20 Corn | Grain Sack decoration |

---

## LONG-TERM HOOKS

### Town Narrative
Every 5 in-game days, a new narrative postcard appears in the HUD:

```
  Day 5:  "A stranger passing through tried your whiskey. Word is spreading."
  Day 10: "The general store started stocking bottles. Tourism is up."
  Day 15: "A travel writer mentioned Whiskeyville in a magazine."
  Day 20: "Mayor Hank officially declared you the town's Official Distiller."
```

These cost nothing to implement (text + postcard sprite) but create a sense that the
world is *reacting* to player actions — one of the strongest retention mechanisms known.

### NPC Relationship Depth (Phase 3)
Named NPCs (Hank, Mae, Earl, Dolly) return periodically with new dialogue lines that
reference the player's stats. A player who has completed 50 sales gets different dialogue
from Mae than a player with 5 sales. This creates an emotional investment in a persistent
NPC relationship.

---

## SOCIAL PROOF & COMMUNITY (Phase 4)

### Leaderboard Pressure
In Phase 4, opt-in leaderboards show the player how their batch count or Fame score
compares globally. Seeing familiar usernames climb the board is a proven retention driver.

### Shareable Screenshots
A "Share Your Label" feature in the Bottle Customization UI generates a clean stylized
image of the player's bottle for social sharing. The image is stamped with the distillery
name and vintage. Players become free marketing agents when they share.

---

## RESEARCH UPGRADES (ResearchManager)

Unlock via Retention Strategies research node in the Research Lab.

| Research Node | Cost | Effect | Prerequisite |
|--------------|------|--------|-------------|
| Session Summary | 3 Research Pts | Enables end-of-session summary card | None |
| Daily Bonuses | 5 Research Pts | Activates daily barrel streak system | None |
| Narrative Postcards | 8 Research Pts | Town story postcards appear every 5 days | None |
| Weekly Events | 12 Research Pts | Unlocks weekly event schedule | Daily Bonuses |
| Offline Retention | 15 Research Pts | Enables push notifications (mobile) | None |
| Social Export | 20 Research Pts | Enables shareable label screenshot | Narrative Postcards |

---

## ANTI-PATTERNS TO AVOID

- **Energy/stamina mechanics** — never gate core gameplay behind a timer that blocks play
- **Mandatory daily check-ins** — bonuses are additive rewards, not punishments for absence
- **FOMO without accessibility** — if an event is missed, players should be able to earn
  a comparable (not identical) reward another way
- **Social comparison pressure** — leaderboards are opt-in only; hide by default

---

## METRICS TO TRACK (Post-Launch)

| Metric | Target | Notes |
|--------|--------|-------|
| Day-1 Retention | ≥ 40% | Industry mobile average is ~30% |
| Day-7 Retention | ≥ 20% | Strong if >25% |
| Day-30 Retention | ≥ 10% | Excellent if >15% |
| Avg. Session Length | 8–15 min | Short sessions = healthy mobile game |
| Sessions per Day | 1.5–2.5 | Multiple short sessions = good |
| Push Notification CTR | ≥ 15% | If offering offline production alert |

---

## REFERENCES

- Nir Eyal (2014). Hooked: How to Build Habit-Forming Products
- Wolter, B. & Kuber, R. (2015). Game mechanics as retention tools in mobile games
- Applied: Clash Royale (streak system), Animal Crossing (daily visitor model),
  Stardew Valley (season/event calendar), Hay Day (push notification timing)
