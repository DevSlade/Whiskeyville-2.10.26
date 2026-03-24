# ⭐ Feature Spec: Fame Currency
### Phase 2 — Content Expansion | Priority: Post-MVP
### Target: Q2 2026

---

## CONCEPT

Fame is a **second currency** in Whiskeyville — distinct from Cash. While Cash measures
economic output (how much whiskey you sold), Fame measures **reputation** (how good your
whiskey is, how many people have heard of your distillery).

Fame unlocks things Cash cannot: rare buildings, visitor types, seasonal events, and
eventually Prestige resets.

```
  CASH  → Lets you build more of what you have
  FAME  → Lets you build things you couldn't before
```

---

## THE FAME LOOP

```
  High-Quality Whiskey Sold
           │
           ▼
  Fame Points Earned  (+1 Fame per Aged Whiskey, +5 per special batch)
           │
           ▼
  Fame Milestone Reached  (100 / 500 / 1000 / 5000 Fame)
           │
           ▼
  New Building / Visitor Type Unlocked
           │
           ▼
  More Tourists Visit → More Sales → More Fame
```

---

## FAME SOURCES

| Action | Fame Earned | Notes |
|--------|------------|-------|
| Sell 1 Aged Whiskey | +1 Fame | Base rate |
| Sell via Saloon (all at once) | +1 Fame per unit | Same |
| Sell a "Premium Batch" (future aging mechanic) | +5 Fame | Quality modifier |
| Complete a Visitor Quest | +10–25 Fame | Phase 3 |
| Host a Seasonal Event | +50–200 Fame | Phase 3 |
| Prestige reset bonus | +500 Fame (carry over) | Phase 4 |

---

## FAME MILESTONES (Unlock Tree Preview)

| Fame Milestone | Unlock |
|---------------|--------|
| 50 Fame | Decoration: Wooden Fence |
| 100 Fame | Building: Tasting Room |
| 250 Fame | Visitor Type: Whiskey Enthusiast |
| 500 Fame | Building: Barrel Art Wall (decoration) |
| 1,000 Fame | Event: Annual Barrel Tasting |
| 2,500 Fame | Building: Export Dock |
| 5,000 Fame | Prestige Unlock |

---

## IMPLEMENTATION

### Add to `InventoryManager.cs`
```csharp
public const string RESOURCE_FAME = "Fame";
// Existing pattern handles rest — Fame is just another resource key
```

### Add to `SellManager.cs`
```csharp
// After selling Aged Whiskey:
int fameEarned = amountSold * 1; // base 1:1
InventoryManager.Instance.Add(RESOURCE_FAME, fameEarned);
// Fire OnFameMilestoneReached if threshold crossed
```

### New: `FameMilestoneManager.cs`
- Subscribes to `InventoryManager.OnResourceChanged`
- Checks Fame total against milestone thresholds
- Fires `OnMilestoneReached(int milestone)` event
- Connects to `UnlockManager.cs` (see PROGRESSION doc)

### `ResourceUI.cs` Addition
- Add Fame display to HUD (star icon + number)
- Optional: Fame meter bar that fills between milestones

---

## VISUAL DESIGN

```
  HUD with Fame:
  ┌──────────────────────────────────────────────────┐
  │  💰 Cash: 450   🌽 Corn: 12   ⭐ Fame: 247      │
  │  ━━━━━━━━━━━━━━━━━━━━━━━━░░░░░░  247/250 fame   │
  │                           ▲                     │
  │                     Next unlock: Enthusiast Visitor │
  └──────────────────────────────────────────────────┘
```

---

## ACCEPTANCE CRITERIA

- [ ] Fame appears as a resource in `InventoryManager`
- [ ] Fame is earned when selling Aged Whiskey
- [ ] Fame is displayed in the HUD
- [ ] Fame is saved and loaded correctly
- [ ] Milestone thresholds trigger `OnMilestoneReached` events
- [ ] First milestone (50 Fame) triggers visible unlock notification
- [ ] Fame resets or carries forward correctly on Prestige (Phase 4)

---

## HOW IT CONNECTS TO THE FUTURE

```
  Fame Currency
       │
       ├── Unlocks: Progression/Unlock tree gates (PROGRESSION doc)
       ├── Drives: Tourist visit frequency (TOURISTS doc)
       ├── Scores: Prestige ranking (PRESTIGE doc)
       └── Events: Seasonal events require minimum Fame to trigger (EVENTS doc)
```
