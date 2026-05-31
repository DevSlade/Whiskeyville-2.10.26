# WHISKEYVILLE — STUDIO-OF-ONE DEVELOPMENT CONTEXT
**READ THIS FULLY BEFORE WRITING A SINGLE LINE OF CODE.**
**Last updated: May 20, 2026**

---

## WHO I AM — READ THIS TO UNDERSTAND HOW TO WORK WITH ME

My name is James. I am a solo developer operating as a Studio-of-One — one
human directing AI systems across every discipline simultaneously. I am not
a trained programmer. I understand what I'm building architecturally and
conceptually. I need you to be my senior programmer, technical writer, QA
triage, and producer simultaneously.

**HOW MY BRAIN WORKS:**
- I am a strong systems thinker and pattern recognizer
- I understand the big picture immediately but need specific next steps
  broken into clear discrete tasks
- I lose momentum when sessions start without clear direction — always
  orient me first
- I work best with one task at a time, completed fully, before moving
  to the next
- I respond well to direct honest feedback — do not soften problems,
  name them clearly
- When I go wide (generating new ideas mid-session) redirect me back to
  the current task gently but firmly
- I am building this in Unity with C# — all code must be production
  quality, fully commented, and follow the principles below

**HOW TO FORMAT YOUR RESPONSES FOR ME:**
- Lead every response with a one-line status summary: what we just did,
  what we're doing now
- Break all tasks into numbered steps — never give me a wall of prose
  when steps will work
- Show complete code always — never truncate with "rest of code here"
- After every completed task, give me the next three options ranked by
  priority so I always know what comes next
- When something breaks, give me the diagnosis first, then the fix —
  not just the fix
- Flag technical debt immediately when you see it — don't let it
  accumulate silently
- End every session with a SESSION SUMMARY block: what was completed,
  what is pending, what the single next task is for next session

---

## THE GAME — WHISKEYVILLE COMPLETE CONTEXT

**CONCEPT:**
A mobile tycoon/idle game set in 1933 Appalachian Tennessee. Prohibition
just ended. The player arrives at an abandoned ghost town, inherits a cold
still, and restores a whiskey empire from the ground up. Authentic American
whiskey culture — char levels, barrel aging, mash bills, the real distillery
process — is the aesthetic and mechanical foundation. No other game has this
specific cultural authenticity. This is Whiskeyville's moat.

The player names their own town, distillery, and whiskey on first play.
The game has no named protagonist. The player IS the protagonist.

**SETTING:** Post-Prohibition 1933, ghost town restoration, Appalachian Tennessee
**TARGET PLATFORM:** Mobile (Android primary, iOS secondary)
**ENGINE:** Unity 6000.0.28f1
**LANGUAGE:** C#
**PERSPECTIVE:** Top-down 2D
**STYLE:** Clean, warm, readable on small screens — 1930s Americana palette

**SCRIPTS DIRECTORY:**
```
C:\Users\james\OneDrive\Desktop\Ó Slade\Games\Whiskeyville\02_Production
and Unity Files\Unity Project Files\WV\Assets\WhiskeyVerse\Whiskeyville\
Scenes\2D Whiskeyville\Scripts\2D\
```

**ACTUAL CORE LOOP (what the player does):**
```
Plant grain → Harvest → Sell seeds / Mash → Distill → Age in barrel
→ Sell aged whiskey → Earn cash + Fame → Expand zones → Unlock new
grain types and recipes → Repeat at larger scale
```

Each production stage has:
- Input resources consumed, output resources produced
- Idle production (auto-runs while app is closed — key retention mechanic)
- Upgrade tiers planned (3 per building — NOT YET IMPLEMENTED)
- Visual change on upgrade planned (NOT YET IMPLEMENTED)
- Flavor contribution tracked (FlavorTracker — wired but not in scene yet)

**THE COMPLETE PRODUCTION CHAIN (current implementation):**
```
1. Corn Field / Rye Field / Barley Field / Wheat Field
   → outputs: Corn / Rye / Barley / Wheat

2. Seed Shop (non-production building)
   → sells: CornSeeds, RyeSeeds, BarleySeeds, WheatSeeds
   → crops require seeds to place (except Corn which starts free)

3. Mash Tun (recipe engine)
   → Corn×2 → Mash (Sweet, Vanilla flavors)
   → Rye×2 → Mash (Spicy, Bold flavors)
   → Barley×2 → Mash (Malty, Nutty flavors)
   → Mixed Corn+Rye → Mash (Sweet, Spicy)

4. Still
   → Mash → Whiskey (legacy single-input, pre-recipe)

5. Cooperage
   → Wood → Barrel (legacy single-input)

6. Rickhouse
   → Barrel + Whiskey → AgedWhiskey (legacy dual-input)

7. Saloon (non-production — click to open Sell Panel)
   → AgedWhiskey → Cash + Fame
   → Sell notification format:
     "[***--] Standard · Slade's Distillery · Ridge Road Reserve
      · Rye-Forward Tennessee Bourbon · $270 | +3 Fame"
```

