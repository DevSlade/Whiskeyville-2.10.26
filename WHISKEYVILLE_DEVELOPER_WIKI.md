# 🥃 WHISKEYVILLE — Developer Wiki
### *Project Foundry Engine · Build 2.10.26 · Slade Empire*

> **Classification:** Internal Development Reference  
> **Engine:** Unity 6 (6000.0.28f1) · 2D · C# · Mono  
> **Target:** Windows, Mac, Linux (DX11)  
> **Last Audit:** March 24, 2026  
> **Total Codebase:** ~4,950 lines across 31 C# scripts  

---

## Table of Contents

1. [The Core Engine (Project Foundry Mechanics)](#1-the-core-engine-project-foundry-mechanics)
   - 1.1 [Decoupled Systems & Manager Architecture](#11-decoupled-systems--manager-architecture)
   - 1.2 [The Singleton Network](#12-the-singleton-network)
   - 1.3 [The Event Bus](#13-the-event-bus)
   - 1.4 [The Fluid Matrix (Production Thermodynamics)](#14-the-fluid-matrix-production-thermodynamics)
   - 1.5 [Chrono-Maturation (Barrel Aging & Time)](#15-chrono-maturation-barrel-aging--time)
   - 1.6 [The Grid Engine (Terrain Generation)](#16-the-grid-engine-terrain-generation)
   - 1.7 [Persistence Layer (Save/Load Architecture)](#17-persistence-layer-saveload-architecture)
   - 1.8 [Visual Systems (Day/Night, Popups, Sorting)](#18-visual-systems-daynight-popups-sorting)
2. [Game Flow & The Tourist Economy](#2-game-flow--the-tourist-economy)
   - 2.1 [The Player Loop](#21-the-player-loop)
   - 2.2 [The Micro-Economy](#22-the-micro-economy)
   - 2.3 [The Physical Bridge (QR/Webhook)](#23-the-physical-bridge-qrwebhook)
3. [Narrative & World-Building](#3-narrative--world-building)
   - 3.1 [The Setting](#31-the-setting)
   - 3.2 [Characters & Factions](#32-characters--factions)
4. [Feature Classification Ledger](#4-feature-classification-ledger)
   - 4.1 [Confirmed (Production Ready)](#41-confirmed-production-ready)
   - 4.2 [Experimental (Volatile)](#42-experimental-volatile)
   - 4.3 [Debug/Developer Tools](#43-debugdeveloper-tools)
5. [The QoL & Expansion Roadmap](#5-the-qol--expansion-roadmap)
   - 5.1 [Critical QoL Fixes](#51-critical-qol-fixes)
   - 5.2 [UI/UX Panels Needed](#52-uiux-panels-needed)
   - 5.3 [Automation & Data Cartridges](#53-automation--data-cartridges)
   - 5.4 [Expansion Vectors](#54-expansion-vectors)
6. [Technical Appendices](#6-technical-appendices)

---

# 1. The Core Engine (Project Foundry Mechanics)

## 1.1 Decoupled Systems & Manager Architecture

Whiskeyville is built on a **Manager-Singleton architecture** where each gameplay domain is owned by a single MonoBehaviour manager. These managers are loosely coupled — they communicate through **public method calls on singleton instances** and a lightweight **event delegate system**, rather than tight inheritance chains or direct field references.

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        GAME SCENE HIERARCHY                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │ GameInitializer│  │  GridManager  │  │ SaveManager  │                  │
│  │  (Bootstrap)  │  │  (Singleton)  │  │  (Singleton)  │                  │
│  │               │  │  20×15 Grid   │  │  JSON I/O    │                  │
│  │  Ensures:     │  │  Terrain Gen  │  │  whiskeyville │                  │
│  │  - Inventory  │  │  Obstacle Mgr │  │  _save.json  │                  │
│  │  - Audio      │  │               │  │               │                  │
│  └───────┬───────┘  └───────┬───────┘  └───────┬───────┘                  │
│          │                  │                  │                          │
│  ┌───────▼───────┐  ┌──────▼───────┐  ┌───────▼───────┐                  │
│  │ InventoryMgr  │  │BuildingDB    │  │  SellManager  │                  │
│  │ (Singleton)   │  │(Singleton)   │  │  (Singleton)  │                  │
│  │ Dict<str,int> │  │BuildingData[]│  │  $50/bottle   │                  │
│  │ OnResource ◄──┼──┤ 6 buildings  │  │               │                  │
│  │  Changed      │  │              │  │               │                  │
│  └───────┬───────┘  └──────┬───────┘  └───────────────┘                  │
│          │                 │                                             │
│  ┌───────▼───────┐  ┌─────▼────────┐  ┌──────────────┐                  │
│  │  UIManager    │  │BuildSelector │  │ AudioManager │                  │
│  │  (Singleton)  │  │(Singleton)   │  │ (Singleton)  │                  │
│  │  Panel States │  │ Hotkeys 1-6  │  │ DontDestroy  │                  │
│  │  Pause/Resume │  │ OnSelection  │  │ Music + SFX  │                  │
│  │  Build/Sell   │  │   Changed    │  │              │                  │
│  └───────────────┘  └──────────────┘  └──────────────┘                  │
│                                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │ BuildingPlace │  │ DayNightCycle│  │ PopupPool    │                  │
│  │  mentManager │  │ (Visual Only)│  │ (Singleton)  │                  │
│  │ Raycast+Place│  │ 120s Cycle   │  │ Object Pool  │                  │
│  │              │  │ 4 Phases     │  │ 10 initial   │                  │
│  └──────────────┘  └──────────────┘  └──────────────┘                  │
│                                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │CameraControll│  │ DevTools     │  │TileHighlight │                  │
│  │  Pan + Zoom  │  │ F1-F10 Keys  │  │ Controller   │                  │
│  │  RMB Drag    │  │ Debug Only   │  │ Per-frame    │                  │
│  │  Scroll Wheel│  │              │  │ Raycast      │                  │
│  └──────────────┘  └──────────────┘  └──────────────┘                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Manager Responsibilities

| Manager | File | Lines | Singleton | Primary Responsibility |
|---------|------|-------|-----------|----------------------|
| **GridManager** | `GridManager.cs` | 334 | ✅ | Terrain generation, tile grid, coordinate math |
| **InventoryManager** | `InventoryManager.cs` | 169 | ✅ | Resource state (SSOT), event broadcasting |
| **SaveManager** | `SaveManager.cs` | 285 | ✅ | JSON persistence, game state serialization |
| **UIManager** | `UIManager.cs` | 352 | ✅ | Panel state machine, pause system, input routing |
| **AudioManager** | `AudioManager.cs` | 304 | ✅ | Music/SFX playback, volume control, cross-scene persistence |
| **BuildingDatabase** | `BuildingDatabase.cs` | 59 | ✅ | ScriptableObject registry, O(1) building lookup |
| **BuildingSelector** | `BuildingSelector.cs` | 143 | ✅ | Hotkey input (1-6), selection state, event dispatch |
| **SellManager** | `SellManager.cs` | 145 | ✅ | Whiskey-to-cash conversion, sell price config |
| **ProductionPopupPool** | `ProductionPopupPool.cs` | 111 | ✅ | Object pooling for floating text animations |
| **BuildingPlacementManager** | `BuildingPlacementManager.cs` | 190 | ❌ | Raycast validation, prefab instantiation, cost deduction |
| **CameraController** | `CameraController.cs` | 79 | ❌ | Pan (RMB drag), zoom (scroll wheel) |
| **TileHighlightController** | `TileHighlightController.cs` | 177 | ❌ | Per-frame tile hover highlighting |
| **DayNightCycle** | `DayNightCycle.cs` | 143 | ❌ | Visual-only 120s day/night color cycle |
| **GameInitializer** | `GameInitializer.cs` | 75 | ❌ | Bootstrap: ensures InventoryManager + AudioManager exist |
| **DevTools** | `DevTools.cs` | 99 | ❌ | F-key debug commands |

---

## 1.2 The Singleton Network

All core managers use the same singleton pattern:

```csharp
public static T Instance { get; private set; }

private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        // AudioManager only: DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
```

**Key distinction:** Only `AudioManager` uses `DontDestroyOnLoad` to persist across scene transitions (IntroLoading → MainMenu → GameScene). All other singletons are scene-scoped and recreated on scene load.

**Bootstrap Order:**
1. `GameInitializer.Awake()` — Ensures `InventoryManager` and `AudioManager` exist (via prefab instantiation or `AddComponent`)
2. All other singletons self-initialize in their own `Awake()` methods
3. `SaveManager.Start()` — Attempts to load grid seed from save file, triggering grid regeneration if save exists

---

## 1.3 The Event Bus

Whiskeyville uses a **lightweight event system** built on C# delegates/events. There are exactly **two event channels** in the current build:

### Event 1: `InventoryManager.OnResourceChanged`

```
Signature:  event Action<string, int> OnResourceChanged
Fired When: Any resource quantity changes (AddResource, SetResource)
Parameters: (resourceName, newAmount)
```

| Subscriber | Reaction |
|-----------|----------|
| `ResourceUI` | Updates all 7 HUD resource text labels |
| `SellPanelUI` | Refreshes stock count, total value, button state |
| `SelectedBuildingUI` | Re-evaluates affordability, updates text color |
| `BuildPanelUI` | Refreshes button enable/disable states based on cash |

### Event 2: `BuildingSelector.OnSelectionChanged`

```
Signature:  event Action<int> OnSelectionChanged
Fired When: Player selects a building (hotkey 1-6 or UI button click)
Parameters: (selectedBuildingIndex)
```

| Subscriber | Reaction |
|-----------|----------|
| `SelectedBuildingUI` | Updates "Selected: Name ($Cost)" display with affordability color |
| `BuildPanelUI` | Highlights the selected button in the build panel |

All other inter-system communication uses **direct singleton method calls** (e.g., `InventoryManager.Instance.AddResource()`), which creates implicit coupling but keeps the system simple.

---

## 1.4 The Fluid Matrix (Production Thermodynamics)

The production system is the mathematical heart of Whiskeyville. Each building operates as an **autonomous production node** running an infinite coroutine loop.

### The Production Chain

```
                    ┌─────────┐
                    │  FIELD  │ (Crop, click-to-harvest)
                    │ Cost:$50│
                    │ Output: │
                    │ 1 Corn  │
                    │ per 5s  │
                    └────┬────┘
                         │ Corn
                    ┌────▼────┐
                    │ MASHTUN │ (Auto-producer)
                    │Cost:$75 │
                    │ 2 Corn  │──► 1 Mash
                    │ per 8s  │
                    └────┬────┘
                         │ Mash
                    ┌────▼────┐             ┌───────────┐
                    │  STILL  │             │ COOPERAGE │
                    │Cost:$150│             │ Cost:$100 │
                    │ 2 Mash  │──► 1 Whiskey│ 2 Wood ──►│ 1 Barrel
                    │ per 10s │             │ per 5s    │
                    └────┬────┘             └─────┬─────┘
                         │ Whiskey                │ Barrel
                         │    ┌───────────┐       │
                         └───►│ RICKHOUSE │◄──────┘
                              │ Cost:$250 │
                              │ 1 Whiskey │
                              │ 1 Barrel  │──► 1 Aged Whiskey
                              │ per 8s    │    (DUAL INPUT)
                              └─────┬─────┘
                                    │ Aged Whiskey
                              ┌─────▼─────┐
                              │  SALOON   │
                              │ Cost:$300 │
                              │  Click to │──► Sell Panel UI
                              │  open sell│    $50 per bottle
                              │  panel    │
                              └───────────┘
```

### Building Data Cartridges (ScriptableObject Values)

| Building | Cost | Input 1 | Qty | Input 2 | Qty | Output | Qty | Interval | Type |
|----------|------|---------|-----|---------|-----|--------|-----|----------|------|
| **Field** | $50 | — | — | — | — | Corn | 1 | 5s | Crop (click harvest) |
| **Mash Tun** | $75 | Corn | 2 | — | — | Mash | 1 | 8s | Auto-producer |
| **Still** | $150 | Mash | 2 | — | — | Whiskey | 1 | 10s | Auto-producer |
| **Cooperage** | $100 | Wood | 2 | — | — | Barrel | 1 | 5s | Auto-producer |
| **Rickhouse** | $250 | Whiskey | 1 | Barrel | 1 | Aged Whiskey | 1 | 8s | Dual-input auto |
| **Saloon** | $300 | — | — | — | — | — | — | — | Sell interface |

### Production Cycle Algorithm (`BuildingBehavior.TryProduce()`)

```
EVERY [productionInterval] SECONDS:
│
├── IF requiresDualInput:
│   ├── CHECK: HasResource(input1, amount1) AND HasResource(input2, amount2)
│   │   ├── BOTH TRUE:
│   │   │   ├── AddResource(input1, -amount1)    // Consume input 1
│   │   │   ├── AddResource(input2, -amount2)    // Consume input 2
│   │   │   ├── _isWaitingForInput = false
│   │   │   └── CONTINUE TO OUTPUT ▼
│   │   └── EITHER FALSE:
│   │       ├── _isWaitingForInput = true
│   │       └── RETURN (skip this cycle)
│   │
├── ELSE IF requiresInput:
│   ├── CHECK: HasResource(input1, amount1)
│   │   ├── TRUE:
│   │   │   ├── AddResource(input1, -amount1)    // Consume input
│   │   │   ├── _isWaitingForInput = false
│   │   │   └── CONTINUE TO OUTPUT ▼
│   │   └── FALSE:
│   │       ├── _isWaitingForInput = true
│   │       └── RETURN (skip this cycle)
│
├── OUTPUT:
│   ├── AddResource(outputResource, +outputAmount)
│   ├── ShowPopup("+{amount} {resource}", position, color)
│   ├── PlaySFX("Collect")
│   └── Log production event
```

### Resource Flow Mathematics

**Steady-State Production Rates** (per building, per second):

| Building | Output Rate | Input Consumption Rate |
|----------|-------------|----------------------|
| Field | 0.2 Corn/s | — |
| Mash Tun | 0.125 Mash/s | 0.25 Corn/s |
| Still | 0.1 Whiskey/s | 0.2 Mash/s |
| Cooperage | 0.2 Barrel/s | 0.4 Wood/s |
| Rickhouse | 0.125 AgedWhiskey/s | 0.125 Whiskey/s + 0.125 Barrel/s |

**Bottleneck Analysis — Minimum buildings for saturation:**

To produce 1 Aged Whiskey every 8 seconds (1 Rickhouse), you need:
- **Whiskey supply:** 1 Whiskey per 8s → 1 Still produces 1 per 10s → **need ~1.25 Stills ≈ 2 Stills**
- **Mash supply:** 2 Stills × 2 Mash per 10s = 0.4 Mash/s → 1 Mash Tun produces 0.125/s → **need ~3.2 Mash Tuns ≈ 4 Mash Tuns**
- **Corn supply:** 4 Mash Tuns × 2 Corn per 8s = 1 Corn/s → 1 Field produces 0.2/s → **need 5 Fields**
- **Barrel supply:** 1 Barrel per 8s → 1 Cooperage produces 1 per 5s → **need ~0.63 Cooperage ≈ 1 Cooperage**
- **Wood supply:** 1 Cooperage × 2 Wood per 5s = 0.4 Wood/s → Trees produce 1 Wood per click (+ 30s respawn)

**Minimum viable distillery for 1 Rickhouse:**
> 5 Fields + 4 Mash Tuns + 2 Stills + 1 Cooperage + 1 Rickhouse + 1 Saloon = **14 buildings, $1,775 total cost**

### Crop System (Fields — `CropBehavior.cs`)

Fields use a **click-to-harvest** model rather than auto-production:

```
LIFECYCLE:
  StartGrowth() → Activate stage 0 → Begin GrowthLoop()
  
  GrowthLoop():
    FOR each stage in _growthStages[]:
      WAIT _growthInterval seconds (default: 5s)
      Deactivate current stage visual
      Activate next stage visual
      _currentStage++
    END
    _isFullyGrown = true    // Ready for harvest
  
  OnMouseDown() → TryHarvest():
    IF _isFullyGrown:
      AddResource(_harvestResource, _harvestAmount)
      ShowPopup("+1 Corn", position, yellow)
      PlaySFX("Collect")
      ResetGrowth()        // Restart from stage 0
    ELSE:
      PlaySFX("Error")     // Not ready yet
```

**Total growth time per harvest:** `_growthInterval × (_growthStages.Length - 1)` = 5s × (stages - 1)

---

## 1.5 Chrono-Maturation (Barrel Aging & Time)

### Current Implementation: Simulated Aging via Production Interval

The Rickhouse building currently implements barrel aging as a **fixed-interval production cycle** (8 seconds). There is **no true offline/real-time aging system** in the current build. The aging process is:

```
Every 8 seconds:
  IF HasResource("Whiskey", 1) AND HasResource("Barrel", 1):
    Consume 1 Whiskey + 1 Barrel
    Produce 1 Aged Whiskey
```

### Angel's Share — NOT IMPLEMENTED

There is currently **no Angel's Share evaporation mechanic** in the codebase. The concept (where a percentage of aging whiskey is lost over time due to evaporation) is referenced in the project's design vision but has zero code support.

**Recommended Implementation Path:**
- Add a `float evaporationRate` field to `BuildingData` (default: 0.02 = 2% per cycle)
- In `TryProduce()`, apply `outputAmount *= (1.0f - evaporationRate)` with accumulation
- Introduce a `float _accumulatedOutput` field for fractional tracking
- When `_accumulatedOutput >= 1.0f`, produce 1 unit and subtract

### Day/Night Cycle (`DayNightCycle.cs`)

The day/night system is **purely visual** — it has zero gameplay impact. No production bonuses, no tourist schedules, no aging multipliers.

```
Cycle Duration:    120 seconds (configurable)
Phases:            4 equal phases (30s each)
Start Position:    25% (morning/midday)

Phase Map:
  t = 0.00 → 0.25   Sunrise → Midday    (Sky: Orange → Blue)
  t = 0.25 → 0.50   Midday → Sunset     (Sky: Blue → Orange)
  t = 0.50 → 0.75   Sunset → Midnight   (Sky: Orange → Dark Blue)
  t = 0.75 → 1.00   Midnight → Sunrise  (Sky: Dark Blue → Orange)

Color Values:
  Day:     RGB(0.53, 0.81, 0.92)  — Bright sky blue
  Sunset:  RGB(1.00, 0.55, 0.20)  — Warm orange
  Night:   RGB(0.05, 0.05, 0.20)  — Deep navy

Overlay Tint (optional):
  Day:     RGBA(1, 1, 1, 0.0)     — Fully transparent
  Night:   RGBA(0.1, 0.1, 0.3, 0.35) — 35% blue overlay
```

---

## 1.6 The Grid Engine (Terrain Generation)

### Grid Specifications

| Parameter | Value | Notes |
|-----------|-------|-------|
| Grid Width | 20 tiles | X-axis |
| Grid Height | 15 tiles | Y-axis |
| Tile Size | 1.0 Unity units | World space scale |
| Total Tiles | 300 | 20 × 15 |
| Dirt Chance | 12% | Non-buildable terrain variety |
| Tree Chance | 8% | Harvestable obstacle (Wood) |
| Rock Chance | 5% | Non-harvestable obstacle |

### Generation Algorithm (`GridManager.GenerateGrid()`)

```
INPUT: seed (int) — if < 0, generate random seed

1. INITIALIZE:
   Random.InitState(seed)
   Create _tiles[gridWidth, gridHeight] array
   Create tile container + obstacle container GameObjects

2. PASS 1 — TERRAIN TILES (20×15 = 300 iterations):
   FOR y = 0 to gridHeight:
     FOR x = 0 to gridWidth:
       roll = Random.value
       IF roll < _dirtChance (0.12):
         terrain = Dirt (non-buildable)
         tile.SetOccupied(true)
       ELSE:
         terrain = Grass (buildable)
       
       Instantiate tile prefab at (x * tileSize, y * tileSize, 0)
       tile.Initialize(x, y, terrain)

3. PASS 2 — OBSTACLES (on Grass tiles only):
   FOR each unoccupied Grass tile:
     roll = Random.value
     IF roll < _treeChance (0.08):
       Spawn Tree prefab → tile.SetOccupied(true)
     ELSE IF roll < _treeChance + _rockChance (0.13):
       Spawn Rock prefab → tile.SetOccupied(true)
```

### Coordinate Systems

```
Grid → World:    worldPos = (gridX * tileSize, gridY * tileSize, 0)
World → Grid:    gridPos = (floor(worldX / tileSize), floor(worldY / tileSize))
```

### Depth Sorting (Y-axis based)

```
sortOrder = (gridHeight - gridY) * SORT_MULTIPLIER
         = (15 - y) * 10

Example:
  y=0  (bottom row):  sortOrder = 150  (renders in front)
  y=14 (top row):     sortOrder = 10   (renders behind)
```

**Sorting Layers:**
- `"Ground"` — Crop soil sprites (sortOrder: 1)
- `"Buildings"` / `"Objects"` — All buildings and crop growth stages (Y-sorted)
- `"UI"` — Production popups (sortOrder: 100)

### Seed System

The grid seed is saved and restored, ensuring **deterministic world regeneration**. The same seed always produces the same terrain layout, obstacle placement, and tile distribution.

```
Save: GameData.gridSeed = GridManager.CurrentSeed
Load: GridManager.RegenerateWithSeed(savedSeed)
```

---

## 1.7 Persistence Layer (Save/Load Architecture)

### File Format

```
Path:     Application.persistentDataPath + "/whiskeyville_save.json"
Format:   JSON (Unity JsonUtility, pretty-printed)
Encoding: UTF-8
```

### Save Schema (`GameData`)

```json
{
  "gridSeed": 12345,
  "cash": 200,
  "corn": 0,
  "mash": 0,
  "whiskey": 0,
  "agedWhiskey": 0,
  "wood": 0,
  "barrel": 0,
  "buildings": [
    {
      "buildingIndex": 0,
      "posX": 5.0,
      "posY": 3.0,
      "gridX": 5,
      "gridY": 3,
      "isCrop": true,
      "growthStage": 2,
      "isFullyGrown": false
    }
  ]
}
```

### Save Flow

```
SaveManager.SaveGame():
  1. Create new GameData()
  2. data.gridSeed = GridManager.Instance.CurrentSeed
  3. FOR EACH of 7 resources:
       data.{resource} = InventoryManager.Instance.GetResource(name)
  4. Find all BuildingBehavior[] in scene:
       Create BuildingSaveData(index, worldPos, gridPos, false, 0, false)
  5. Find all CropBehavior[] in scene:
       Create BuildingSaveData(index, worldPos, gridPos, true, currentStage, isFullyGrown)
  6. JsonUtility.ToJson(data, prettyPrint: true)
  7. File.WriteAllText(savePath, json)
```

### Load Flow

```
SaveManager.LoadGame() → bool:
  1. File.ReadAllText(savePath)
  2. JsonUtility.FromJson<GameData>(json)
  3. ClearExistingBuildings():
       Destroy all BuildingBehavior GameObjects
       Destroy all CropBehavior GameObjects
  4. GridManager.Instance.RegenerateWithSeed(data.gridSeed)
  5. FOR EACH of 7 resources:
       InventoryManager.Instance.SetResource(name, data.value)
  6. LoadBuildings():
       FOR EACH BuildingSaveData in data.buildings:
         prefab = BuildingDatabase.Instance.GetBuilding(buildingIndex)
         IF isCrop:
           Instantiate → CropBehavior.RestoreGrowthState(stage, fullyGrown)
         ELSE:
           Instantiate → BuildingBehavior.Initialize(buildingData)
         tile.SetOccupied(true)
  7. Return true (or false on error)
```

### Startup Behavior

On `SaveManager.Start()`, the system calls `TryLoadGridSeed()`:
- If a save file exists: loads the seed and calls `GridManager.RegenerateWithSeed()` to ensure the terrain matches the saved layout
- If no save file: allows `GridManager` to generate a fresh random world

---

## 1.8 Visual Systems (Day/Night, Popups, Sorting)

### Production Popup Animation (`ProductionPopup.cs`)

Floating text that appears when resources are produced/collected:

```
Duration: 1.5 seconds
Random X Offset: ±0.3 units (horizontal spawn variation)

ANIMATION TIMELINE:
  t = 0.0-0.2   Scale UP:   0.10 → 0.15  (pop-in)
  t = 0.2-0.4   Scale DOWN: 0.15 → 0.12  (settle)
  t = 0.4-0.6   Scale HOLD: 0.12          (readable)
  t = 0.0-0.6   Alpha:      1.0           (fully visible)
  t = 0.6-1.0   Alpha FADE: 1.0 → 0.0    (fade out)
  t = 0.0-1.0   Position:   RISE 1.0 units vertically (linear)

TextMesh Properties:
  Anchor:     MiddleCenter
  FontSize:   32
  FontStyle:  Bold
  SortLayer:  "UI"
  SortOrder:  100
```

### Popup Color Coding

| Resource | Color (RGB) | Visual |
|----------|------------|--------|
| Corn | (1.00, 0.92, 0.02) | 🟡 Bright Yellow |
| Mash | (1.00, 0.56, 0.02) | 🟠 Orange |
| Whiskey | (0.02, 1.00, 0.86) | 🩵 Teal/Cyan |
| Barrel | (0.46, 0.31, 0.14) | 🟤 Brown |
| Aged Whiskey | (1.00, 0.82, 0.02) | 🟡 Gold |
| Wood (trees) | (0.55, 0.27, 0.07) | 🟤 Dark Brown |
| Cash (sell) | (0.00, 0.80, 0.00) | 🟢 Green |

### Object Pool (`ProductionPopupPool.cs`)

```
Initial Pool Size: 10 popups (pre-allocated)
Growth Strategy:   Dynamic — creates new popup if all 10 are active
Recycling:         Popups deactivate themselves after animation → returned to pool
```

---

# 2. Game Flow & The Tourist Economy

## 2.1 The Player Loop

### Complete Physical Flow

```
╔══════════════════════════════════════════════════════════════════╗
║                    WHISKEYVILLE PLAYER LOOP                      ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          ║
║  │ 1. SOURCE   │    │ 2. PLANT    │    │ 3. HARVEST  │          ║
║  │             │    │             │    │             │          ║
║  │ Chop Trees  │───►│ Buy Fields  │───►│ Click Ripe  │          ║
║  │ for Wood    │    │ ($50 each)  │    │ Crops       │          ║
║  │ (click, 30s │    │ Place on    │    │ Wait 5s per │          ║
║  │  respawn)   │    │ Grass tiles │    │ growth stage│          ║
║  └─────────────┘    └─────────────┘    └──────┬──────┘          ║
║                                               │ Corn            ║
║  ┌─────────────┐    ┌─────────────┐    ┌──────▼──────┐          ║
║  │ 4. MASH     │    │ 5. DISTILL  │    │  (AUTO)     │          ║
║  │             │◄───│             │◄───│ Mash Tun    │          ║
║  │ Mash Tun    │    │ Still       │    │ 2 Corn →    │          ║
║  │ consumes    │    │ 2 Mash →    │    │ 1 Mash/8s   │          ║
║  │ 2 Corn/8s   │    │ 1 Whiskey   │    └─────────────┘          ║
║  └─────────────┘    │ per 10s     │                              ║
║                     └──────┬──────┘                              ║
║                            │ Whiskey                             ║
║  ┌─────────────┐    ┌──────▼──────┐    ┌─────────────┐          ║
║  │ 6. COOPERAGE│    │ 7. AGE      │    │ 8. SELL     │          ║
║  │             │───►│             │───►│             │          ║
║  │ 2 Wood →    │    │ 1 Whiskey + │    │ Click Saloon│          ║
║  │ 1 Barrel/5s │    │ 1 Barrel →  │    │ Sell panel  │          ║
║  │             │    │ 1 Aged      │    │ $50 per     │          ║
║  │             │    │ Whiskey/8s  │    │ bottle      │          ║
║  └─────────────┘    └─────────────┘    └──────┬──────┘          ║
║                                               │ Cash            ║
║                                        ┌──────▼──────┐          ║
║                                        │ 9. EXPAND   │          ║
║                                        │             │          ║
║                                        │ Buy more    │          ║
║                                        │ buildings,  │          ║
║                                        │ scale up    │──► LOOP  ║
║                                        │ production  │          ║
║                                        └─────────────┘          ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

### Scene Flow

```
App Launch → IntroLoading Scene
  │         (plays Untitled design.mp4)
  │         (skip after 1s, any key/click)
  ▼
MainMenu Scene
  │  ├── Play → GameScene
  │  ├── Settings → (TODO: not implemented)
  │  └── Quit → Application.Quit()
  │
  │  Music: "Menu" track (Tambul.mp3 or gone_fishin.mp3)
  ▼
GameScene
  │  ├── Gameplay (Build, Farm, Produce, Sell)
  │  ├── ESC → Pause Panel
  │  │    ├── Resume
  │  │    ├── Save (JSON to disk)
  │  │    ├── Load (JSON from disk)
  │  │    ├── Main Menu → MainMenu Scene
  │  │    └── Quit → Application.Quit()
  │  ├── B key → Toggle Build Panel
  │  └── Keys 1-6 → Select building hotkeys
  │
  │  Music: "Gameplay" track
  │  Time: Pauseable via Time.timeScale = 0/1
```

### Input Map

| Input | Context | Action |
|-------|---------|--------|
| **Left Click** | On Grass tile (building selected) | Place building |
| **Left Click** | On ripe crop | Harvest crop |
| **Left Click** | On tree | Chop for wood |
| **Left Click** | On Saloon building | Open sell panel |
| **Right Click + Drag** | Anywhere | Pan camera |
| **Scroll Wheel** | Anywhere | Zoom in/out (ortho 3-15) |
| **1-6 Keys** | Gameplay | Select building type |
| **B Key** | Gameplay | Toggle build panel |
| **ESC** | Gameplay | Pause / Close panels |
| **F1-F10** | DevTools enabled | Debug commands |

---

## 2.2 The Micro-Economy

### Resource Registry

| Resource | Key String | Starting Value | Source | Sink |
|----------|-----------|---------------|--------|------|
| **Cash** | `"Cash"` | 200 | Selling Aged Whiskey | Building costs |
| **Corn** | `"Corn"` | 0 | Field harvest (click) | Mash Tun (2 per cycle) |
| **Mash** | `"Mash"` | 0 | Mash Tun production | Still (2 per cycle) |
| **Whiskey** | `"Whiskey"` | 0 | Still production | Rickhouse (1 per cycle) |
| **Aged Whiskey** | `"AgedWhiskey"` | 0 | Rickhouse production | Sell via Saloon |
| **Wood** | `"Wood"` | 0 | Tree chopping (click) | Cooperage (2 per cycle) |
| **Barrel** | `"Barrel"` | 0 | Cooperage production | Rickhouse (1 per cycle) |

### Economic Constants

```
Sell Price per Aged Whiskey:  $50
Starting Cash:                $200
Building Costs:               $50 - $300
Total cost for minimum viable distillery: $1,775
```

### Sell Mechanics (`SellManager.cs`)

```
SellOne():
  IF HasResource("AgedWhiskey", 1):
    AddResource("AgedWhiskey", -1)
    AddResource("Cash", +50)
    ShowPopup("+$50")
    Return TRUE

SellAll():
  stock = GetResource("AgedWhiskey")
  IF stock > 0:
    AddResource("AgedWhiskey", -stock)
    AddResource("Cash", +stock × 50)
    ShowPopup("+${stock × 50}")
    Return stock
```

### Tourist AI / NPC Economy — NOT IMPLEMENTED

The current build has **no tourist AI, NPC behavior, budget allocation, or visitor system**. The Saloon building acts purely as a UI trigger for the sell panel. There is no:
- Tourist spawn/pathfinding logic
- Dynamic pricing based on demand
- Reputation or quality scoring
- Seasonal or event-based demand curves
- Illegal moonshining → legal distillery progression arc

The economy is currently a **static, player-driven resource conversion chain** with a fixed sell price.

---

## 2.3 The Physical Bridge (QR/Webhook)

### Status: NOT IMPLEMENTED

There is **zero webhook, API, QR code, or external service integration** in the current codebase. No HTTP clients, no REST endpoints, no URL handlers, no deep linking.

This remains a **design-phase concept** for future guerrilla marketing mechanics.

---

# 3. Narrative & World-Building

## 3.1 The Setting

### Environmental Evidence

Based on the asset composition, sprite libraries, and game naming conventions:

**Location:** Rural Appalachian/Southern United States — evocative of Lynchburg, Tennessee (home of Jack Daniel's) and the broader Appalachian moonshining tradition.

**Visual Vocabulary:**
- **Terrain:** Green grass tiles with brown dirt patches, trees, and rocks
- **Architecture:** Rustic wooden structures — barns, cooper shops, rickhouses
- **Flora:** Deciduous trees (oaks), bushes, corn fields
- **Palette:** Warm earth tones (browns, greens, golds)
- **Art Style:** Pixel art, top-down 2D, LPC (Liberated Pixel Cup) aesthetic with Kenney Tiny Town tileset accents

**Atmospheric Systems:**
- Day/night cycle with warm sunset oranges and deep midnight blues
- Southern country music tracks ("Tambul," "Gone Fishin'")
- Sound effects: wood chopping, hammering, typewriter clicks (invoking a hand-built, artisanal feel)

**Tonal Identity:** The game occupies the space between cozy farming sim (Stardew Valley) and idle resource management (Factory Town), wrapped in Southern whiskey heritage aesthetics.

### The World Rules (Implied)

1. The player is a sole proprietor building a whiskey operation from scratch
2. Resources are physical and tangible — corn grows in fields, wood comes from trees, barrels must be crafted
3. The land is finite (20×15 grid = 300 tiles) — placement is strategic
4. Time moves forward continuously (production never stops while unpaused)
5. The economic model is closed-loop — there is no external market simulation

---

## 3.2 Characters & Factions

### Status: NO NPC SYSTEMS EXIST

The current build contains **zero NPC logic, character definitions, dialogue systems, or faction mechanics**. There are no:
- Rival distilleries or competing AI
- Tax collectors, federal agents, or bureaucratic entities
- Customers with preferences or personality
- Quest givers or story NPCs
- Faction reputation systems

The player is currently the sole actor in the world. All interactions are with the grid, buildings, and resources.

### Implied Future Characters (from design vision)

Based on the game's thematic positioning and the whiskey-making pipeline:
- **Revenue Agents** — Federal officials who could inspect/shut down illegal operations
- **Rival Moonshiners** — Competing distillers who might sabotage or compete for resources
- **Tourists/Visitors** — NPCs who visit the Saloon and generate revenue
- **Suppliers** — NPCs who could sell rare grain varieties or equipment upgrades

---

# 4. Feature Classification Ledger

## 4.1 Confirmed (Production Ready)

These systems are fully coded, decoupled, tested in-editor, and stable:

| Feature | Components | Status | Notes |
|---------|-----------|--------|-------|
| **Grid Generation** | GridManager, TileBehavior, TerrainType | ✅ STABLE | Seeded RNG, deterministic regeneration |
| **Building Placement** | BuildingPlacementManager, BuildingSelector, BuildingDatabase | ✅ STABLE | Raycast validation, tile occupancy, cost deduction |
| **Production Chain** | BuildingBehavior (single + dual input) | ✅ STABLE | Coroutine-based, auto-producing, resource gating |
| **Crop System** | CropBehavior, FieldData | ✅ STABLE | Multi-stage growth, click-to-harvest, visual stages |
| **Tree Harvesting** | TreeBehavior | ✅ STABLE | Click-to-chop, 30s respawn, Wood yield |
| **Inventory System** | InventoryManager | ✅ STABLE | Dictionary-backed, event-driven, clamped to ≥0 |
| **Resource HUD** | ResourceUI | ✅ STABLE | 7 resource labels, event-subscribed, real-time updates |
| **Save/Load** | SaveManager, GameData | ✅ STABLE | JSON serialization, building + crop state restoration |
| **Sell System** | SellManager, SellPanelUI, Saloon click | ✅ STABLE | Sell one/all, dynamic UI, stock tracking |
| **Build Panel UI** | BuildPanelUI, SelectedBuildingUI | ✅ STABLE | Auto-generated buttons, affordability color coding |
| **Pause System** | UIManager, PausePanelUI | ✅ STABLE | Time.timeScale toggle, panel state machine |
| **Camera Controls** | CameraController | ✅ STABLE | RMB pan, scroll zoom, ortho 3-15 range |
| **Audio System** | AudioManager | ✅ STABLE | DontDestroyOnLoad, music + SFX, volume control |
| **Intro Video** | IntroVideoController | ✅ STABLE | Skip delay, transition guard, event-based |
| **Main Menu** | MainMenuManager | ✅ STABLE | Play/Settings/Quit, music integration |
| **Tile Highlighting** | TileHighlightController | ✅ STABLE | Per-frame raycast, green/red validation colors |
| **Production Popups** | ProductionPopup, ProductionPopupPool | ✅ STABLE | Object pooled, animated (scale + fade + rise) |
| **Day/Night Cycle** | DayNightCycle | ✅ STABLE | 120s visual cycle, 4-phase color interpolation |
| **ScriptableObject Data** | BuildingData (6 assets) | ✅ STABLE | All 6 buildings fully configured |
| **Bootstrap System** | GameInitializer | ✅ STABLE | Generic EnsureSingleton<T>() pattern |

---

## 4.2 Experimental (Volatile)

Features with partial implementation, tight coupling, or unverified logic:

| Feature | Status | Issue |
|---------|--------|-------|
| **Dual-Input Production (Rickhouse)** | ⚠️ EXPERIMENTAL | Logic exists and appears functional, but atomic consumption of 2 different resources in a single cycle is not unit-tested. Race conditions possible if resources change between the two `HasResource()` checks, though unlikely with single-threaded coroutines. |
| **Settings Panel** | ⚠️ STUB | `MainMenuManager.OpenSettings()` logs "Settings panel not yet implemented." Button exists in UI but has no target panel. |
| **Water Terrain Type** | ⚠️ DEFINED, UNUSED | `TerrainType.Water` exists in the enum but is never generated by `GridManager`. Zero tiles will ever be Water in the current build. |
| **Building Name Check (Saloon)** | ⚠️ TIGHT COUPLING | `BuildingBehavior.OnMouseDown()` uses a hardcoded string comparison `_buildingName == "Saloon"` to determine click behavior. This is fragile — renaming the Saloon asset breaks the sell panel integration. Should use a `BuildingType` enum or `isSellPoint` bool. |
| **Crop Sort Layer Split** | ⚠️ COMPLEX | Crops use a two-layer sorting approach (root on "Ground", children on "Objects") that works but is more complex than necessary and could cause visual artifacts with overlapping crops. |

---

## 4.3 Debug/Developer Tools

| Tool | Trigger | Function | File |
|------|---------|----------|------|
| **Add Cash** | F1 | `+100 Cash` | DevTools.cs |
| **Add Corn** | F2 | `+100 Corn` | DevTools.cs |
| **Add Mash** | F3 | `+100 Mash` | DevTools.cs |
| **Add Whiskey** | F4 | `+100 Whiskey` | DevTools.cs |
| **Add Aged Whiskey** | F5 | `+100 Aged Whiskey` | DevTools.cs |
| **Sell All** | F6 | `SellManager.SellAll()` | DevTools.cs |
| **Quick Save** | F7 | `SaveManager.SaveGame()` | DevTools.cs |
| **Quick Load** | F8 | `SaveManager.LoadGame()` | DevTools.cs |
| **Resource Dump** | F9 | Logs all 7 resources to console | DevTools.cs |
| **Reset Resources** | F10 | Cash→200, all others→0 | DevTools.cs |
| **Debug Logs** | Toggle | `enableDebugLogs` on TileHighlightController | TileHighlightController.cs |
| **DevTools Master Toggle** | Inspector | `_enableDevTools` bool | DevTools.cs |

**Security Note:** DevTools has a `_enableDevTools` master toggle (inspector-configurable) but there is **no build-stripping** — these tools will ship in release builds unless manually disabled or wrapped in `#if UNITY_EDITOR` / `#if DEBUG` preprocessor directives.

---

# 5. The QoL & Expansion Roadmap

## 5.1 Critical QoL Fixes

### P0 — Must Fix Before Release

| Issue | Location | Impact | Recommendation |
|-------|----------|--------|----------------|
| **DevTools in Release Builds** | `DevTools.cs` | Players can add infinite resources with F1-F5 | Wrap in `#if UNITY_EDITOR` or use `Debug.isDebugBuild` |
| **Hardcoded Saloon String** | `BuildingBehavior.cs:135` | Renaming Saloon asset breaks sell panel | Add `bool isSellPoint` to `BuildingData` ScriptableObject |
| **No Input Validation on Tile Click** | `BuildingPlacementManager.cs` | Clicking UI elements can accidentally trigger building placement | `IsPointerOverUI()` check exists but verify it covers all UI layers |
| **No Error Recovery on Save Corruption** | `SaveManager.cs` | Corrupted JSON crashes the load flow | Add try/catch around `JsonUtility.FromJson<>()`, implement backup saves |
| **Production Never Pauses** | `BuildingBehavior.cs` | Coroutines use `WaitForSeconds` which respects `Time.timeScale`, so pausing works. However, there's no way to individually pause/stop a building. | Add per-building pause toggle |
| **No Confirmation on Sell All** | `SellPanelUI.cs` | Accidentally selling entire stock is irreversible | Add confirmation dialog |

### P1 — High Priority

| Issue | Location | Impact | Recommendation |
|-------|----------|--------|----------------|
| **No Tutorial/Onboarding** | — | New players have no guidance on the production chain | Add tooltip system or guided first-build flow |
| **No Building Demolition** | — | Misplaced buildings cannot be removed; tiles are permanently occupied | Add demolish button with partial cost refund |
| **Trees Are Finite Visually** | `TreeBehavior.cs` | Trees respawn after 30s but during downtime the world looks barren | Consider wood purchase alternative or faster respawn |
| **No Production Queue Visibility** | — | Player cannot see which buildings are waiting for input | Add "waiting for resource" icon overlay on buildings |
| **Camera Has No Bounds** | `CameraController.cs` | Camera can pan infinitely past the grid | Clamp camera position to grid bounds |
| **No Resource Tooltips** | `ResourceUI.cs` | Player sees "Mash: 5" but doesn't know what Mash is for | Add hover tooltips explaining each resource |

---

## 5.2 UI/UX Panels Needed

| Panel | Purpose | Priority |
|-------|---------|----------|
| **Settings Panel** | Audio volume sliders, graphics options, keybind remapping | 🔴 HIGH — Button exists, panel is stub |
| **Building Info Panel** | Click any building to see its input/output, production rate, status | 🔴 HIGH |
| **Production Overview** | Dashboard showing all buildings, their status (producing/waiting), and rates | 🟡 MEDIUM |
| **Tutorial Overlay** | Step-by-step first-time player guide | 🟡 MEDIUM |
| **Confirmation Dialogs** | "Are you sure?" for sell all, demolish, quit | 🟡 MEDIUM |
| **Statistics Panel** | Total produced, total sold, play time, buildings placed | 🟢 LOW |
| **Minimap** | Small overview of entire grid with building positions | 🟢 LOW |
| **Achievement/Milestone Panel** | Track player progress milestones | 🟢 LOW |

---

## 5.3 Automation & Data Cartridges

### Data Cartridges to Build

| Cartridge | Format | Purpose |
|-----------|--------|---------|
| **Building Definitions** | JSON/ScriptableObject | Currently 6 hardcoded assets. Should support dynamic loading for modding/expansion |
| **Recipe Definitions** | JSON | Decouple input/output recipes from BuildingData. Allow multiple recipes per building |
| **Terrain Generation Rules** | JSON | Make dirt%, tree%, rock% configurable without code changes |
| **Economy Balancing Sheet** | JSON | Sell prices, building costs, production intervals — all in one tunable config |
| **Audio Manifest** | JSON | Map SFX/Music names to clip paths for runtime audio swapping |
| **Localization Strings** | JSON | All UI text currently hardcoded. Extract to localization file |

### Automation Systems to Build

| System | Description |
|--------|-------------|
| **Auto-Harvester** | Crops auto-harvest when ripe (toggle or building upgrade) |
| **Resource Alerts** | Notification when a resource hits 0 or a building is starved |
| **Production Chains Visualizer** | Show animated arrows between connected buildings |
| **Batch Building** | Place multiple buildings of the same type without re-selecting |
| **Speed Controls** | 1×, 2×, 3× game speed buttons (multiply Time.timeScale) |

---

## 5.4 Expansion Vectors

### Near-Term (Next 2-3 Sprints)

| Feature | Description | Complexity |
|---------|-------------|------------|
| **Building Demolition** | Remove buildings, free tiles, partial refund | Low |
| **Settings Panel** | Wire up existing stub with volume sliders | Low |
| **Camera Bounds** | Clamp pan to grid area | Low |
| **Building Upgrades** | Level up buildings to increase output or reduce interval | Medium |
| **Water Terrain** | Generate water tiles, add fishing or mill mechanics | Medium |
| **Angel's Share** | Evaporation mechanic on Rickhouse aging | Medium |

### Mid-Term (Feature Expansion)

| Feature | Description | Complexity |
|---------|-------------|------------|
| **Tourist NPC System** | Spawn visitors at Saloon with budgets and preferences | High |
| **Dynamic Pricing** | Aged Whiskey price fluctuates based on supply/demand | Medium |
| **Seasons & Weather** | Affect crop growth rates, tourist volume | High |
| **Multiple Whiskey Types** | Bourbon, Rye, Corn Whiskey with different recipes | Medium |
| **Rival Distilleries** | AI competitors on adjacent plots | High |
| **Reputation System** | Quality affects pricing and tourist attraction | Medium |

### Long-Term (Vision)

| Feature | Description | Complexity |
|---------|-------------|------------|
| **Illegal → Legal Arc** | Start as moonshiner, progress to bonded distillery | Very High |
| **Federal Agent Events** | Random inspections, need permits and compliance | High |
| **QR Marketing Bridge** | Real-world QR codes link to in-game rewards | High |
| **Multiplayer/Leaderboards** | Compare distillery stats online | Very High |
| **Modding Support** | JSON data cartridges for community buildings/recipes | Medium |

---

# 6. Technical Appendices

## Appendix A: Complete File Manifest

### Scripts (31 files, ~4,950 lines)

| File | Lines | Category |
|------|-------|----------|
| `AudioManager.cs` | 304 | Core / Audio |
| `BuildPanelUI.cs` | 216 | UI / Build |
| `BuildingBehavior.cs` | 242 | Gameplay / Production |
| `BuildingData.cs` | 53 | Data / ScriptableObject |
| `BuildingDatabase.cs` | 59 | Data / Registry |
| `BuildingPlacementManager.cs` | 190 | Gameplay / Placement |
| `BuildingSelector.cs` | 143 | Input / Selection |
| `CameraController.cs` | 79 | Core / Camera |
| `CropBehavior.cs` | 250 | Gameplay / Farming |
| `DayNightCycle.cs` | 143 | Visual / Atmosphere |
| `DevTools.cs` | 99 | Debug / Testing |
| `GameData.cs` | 83 | Data / Serialization |
| `GameInitializer.cs` | 75 | Core / Bootstrap |
| `GridManager.cs` | 334 | Core / World |
| `HUDButtonsUI.cs` | 71 | UI / HUD |
| `IntroVideoController.cs` | 124 | UI / Video |
| `InventoryManager.cs` | 169 | Core / Resources |
| `MainMenuManager.cs` | 153 | UI / Menu |
| `PausePanelUI.cs` | 102 | UI / Pause |
| `ProductionPopup.cs` | 150 | Visual / Feedback |
| `ProductionPopupPool.cs` | 111 | Core / Pooling |
| `ResourceUI.cs` | 124 | UI / HUD |
| `SaveManager.cs` | 285 | Core / Persistence |
| `SelectedBuildingUI.cs` | 117 | UI / Build |
| `SellManager.cs` | 145 | Gameplay / Economy |
| `SellPanelUI.cs` | 205 | UI / Sell |
| `TerrainType.cs` | 36 | Data / Enum |
| `TileBehavior.cs` | 242 | Core / Grid |
| `TileHighlightController.cs` | 177 | Visual / Interaction |
| `TreeBehavior.cs` | 115 | Gameplay / Harvesting |
| `UIManager.cs` | 352 | Core / UI |

### Scenes (3)

| Scene | Purpose |
|-------|---------|
| `IntroLoading.unity` | Video splash screen |
| `MainMenu.unity` | Title screen with Play/Settings/Quit |
| `GameScene.unity` | Main gameplay environment |

### ScriptableObject Assets (6)

| Asset | Building | Cost | Production |
|-------|----------|------|------------|
| `FieldData.asset` | Field | $50 | Corn (crop) |
| `MashTunData.asset` | Mash Tun | $75 | 2 Corn → 1 Mash / 8s |
| `CooperageData.asset` | Cooperage | $100 | 2 Wood → 1 Barrel / 5s |
| `StillData.asset` | Still | $150 | 2 Mash → 1 Whiskey / 10s |
| `RickhouseData.asset` | Rickhouse | $250 | 1 Whiskey + 1 Barrel → 1 Aged Whiskey / 8s |
| `SaloonData.asset` | Saloon | $300 | Non-producing (sell interface) |

### Audio Assets (14)

| Category | File | Usage |
|----------|------|-------|
| Music | `Tambul.mp3` | Menu/Gameplay track |
| Music | `gone_fishin_by_memoraphile_CC0.mp3` | Alternative track |
| SFX | `hammering-1.mp3` | Building placement |
| SFX | `wood-chop-axe-hit-01.mp3` | Tree chopping |
| SFX | `cassette-out-1.mp3` | Collect/produce |
| SFX | `button-20/21/22/30.mp3` | UI clicks |
| SFX | `error.ogg` | Invalid action |
| SFX | `writing-signature-1.mp3` | Success confirm |
| SFX | `marker-1.mp3` | Selection |
| SFX | `typewriter-backspace-1.mp3` | Cancel/back |

### Prefabs (17)

| Prefab | Category |
|--------|----------|
| `GrassTileBase.prefab` | Grid tile |
| `DirtTile.prefab` | Grid tile |
| `Tree.prefab` | Obstacle |
| `DeadTree.prefab` | Obstacle variant |
| `TreeStump.prefab` | Obstacle variant |
| `Rock.prefab` | Obstacle |
| `Bush.prefab` | Obstacle/decoration |
| `FieldPrefab.prefab` | Crop building |
| `Mashtun.prefab` | Production building |
| `Stillprefab.prefab` | Production building |
| `Cooperage.prefab` | Production building |
| `Rickhouse.prefab` | Production building (dual-input) |
| `Saloon.prefab` | Sell interface building |
| `BasicBuilding.prefab` | Generic template |
| `BarrelSprite.prefab` | Decorative |
| `popupTXT.prefab` | Production popup |
| `AudioManager.prefab` | Audio singleton |

---

## Appendix B: Singleton Dependency Graph

```
INITIALIZATION ORDER (Awake → Start):

1. GameInitializer.Awake()
   ├── Creates InventoryManager (if missing)
   └── Creates AudioManager (if missing)

2. All Singleton.Awake() (parallel, Unity order):
   ├── GridManager.Instance
   ├── BuildingDatabase.Instance
   ├── BuildingSelector.Instance
   ├── UIManager.Instance
   ├── SellManager.Instance
   └── ProductionPopupPool.Instance

3. SaveManager.Start()
   └── TryLoadGridSeed() → may trigger GridManager.RegenerateWithSeed()

4. GridManager.Start()
   └── GenerateGrid(-1) if not already generated

RUNTIME DEPENDENCY MAP:
  BuildingPlacementManager → BuildingSelector, BuildingDatabase, InventoryManager, GridManager, AudioManager, UIManager
  BuildingBehavior → InventoryManager, ProductionPopupPool, AudioManager, UIManager
  CropBehavior → InventoryManager, ProductionPopupPool, AudioManager
  TreeBehavior → InventoryManager, ProductionPopupPool, AudioManager
  SellManager → InventoryManager, ProductionPopupPool, AudioManager
  SaveManager → GridManager, InventoryManager, BuildingDatabase
  ResourceUI → InventoryManager (event subscription)
  SellPanelUI → InventoryManager (event), SellManager, UIManager, AudioManager
  SelectedBuildingUI → InventoryManager (event), BuildingSelector (event)
  BuildPanelUI → BuildingDatabase, BuildingSelector, InventoryManager (event), UIManager, AudioManager
  PausePanelUI → UIManager, SaveManager, AudioManager
  MainMenuManager → AudioManager
  HUDButtonsUI → UIManager
```

---

## Appendix C: Resource Constants Reference

```csharp
// InventoryManager.cs
public const string RESOURCE_CASH         = "Cash";
public const string RESOURCE_CORN         = "Corn";
public const string RESOURCE_MASH         = "Mash";
public const string RESOURCE_WHISKEY      = "Whiskey";
public const string RESOURCE_AGED_WHISKEY = "AgedWhiskey";
public const string RESOURCE_WOOD         = "Wood";
public const string RESOURCE_BARREL       = "Barrel";

// AudioManager.cs
public const string SFX_PLACE   = "Place";
public const string SFX_COLLECT = "Collect";
public const string SFX_CLICK   = "Click";
public const string SFX_ERROR   = "Error";
public const string SFX_SUCCESS = "Success";
public const string MUSIC_MENU     = "Menu";
public const string MUSIC_GAMEPLAY = "Gameplay";

// GridManager.cs
public const string SORT_LAYER_OBJECTS = "Buildings";
public const int    SORT_MULTIPLIER    = 10;
```

---

## Appendix D: Screenshots Reference

The `/WhiskeyvilleImages/` directory contains 20 development screenshots documenting:

| Screenshots | Content |
|-------------|---------|
| 38-43 | Early gameplay captures — grid layout, building placement, UI panels |
| 72-79 | Mid-development — production popups, resource HUD, day/night cycle |
| 80-85 | Late-development — full production chain running, sell panel, pause menu |

These screenshots document the visual evolution of the game from basic grid rendering to full gameplay loop implementation.

---

## Appendix E: Third-Party Asset Licenses

| Asset Pack | License | Source |
|-----------|---------|--------|
| Kenney Tiny Town | CC0 (Public Domain) | kenney.nl |
| LPC Base Assets | CC-BY-SA 3.0 / GPL 3.0 | OpenGameArt.org |
| LPC Structure Pack | CC-BY-SA 3.0 / GPL 3.0 | OpenGameArt.org |
| Crops v2.1 | CC-BY-SA 3.0 | OpenGameArt.org |
| Daneeklu Submission | CC-BY-SA 3.0 | OpenGameArt.org |
| Farming Stuff | Varies (see credits) | OpenGameArt.org |
| Gone Fishin' Music | CC0 (Public Domain) | Memoraphile |

---

> **End of Whiskeyville Developer Wiki**  
> *Generated by automated code audit — Build 2.10.26*  
> *Slade Empire · Project Foundry Engine*
