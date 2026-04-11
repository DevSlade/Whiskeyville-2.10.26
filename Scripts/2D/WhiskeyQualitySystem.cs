// ============================================================================
// WHISKEYQUALITYSYSTEM.CS
// ============================================================================
// PURPOSE:      Calculates whiskey quality using the 6-factor weighted formula
//               from the Master GDD. Quality affects sell price and fame.
// VERSION:      v1 — Foundation (static calculator + batch data)
// CREATED:      April 4, 2026
// ============================================================================
// DEV GUIDE:
//   BASIC USAGE (demo — all defaults):
//     float quality = WhiskeyQualitySystem.CalculateQuality();
//     int stars = WhiskeyQualitySystem.QualityToStars(quality);
//     float priceMultiplier = WhiskeyQualitySystem.GetPriceMultiplier(stars);
//
//   ADVANCED USAGE (with factors):
//     var factors = new QualityFactors
//     {
//         AgingDays = 180,
//         GrainQuality = 0.8f,
//         MashBillScore = 0.9f,
//         CharLevel = 3,
//         BarrelQuality = 0.7f,
//         WaterPurity = 0.85f
//     };
//     float quality = WhiskeyQualitySystem.CalculateQuality(factors);
//
//   6-FACTOR FORMULA (from GDD Section 7):
//     Quality = (Aging * 0.25) + (Grain * 0.20) + (MashBill * 0.20) +
//               (Char * 0.15) + (Barrel * 0.10) + (Water * 0.10)
//     Result: 0.0 - 1.0 → mapped to 1-5 stars
//
//   INTEGRATION POINTS:
//     - SellManager: multiply base price by GetPriceMultiplier(stars)
//     - FameManager: higher stars = more fame per sale
//     - Leaderboard: track best quality batch
//     - Achievement: "Craft a 5-star whiskey"
// ============================================================================

using UnityEngine;

/// <summary>
/// Data container for all factors that affect whiskey quality.
/// Pass to CalculateQuality() for full formula evaluation.
/// </summary>
[System.Serializable]
public struct QualityFactors
{
    [Tooltip("Days aged in rickhouse (0-365+). Sweet spot: 120-180.")]
    public int AgingDays;

    [Tooltip("Base grain quality (0.0-1.0). Affected by crop type and farm tile.")]
    [Range(0f, 1f)] public float GrainQuality;

    [Tooltip("Mash bill score (0.0-1.0). Affected by grain combination.")]
    [Range(0f, 1f)] public float MashBillScore;

    [Tooltip("Barrel char level (1-5). Level 4 = 'alligator char' = optimal.")]
    [Range(1, 5)] public int CharLevel;

    [Tooltip("Barrel quality (0.0-1.0). New oak = 1.0, reused = lower.")]
    [Range(0f, 1f)] public float BarrelQuality;

    [Tooltip("Water purity (0.0-1.0). Limestone spring = 1.0, basic well = 0.5.")]
    [Range(0f, 1f)] public float WaterPurity;

    /// <summary>Returns default demo-friendly factors (mid-range quality).</summary>
    public static QualityFactors Default => new QualityFactors
    {
        AgingDays = 90,
        GrainQuality = 0.6f,
        MashBillScore = 0.5f,
        CharLevel = 3,
        BarrelQuality = 0.6f,
        WaterPurity = 0.5f
    };
}

/// <summary>
/// Static utility class for whiskey quality calculation.
/// No MonoBehaviour needed — call directly from any script.
/// </summary>
public static class WhiskeyQualitySystem
{
    // ========================================================================
    // CORE FORMULA
    // ========================================================================

    /// <summary>
    /// Calculate whiskey quality from all 6 factors.
    /// Returns 0.0 - 1.0 (maps to 1-5 stars).
    /// </summary>
    public static float CalculateQuality(QualityFactors factors)
    {
        float agingScore    = CalculateAgingScore(factors.AgingDays);
        float grainScore    = Mathf.Clamp01(factors.GrainQuality);
        float mashBillScore = Mathf.Clamp01(factors.MashBillScore);
        float charScore     = CalculateCharScore(factors.CharLevel);
        float barrelScore   = Mathf.Clamp01(factors.BarrelQuality);
        float waterScore    = Mathf.Clamp01(factors.WaterPurity);

        float quality =
            (agingScore    * GameConstants.Balance.QUALITY_AGING_WEIGHT) +
            (grainScore    * GameConstants.Balance.QUALITY_GRAIN_WEIGHT) +
            (mashBillScore * GameConstants.Balance.QUALITY_MASHBILL_WEIGHT) +
            (charScore     * GameConstants.Balance.QUALITY_CHAR_WEIGHT) +
            (barrelScore   * GameConstants.Balance.QUALITY_BARREL_WEIGHT) +
            (waterScore    * GameConstants.Balance.QUALITY_WATER_WEIGHT);

        return Mathf.Clamp01(quality);
    }