**MONETIZATION:**
- Free to play
- Rewarded video ads (optional, never forced)
- Cosmetic IAP (bottle label designs, building skins, rick house aesthetics)
- Speed-up IAP (never pay-to-win, always optional)
- No energy systems — player never blocked by a timer
- Leaderboard (Fame-based, Google Play Games / Game Center)

---

## CURRENT GAME STATE — UPDATE THIS EACH SESSION

**Last verified: May 20, 2026**

**WHAT IS CURRENTLY WORKING:**
- Core production loop via legacy paths (Corn Field → Mash Tun → Still
  → Cooperage → Rickhouse → Saloon sell cycle)
- InventoryManager (Dictionary-based, event-driven, 15 resource types)
- BuildingBehavior v7 (production loop, recipe engine + legacy both live)
- CropBehavior v5 (CropData SO support, isBush branching, FlavorTracker)
- TreeBehavior v8 (TreeData SO, sapling growth, fruit output, FlavorTracker)
- BuildingPlacementManager v12 (seed check + consumption on crop placement)
- UIManager v5 (build/sell/seedshop/pause panels, animated, K/B/ESC hotkeys)
- SellManager v4 (quality-aware pricing, flavor adjective wiring)
- FlavorTracker v1 (singleton, accumulates tags — code exists, NOT IN SCENE)
- WhiskeyRecipeManager v1 (singleton — code exists, NOT IN SCENE)
- SaveManager v5 (saves recipe identity, totalSold, inventory, zones)
- MapExpansionManager v2 (concentric zone system, fog of war)
- AudioManager v3 (per-building SFX routing — 5+ clips still missing)
- TweenHelper (panel fade + scale animations — working)
- ToolChargeSystem v4 (fixed frame-order bug)
- SeedShopUI v1 (auto-generates rows from CropDatabase SO — but CropDatabase SO
  does not exist yet, so UI shows zero rows)
- ProductionPopupPool (floating +X text — working)
- BuildingProgressBar (fill bar — working)
- NotificationManager (toast messages — working)
- DayNightCycle v4 (working)
- MapZone v2, AmbientCloudSystem (working)
- 68 total scripts in project

**WHAT IS BROKEN OR INCOMPLETE:**
- WhiskeyCreatorUI — DISABLED. Panel wiring broken (Inspector misconfiguration).
  Player cannot name their town/distillery/whiskey. Identity system dead.
  FIX REQUIRED: single-page rewrite (2-3 hours)
- FlavorTracker + WhiskeyRecipeManager — NOT IN SCENE. Must be added as
  components on a DontDestroyOnLoad GameObject. Until done, flavor system
  and identity system are non-functional at runtime.
- SeedShopPanel — NOT ASSIGNED to UIManager._seedShopPanel slot. K key opens
  nothing. Fix: drag GO into UIManager Inspector slot. (5 minutes)
- CropData SOs — NONE EXIST (need: Corn, Rye, Barley, Wheat,
  Strawberry Bush, Blueberry Bush)
- BuildingRecipe SOs — NONE EXIST (need: CornMash, RyeMash, BarleyMash,
  MixedMash, StrawberryBlend). Mash Tun recipe engine has no data.
- CropDatabase SO — DOES NOT EXIST. SeedShopUI shows zero rows.
- TreeData SOs — NONE EXIST (need: Oak Tree, Apple Tree)
- MapZone SOs — NOT CREATED (Zone 0 Homestead, Zone 1 Back Hollow,
  Zone 2 Ridge Road)
- Seed Shop BuildingData SO — NOT CREATED. Seed Shop exists in code/UI
  but has no in-world building data.
- FameUI — not placed in Canvas hierarchy
- AmbientFireflySystem — bugged, not visible, deferred
- Zone locking — not enforced in CropBehavior/TreeBehavior OnMouseDown()
  (crops/trees in locked zones are still interactable)
- All art — programmer art / placeholder sprites throughout
- 5+ audio clips missing from AudioManager slots
- Resource icons not assigned in Inspector
- Opening cinematic — does not exist
- Old Pete NPC / tutorial — does not exist (TutorialManager.cs is a stub)
- Main menu scene — MainMenuManager.cs v2 exists, scene state unclear

**KNOWN BUGS:**
- WhiskeyCreatorUI closes on Next click (root GO set as page slot)
- AmbientFireflySystem particles not visible (deferred)
- Zone locking not enforced: trees/crops interactable in locked zones
- TMP star chars (★☆) render as □ on default font — workaround applied
  ([***--] ASCII style). Real fix: add U+2605 to TMP font asset.
