# 🥃 Whiskeyville — Full Development Roadmap
### Black Label Interactive | Last Updated: March 2026

---

## THE BIG PICTURE

Whiskeyville is a 2D town-builder centered on the whiskey production lifecycle. The game's
foundation — a functional production chain from corn field to cash register — is complete and
confirmed working. Everything from here builds **outward** from that chain.

```
FOUNDATION (done) ──────────────────────────────────────────────────────────────────────►
│
│  Corn Field → Mash Tun → Still → Cooperage → Rickhouse → Saloon → Cash
│      ↕                                                         ↕
│  Wood/Trees                                              Save/Load
│      ↕
│  Barrels
│
▼  This single vertical slice is the spine every future feature attaches to.
```

---

## OVERALL GAME COMPLETION

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  FULL GAME VISION COMPLETION                                                │
│                                                                             │
│  MVP (Playable Vertical Slice) ████████████████████░░░░  80% ← WE ARE HERE │
│  Post-MVP v1.0 (Content Layer) ████░░░░░░░░░░░░░░░░░░░░  18%               │
│  v1.5 (Social / Live Layer)    ██░░░░░░░░░░░░░░░░░░░░░░   9%               │
│  Full Vision (Prestige + Meta) █░░░░░░░░░░░░░░░░░░░░░░░   4%               │
│                                                                             │
│  AS % OF THE FULL GAME:  ~28%  complete                                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

> The production chain is done. The world around it is 20–30% built. There is a lot of
> interesting work ahead — and the hard part (making the core loop feel good) is already solved.

---

## PHASE OVERVIEW

```
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                        WHISKEYVILLE — DEVELOPMENT PHASES                                 │
├──────────────┬──────────────┬──────────────┬──────────────┬──────────────┬──────────────┤
│  PHASE 0     │  PHASE 1     │  PHASE 2     │  PHASE 3     │  PHASE 4     │  PHASE 5     │
│  Foundation  │  MVP Polish  │  Content     │  Social      │  Prestige    │  Iso / 3D    │
│  (DONE ✅)   │  (NOW 🔥)   │  Expansion   │  + Live      │  + Meta      │  Art Pivot   │
│              │              │              │              │              │              │
│  Jan–Feb 26  │  Feb–Mar 26  │  Q2 2026     │  Q3 2026     │  Q4 2026     │  2027+       │
├──────────────┼──────────────┼──────────────┼──────────────┼──────────────┼──────────────┤
│  Core loop   │  Tools       │  Fame        │  Tourists    │  Prestige    │  Isometric   │
│  31 scripts  │  Mobile      │  Unlocks     │  Events      │  Leaderboard │  conversion  │
│  Save/Load   │  Tutorial    │  Decorations │  NPCs        │  New game +  │  3D assets   │
│  UI panels   │  Audio pass  │  Multi-crop  │  Quests      │  Daily login │  Online sync │
│  Day/Night   │  Balance     │  Premium $   │  Seasons     │  Cloud save  │  Cross-plat  │
│  6 buildings │  Main menu   │  Rickhouse+  │  Social      │  Battle pass │  Steam?      │
└──────────────┴──────────────┴──────────────┴──────────────┴──────────────┴──────────────┘
```

---

## PHASE 1 — MVP POLISH (80% → 100%)

**Goal:** Take the working vertical slice to a shippable, player-friendly MVP.
**Target Date:** March 2026
**Estimated Time Remaining:** ~4–6 hours of dev work

### Remaining MVP Tasks

