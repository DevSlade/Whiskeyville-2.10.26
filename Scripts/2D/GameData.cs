// ============================================================================
// GAMEDATA.CS
// ============================================================================
// PURPOSE:      Serializable class holding all save data
// USED BY:      SaveManager for JSON serialization
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