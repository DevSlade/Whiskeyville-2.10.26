# 🏗️ Whiskeyville — System Architecture
### Black Label Interactive | Last Updated: March 2026

This document describes how all 31+ scripts in Whiskeyville connect, depend on each other,
and are designed to accommodate future expansion.

---

## SYSTEM MAP (High Level)

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                           WHISKEYVILLE SYSTEM MAP                                   │
│                                                                                     │
│  ┌─────────────┐    ┌────────────────┐    ┌───────────────┐    ┌────────────────┐  │
│  │  INPUT      │    │  WORLD         │    │  ECONOMY      │    │  PERSISTENCE   │  │
│  │  LAYER      │    │  LAYER         │    │  LAYER        │    │  LAYER         │  │
│  │             │    │                │    │               │    │                │  │
│  │ UIManager   │    │ GridManager    │    │ InventoryMgr  │    │ SaveManager    │  │
│  │ BuildSel    │    │ TileBehavior   │    │ SellManager   │    │ GameData       │  │
│  │ CameraCon   │    │ TileHighlight  │    │ BuildingBeh   │    │                │  │
│  │ DevTools    │    │ BuildPlaceM    │    │ CropBehavior  │    │                │  │
│  │ [ToolSel*]  │    │ TreeBehavior   │    │               │    │                │  │
│  │ [TouchIn*]  │    │ DayNightCycle  │    │               │    │                │  │
│  └──────┬──────┘    └───────┬────────┘    └───────┬───────┘    └───────┬────────┘  │
│         │                   │                     │                    │            │
│         └───────────────────┼─────────────────────┼────────────────────┘            │
│                             │                     │                                 │
│                    ┌────────▼─────────────────────▼────────┐                        │
│                    │         INVENTORY MANAGER              │                        │
│                    │   (Single Source of Truth — SSOT)      │                        │
│                    │                                        │                        │
│                    │  Cash | Corn | Mash | Whiskey |         │                        │
│                    │  AgedWhiskey | Wood | Barrels            │                        │
│                    │  + OnResourceChanged event              │                        │
│                    └─────────────────┬──────────────────────┘                        │
│                                      │                                               │
│            ┌─────────────────────────┼────────────────────────────┐                  │
│            │                         │                            │                  │
│  ┌─────────▼──────┐       ┌──────────▼──────────┐     ┌──────────▼──────────┐       │
│  │   UI LAYER     │       │   AUDIO LAYER        │     │   VISUAL LAYER      │       │
│  │                │       │                      │     │                     │       │
│  │ ResourceUI     │       │ AudioManager         │     │ ProductionPopup     │       │
│  │ BuildPanelUI   │       │                      │     │ ProductionPopupPool │       │
│  │ SellPanelUI    │       │                      │     │ DayNightCycle       │       │
│  │ SelectedBldUI  │       │                      │     │ [Clouds*]           │       │
│  │ HUDButtonsUI   │       │                      │     │                     │       │
│  │ PausePanelUI   │       │                      │     │                     │       │
│  │ [TutorialUI*]  │       │                      │     │                     │       │
│  └────────────────┘       └──────────────────────┘     └─────────────────────┘       │
│                                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────┐    │
│  │   BOOTSTRAP / SCENE MANAGEMENT                                               │    │
│  │   GameInitializer | MainMenuManager | IntroVideoController                   │    │
│  └─────────────────────────────────────────────────────────────────────────────┘    │
│                                                                                     │
│  [*] = Not yet built — planned for Phase 1                                          │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

## DATA FLOW: PRODUCTION CHAIN

