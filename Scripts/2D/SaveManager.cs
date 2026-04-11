// ============================================================================
// SAVEMANAGER.CS
// ============================================================================
// PURPOSE:      Handles saving and loading game state to/from JSON
// VERSION:      v3 — Fixed building placement on load, terrain persistence, tile occupancy
// UPDATED:      April 8, 2026
// ATTACHED TO:  Persistent SaveManager GameObject
// DEPENDENCIES: InventoryManager, GridManager, BuildingDatabase
// ============================================================================
// AUTO-SAVE TRIGGERS:
//   1. OnApplicationPause (app backgrounded on mobile)
//   2. OnApplicationQuit  (player closes the game)
//   3. AutoSaveCoroutine  (every _autoSaveInterval seconds, default 5 min)
// ============================================================================
// EVENTS:
//   SaveManager.OnGameSaved — fires after every successful save
//   → Subscribe to show "Game Saved ✓" toast notification
// ============================================================================

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static SaveManager Instance { get; private set; }

    // ========================================================================
    // 📡 EVENTS
    // ========================================================================

    /// <summary>
    /// Fires after every successful save (auto or manual).
    /// Subscribe to show "Game Saved ✓" toast notification.
    /// </summary>
    public static event Action OnGameSaved;

    // ========================================================================
    // ⚙️ INSPECTOR
    // ========================================================================

    [Header("⏱️ Auto-Save")]
    [Tooltip("Seconds between automatic saves during gameplay (default 300 = 5 min)")]
    [SerializeField] private float _autoSaveInterval = 300f;

    [Tooltip("If true, auto-save coroutine runs. Disable for testing.")]
    [SerializeField] private bool _enableAutoSave = true;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private string _savePath;
    private const string SAVE_FILENAME = "whiskeyville_save.json";
    private bool _isSaving = false;

    /// <summary>
    /// Set to true by MainMenuManager before loading GameScene.
    /// When true, SaveManager will call LoadGame() after the scene initializes.
    /// This fixes the Continue button not restoring resources/buildings.
    /// </summary>
    public bool PendingLoad { get; set; } = false;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
            Debug.Log($"[SaveManager] ✅ Initialized. Path: {_savePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ---- ⏱️ START AUTO-SAVE COROUTINE ----
        if (_enableAutoSave)
        {
            StartCoroutine(AutoSaveCoroutine());
            Debug.Log($"[SaveManager] Auto-save every {_autoSaveInterval}s.");
        }

        // Register for scene loaded events to handle pending loads
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Called every time a scene finishes loading.
    /// If PendingLoad is true (set by Continue button), performs full LoadGame()
    /// after a short delay to let all singletons initialize.
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (PendingLoad && scene.name == GameConstants.Scenes.GAME)
        {
            PendingLoad = false;
            // Delay 1 frame so GridManager, InventoryManager, etc. finish their Awake/Start
            StartCoroutine(DelayedLoad());
        }
    }

    private IEnumerator DelayedLoad()
    {
        // Wait 2 frames for all managers to initialize
        yield return null;
        yield return null;

        Debug.Log("[SaveManager] Performing full load from Continue...");
        bool success = LoadGame();
        if (success)
        {
            Debug.Log("[SaveManager] Continue load complete.");
        }
        else
        {
            Debug.LogWarning("[SaveManager] Continue load failed — starting fresh.");
        }
    }

    /// <summary>
    /// Fires when the app is backgrounded (mobile) or regains focus (desktop).
    /// On mobile: pause = true means going to background — save immediately.
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && HasSaveFile())
        {
            Debug.Log("[SaveManager] 📱 App paused — auto-saving...");
            SaveGame();
        }
    }

    /// <summary>
    /// Fires on application quit (PC/Mac/Linux).
    /// </summary>
    private void OnApplicationQuit()
    {
        Debug.Log("[SaveManager] 🚪 App quitting — auto-saving...");
        SaveGame();
    }

    // ========================================================================
    // ⏱️ AUTO-SAVE COROUTINE
    // ========================================================================

    /// <summary>
    /// Saves the game every _autoSaveInterval seconds while in gameplay.
    /// Only runs in game scene (not main menu).
    /// </summary>
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_autoSaveInterval);

            // Only auto-save if we're in a game scene (InventoryManager is alive)
            if (InventoryManager.Instance != null)
            {
                Debug.Log("[SaveManager] ⏱️ Auto-save triggered.");
                SaveGame();
            }
        }
    }

    // ========================================================================
    // 🌱 GRID SEED LOADING
    // ========================================================================

    private void TryLoadGridSeed()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("[SaveManager] 📂 No save file. New seed will generate.");
            return;
        }

        try
        {
            string json = File.ReadAllText(_savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data != null && data.gridSeed > 0 && GridManager.Instance != null)
            {
                if (!GridManager.Instance.IsGenerated)
                {
                    GridManager.Instance.GenerateGrid(data.gridSeed);
                    Debug.Log($"[SaveManager] 🌱 Grid seed loaded: {data.gridSeed}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] ❌ Failed to read grid seed: {e.Message}");
        }
    }

    // ========================================================================
    // 💾 SAVE
    // ========================================================================

    /// <summary>
    /// Saves the full game state to JSON.
    /// Fires OnGameSaved event on success.
    /// </summary>
    public void SaveGame()
    {
        // Guard against overlapping saves
        if (_isSaving)
        {
            Debug.LogWarning("[SaveManager] ⚠️ Save already in progress — skipping.");
            return;
        }

        _isSaving = true;

        try
        {
            GameData data = new GameData();

            // ---- 🌱 GRID SEED ----
            if (GridManager.Instance != null)
            {
                data.gridSeed = GridManager.Instance.CurrentSeed;
            }

            // ---- 📦 RESOURCES ----
            if (InventoryManager.Instance != null)
            {
                data.cash        = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CASH);
                data.corn        = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CORN);
                data.mash        = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_MASH);
                data.whiskey     = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WHISKEY);
                data.agedWhiskey = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY);
                data.wood        = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WOOD);
                data.barrel      = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_BARREL);
            }

            // ---- 🏗️ BUILDINGS ----
            data.buildings = CollectBuildingData();

            // ---- 🌾 TERRAIN CHANGES (hoe-tilled tiles) ----
            data.tileTerrainChanges = CollectTerrainChanges();

            // ---- 📝 WRITE FILE ----
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);

            Debug.Log($"[SaveManager] 💾 Saved. Seed: {data.gridSeed}, Buildings: {data.buildings.Count}");

            // ---- 📡 FIRE EVENT ----
            OnGameSaved?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] ❌ Save failed: {e.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private List<BuildingSaveData> CollectBuildingData()
    {
        List<BuildingSaveData> buildingList = new List<BuildingSaveData>();

        BuildingBehavior[] buildings = FindObjectsOfType<BuildingBehavior>();
        foreach (BuildingBehavior building in buildings)
        {
            // Skip buildings that also have CropBehavior — those get saved in the crop loop below.
            // Without this check, farm prefabs with both components get saved TWICE,
            // causing duplicate/misplaced buildings on load.
            if (building.GetComponent<CropBehavior>() != null) continue;

            // Look up building name from database for safe save/load
            string name = "";
            BuildingData data = BuildingDatabase.Instance?.GetBuilding(building.BuildingIndex);
            if (data != null) name = data.buildingName;

            BuildingSaveData saveData = new BuildingSaveData(
                building.BuildingIndex,
                name,
                building.transform.position,
                building.GridPosition,
                false,
                0,
                false
            );
            buildingList.Add(saveData);
        }

        CropBehavior[] crops = FindObjectsOfType<CropBehavior>();
        foreach (CropBehavior crop in crops)
        {
            // Look up crop name from database for safe save/load
            string name = "";
            BuildingData data = BuildingDatabase.Instance?.GetBuilding(crop.BuildingIndex);
            if (data != null) name = data.buildingName;

            BuildingSaveData saveData = new BuildingSaveData(
                crop.BuildingIndex,
                name,
                crop.transform.position,
                crop.GridPosition,
                true,
                crop.CurrentStage,
                crop.IsFullyGrown
            );
            buildingList.Add(saveData);
        }

        return buildingList;
    }

    /// <summary>
    /// Collects tiles whose terrain was modified from the seed-generated default.
    /// Saves hoe-tilled Farm tiles so they persist across save/load.
    /// </summary>
    private List<TileTerrainSaveData> CollectTerrainChanges()
    {
        List<TileTerrainSaveData> changes = new List<TileTerrainSaveData>();

        if (GridManager.Instance == null) return changes;

        for (int x = 0; x < GridManager.Instance.GridWidth; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridHeight; y++)
            {
                TileBehavior tile = GridManager.Instance.GetTileAt(x, y);
                if (tile == null) continue;

                // Save any tile that was changed to Farm (hoe-tilled)
                if (tile.TerrainType == TerrainType.Farm)
                {
                    changes.Add(new TileTerrainSaveData(x, y, TerrainType.Farm));
                }
            }
        }

        return changes;
    }

    // ========================================================================
    // 📂 LOAD
    // ========================================================================

    /// <summary>
    /// Loads game state from JSON. Returns true on success.
    /// </summary>
    public bool LoadGame()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("[SaveManager] 📂 No save file found.");
            return false;
        }

        try
        {
            string json = File.ReadAllText(_savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data == null)
            {
                Debug.LogError("[SaveManager] ❌ Failed to parse save file.");
                return false;
            }

            ClearExistingBuildings();

            // ---- 🌱 REGENERATE GRID ----
            if (GridManager.Instance != null && data.gridSeed > 0)
            {
                GridManager.Instance.RegenerateWithSeed(data.gridSeed);
            }

            // ---- 📦 RESTORE RESOURCES ----
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_CASH,         data.cash);
                InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_CORN,         data.corn);
                InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_MASH,         data.mash);
                InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_WHISKEY,      data.whiskey);
                InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_AGED_WHISKEY, data.agedWhiskey);
                InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_WOOD,         data.wood);
                InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_BARREL,       data.barrel);
            }

            // ---- 🌾 RESTORE TERRAIN CHANGES (hoe-tilled tiles) ----
            RestoreTerrainChanges(data.tileTerrainChanges);

            // ---- 🏗️ RESTORE BUILDINGS ----
            LoadBuildings(data.buildings);

            Debug.Log($"[SaveManager] 📂 Loaded. Seed: {data.gridSeed}, Buildings: {data.buildings.Count}, Terrain changes: {(data.tileTerrainChanges != null ? data.tileTerrainChanges.Count : 0)}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] ❌ Load failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clears all placed buildings AND resets tile occupancy state.
    /// Must run before loading to prevent stale references.
    /// </summary>
    private void ClearExistingBuildings()
    {
        // Clear tile occupancy state first (before destroying GameObjects)
        if (GridManager.Instance != null)
        {
            for (int x = 0; x < GridManager.Instance.GridWidth; x++)
            {
                for (int y = 0; y < GridManager.Instance.GridHeight; y++)
                {
                    TileBehavior tile = GridManager.Instance.GetTileAt(x, y);
                    if (tile != null && tile.IsOccupied)
                        tile.SetOccupied(false);
                }
            }
        }

        // Destroy all building GameObjects
        BuildingBehavior[] buildings = FindObjectsOfType<BuildingBehavior>();
        foreach (BuildingBehavior building in buildings) Destroy(building.gameObject);

        CropBehavior[] crops = FindObjectsOfType<CropBehavior>();
        foreach (CropBehavior crop in crops) Destroy(crop.gameObject);

        Debug.Log("[SaveManager] 🗑️ Cleared existing buildings and tile states.");
    }

    /// <summary>
    /// Restores tiles that were hoe-tilled to Farm terrain after grid regeneration.
    /// Must run BEFORE LoadBuildings so farm prefabs land on correct terrain.
    /// </summary>
    private void RestoreTerrainChanges(List<TileTerrainSaveData> terrainChanges)
    {
        if (terrainChanges == null || GridManager.Instance == null) return;

        foreach (TileTerrainSaveData td in terrainChanges)
        {
            TileBehavior tile = GridManager.Instance.GetTileAt(td.gridX, td.gridY);
            if (tile != null)
            {
                tile.SetTerrainType((TerrainType)td.terrainType);
            }
        }

        if (terrainChanges.Count > 0)
            Debug.Log($"[SaveManager] 🌾 Restored {terrainChanges.Count} terrain changes.");
    }

    /// <summary>
    /// Restores all buildings from save data.
    /// FIXES from v2:
    ///   - Uses grid position (not world pos) for placement — snaps to grid correctly
    ///   - Passes building reference to tile.SetOccupied — fixes demolish after load
    ///   - Applies sorting — fixes visual layering after load
    ///   - Falls back to buildingName if index lookup fails — handles database reorder
    /// </summary>
    private void LoadBuildings(List<BuildingSaveData> buildingList)
    {
        if (BuildingDatabase.Instance == null)
        {
            Debug.LogError("[SaveManager] ❌ BuildingDatabase not found!");
            return;
        }

        int loadedCount = 0;
        int failedCount = 0;

        foreach (BuildingSaveData saveData in buildingList)
        {
            // Primary lookup: by index
            BuildingData buildingData = BuildingDatabase.Instance.GetBuilding(saveData.buildingIndex);

            // Fallback: by name (handles database reorder)
            if ((buildingData == null || buildingData.prefab == null) &&
                !string.IsNullOrEmpty(saveData.buildingName))
            {
                int nameIndex = BuildingDatabase.Instance.FindIndexByName(saveData.buildingName);
                if (nameIndex >= 0)
                {
                    buildingData = BuildingDatabase.Instance.GetBuilding(nameIndex);
                    Debug.Log($"[SaveManager] 🔄 Index {saveData.buildingIndex} failed, resolved '{saveData.buildingName}' to index {nameIndex}");
                }
            }

            if (buildingData == null || buildingData.prefab == null)
            {
                Debug.LogWarning($"[SaveManager] ⚠️ Skipping invalid building: index={saveData.buildingIndex}, name='{saveData.buildingName}'");
                failedCount++;
                continue;
            }

            // Use grid position for placement — snaps to grid correctly
            // Falls back to saved world position if GridManager unavailable
            Vector3 position;
            if (GridManager.Instance != null)
                position = GridManager.Instance.GridToWorldPosition(saveData.gridX, saveData.gridY);
            else
                position = new Vector3(saveData.posX, saveData.posY, 0f);

            GameObject building = Instantiate(buildingData.prefab, position, Quaternion.identity);
            Vector2Int gridPos = new Vector2Int(saveData.gridX, saveData.gridY);

            if (saveData.isCrop)
            {
                // ---- CROP RESTORE ----
                CropBehavior crop = building.GetComponent<CropBehavior>();
                if (crop != null)
                {
                    crop.SetBuildingIndex(saveData.buildingIndex);
                    crop.SetGridPosition(gridPos);
                    crop.RestoreGrowthState(saveData.growthStage, saveData.isFullyGrown);
                }

                // Apply crop sorting (soil on Ground, growth on Buildings layer)
                ApplyCropSorting(building, gridPos.y);
            }
            else
            {
                // ---- BUILDING RESTORE ----
                BuildingBehavior behavior = building.GetComponent<BuildingBehavior>();
                if (behavior != null)
                {
                    behavior.Initialize(buildingData);
                    behavior.SetBuildingIndex(saveData.buildingIndex);
                    behavior.SetGridPosition(gridPos);
                }

                // Apply building sorting (all sprites on Buildings layer)
                ApplyBuildingSorting(building, gridPos.y);
            }

            // Mark tile as occupied WITH building reference (fixes demolish after load)
            TileBehavior tile = GridManager.Instance?.GetTileAt(saveData.gridX, saveData.gridY);
            if (tile != null)
            {
                tile.SetOccupied(true, building);

                // Ensure farm terrain is set for crops
                if (saveData.isCrop)
                    tile.SetTerrainType(TerrainType.Farm);
            }

            loadedCount++;
        }

        Debug.Log($"[SaveManager] 🏗️ Loaded {loadedCount} buildings ({failedCount} failed).");
    }

    // ========================================================================
    // 🎨 SORTING HELPERS (mirrors BuildingPlacementManager logic)
    // ========================================================================

    /// <summary>
    /// Applies correct sprite sorting for buildings loaded from save.
    /// All child renderers go on Buildings sorting layer, ordered by Y position.
    /// </summary>
    private void ApplyBuildingSorting(GameObject obj, int gridY)
    {
        int baseSortOrder = GridManager.Instance != null
            ? GridManager.GetSortOrder(gridY, GridManager.Instance.GridHeight)
            : 0;

        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingLayerName = GridManager.SORT_LAYER_OBJECTS;
            renderers[i].sortingOrder = baseSortOrder + renderers[i].sortingOrder;
        }
    }

    /// <summary>
    /// Applies correct sprite sorting for crops loaded from save.
    /// Root (soil) goes on Ground layer, child growth stages on Buildings layer.
    /// </summary>
    private void ApplyCropSorting(GameObject cropObj, int gridY)
    {
        int baseSortOrder = GridManager.Instance != null
            ? GridManager.GetSortOrder(gridY, GridManager.Instance.GridHeight)
            : 0;

        // Root sprite = soil/dirt on Ground layer
        SpriteRenderer rootSR = cropObj.GetComponent<SpriteRenderer>();
        if (rootSR != null)
        {
            rootSR.sortingLayerName = "Ground";
            rootSR.sortingOrder = 1;
        }

        // Child sprites = growth stages on Buildings layer
        SpriteRenderer[] childRenderers = cropObj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] == rootSR) continue; // Skip root
            childRenderers[i].sortingLayerName = GridManager.SORT_LAYER_OBJECTS;
            childRenderers[i].sortingOrder = baseSortOrder + childRenderers[i].sortingOrder;
        }
    }

    // ========================================================================
    // 🗑️ UTILITY
    // ========================================================================

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Deletes the save file. Called before starting a fresh New Game.
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
            Debug.Log("[SaveManager] 🗑️ Save file deleted.");
        }
    }

    /// <summary>
    /// Returns true if a save file exists on disk.
    /// Used by MainMenuManager to show/hide the Continue button.
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(_savePath);
    }
}