| # | Feature | Priority | Est. Time | Spec Doc | Target % |
|---|---------|----------|-----------|----------|----------|
| 1 | **Tool System** (axe, hoe, demolish) | 🔴 Critical | 30–45 min | [MVP_TOOLS_SYSTEM.md](features/MVP_TOOLS_SYSTEM.md) | 80→85% |
| 2 | **Main Menu Save/Load** (Continue button) | 🔴 Critical | 15 min | [MVP_MAIN_MENU_SAVE_LOAD.md](features/MVP_MAIN_MENU_SAVE_LOAD.md) | 85→87% |
| 3 | **Clouds** (scrolling sky) | 🟡 Medium | 10 min | — | 87→88% |
| 4 | **Balance Pass** (economy tuning) | 🟡 Medium | 20 min | [MVP_BALANCE_PASS.md](features/MVP_BALANCE_PASS.md) | 88→90% |
| 5 | **Audio Polish** (full SFX/music pass) | 🟡 Medium | 30 min | [MVP_AUDIO_POLISH.md](features/MVP_AUDIO_POLISH.md) | 90→93% |
| 6 | **Tutorial / Onboarding** (first-run) | 🔴 Critical | 45 min | [MVP_TUTORIAL_ONBOARDING.md](features/MVP_TUTORIAL_ONBOARDING.md) | 93→97% |
| 7 | **Mobile Touch Input** | 🔴 Critical | 60–90 min | [MVP_MOBILE_INPUT.md](features/MVP_MOBILE_INPUT.md) | 97→100% |
| 8 | **Console Cleanup** | 🟢 Low | 10 min | — | — |

### Phase 1 Milestone: ✅ MVP COMPLETE
A new player can download the game, understand what to do, play the full loop, save progress, and
return to their town — all without dev knowledge or hidden hotkeys.

---

## PHASE 2 — CONTENT EXPANSION (v0.9 → v1.0)

**Goal:** Give the player reasons to keep playing past their first whiskey barrel.
**Target:** Q2 2026

### Feature List

| Feature | Why It Matters | Spec Doc |
|---------|---------------|----------|
| **Fame Currency** | Second win condition beyond cash. Drives prestige loop. | [FAME_CURRENCY.md](features/FAME_CURRENCY.md) |
| **Progression & Unlocks** | Rewards investment. Unlocks new buildings over time. | [PROGRESSION_AND_UNLOCKS.md](features/PROGRESSION_AND_UNLOCKS.md) |
| **Multiple Crops** | Adds variety. Different crops → different mash recipes. | [MULTIPLE_CROPS.md](features/MULTIPLE_CROPS.md) |
| **Decorations** | Emotional ownership. QoL for town builders. | [DECORATIONS.md](features/DECORATIONS.md) |
| **Expanded Rickhouse+** | Deeper aging mechanics. Quality tiers of whiskey. | See PROGRESSION doc |

### Phase 2 Milestone: First version people want to tell friends about.

---

## PHASE 3 — SOCIAL & LIVE LAYER (v1.0 → v1.5)

**Goal:** Create ongoing reasons to return daily/weekly.
**Target:** Q3 2026

| Feature | Why It Matters | Spec Doc |
|---------|---------------|----------|
| **Tourists & NPCs** | World feels alive. Quests + cash sink + storytelling. | [TOURISTS_AND_NPCS.md](features/TOURISTS_AND_NPCS.md) |
| **Events System** | Seasonal hooks. Limited-time content. | [EVENTS_SYSTEM.md](features/EVENTS_SYSTEM.md) |
| **Saloon Visitors** | Converts fame into foot traffic. Validates whiskey quality. | See TOURISTS doc |
| **Quest System** | Directed play for onboarding and mid-game goals. | See TOURISTS doc |

### Phase 3 Milestone: Players have a reason to open the game every day.

---

## PHASE 4 — PRESTIGE & META (v1.5 → v2.0)

**Goal:** Create long-term mastery and replayability.
**Target:** Q4 2026

| Feature | Why It Matters | Spec Doc |
|---------|---------------|----------|
| **Prestige System** | New-game-plus loop. Start over with bonuses. | [PRESTIGE_SYSTEM.md](features/PRESTIGE_SYSTEM.md) |
| **Premium Currency** | Monetization layer. Cosmetics, not pay-to-win. | — |
| **Cloud Save / Cross-platform** | Mobile + PC continuity. | — |
| **Leaderboards** | Fame score rankings. Community. | — |

---

## PHASE 5 — ISOMETRIC CONVERSION (v2.0+)

**Goal:** Upgrade the game's visual identity to isometric 2.5D.
**Target:** 2027+
**Risk:** All sprites must be redrawn. Art-heavy. Code changes are moderate.

