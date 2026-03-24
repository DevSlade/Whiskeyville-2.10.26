// ============================================================================
// SAVEMANAGER.CS
// ============================================================================
// PURPOSE:      Handles saving and loading game state
// ATTACHED TO:  Persistent SaveManager GameObject
// VERSION:      v2 — Added whiskey properties, bottle customization, research
// ============================================================================

using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static SaveManager Instance { get; private set; }

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private string _savePath;
    private const string SAVE_FILENAME = "whiskeyville_save.json";

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
        TryLoadGridSeed();
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
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] ❌ Failed to read grid seed: {e.Message}");
        }
    }

    // ========================================================================
    // 💾 SAVE
    // ========================================================================

    public void SaveGame()
    {
        GameData data = new GameData();

        // Save grid seed
        if (GridManager.Instance != null)
        {
            data.gridSeed = GridManager.Instance.CurrentSeed;
        }

        // Save resources
        if (InventoryManager.Instance != null)
        {
            data.cash = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CASH);
            data.corn = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CORN);
            data.mash = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_MASH);
            data.whiskey = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WHISKEY);
            data.agedWhiskey = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY);
            data.wood = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WOOD);
            data.barrel = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_BARREL);
        }

        // Save buildings
        data.buildings = CollectBuildingData();

        // Save whiskey properties
        if (WhiskeyPropertyManager.Instance != null)
        {
            data.currentFlavorProfile      = WhiskeyPropertyManager.Instance.GetFlavorIndex();
            data.currentQuality            = WhiskeyPropertyManager.Instance.CurrentQuality;
            data.currentTemperatureProfile = WhiskeyPropertyManager.Instance.GetTemperatureIndex();
            data.currentBatchSize          = WhiskeyPropertyManager.Instance.CurrentBatchSize;
        }

        // Save bottle customization
        if (BottleCustomizationManager.Instance != null)
        {
            BottleCustomizationData design = BottleCustomizationManager.Instance.ActiveDesign;
            data.glassTypeIndex      = design.glassTypeIndex;
            data.labelColorIndex     = design.labelColorIndex;
            data.emblemIndex         = design.emblemIndex;
            data.distilleryName      = design.distilleryName;
            data.whiskeyName         = design.whiskeyName;
            data.tagline             = design.tagline;
            data.vintageYear         = design.vintageYear;
            data.bottlingHouseBuilt  = BottleCustomizationManager.Instance.IsBottlingHouseBuilt;
        }

        // Save research
        if (ResearchManager.Instance != null)
        {
            data.researchPoints     = ResearchManager.Instance.ResearchPoints;
            data.unlockedResearch   = ResearchManager.Instance.GetUnlockedNodeIds();
        }

        // Write to file
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);

        Debug.Log($"[SaveManager] 💾 Saved. Seed: {data.gridSeed}, Buildings: {data.buildings.Count}");
    }

    private List<BuildingSaveData> CollectBuildingData()
    {
        List<BuildingSaveData> buildingList = new List<BuildingSaveData>();

        BuildingBehavior[] buildings = FindObjectsOfType<BuildingBehavior>();
        foreach (BuildingBehavior building in buildings)
        {
            BuildingSaveData saveData = new BuildingSaveData(
                building.BuildingIndex,
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
            BuildingSaveData saveData = new BuildingSaveData(
                crop.BuildingIndex,
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

    // ========================================================================
    // 📂 LOAD
    // ========================================================================

    public bool LoadGame()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("[SaveManager] 📂 No save file found.");
            return false;
        }

        string json = File.ReadAllText(_savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        if (data == null)
        {
            Debug.LogError("[SaveManager] ❌ Failed to parse save file.");
            return false;
        }

        ClearExistingBuildings();

        // Regenerate grid with saved seed
        if (GridManager.Instance != null && data.gridSeed > 0)
        {
            GridManager.Instance.RegenerateWithSeed(data.gridSeed);
        }

        // Load resources
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_CASH, data.cash);
            InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_CORN, data.corn);
            InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_MASH, data.mash);
            InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_WHISKEY, data.whiskey);
            InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_AGED_WHISKEY, data.agedWhiskey);
            InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_WOOD, data.wood);
            InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_BARREL, data.barrel);
        }

        // Load buildings
        LoadBuildings(data.buildings);

        // Load whiskey properties
        if (WhiskeyPropertyManager.Instance != null)
        {
            WhiskeyPropertyManager.Instance.LoadFromSaveData(
                data.currentFlavorProfile,
                data.currentQuality,
                data.currentTemperatureProfile,
                data.currentBatchSize
            );
        }

        // Load bottle customization
        if (BottleCustomizationManager.Instance != null)
        {
            BottleCustomizationManager.Instance.LoadFromSaveData(
                data.glassTypeIndex,
                data.labelColorIndex,
                data.emblemIndex,
                data.distilleryName,
                data.whiskeyName,
                data.tagline,
                data.vintageYear,
                data.bottlingHouseBuilt
            );
        }

        // Load research
        if (ResearchManager.Instance != null)
        {
            ResearchManager.Instance.LoadFromSaveData(data.researchPoints, data.unlockedResearch);
        }

        Debug.Log($"[SaveManager] 📂 Loaded. Seed: {data.gridSeed}, Buildings: {data.buildings.Count}");
        return true;
    }

    private void ClearExistingBuildings()
    {
        BuildingBehavior[] buildings = FindObjectsOfType<BuildingBehavior>();
        foreach (BuildingBehavior building in buildings)
        {
            Destroy(building.gameObject);
        }

        CropBehavior[] crops = FindObjectsOfType<CropBehavior>();
        foreach (CropBehavior crop in crops)
        {
            Destroy(crop.gameObject);
        }

        Debug.Log("[SaveManager] 🗑️ Cleared existing buildings.");
    }

    private void LoadBuildings(List<BuildingSaveData> buildingList)
    {
        if (BuildingDatabase.Instance == null)
        {
            Debug.LogError("[SaveManager] ❌ BuildingDatabase not found!");
            return;
        }

        foreach (BuildingSaveData saveData in buildingList)
        {
            BuildingData buildingData = BuildingDatabase.Instance.GetBuilding(saveData.buildingIndex);

            if (buildingData == null || buildingData.prefab == null)
            {
                Debug.LogWarning($"[SaveManager] ⚠️ Invalid building index: {saveData.buildingIndex}");
                continue;
            }

            Vector3 position = new Vector3(saveData.posX, saveData.posY, 0f);
            GameObject building = Instantiate(buildingData.prefab, position, Quaternion.identity);

            if (saveData.isCrop)
            {
                CropBehavior crop = building.GetComponent<CropBehavior>();
                if (crop != null)
                {
                    crop.SetBuildingIndex(saveData.buildingIndex);
                    crop.SetGridPosition(new Vector2Int(saveData.gridX, saveData.gridY));
                    crop.RestoreGrowthState(saveData.growthStage, saveData.isFullyGrown);
                }
            }
            else
            {
                BuildingBehavior behavior = building.GetComponent<BuildingBehavior>();
                if (behavior != null)
                {
                    behavior.Initialize(buildingData);
                    behavior.SetBuildingIndex(saveData.buildingIndex);
                    behavior.SetGridPosition(new Vector2Int(saveData.gridX, saveData.gridY));
                }
            }

            TileBehavior tile = GridManager.Instance?.GetTileAt(saveData.gridX, saveData.gridY);
            if (tile != null)
            {
                tile.SetOccupied(true);
            }
        }
    }

    // ========================================================================
    // 🗑️ UTILITY
    // ========================================================================

    public void DeleteSave()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
            Debug.Log("[SaveManager] 🗑️ Save file deleted.");
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(_savePath);
    }
}