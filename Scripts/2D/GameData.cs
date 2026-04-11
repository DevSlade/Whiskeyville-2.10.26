// ============================================================================
// GAMEDATA.CS
// ============================================================================
// PURPOSE:      Serializable class holding all save data
// VERSION:      v2 — Added buildingName for safe save/load, terrain data
// UPDATED:      April 8, 2026
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
    // 🌾 TILE TERRAIN (hoe-tilled tiles that differ from grid seed)
    // ========================================================================

    public List<TileTerrainSaveData> tileTerrainChanges = new List<TileTerrainSaveData>();

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
        tileTerrainChanges = new List<TileTerrainSaveData>();
    }
}

[Serializable]
public class BuildingSaveData
{
    public int buildingIndex;

    // Safety net: building name for fallback lookup if database order changes
    public string buildingName;

    public float posX;
    public float posY;
    public int gridX;
    public int gridY;

    // Crop-specific
    public bool isCrop;
    public int growthStage;
    public bool isFullyGrown;

    public BuildingSaveData() { }

    public BuildingSaveData(int index, string name, Vector3 position, Vector2Int gridPos,
                            bool crop, int stage, bool fullyGrown)
    {
        buildingIndex = index;
        buildingName = name ?? "";
        posX = position.x;
        posY = position.y;
        gridX = gridPos.x;
        gridY = gridPos.y;
        isCrop = crop;
        growthStage = stage;
        isFullyGrown = fullyGrown;
    }
}

/// <summary>
/// Stores tiles that were modified from their seed-generated terrain type
/// (e.g., Grass tilled to Farm). Restored after grid regeneration.
/// </summary>
[Serializable]
public class TileTerrainSaveData
{
    public int gridX;
    public int gridY;
    public int terrainType; // Cast from TerrainType enum

    public TileTerrainSaveData() { }

    public TileTerrainSaveData(int x, int y, TerrainType terrain)
    {
        gridX = x;
        gridY = y;
        terrainType = (int)terrain;
    }
}