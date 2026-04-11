// ============================================================================
// FAMEMANAGER.CS
// ============================================================================
// PURPOSE:      Tracks Fame (second currency), fame tiers, and fame events
//               Fame is earned by selling quality whiskey, completing quests,
//               winning events, and attracting tourists
// VERSION:      v1 — Foundation
// CREATED:      April 4, 2026
// ARCHITECTURE: Singleton (BaseSingleton), event-driven
// ============================================================================
// DEV GUIDE:
//   EARNING FAME:
//     FameManager.Instance.AddFame(50, "Sold 5-star whiskey");
//     FameManager.Instance.AddFame(100, "Won Harvest Festival");
//
//   CHECKING FAME:
//     int total = FameManager.Instance.TotalFame;
//     FameTier tier = FameManager.Instance.CurrentTier;
//     bool canAccess = FameManager.Instance.HasTier(FameTier.Famous);
//
//   EVENTS:
//     FameManager.OnFameChanged += (newTotal, delta) => { ... };
//     FameManager.OnTierUp += (newTier) => { ... };
//
//   FAME TIERS (from GDD):
//     Unknown  (0-99)     — Starting state
//     Known    (100-499)  — Unlock: General Store
//     Respected(500-1999) — Unlock: Bank, Train Depot
//     Famous   (2000-9999)— Unlock: Tasting Room, Tourism
//     Legendary(10000-49999) — Unlock: Casino, River Trade
//     Iconic   (50000+)   — Unlock: Prestige system
// ============================================================================

using UnityEngine;
using System;

/// <summary>Fame tier progression levels.</summary>
public enum FameTier
{
    Unknown   = 0,
    Known     = 1,
    Respected = 2,
    Famous    = 3,
    Legendary = 4,
    Iconic    = 5
}

public class FameManager : BaseSingleton<FameManager>
{
    // ========================================================================
    // EVENTS
    // ========================================================================

    /// <summary>Fires when fame changes. Args: (newTotal, deltaAmount)</summary>
    public static event Action<int, int> OnFameChanged;

    /// <summary>Fires when player reaches a new fame tier. Args: (newTier)</summary>
    public static event Action<FameTier> OnTierUp;

    // ========================================================================
    // STATE
    // ========================================================================

    private int _totalFame = 0;
    private FameTier _currentTier = FameTier.Unknown;

    // ========================================================================
    // PUBLIC PROPERTIES
    // ========================================================================

    public int TotalFame => _totalFame;
    public FameTier CurrentTier => _currentTier;

    /// <summary>Descriptive name for current tier.</summary>
    public string TierName => _currentTier switch
    {
        FameTier.Unknown   => "Unknown Distiller",
        FameTier.Known     => "Known Distiller",
        FameTier.Respected => "Respected Distiller",
        FameTier.Famous    => "Famous Distiller",
        FameTier.Legendary => "Legendary Distiller",
        FameTier.Iconic    => "Iconic Distiller",
        _ => "Unknown"
    };

    /// <summary>Fame needed to reach next tier. -1 if at max.</summary>
    public int FameToNextTier
    {
        get
        {
            int threshold = GetTierThreshold(_currentTier + 1);
            return threshold < 0 ? -1 : threshold - _totalFame;
        }
    }

    /// <summary>Progress to next tier as 0-1 float.</summary>
    public float TierProgress
    {
        get
        {
            int currentThreshold = GetTierThreshold(_currentTier);
            int nextThreshold = GetTierThreshold(_currentTier + 1);
            if (nextThreshold < 0) return 1f;

            int range = nextThreshold - currentThreshold;
            int progress = _totalFame - currentThreshold;
            return range > 0 ? Mathf.Clamp01((float)progress / range) : 1f;
        }
    }

    // ========================================================================
    // SINGLETON INIT
    // ========================================================================

    protected override bool Persistent => true;

    protected override void OnSingletonAwake()
    {
        _totalFame = 0;
        _currentTier = FameTier.Unknown;
    }

    // ========================================================================
    // PUBLIC API
    // ========================================================================

    /// <summary>Add fame points. Negative values are clamped to 0 total.</summary>
    public void AddFame(int amount, string reason = "")
    {
        if (amount == 0) return;

        int oldTotal = _totalFame;
        _totalFame = Mathf.Max(0, _totalFame + amount);

        // Check for tier change
        FameTier oldTier = _currentTier;
        _currentTier = CalculateTier(_totalFame);

        // Fire events
        OnFameChanged?.Invoke(_totalFame, amount);

        if (_currentTier > oldTier)
        {
            OnTierUp?.Invoke(_currentTier);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.SFX_SUCCESS);

            Debug.Log($"[FameManager] TIER UP! {oldTier} -> {_currentTier} ({TierName})");
        }

        string logReason = string.IsNullOrEmpty(reason) ? "" : $" ({reason})";
        Debug.Log($"[FameManager] Fame {(amount > 0 ? "+" : "")}{amount}{logReason}. Total: {_totalFame} [{TierName}]");
    }

    /// <summary>Set fame to exact value (for save/load).</summary>
    public void SetFame(int value)
    {
        _totalFame = Mathf.Max(0, value);
        _currentTier = CalculateTier(_totalFame);
        OnFameChanged?.Invoke(_totalFame, 0);
    }

    /// <summary>Check if player has reached at least this tier.</summary>
    public bool HasTier(FameTier tier) => _currentTier >= tier;

    /// <summary>Check if player has at least this much fame.</summary>
    public bool HasFame(int amount) => _totalFame >= amount;

    // ========================================================================
    // TIER CALCULATION
    // ========================================================================

    private static FameTier CalculateTier(int fame)
    {
        if (fame >= GameConstants.Balance.FAME_TIER_5) return FameTier.Iconic;
        if (fame >= GameConstants.Balance.FAME_TIER_4) return FameTier.Legendary;
        if (fame >= GameConstants.Balance.FAME_TIER_3) return FameTier.Famous;
        if (fame >= GameConstants.Balance.FAME_TIER_2) return FameTier.Respected;
        if (fame >= GameConstants.Balance.FAME_TIER_1) return FameTier.Known;
        return FameTier.Unknown;
    }

    private static int GetTierThreshold(FameTier tier)
    {
        return tier switch
        {
            FameTier.Unknown   => 0,
            FameTier.Known     => GameConstants.Balance.FAME_TIER_1,
            FameTier.Respected => GameConstants.Balance.FAME_TIER_2,
            FameTier.Famous    => GameConstants.Balance.FAME_TIER_3,
            FameTier.Legendary => GameConstants.Balance.FAME_TIER_4,
            FameTier.Iconic    => GameConstants.Balance.FAME_TIER_5,
            _ => -1  // Max tier, no next threshold
        };
    }

    private static int GetTierThreshold(int tierIndex)
    {
        if (tierIndex < 0 || tierIndex > 5) return -1;
        return GetTierThreshold((FameTier)tierIndex);
    }
}