| Component | Effort | Spec Doc |
|-----------|--------|----------|
| Camera system rewrite | Medium (3–5 days) | [ISOMETRIC_CONVERSION.md](features/ISOMETRIC_CONVERSION.md) |
| Grid system rewrite | High (1–2 weeks) | See ISOMETRIC doc |
| All building sprites redrawn | Very High (months) | Art team scope |
| Character/NPC sprites | Very High | Art team scope |

---

## HOW THE FOUNDATION PROJECTS OUTWARD

Every future feature connects back to the same spine:

```
                              ┌──────────────────────────────────────┐
                              │       INVENTORYMANAGER.CS            │
                              │  (Single Source of Truth — SSOT)     │
                              │  Cash | Corn | Mash | Whiskey |       │
                              │  AgedWhiskey | Wood | Barrels         │
                              │  + Future: Fame | XP | Ingredients    │
                              └──────────────┬───────────────────────┘
                                             │ reads/writes
          ┌──────────────────────────────────┼──────────────────────────────────┐
          │                                  │                                  │
  ┌───────▼────────┐               ┌─────────▼──────────┐            ┌─────────▼──────────┐
  │ BuildingBehavior│               │   SellManager       │            │   SaveManager       │
  │ (Production)   │               │   (Economy)         │            │   (Persistence)     │
  │                │               │                     │            │                     │
  │ Corn→Mash      │               │ AgedWhiskey→Cash    │            │ JSON to disk        │
  │ Mash→Whiskey   │               │ + Future Fame earn  │            │ + Future cloud sync │
  │ Whiskey→Aged   │               │ + Sell price tiers  │            │ + Prestige resets   │
  │ Wood→Barrels   │               │ + Tourist purchases │            │ + Multiple slots    │
  └───────┬────────┘               └─────────┬──────────┘            └─────────────────────┘
          │                                  │
   ┌──────▼───────────────────────────────────▼──────────┐
   │              FUTURE SYSTEMS ATTACH HERE             │
   │                                                     │
   │  Fame Currency ──────────────── InventoryManager    │
   │  Unlock Trees ───────────────── BuildingDatabase    │
   │  Crop Varieties ─────────────── CropBehavior        │
   │  NPC Quests ─────────────────── BuildingBehavior    │
   │  Events ─────────────────────── DayNightCycle       │
   │  Prestige ───────────────────── SaveManager         │
   │  Isometric ──────────────────── GridManager         │
   │  Mobile Touch ───────────────── InputSystem         │
   └─────────────────────────────────────────────────────┘
```

> **Key insight:** InventoryManager as a singleton SSOT is the right call. Every new resource
> (Fame, XP, Season tokens) is just a new key in the dictionary. No rewrites needed.
> New features plug into existing hooks. The architecture scales.

---

## TIMELINE VISUAL

```
2026                                                                    2027+
│                                                                          │
JAN  FEB  MAR  APR  MAY  JUN  JUL  AUG  SEP  OCT  NOV  DEC  ............│
│◄──────────────────────────────────────────────────────────────────────►│
│                                                                          │
│ ████████████████████████                                                 │
│ Phase 0+1 (Foundation + MVP Polish)   ←WE ARE HERE (Mar 2026)           │
│                         ██████████████████████                          │
│                         Phase 2 (Content Expansion)                     │
│                                          █████████████████              │
│                                          Phase 3 (Social/Live)          │
│                                                          ████████        │
│                                                          Phase 4         │
│                                                               ...........│
│                                                               Phase 5    │
```

---

## WHAT "DONE" LOOKS LIKE

**MVP Done (100%):**
A solo player can discover the game, understand it without help, run the full whiskey production
loop, save their progress, come back later, and feel satisfied. Works on mobile and desktop.

**v1.0 Done:**
That same player comes back the next day because there's always something new to do. They've
named their town in their head. They want to tell someone about it.

**Full Vision Done:**
Whiskeyville is a live service game with a thriving community, seasonal events, and a prestige
loop that keeps players engaged for years. The isometric art style makes it visually distinctive
on storefronts.

---

*"The chain runs. The whiskey flows. The cash stacks. Now we build the world around the barrel."*
*— Black Label Interactive*