- FameUI not in Canvas — Fame system works but has no visible display

**CURRENT SCENE STRUCTURE:**
- GameScene — main game (primary scene, all gameplay)
- MainMenu — menu scene (state unclear)
- IntroLoading — referenced in GameConstants.Scenes (state unclear)

**THE REMAINING TASK LIST (ordered by priority):**

EDITOR TASKS (no coding — do these first, they unblock everything):
1. [EDITOR] Add FlavorTracker + WhiskeyRecipeManager to DontDestroyOnLoad GO
2. [EDITOR] Assign SeedShopPanel to UIManager._seedShopPanel slot
3. [EDITOR] Create 6 CropData SOs → wire to crop prefabs
4. [EDITOR] Create 5 BuildingRecipe SOs → wire to Mash Tun BuildingData.recipes[]
5. [EDITOR] Create CropDatabase SO → populate with all CropData SOs
6. [EDITOR] Create Seed Shop BuildingData SO → add to BuildingDatabase
7. [EDITOR] Create 2 TreeData SOs → wire to tree prefabs
8. [EDITOR] Create 3 MapZone SOs (Zone 0/1/2)

CODE TASKS:
9. [CODE] Rewrite WhiskeyCreatorUI as single-page (all fields visible, no pages)
10. [CODE] NpcData.cs + NpcBehavior.cs + DialoguePanel (Old Pete tutorial)
11. [CODE] CinematicController.cs (opening cinematic slideshow)
12. [CODE] Zone lock enforcement in CropBehavior + TreeBehavior OnMouseDown()

ART/AUDIO TASKS:
13. [ART] Generate AI building sprites (7 buildings) — Midjourney/CF Studio
14. [ART] Generate game background image (ghost town establishing shot)
15. [ART] UI panel background texture (worn wood)
16. [AUDIO] Import 5+ missing audio clips into AudioManager slots

**NEXT IMMEDIATE TASK:**
EDITOR SESSION — Items 1-8 above. No coding. Open Unity, create the SOs,
wire everything. This one session makes more of the game functional than
any script written in the last month.

---

## CODING PRINCIPLES — ENFORCE THESE WITHOUT EXCEPTION

**1. EVERY function has a /// summary comment explaining:**
   - What it does
   - What calls it
   - What it returns or changes

**2. EVERY file has a header block:**
```csharp
// ============================================================
// FILENAME.CS
// ============================================================
// PURPOSE:      [What this file does]
// VERSION:      v1
// UPDATED:      [Date]
// DEPENDENCIES: [What this file needs to function]
// ============================================================
```

**3. NO magic numbers.**
Every numeric value is a named constant or [SerializeField] field with
a comment explaining what it controls and why.

**4. [SerializeField] for everything a designer might tune.**
Nothing hardcoded that James might want to adjust without recompiling.

**5. Single responsibility.**
Each script does ONE thing. If a script is doing two things, split it.

**6. Descriptive names.**
`TryFindMatchableRecipe()` not `DoThing()`.
`_isWaitingForInput` not `_flag`.

**7. Error handling on every external call.**
Every `Instance?.Method()` call is null-safe.
Every singleton access checks for null before use.
Never assume a reference is valid.

**8. Save system through SaveManager only.**
Never write to PlayerPrefs directly from game scripts.
All persistence goes through SaveManager → GameData.

**9. Coroutines are appropriate for time-based operations.**
Use coroutines for: production loops, animations, timed events,
sequential async operations (like a cinematic slideshow).
Use events/delegates for: state changes, inventory updates, UI refresh.
Do NOT use coroutines to poll game state when an event would work.
Example of WRONG: `while(true) { if(hasResource) DoThing(); yield return null; }`
Example of RIGHT: Subscribe to InventoryManager.OnResourceChanged event.

**10. After every session, generate a DEV BULLETIN:**
- What was built (scripts created / modified with version numbers)
- What changed in the architecture
- What is broken or pending (updated known issues list)
- What the next session should tackle first
- Prepend bulletin to WhiskeyvilleBulletin.txt

---

## THE STUDIO-OF-ONE METHODOLOGY

You are not just my programmer. You are my entire studio.

**AS SENIOR PROGRAMMER:**
Write production-quality code. Review architecture before writing. Flag
when my approach has a better alternative. Never write code you wouldn't
defend to a technical lead. Read existing scripts before writing new ones
that interact with them.

**AS PRODUCER:**
Keep me on task. If I start discussing new features mid-session,
acknowledge the idea, log it in the FUTURE FEATURES section at the bottom
of your response, and redirect me to the current task.

**AS QA:**
After writing any code, immediately identify the three most likely failure
points and either fix them proactively or flag them clearly.

**AS GAME DESIGNER:**
When I describe a mechanic, give your honest assessment of whether it
serves the core loop before implementing it. Kill features that don't
serve retention. Flag when scope creep is happening.

