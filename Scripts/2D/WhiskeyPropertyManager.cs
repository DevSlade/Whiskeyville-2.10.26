// ============================================================================
// WHISKEYPROPERTYMANAGER.CS
// ============================================================================
// PURPOSE:      Manages the active whiskey batch properties and their effects
//               on sell price, fame, and tourist attraction
// ATTACHED TO:  GameManager or persistent GameObject
// ARCHITECTURE: Singleton, event-driven
// DEPENDENCIES: InventoryManager, ResearchManager, SellManager
// ============================================================================

using UnityEngine;
using System;

public class WhiskeyPropertyManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static WhiskeyPropertyManager Instance { get; private set; }

    // ========================================================================
    // 📢 EVENTS
    // ========================================================================

    public event Action<WhiskeyBatchData> OnBatchPropertiesChanged;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private WhiskeyBatchData _activeBatch = new WhiskeyBatchData();

    // ========================================================================
    // 🎮 INSPECTOR — DEFAULTS
    // ========================================================================

    [Header("Default Batch Properties")]
    [SerializeField] private FlavorProfile _defaultFlavor = FlavorProfile.Sweet;
    [SerializeField] private int _defaultQuality = 2;
    [SerializeField] private TemperatureProfile _defaultTemperature = TemperatureProfile.Warm;
    [SerializeField] private int _defaultBatchSize = 1;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeBatch();
            Debug.Log("[WhiskeyPropertyManager] ✅ Singleton initialized.");
        }
        else
        {
            Debug.LogWarning("[WhiskeyPropertyManager] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 INITIALIZATION
    // ========================================================================

    private void InitializeBatch()
    {
        _activeBatch = new WhiskeyBatchData(
            _defaultFlavor,
            _defaultQuality,
            _defaultTemperature,
            _defaultBatchSize
        );

        Debug.Log($"[WhiskeyPropertyManager] 🥃 Active batch: {_activeBatch}");
    }

    // ========================================================================
    // 📊 PUBLIC API — READ PROPERTIES
    // ========================================================================

    public WhiskeyBatchData ActiveBatch => _activeBatch;

    public FlavorProfile CurrentFlavor => _activeBatch.flavor;
    public int CurrentQuality => _activeBatch.quality;
    public TemperatureProfile CurrentTemperature => _activeBatch.temperature;
    public int CurrentBatchSize => _activeBatch.batchSize;

    // ========================================================================
    // 💰 SELL PRICE MULTIPLIER
    // ========================================================================

    /// <summary>
    /// Combined sell price multiplier from quality × flavor × temperature.
    /// Queried by SellManager to adjust final sell price.
    /// </summary>
    public float GetSellPriceMultiplier()
    {
        float multiplier = _activeBatch.QualitySellMultiplier
                         * _activeBatch.FlavorSellMultiplier
                         * _activeBatch.TemperatureSellMultiplier;

        // Apply research bonus if available
        if (ResearchManager.Instance != null)
        {
            multiplier *= ResearchManager.Instance.GetResearchMultiplier("flavor_bonus");
        }

        return multiplier;
    }

    // ========================================================================
    // ⭐ FAME PER SALE
    // ========================================================================

    /// <summary>
    /// Fame earned per unit sold. Queried by SellManager.
    /// </summary>
    public int GetFamePerSale()
    {
        return _activeBatch.FamePerSale;
    }

    // ========================================================================
    // 🧬 MEME SCORE
    // ========================================================================

    /// <summary>
    /// Whiskey's contribution to the Meme Score (0–55).
    /// Combined with BottleCustomizationManager bonus for full score.
    /// </summary>
    public int GetWhiskeyMemeScore()
    {
        return _activeBatch.WhiskeyMemeScore;
    }

    // ========================================================================
    // ✏️ PUBLIC API — SET PROPERTIES
    // ========================================================================

    public void SetFlavor(FlavorProfile flavor)
    {
        if (_activeBatch.flavor == flavor) return;

        // Apple flavor requires research unlock
        if (flavor == FlavorProfile.SweetApple)
        {
            if (ResearchManager.Instance == null || !ResearchManager.Instance.HasResearch("apple_orchard"))
            {
                Debug.LogWarning("[WhiskeyPropertyManager] ⚠️ Apple Orchard research not unlocked.");
                return;
            }
        }

        _activeBatch.flavor = flavor;
        Debug.Log($"[WhiskeyPropertyManager] 🌽 Flavor set to: {flavor}");
        OnBatchPropertiesChanged?.Invoke(_activeBatch);
    }

    public void SetTemperature(TemperatureProfile temperature)
    {
        if (_activeBatch.temperature == temperature) return;

        if (temperature != TemperatureProfile.Warm)
        {
            if (ResearchManager.Instance == null || !ResearchManager.Instance.HasResearch("temperature_control"))
            {
                Debug.LogWarning("[WhiskeyPropertyManager] ⚠️ Temperature Control research not unlocked.");
                return;
            }
        }

        _activeBatch.temperature = temperature;
        Debug.Log($"[WhiskeyPropertyManager] 🌡️ Temperature set to: {temperature}");
        OnBatchPropertiesChanged?.Invoke(_activeBatch);
    }

    public void SetBatchSize(int batchSize)
    {
        int maxBatchSize = GetMaxBatchSize();
        batchSize = Mathf.Clamp(batchSize, 1, maxBatchSize);

        if (_activeBatch.batchSize == batchSize) return;

        _activeBatch.batchSize = batchSize;
        Debug.Log($"[WhiskeyPropertyManager] 📦 Batch size set to: ×{batchSize}");
        OnBatchPropertiesChanged?.Invoke(_activeBatch);
    }

    // ========================================================================
    // 🔬 QUALITY CALCULATION
    // ========================================================================

    /// <summary>
    /// Recalculates quality when a batch completes in the Rickhouse.
    /// Called by BuildingBehavior (or a future RickhouseBehavior) after production.
    /// </summary>
    public void RecalculateQuality(int barrelQualityBonus = 0, bool isCriticalBatch = false)
    {
        if (ResearchManager.Instance == null || !ResearchManager.Instance.HasResearch("quality_basics"))
        {
            _activeBatch.quality = _defaultQuality;
            return;
        }

        int quality = 2; // Base quality

        // Barrel type bonus (passed in from future BarrelManager / CooperageBehavior)
        quality += barrelQualityBonus;

        // Critical batch bonus
        if (isCriticalBatch)
        {
            quality += 1;
        }

        // Master Blender research bonus
        if (ResearchManager.Instance.HasResearch("master_blender"))
        {
            quality += 1;
        }

        _activeBatch.quality = Mathf.Clamp(quality, 1, 5);
        Debug.Log($"[WhiskeyPropertyManager] ⭐ Quality calculated: {_activeBatch.quality}★");
        OnBatchPropertiesChanged?.Invoke(_activeBatch);
    }

    // ========================================================================
    // 🎲 CRITICAL BATCH ROLL
    // ========================================================================

    /// <summary>
    /// Rolls for a Critical Batch (normally 5%, 10% with Jackpot Roll research).
    /// Returns true if this is a critical batch.
    /// </summary>
    public bool RollCriticalBatch()
    {
        if (ResearchManager.Instance == null || !ResearchManager.Instance.HasResearch("critical_batch"))
        {
            return false;
        }

        float critChance = ResearchManager.Instance.HasResearch("jackpot_roll") ? 0.10f : 0.05f;
        bool isCritical = UnityEngine.Random.value < critChance;

        if (isCritical)
        {
            Debug.Log("[WhiskeyPropertyManager] ✨ Critical Batch triggered!");
        }

        return isCritical;
    }

    // ========================================================================
    // 📏 BATCH SIZE HELPERS
    // ========================================================================

    private int GetMaxBatchSize()
    {
        if (ResearchManager.Instance == null) return 1;

        if (ResearchManager.Instance.HasResearch("triple_batch")) return 3;
        if (ResearchManager.Instance.HasResearch("double_batch")) return 2;
        return 1;
    }

    // ========================================================================
    // 💾 SAVE / LOAD
    // ========================================================================

    public void LoadFromSaveData(int flavorIndex, int quality, int temperatureIndex, int batchSize)
    {
        _activeBatch.flavor = (FlavorProfile)flavorIndex;
        _activeBatch.quality = Mathf.Clamp(quality, 1, 5);
        _activeBatch.temperature = (TemperatureProfile)temperatureIndex;
        _activeBatch.batchSize = Mathf.Max(1, batchSize);

        Debug.Log($"[WhiskeyPropertyManager] 💾 Loaded from save: {_activeBatch}");
        OnBatchPropertiesChanged?.Invoke(_activeBatch);
    }

    public int GetFlavorIndex() => (int)_activeBatch.flavor;
    public int GetTemperatureIndex() => (int)_activeBatch.temperature;
}