```
  PLAYER CLICKS TILE                CROP GROWS (4 stages)             PLAYER CLICKS
       │                                    │                          TO HARVEST
       ▼                                    ▼                               │
  BuildingSelector ──► BuildingPlacementManager ──► CropBehavior ──────────┘
                                 │                       │
                                 │           InventoryManager.Add("Corn", 1)
                                 │
                          BuildingBehavior (Mash Tun)
                                 │
                           Checks: Corn >= 1?
                                 │  YES
                           Consume("Corn", 1)
                           Add("Mash", 1)
                                 │
                          BuildingBehavior (Still)
                                 │
                           Checks: Mash >= 1?
                                 │  YES
                           Consume("Mash", 1)
                           Add("Whiskey", 1)
                                 │
                          BuildingBehavior (Cooperage)
                                 │
                           Checks: Wood >= 1?
                                 │  YES
                           Consume("Wood", 1)
                           Add("Barrel", 1)
                                 │
                          BuildingBehavior (Rickhouse)
                                 │
                           Checks: Whiskey >= 1 AND Barrel >= 1?
                                 │  YES
                           Consume("Whiskey", 1) + Consume("Barrel", 1)
                           Add("AgedWhiskey", 1)
                                 │
                       PLAYER CLICKS SALOON
                                 │
                          SellManager.SellAll()
                                 │
                           Consume("AgedWhiskey", n)
                           Add("Cash", n * 50)
                                 │
                          ResourceUI updates
                          ProductionPopup fires
```

---

## SCRIPT INVENTORY (31 Scripts)

### Core Architecture

| Script | Role | Singleton | Key Events/Hooks |
|--------|------|-----------|-----------------|
| `InventoryManager.cs` | SSOT for all resources | ✅ Yes | `OnResourceChanged` |
| `SaveManager.cs` | JSON persistence | ✅ Yes | `SaveGame()`, `LoadGame()` |
| `GameData.cs` | Serializable save payload | No | Data container |
| `GameInitializer.cs` | Bootstrap ordering | No | Scene entry point |
| `AudioManager.cs` | SFX + Music playback | ✅ Yes | `PlaySFX(clip)`, `PlayMusic(clip)` |

### World / Grid

| Script | Role | Depends On |
|--------|------|-----------|
| `GridManager.cs` | Spawn + manage tile grid | `TileBehavior`, `TerrainType` |
| `TileBehavior.cs` | Per-tile state (occupied, type) | `TerrainType` |
| `TerrainType.cs` | Enum for tile types | — |
| `TileHighlightController.cs` | Hover glow on tiles | `GridManager` |
| `TreeBehavior.cs` | Wood-producing trees with respawn | `InventoryManager` |
| `DayNightCycle.cs` | Ambient lighting over time | — |

### Buildings

| Script | Role | Depends On |
|--------|------|-----------|
| `BuildingData.cs` | ScriptableObject: building config | — |
| `BuildingDatabase.cs` | Array of all BuildingData assets | `BuildingData` |
| `BuildingSelector.cs` | Hotkey + UI selection of building type | `BuildingDatabase` |
| `BuildingPlacementManager.cs` | Click-to-place on grid | `BuildingSelector`, `GridManager`, `InventoryManager` |
| `BuildingBehavior.cs` | Production logic (consume → produce) | `InventoryManager`, `ProductionPopupPool`, `UIManager` |
| `CropBehavior.cs` | 4-stage crop growth + harvest | `InventoryManager` |

### Economy

| Script | Role | Depends On |
|--------|------|-----------|
| `SellManager.cs` | AgedWhiskey → Cash conversion | `InventoryManager`, `AudioManager` |

### UI Layer

| Script | Role | Depends On |
|--------|------|-----------|
| `UIManager.cs` | Panel visibility, pause control | All panel scripts |
| `ResourceUI.cs` | Live resource display | `InventoryManager` (event-driven) |
| `BuildPanelUI.cs` | Build button panel | `BuildingSelector`, `InventoryManager` |
| `SellPanelUI.cs` | Sell button panel | `SellManager`, `InventoryManager` |
| `SelectedBuildingUI.cs` | "Selected: Field (25 Cash)" display | `BuildingSelector` |
| `HUDButtonsUI.cs` | Open Build/Sell panels from HUD | `UIManager` |
| `PausePanelUI.cs` | Resume / Save / Load / Quit buttons | `UIManager`, `SaveManager` |

### Visual Feedback

