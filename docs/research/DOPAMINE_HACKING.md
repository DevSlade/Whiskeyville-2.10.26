# 🧠 Dopamine Hacking — Reward Loop Design in Whiskeyville
### Research Folder | Internal Design Reference
### Updated: March 2026

---

## CONCEPT

"Dopamine hacking" is the practice of structuring reward delivery so that the player's
brain is primed to stay engaged. This is **not** about manipulation — it is about
*respecting the player's time* by ensuring every minute they spend playing feels
purposeful and rewarding. The goal is to create a game that is hard to put down
because every session produces visible, satisfying progress.

Whiskeyville's production-chain structure is a natural dopamine machine: things are
always in progress, outputs arrive on predictable schedules, and the player can always
make *one more decision* before quitting.

---

## THE FIVE CORE REWARD LOOPS

```
  ┌─────────────┐
  │  MICRO LOOP │  ~5–15 seconds
  │  Harvest corn, see +1 Corn popup (yellow)
  │  Immediate tactile feedback, low friction
  └─────────────┘

  ┌─────────────────┐
  │  SHORT LOOP     │  ~2–5 minutes
  │  Building produces → inventory grows → can afford next building
  │  Clear cause/effect, visible progress on HUD
  └─────────────────┘

  ┌─────────────────┐
  │  MEDIUM LOOP    │  ~20–60 minutes
  │  Full Corn→Aged Whiskey chain completes, sell for big Cash burst
  │  "Session completion" feeling — good stopping point
  └─────────────────┘

  ┌──────────────────────┐
  │  LONG LOOP           │  Hours/Days
  │  Fame milestones unlock new buildings, tourist types, events
  │  Maintains long-term direction
  └──────────────────────┘

  ┌──────────────────────────┐
  │  PRESTIGE LOOP           │  Multiple sessions
  │  Reset for permanent bonuses — meta-progression
  │  The "one more run" engine (Phase 4)
  └──────────────────────────┘
```

---

## VARIABLE REWARD SCHEDULE

Fixed-interval rewards (every N seconds) are less engaging than variable schedules.
Whiskeyville should add *variance* to predictable systems without betraying player trust.

### Implemented Variance Mechanisms

| System | Variance Type | Effect |
|--------|--------------|--------|
| Whiskey quality roll | Random (weighted by inputs) | Sometimes a batch is higher quality (+bonus fame) |
| Tourist spending | Gaussian distribution ± 30% | Not every tourist pays the same; surprises feel good |
| Spread Events (Memetic) | Conditional random trigger | Big wins feel earned, not guaranteed |
| Seasonal Events | Calendar-gated random pool | Players check in because something might be happening |
| Critical Batch | Rare (<5%) — 2× output | Pure dopamine moment; popup + special sound |

### Critical Batch

A **Critical Batch** fires when a Still or Rickhouse produces with a 5% probability.
- Output ×2 for that single production cycle
- Special golden popup: "✨ Perfect Batch! +2 [Resource]"
- Distinct SFX (bell chime, not the normal collect sound)
- Doesn't affect the production timer — purely additive

---

## NEAR-MISS & ASPIRATION

Players should always be *almost* able to do something desirable.

### Almost-There Signals
- **Build Panel**: Locked buildings show cost, current cash, and "XX more cash needed" text
- **Fame Progress Bar**: Always shows % to next milestone (never blank)
- **Resource Counters**: Color shifts orange when stock hits 80% of a building's input requirement

### Aspiration Windows
Display the *next unlock* prominently. When player has 80 Fame, show the 100 Fame unlock
reward with a soft glow — they can *see* what they are working toward.

---

## FEEDBACK SYSTEMS

### Visual Feedback
- Floating text popups (+1 Corn, +50 Cash) with color coding matching the resource
- Building animation on production (scale pulse or particle burst)
- HUD counters animate when values change (count-up tween)

### Audio Feedback
| Event | Sound Character |
|-------|----------------|
| Corn harvested | Soft thud/rustle |
| Building produces | Gentle mechanical ding |
| Cash earned | Coin shimmer |
| Fame gained | Rising chord (satisfying) |
| Viral Batch | Crowd cheer + fanfare sting |
| New unlock | Triumphant ascending notes |

### Progression Feedback
- Day counter visible at all times (sense of elapsed time without pressure)
- Session summary on pause: "Today: +$X Cash, +Y Fame, Z batches produced"

---

## PACING TARGETS

| Time in Session | Desired Feeling | Design Target |
|----------------|-----------------|--------------|
| 0–2 min | "I remember where I was" | Immediate visible production in progress |
| 2–5 min | "I can make a decision" | Enough cash/resources to place 1 new building |
| 5–20 min | "I'm building something" | 2–3 supply chain improvements possible |
| 20–45 min | "Satisfying session" | Full chain running, first big Sell All |
| 45–60 min | "I could keep going" | Fame milestone approaching (within 20%) |

---

## IDLE / OFFLINE PROGRESS

Players who return after absence should feel rewarded, not punished.

- **Offline Production Cap**: Buildings continue running up to 3 hours offline. Beyond that,
  they pause (avoids exponential catch-up breaking balance)
- **Return Popup**: "Welcome back! Your distillery produced [X Corn, Y Mash, Z Whiskey]
  while you were away" — shows offline gains as a gift
- **Catch-Up Accelerator**: First 5 minutes after returning, production speed +20%

---

## RESEARCH UPGRADES (ResearchManager)

Unlocked via Dopamine Research node in the Research Lab.

| Research Node | Cost | Effect | Prerequisite |
|--------------|------|--------|-------------|
| Visual Feedback+ | 5 Research Pts | All popups add screen shake (subtle) | None |
| Critical Batch | 8 Research Pts | Enables 5% critical batch chance | None |
| Variable Tourists | 12 Research Pts | Tourist spend varies ±30% | None |
| Offline Accelerator | 15 Research Pts | Return bonus extends to 10 min | None |
| Jackpot Roll | 25 Research Pts | Critical Batch chance increases to 10% | Critical Batch |

---

## ANTI-PATTERNS TO AVOID

- **Punishing absence** — never delete progress or add negative events while offline
- **Too many decisions at once** — introduce systems one at a time (tutorial principle)
- **False urgency** — do not use countdown timers that force the player to play "right now"
- **Pay-to-win loops** — premium content is cosmetic (bottle styles, decorations), never
  functional shortcuts that break the core economy

---

## REFERENCES

- Przybylski, A.K. et al. (2010). Motivational, emotional, and behavioral correlates of
  fear of missing out. Computers in Human Behavior.
- Rigby, S. & Ryan, R.M. (2011). Glued to Games — Self-Determination Theory
- Applied: Cookie Clicker (idle loop model), Stardew Valley (micro/medium/long loop balance),
  Clash of Clans (return reward model)
