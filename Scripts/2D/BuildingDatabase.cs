// ============================================================================
// BUILDINGDATABASE. CS
// ============================================================================
// PURPOSE:      Holds all building definitions, accessed by index or hotkey
// ATTACHED TO:  GameManager or standalone GameObject
// ============================================================================

using UnityEngine;

public class BuildingDatabase : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static BuildingDatabase Instance { get; private set; }

    // ========================================================================
    // 🏗️ BUILDING DATA
    // ========================================================================

    [Header("Building Definitions (Index = Hotkey - 1)")]
    [Tooltip("0 = Key 1, 1 = Key 2, etc.")]
    public BuildingData[] buildings;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log($"[BuildingDatabase] ✅ Initialized with {buildings.Length} buildings.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 PUBLIC METHODS
    // ========================================================================

    public BuildingData GetBuilding(int index)
    {
        if (index >= 0 && index < buildings.Length)
        {
            return buildings[index];
        }
        return null;
    }

    public int GetBuildingCount()
    {
        return buildings.Length;
    }

    /// <summary>
    /// Finds the database index for a building by matching its prefab reference.
    /// Used by hoe tool to set correct buildingIndex for save/load.
    /// Returns -1 if not found.
    /// </summary>
    public int FindIndexByPrefab(GameObject prefab)
    {
        if (prefab == null) return -1;

        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] != null && buildings[i].prefab == prefab)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Finds a building's database index by name (case-insensitive).
    /// Fallback for save/load when prefab matching isn't possible.
    /// Returns -1 if not found.
    /// </summary>
    public int FindIndexByName(string buildingName)
    {
        if (string.IsNullOrEmpty(buildingName)) return -1;

        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] != null &&
                string.Equals(buildings[i].buildingName, buildingName, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
}