| Script | Role | Depends On |
|--------|------|-----------|
| `ProductionPopup.cs` | Single floating "+1 Mash" popup | — |
| `ProductionPopupPool.cs` | Object pool for popups | `ProductionPopup` |

### Scene Management

| Script | Role | Depends On |
|--------|------|-----------|
| `MainMenuManager.cs` | Title screen buttons | `SaveManager` |
| `IntroVideoController.cs` | Plays intro video, transitions | `MainMenuManager` |
| `CameraController.cs` | Pan + Zoom | — |

### Development

| Script | Role |
|--------|------|
| `DevTools.cs` | Hotkeys: F1–F10 for resource manipulation |

---

## DEPENDENCY GRAPH (Simplified)

```
  GameInitializer
       │
       ├── InventoryManager (SSOT — everything reads/writes here)
       │        │
       │        ├── BuildingBehavior (reads: inputs; writes: outputs)
       │        ├── CropBehavior (writes: Corn)
       │        ├── SellManager (reads: AgedWhiskey; writes: Cash)
       │        ├── TreeBehavior (writes: Wood)
       │        └── ResourceUI (reads via event)
       │
       ├── SaveManager (reads/writes InventoryManager + GridManager)
       │
       ├── GridManager (owns tile grid)
       │        └── TileBehavior (per-tile state)
       │
       ├── BuildingSelector → BuildingDatabase → BuildingData
       │
       ├── BuildingPlacementManager
       │        ├── reads BuildingSelector (what to place)
       │        ├── reads/writes GridManager (which tile)
       │        └── reads InventoryManager (can afford?)
       │
       ├── UIManager (owns all panel show/hide)
       │        ├── PausePanelUI
       │        ├── BuildPanelUI
       │        ├── SellPanelUI
       │        ├── SelectedBuildingUI
       │        └── HUDButtonsUI
       │
       └── AudioManager (singleton, called from many scripts)
```

---

## SINGLETON REGISTRY

Singletons that persist across scenes:

| Singleton | `DontDestroyOnLoad?` | Purpose |
|-----------|---------------------|---------|
| `InventoryManager` | No (scene-resident) | Resource SSOT |
| `SaveManager` | ✅ Yes | Persists save path + load on startup |
| `AudioManager` | ✅ Yes (implied) | Continuous music |
| `ProductionPopupPool` | No | Pooled in-scene |

---

## FUTURE EXTENSION POINTS

Every new system should follow these conventions:

### Adding a New Resource
1. Add constant to `InventoryManager.cs` (e.g., `RESOURCE_FAME = "Fame"`)
2. Add to `_startingValues` in Inspector
3. Update `GameData.cs` save payload
4. Add to `ResourceUI.cs` display

### Adding a New Building Type
1. Create new `BuildingData.asset` ScriptableObject
2. Add to `BuildingDatabase` array
3. Add hotkey in `BuildingSelector.cs`
4. Add button in `BuildPanelUI.cs`
5. Wire cost/input/output data in the asset — `BuildingBehavior` handles the rest

### Adding a New Scene
1. Use `GameInitializer` pattern for singleton registration
2. Subscribe to `InventoryManager.OnResourceChanged` for reactive UI
3. Load/restore state via `SaveManager`

---

## KNOWN ARCHITECTURAL CONCERNS

| Concern | Severity | Mitigation |
|---------|----------|-----------|
| `BuildingBehavior` uses coroutines for production — many buildings = many coroutines | Low | Acceptable at MVP scale. Switch to a manager-driven tick system at v1.0. |
| No input abstraction (uses `KeyCode` directly) | Medium | Replace with Unity's new Input System for mobile support. |
| `DevTools.cs` shares hotkey range with debug features | Low | Guard behind `#if UNITY_EDITOR` or a debug flag before shipping. |
| `SaveManager` writes to a single file — no slots | Low | Add slot support in v1.0 alongside Main Menu improvements. |

---

*"A clean dependency graph is worth a thousand comments."*
*— Black Label Interactive*
