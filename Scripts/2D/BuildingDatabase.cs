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
}