// ============================================================================
// BOTTLECUSTOMIZATIONMANAGER.CS
// ============================================================================
// PURPOSE:      Manages active bottle design and its effects on sell price
//               and Meme Score. Opened by the Bottling House building.
// ATTACHED TO:  GameManager or persistent GameObject
// ARCHITECTURE: Singleton, event-driven
// DEPENDENCIES: UIManager, ResearchManager, SaveManager
// ============================================================================

using UnityEngine;
using System;

public class BottleCustomizationManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static BottleCustomizationManager Instance { get; private set; }

    // ========================================================================
    // 📢 EVENTS
    // ========================================================================

    public event Action<BottleCustomizationData> OnDesignApplied;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private BottleCustomizationData _activeDesign = new BottleCustomizationData();
    private bool _bottlingHouseBuilt = false;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[BottleCustomizationManager] ✅ Singleton initialized.");
        }
        else
        {
            Debug.LogWarning("[BottleCustomizationManager] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🏗️ BOTTLING HOUSE STATE
    // ========================================================================

    /// <summary>
    /// Called by BuildingPlacementManager when a Bottling House is placed.
    /// </summary>
    public void NotifyBottlingHouseBuilt()
    {
        _bottlingHouseBuilt = true;
        Debug.Log("[BottleCustomizationManager] 🏗️ Bottling House built — customization active.");
    }

    /// <summary>
    /// Called by BuildingPlacementManager if the Bottling House is demolished.
    /// </summary>
    public void NotifyBottlingHouseRemoved()
    {
        _bottlingHouseBuilt = false;
        Debug.Log("[BottleCustomizationManager] ❌ Bottling House removed — customization inactive.");
    }

    public bool IsBottlingHouseBuilt => _bottlingHouseBuilt;

    // ========================================================================
    // 🍾 ACTIVE DESIGN
    // ========================================================================

    public BottleCustomizationData ActiveDesign => _activeDesign;

    /// <summary>
    /// Applies a new bottle design. Only effective if Bottling House is built.
    /// </summary>
    public bool ApplyDesign(BottleCustomizationData design)
    {
        if (!_bottlingHouseBuilt)
        {
            Debug.LogWarning("[BottleCustomizationManager] ⚠️ Cannot apply design — Bottling House not built.");
            return false;
        }

        if (design == null)
        {
            Debug.LogError("[BottleCustomizationManager] ❌ Design is null.");
            return false;
        }

        // Validate glass type against research unlocks
        if (!IsGlassTypeUnlocked(design.glassTypeIndex))
        {
            Debug.LogWarning($"[BottleCustomizationManager] ⚠️ Glass type {design.glassTypeIndex} not unlocked.");
            design.glassTypeIndex = 0; // Fallback to Standard
        }

        // Clamp text fields
        design.distilleryName = ClampString(design.distilleryName, 24, "My Distillery");
        design.whiskeyName    = ClampString(design.whiskeyName,    20, "House Whiskey");
        design.tagline        = ClampString(design.tagline,        40, "");

        _activeDesign = design;

        Debug.Log($"[BottleCustomizationManager] 🍾 Design applied: {_activeDesign}");

        OnDesignApplied?.Invoke(_activeDesign);
        return true;
    }

    // ========================================================================
    // 💰 SELL PRICE MULTIPLIER
    // ========================================================================

    /// <summary>
    /// Glass type sell price multiplier. Returns 1.0 if Bottling House not built.
    /// Queried by SellManager to adjust final sell price.
    /// </summary>
    public float GetSellPriceMultiplier()
    {
        if (!_bottlingHouseBuilt) return 1.0f;

        return _activeDesign.GlassSellMultiplier;
    }

    // ========================================================================
    // 🧬 MEME SCORE BONUS
    // ========================================================================

    /// <summary>
    /// Meme Score bonus from bottle design (0–59). Returns 0 if Bottling House
    /// not built. Combined with WhiskeyPropertyManager's score for full Meme Score.
    /// </summary>
    public int GetMemeScoreBonus()
    {
        if (!_bottlingHouseBuilt) return 0;

        int bonus = _activeDesign.BottleMemeScore;

        // Research: Meme Boost increases customization Meme Score by 50%
        if (ResearchManager.Instance != null && ResearchManager.Instance.HasResearch("meme_boost"))
        {
            bonus = Mathf.RoundToInt(bonus * 1.5f);
        }

        return bonus;
    }

    // ========================================================================
    // 🔓 UNLOCK VALIDATION
    // ========================================================================

    /// <summary>
    /// Checks whether a glass type index is unlocked based on current research.
    /// </summary>
    public bool IsGlassTypeUnlocked(int glassIndex)
    {
        if (glassIndex == 0) return true; // Standard always available

        if (ResearchManager.Instance == null) return false;

        switch (glassIndex)
        {
            case 1: // Hip Flask
            case 3: // Ceramic Jug
                return ResearchManager.Instance.HasResearch("glass_variety");

            case 2: // Tall Decanter
            case 5: // Personalized Crock
                return ResearchManager.Instance.HasResearch("premium_glass");

            case 4: // Crystal Decanter
                return ResearchManager.Instance.HasResearch("crystal_tier");

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks whether an emblem index is unlocked based on current research.
    /// </summary>
    public bool IsEmblemUnlocked(int emblemIndex)
    {
        if (emblemIndex <= 1) return true; // None and Simple Star always available

        if (ResearchManager.Instance == null) return false;

        if (emblemIndex <= 4) // Wheat Sheaf, Oak Barrel, Running Fox
        {
            return ResearchManager.Instance.HasResearch("emblem_library");
        }

        // Eagle Crest, Black Label Seal, Custom Emblem
        return ResearchManager.Instance.HasResearch("premium_glass"); // Reuse high-tier research
    }

    // ========================================================================
    // 💾 SAVE / LOAD
    // ========================================================================

    public void LoadFromSaveData(
        int glassTypeIndex,
        int labelColorIndex,
        int emblemIndex,
        string distilleryName,
        string whiskeyName,
        string tagline,
        int vintageYear,
        bool bottlingHouseBuilt)
    {
        _bottlingHouseBuilt = bottlingHouseBuilt;

        _activeDesign = new BottleCustomizationData(
            glassTypeIndex,
            labelColorIndex,
            emblemIndex,
            distilleryName,
            whiskeyName,
            tagline,
            vintageYear
        );

        Debug.Log($"[BottleCustomizationManager] 💾 Loaded from save: {_activeDesign}");
    }

    // ========================================================================
    // 🔧 HELPERS
    // ========================================================================

    private string ClampString(string value, int maxLength, string fallback)
    {
        if (string.IsNullOrEmpty(value)) return fallback;
        return value.Length > maxLength ? value.Substring(0, maxLength) : value;
    }
}