    /// <summary>Simplified quality calculation using default factors. Good for demo.</summary>
    public static float CalculateQuality()
    {
        return CalculateQuality(QualityFactors.Default);
    }

    // ========================================================================
    // STAR RATING
    // ========================================================================

    /// <summary>Convert 0-1 quality to 1-5 star rating.</summary>
    public static int QualityToStars(float quality)
    {
        quality = Mathf.Clamp01(quality);

        if (quality >= 0.90f) return 5;  // Exceptional
        if (quality >= 0.70f) return 4;  // Premium
        if (quality >= 0.50f) return 3;  // Standard
        if (quality >= 0.30f) return 2;  // Below Average
        return 1;                         // Rotgut
    }

    /// <summary>Get descriptive label for star rating.</summary>
    public static string GetStarLabel(int stars)
    {
        return stars switch
        {
            5 => "Exceptional",
            4 => "Premium",
            3 => "Standard",
            2 => "Below Average",
            1 => "Rotgut",
            _ => "Unknown"
        };
    }

    /// <summary>Get star display string (e.g., "★★★★☆").</summary>
    public static string GetStarDisplay(int stars)
    {
        stars = Mathf.Clamp(stars, 1, 5);
        string filled = new string('\u2605', stars);     // ★
        string empty = new string('\u2606', 5 - stars);  // ☆
        return filled + empty;
    }

    // ========================================================================
    // PRICE MULTIPLIER
    // ========================================================================

    /// <summary>
    /// Get price multiplier for whiskey based on star rating.
    /// 1-star = 0.5x, 5-star = 3.0x
    /// </summary>
    public static float GetPriceMultiplier(int stars)
    {
        return stars switch
        {
            1 => 0.5f,
            2 => 0.8f,
            3 => 1.0f,
            4 => 1.8f,
            5 => GameConstants.Balance.QUALITY_PRICE_MULT_MAX,  // 3.0x
            _ => 1.0f
        };
    }

    /// <summary>Calculate sell price for a whiskey batch.</summary>
    public static int CalculateSellPrice(int basePrice, int stars)
    {
        return Mathf.RoundToInt(basePrice * GetPriceMultiplier(stars));
    }

    /// <summary>Calculate fame earned from selling whiskey.</summary>
    public static int CalculateFameEarned(int stars)
    {
        return stars switch
        {
            1 => 0,     // Rotgut earns no fame
            2 => 1,
            3 => 3,
            4 => 8,
            5 => 20,    // Exceptional earns 20 fame per sale
            _ => 0
        };
    }

    // ========================================================================
    // SUB-FACTOR CALCULATIONS
    // ========================================================================

    /// <summary>
    /// Aging follows a bell curve: too little = harsh, sweet spot = 120-180 days,
    /// too much = over-oaked (diminishing returns after 365 days).
    /// </summary>
    private static float CalculateAgingScore(int days)
    {
        if (days <= 0) return 0f;
        if (days < 30) return days / 30f * 0.3f;         // Very young: 0-0.3
        if (days < 90) return 0.3f + (days - 30f) / 60f * 0.4f;  // Maturing: 0.3-0.7
        if (days <= 180) return 0.7f + (days - 90f) / 90f * 0.3f; // Sweet spot: 0.7-1.0
        if (days <= 365) return 1.0f;                      // Peak: 1.0
        // Over-aged: slowly declining
        float overAge = (days - 365f) / 365f;
        return Mathf.Max(0.6f, 1.0f - overAge * 0.2f);
    }

    /// <summary>
    /// Char level follows an inverted-U: Level 4 (alligator char) is optimal.
    /// Level 1 = light toast, Level 5 = heavy char (slightly bitter).
    /// </summary>
    private static float CalculateCharScore(int level)
    {
        return level switch
        {
            1 => 0.4f,   // Light toast — mild vanilla
            2 => 0.6f,   // Medium toast — caramel notes
            3 => 0.8f,   // Medium char — balanced
            4 => 1.0f,   // Alligator char — peak complexity
            5 => 0.85f,  // Heavy char — slightly bitter, but deep
            _ => 0.5f
        };
    }
}
