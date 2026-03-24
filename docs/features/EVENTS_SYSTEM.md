# 🎪 Feature Spec: Events System (Seasonal & Special Events)
### Phase 3 — Social & Live Layer | Priority: Post-MVP
### Target: Q3 2026

---

## CONCEPT

Events are **time-limited occasions** that change the rules of the game temporarily —
boosting demand, introducing rare resources, or challenging the player to meet a goal.
They create reasons to open the game on specific days and give every session a sense of
"something is happening right now."

```
  WITHOUT EVENTS:   Every day is the same. I can play later.
  WITH EVENTS:      The Harvest Festival ends in 2 days. I need to make 20 more barrels.
```

---

## EVENT TYPES

### 1. Seasonal Events (Calendar-Based)
Tied to real-world dates or in-game season cycles.

| Event | Season | Duration | Effect |
|-------|--------|----------|--------|
| **Harvest Festival** | Fall | 3 days | Sell price ×2, extra Corn spawns |
| **Winter Warming** | Winter | 5 days | Tourists +50%, cash bonus for each sale |
| **Spring Distilling** | Spring | 3 days | Production speed +25% |
| **Summer Heatwave** | Summer | 2 days | Crops grow 2× faster, Wood reduced |
| **New Year's Toast** | Jan 1 | 1 day | Mega-tourist surge, Fame ×3 |

### 2. Challenge Events (Skill-Based)
Player is given a goal to hit within a time window.

| Event | Goal | Reward |
|-------|------|--------|
| "The Big Order" | Sell 50 Aged Whiskey in 24 hours | Rare decoration |
| "Expert's Blind Taste" | Produce 10 Premium batches in 1 week | +500 Fame |
| "Town Showcase" | Reach 200 tourists visited | Unlock Celebrity tourist |

### 3. NPC Events (Quest-Triggered)
Triggered by NPCs at Fame milestones (see TOURISTS doc).

| Event | Trigger | Effect |
|-------|---------|--------|
| "Journalist's Visit" | Dolly quest complete | +100 Fame burst, Journalist tourist appears |
| "Critic's Table" | 1,000 Fame | Critic tourist arrives, reviews your whiskey |
| "Prestige Opening" | 5,000 Fame | Special ceremony, enables Prestige reset |

---

## EVENT SYSTEM ARCHITECTURE

### `EventManager.cs` (New)
```csharp
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [SerializeField] private GameEvent[] _calendarEvents;
    private GameEvent _activeEvent;

    public event Action<GameEvent> OnEventStarted;
    public event Action<GameEvent> OnEventEnded;

    private void Update() { CheckForActiveEvents(); }
}

[System.Serializable]
public class GameEvent
{
    public string eventName;
    public string description;
    public EventType type;           // Seasonal / Challenge / NPC
    public int durationInGameDays;
    public EventEffect[] effects;    // price multiplier, speed boost, etc.
    public EventReward reward;       // Fame / Decoration / Unlock
    public int fameTrigger;          // min Fame to be eligible
}
```

### Event Effects (Modifier System)
```csharp
public class EventEffect
{
    public EffectType type;   // SellPriceMultiplier / ProductionSpeedMultiplier / etc.
    public float value;       // 2.0 = ×2, 1.25 = +25%
    public string targetResource; // e.g., "AgedWhiskey" sell price
}
```

---

## IN-GAME CALENDAR (DayNightCycle Integration)

Events are tracked by in-game day count (persisted in `GameData`).

```
  DayNightCycle.cs tracks current day
       │
       ▼
  EventManager checks events each new day
       │
       ▼
  Qualifying event found? → Display event banner → Apply effects
       │
       ▼
  Duration expires → Remove effects → Display "Event Ended" summary
```

### Event Banner UI
```
  ┌─────────────────────────────────────────────────────┐
  │  🌾  HARVEST FESTIVAL — 2 days remaining!           │
  │  Sell prices DOUBLED. Extra corn sprouting!         │
  │  Challenge: Sell 20 Aged Whiskey for bonus Fame     │
  └─────────────────────────────────────────────────────┘
```

---

## LIVE SERVICE LAYER (Phase 3+)

Events can be server-driven for live games:
- Server returns `activeEvent.json` on app open
- Allows holiday events, community challenges, etc.
- Player sees "COMMUNITY GOAL: Reach 1M Aged Whiskeys sold globally"

---

## ACCEPTANCE CRITERIA

- [ ] At least 3 seasonal events exist with distinct effects
- [ ] Event banner displays at start of event
- [ ] Event effects correctly modify sell prices / production speed
- [ ] Event duration tracked in days, visible in HUD
- [ ] Event ends cleanly — effects removed, summary shown
- [ ] Event history saved in `GameData`
- [ ] Player with insufficient Fame does not see inaccessible events

---

## HOW IT CONNECTS TO THE FUTURE

```
  Events System
       │
       ├── Tourists surge during events (TOURISTS doc)
       ├── Prestige: Event rewards carry through resets as trophies
       ├── Premium currency: Limited-edition cosmetics during events
       ├── Social: Community events ("Town reached 10M bottles!")
       └── Live Service: Server-driven events without app update
```
