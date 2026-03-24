# 👥 Feature Spec: Tourists & NPCs
### Phase 3 — Social & Live Layer | Priority: Post-MVP
### Target: Q3 2026

---

## CONCEPT

Tourists are the living proof that your distillery is famous. NPCs are the people who give
your town personality and give you things to do. Together they transform Whiskeyville from
a production simulator into a **living town you care about**.

```
  WITHOUT TOURISTS:   I am producing whiskey in a vacuum.
  WITH TOURISTS:      People are coming to MY town to taste MY whiskey.
```

---

## TOURIST SYSTEM

### How Tourists Work

```
  Player has 250+ Fame
        │
        ▼
  Tourists begin appearing on the grid
  (animated sprite, walks toward Saloon)
        │
        ▼
  Tourist reaches Saloon
        │
        ▼
  Tourist "orders" 1–3 Aged Whiskey
  InventoryManager.Consume("AgedWhiskey", order)
  InventoryManager.Add("Cash", order * sellPrice)
  InventoryManager.Add("Fame", order * 2)
        │
        ▼
  Tourist leaves (walks off grid edge)
  New tourist spawns after cooldown
```

### Tourist Types

| Type | Fame Req | Cash Paid | Fame Given | Sprite | Notes |
|------|---------|----------|-----------|--------|-------|
| Passerby | 50+ | 30/unit | 1/unit | Simple traveler | Just passing through |
| Enthusiast | 250+ | 50/unit | 3/unit | Nice clothes | Seeks you out specifically |
| Critic | 1,000+ | 100/unit | 10/unit | Notebook in hand | Review affects Fame burst |
| Celebrity | 5,000+ | 500 flat | 50 flat | Fancy | Rare, one per season |
| Journalist | 2,500+ | 0 | +100 burst | Camera | Triggers Fame article event |

### Tourist Pathfinding
- Simple grid-based pathfinding (A* or BFS)
- Tourists walk from grid edge → Saloon → back to edge
- Avoid occupied building tiles (walk around them)
- Visual: Walking animation on sprite, direction-dependent flip

---

## NPC SYSTEM

NPCs are **named characters** who live in your town and give **quests**. Distinct from tourists
who are anonymous visitors.

### Core NPCs (Phase 3 Launch)

| NPC | Role | Quest Examples |
|-----|------|---------------|
| **Hank** | The Old Distiller | "Make me 5 Aged Whiskey in one day" |
| **Mae** | The Saloon Keeper | "Sell 20 whiskeys this week" |
| **Earl** | The Barrel Maker | "Chop 15 trees for me" |
| **Dolly** | The Town Reporter | "Reach 500 Fame" → triggers Journalist tourist |

### Quest Structure
```csharp
public class Quest
{
    public string questGiverName;
    public string description;
    public QuestObjective objective; // type: Produce / Sell / Earn / Build
    public int targetAmount;
    public QuestReward reward;       // Cash / Fame / Unlock
    public bool isComplete;
}
```

### Quest Lifecycle
```
  NPC appears on grid (at 100 Fame)
         │
         ▼
  Player taps NPC → Quest dialog opens
         │
         ▼
  Player accepts quest
         │
         ▼
  Quest tracked passively (no additional input needed)
  Progress shown in Quest Log panel
         │
         ▼
  Objective complete → Quest complete notification
  Tap NPC to claim reward
```

---

## UI COMPONENTS NEEDED

| Component | Purpose |
|-----------|---------|
| `NPCBehavior.cs` | Walk, idle, interact animations |
| `QuestManager.cs` | Track active quests, detect completion |
| `QuestDialogUI.cs` | Dialog panel for quest accept/decline |
| `QuestLogUI.cs` | HUD panel showing active quest progress |
| `TouristBehavior.cs` | Walk to Saloon, consume whiskey, leave |
| `TouristSpawner.cs` | Spawn tourists based on Fame level |

---

## VISUAL DESIGN

```
  Town with tourists:

  🌾  🌾  🌿  🌿  🌿
  🌾  [Mash]  🌿  🏚[Rick]
  🌿  🌿  🏭[Still]  🌿
  🌿  [Coop]  🌿  🎪[Salon]
  🌿  🌿  🌿  👤→→→→→🚶

  👤 = tourist walking toward Saloon
  🚶 = another tourist leaving
```

---

## ACCEPTANCE CRITERIA

**Tourists:**
- [ ] Tourists spawn on grid edge when Fame >= 250
- [ ] Tourists pathfind to Saloon (avoid buildings)
- [ ] Tourists consume Aged Whiskey and pay Cash automatically
- [ ] Tourists leave after purchase
- [ ] Tourist frequency scales with Fame level

**NPCs:**
- [ ] At least 2 named NPCs appear at 100 Fame
- [ ] Tapping an NPC opens quest dialog
- [ ] Quest objective is tracked passively
- [ ] Completing quest grants reward and NPC dialogue changes

---

## HOW IT CONNECTS TO THE FUTURE

```
  Tourists & NPCs
       │
       ├── Events: Tourists surge during seasonal events (EVENTS doc)
       ├── Prestige: All tourist types unlocked faster on 2nd run
       ├── Fame: More tourist types → more fame per day
       └── Isometric: Tourist sprites become full characters in iso art style
```
