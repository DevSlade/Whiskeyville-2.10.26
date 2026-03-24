// ============================================================================
// BUILDINGDATA.CS
// ============================================================================
// PURPOSE:      ScriptableObject defining a building type
// VERSION:      v2 — Added dual input support for Rickhouse
// UPDATED:      February 11, 2026
// ============================================================================

using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Whiskeyville/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Basic Info")]
    public string buildingName = "Building";
    public GameObject prefab;
    public int cost = 50;

    [Header("Building Type")]
    [Tooltip("Is this a crop that requires click-to-harvest?")]
    public bool isCrop = false;

    [Header("Production - Output")]
    public string outputResource = "Corn";
    public int outputAmount = 1;
    public float productionInterval = 5f;

    [Header("Production - Input (Primary)")]
    public bool requiresInput = false;
    public string inputResource = "";
    public int inputAmount = 0;

    [Header("Production - Input (Secondary — Dual Input Buildings)")]
    [Tooltip("Leave empty if building only needs one input")]
    public string secondInputResource = "";
    public int secondInputAmount = 0;

    [Header("Popup")]
    public Color popupColor = Color.yellow;

    // ========================================================================
    // 🔧 HELPER PROPERTY
    // ========================================================================

    public bool RequiresDualInput
    {
        get
        {
            return requiresInput
                && !string.IsNullOrEmpty(secondInputResource)
                && secondInputAmount > 0;
        }
    }
}