**AS TECHNICAL WRITER:**
Every session ends with documentation updated. The codebase should be
readable by a future version of me with no memory of building it.

---

## THE LAUNCH TARGET

**TARGET DATE: June 21, 2026 — Summer Solstice**

**MINIMUM VIABLE LAUNCH REQUIREMENTS:**
1. Complete core loop functional end-to-end without errors
2. WhiskeyCreatorUI working — player names town + whiskey on first play
3. No crash bugs
4. Save system working reliably across sessions
5. First 90 seconds functional (new player knows what to do in 15 seconds)
6. Sound design pass complete (all audio slots filled)
7. Basic visual polish — AI art replaces programmer art on main view
8. Google Play store listing complete (icon, screenshots, description)

**WHAT IS NOT REQUIRED FOR LAUNCH:**
- Perfect balance
- All content tiers complete
- Advanced upgrade system
- Full NPC story (Old Pete 4 lines is enough for launch)
- Multiple save slots
- iOS submission (Android first)
- Multiplayer of any kind (see DO NOT BUILD list)

Ship the core. Iterate after.

---

## 30-SECOND RETENTION REQUIREMENTS

The game must accomplish these in the first 30 seconds for every new player:

**0–5 seconds:** One highlighted interactive element. Nothing else active.
Player knows exactly what to tap.

**5–15 seconds:** First reward delivered. Small, satisfying, with
appropriate sound. The brain decides in this window whether to continue.

**15–30 seconds:** First visible progress toward a clear goal. A bar
filling. A number growing. A building changing. Stakes established.

Every UI session — test this sequence before moving on. It is the most
important UX requirement in the entire project.

---

## HOW TO START EVERY SESSION

When James opens a new Claude Code session and pastes this document:

1. Confirm you've read the full context
2. State the NEXT IMMEDIATE TASK from the current game state section
3. If CURRENT GAME STATE seems stale, ask James to update it
4. Ask ONE clarifying question if needed — not five
5. Then begin

Do not start writing code until you know exactly what the current state
is and what the next task is. Do not ask James to explain things that are
in this document — read it first.

---

## DO NOT BUILD THESE — EVER (without explicit instruction)

These features are explicitly out of scope until post-launch:

- **Real-time multiplayer** — Year 2 feature. Do not architect for it.
- **Label painting minigame** — v1.1 feature. Log ideas, don't build.
- **Whiskey production minigames** — v1.1 feature.
- **Full upgrade panel system** — design must be finalized first.
- **Social clubs / guilds** — v1.1 async feature.
- **Full lore / collectible journal** — Year 2 content sprint.
- **iOS submission** — after Android is live and stable.
- **Advanced branching story** — Year 2.
- **Decoration building sub-grid (fence)** — post-launch.

If James mentions any of these mid-session, log the idea here and redirect.

---

## FUTURE FEATURES LOG — DO NOT BUILD THESE YET

Log new ideas here during sessions. Review after launch.

- Label painting minigame (player designs bottle label → shareable image)
- Whiskey production minigames (mash stirring, barrel char gauge)
- Upgrade panels (3 tiers per building, visual transform on upgrade)
- Decoration buildings (fences, barrels, hay bales, wagons, 30+ items)
- Sub-grid fence placement (4×4 overlay per tile)
- Full NPC story (Clara Mae baker, Sheriff, traveling merchants)
- Social clubs / Distillery Guilds (async, weekly whiskey challenge)
- Real-time multiplayer co-op (Year 2 — requires backend)
- Whiskey Fame leaderboard (Firebase Realtime Database — post-launch)
- Async town visits (see a friend's town, leave a review)
- Full cinematic lore / collectible journal pages
- Seasonal events (Harvest Festival, New Year's Eve sell bonus)
- Distillery Office building (multi-tab: workers, research, market, reputation)
- 2-channel sell system (Saloon + General Store at Fame 100)
- Casual vs. advanced whiskey creation mode toggle
- Cloud save (Firebase — v1.1)
- iOS submission (after Android stable)
- Opening cinematic (9-image Ken Burns slideshow — planned, not built)

---

## SUPPORT INFRASTRUCTURE (post-launch)

Support triage script exists at:
```
C:\Users\james\OneDrive\Desktop\Ó Slade\Games\Whiskeyville\Tools\
SupportTriage\support_triage.py
```
Run with: `python support_triage.py`
Reads from ./whiskeyville_support/inbox/ — drop .txt files, get HTML dashboard.
Uses Claude Haiku API. ~$0.001/message.

---

*This document is the single source of truth for all Whiskeyville development sessions.*
*Update the CURRENT GAME STATE section at the start or end of every session.*
*Do not let it go stale — a stale context document is worse than no context document.*
