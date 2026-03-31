// ============================================================================
// GAMEDATA.CS
// ============================================================================
// PURPOSE:      Serializable class holding all save data
// USED BY:      SaveManager for JSON serialization
// VERSION:      v2 — Added whiskey properties, bottle customization, research
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    // ========================================================================
    // 🌱 GRID SEED
    // ========================================================================

    public int gridSeed;

    // ========================================================================
    // 💰 RESOURCES
    // ========================================================================

    public int cash;
    public int corn;
    public int mash;
    public int whiskey;
    public int agedWhiskey;
    public int wood;
    public int barrel;

    // ========================================================================
    // 🏗️ BUILDINGS
    // ========================================================================

    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();

    // ========================================================================
    // 🥃 WHISKEY PROPERTIES
    // ========================================================================

    public int currentFlavorProfile;      // FlavorProfile cast to int
    public int currentQuality;
    public int currentTemperatureProfile; // TemperatureProfile cast to int
    public int currentBatchSize;

    // ========================================================================
    // 🍾 BOTTLE CUSTOMIZATION
    // ========================================================================

    public int    glassTypeIndex;
    public int    labelColorIndex;
    public int    emblemIndex;
    public string distilleryName;
    public string whiskeyName;
    public string tagline;
    public int    vintageYear;
    public bool   bottlingHouseBuilt;

    // ========================================================================
    // 🔬 RESEARCH
    // ========================================================================

    public int          researchPoints;
    public List<string> unlockedResearch = new List<string>();

    // ========================================================================
    // 🔧 CONSTRUCTOR
    // ========================================================================

    public GameData()
    {
        gridSeed = -1;
        cash = 200;
        corn = 0;
        mash = 0;
        whiskey = 0;
        agedWhiskey = 0;
        wood = 0;
        barrel = 0;
        buildings = new List<BuildingSaveData>();

        // Whiskey properties defaults
        currentFlavorProfile      = 0; // FlavorProfile.Sweet
        currentQuality            = 2;
        currentTemperatureProfile = 1; // TemperatureProfile.Warm
        currentBatchSize          = 1;

        // Bottle customization defaults
        glassTypeIndex   = 0;
        labelColorIndex  = 1; // Kraft Brown
        emblemIndex      = 0;
        distilleryName   = "My Distillery";
        whiskeyName      = "House Whiskey";
        tagline          = "";
        vintageYear      = 2026;
        bottlingHouseBuilt = false;

        // Research defaults
        researchPoints   = 0;
        unlockedResearch = new List<string>();
    }
}

[Serializable]
public class BuildingSaveData
{
    public int buildingIndex;
    public float posX;
    public float posY;
    public int gridX;
    public int gridY;

    // Crop-specific
    public bool isCrop;
    public int growthStage;
    public bool isFullyGrown;

    public BuildingSaveData() { }

    public BuildingSaveData(int index, Vector3 position, Vector2Int gridPos, bool crop, int stage, bool fullyGrown)
    {
        buildingIndex = index;
        posX = position.x;
        posY = position.y;
        gridX = gridPos.x;
        gridY = gridPos.y;
        isCrop = crop;
        growthStage = stage;
        isFullyGrown = fullyGrown;
    }
}