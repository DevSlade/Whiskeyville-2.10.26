// ============================================================================
// WHISKEYPROPERTIES.CS
// ============================================================================
// PURPOSE:      Data types defining all whiskey batch properties
//               (flavor, quality, temperature, batch size)
// USED BY:      WhiskeyPropertyManager, SellManager, BottleCustomizationManager
// ARCHITECTURE: Pure data — no MonoBehaviour, no Unity lifecycle
// ============================================================================

using System;

// ============================================================================
// 🌽 FLAVOR PROFILE
// ============================================================================

public enum FlavorProfile
{
    Sweet,       // Corn-based — broad tourist appeal
    Spicy,       // Rye-based — attracts Critics and Celebrities
    Smooth,      // Wheat-based — high Passerby volume
    Rich,        // Barley-based — premium single-unit price
    SweetApple   // Apple Orchard blend — requires Research unlock
}

// ============================================================================
// 🌡️ TEMPERATURE PROFILE
// ============================================================================

public enum TemperatureProfile
{
    Cold,  // Low proof (70–80) — broader audience, more tourists
    Warm,  // Medium proof (86–92) — balanced; no modifier
    Hot    // High proof (100–120) — premium price, niche market
}

// ============================================================================
// 📦 WHISKEY BATCH DATA
// ============================================================================

[Serializable]
public class WhiskeyBatchData
{
    // ========================================================================
    // 🔑 PROPERTIES
    // ========================================================================

    public FlavorProfile flavor = FlavorProfile.Sweet;
    public int quality = 2;                                 // 1–5 stars
    public TemperatureProfile temperature = TemperatureProfile.Warm;
    public int batchSize = 1;                               // units per Rickhouse cycle

    // ========================================================================
    // 🔧 CONSTRUCTOR
    // ========================================================================

    public WhiskeyBatchData() { }

    public WhiskeyBatchData(FlavorProfile flavor, int quality, TemperatureProfile temperature, int batchSize)
    {
        this.flavor = flavor;
        this.quality = UnityEngine.Mathf.Clamp(quality, 1, 5);
        this.temperature = temperature;
        this.batchSize = UnityEngine.Mathf.Max(1, batchSize);
    }

    // ========================================================================
    // 📊 COMPUTED VALUES
    // ========================================================================

    /// <summary>
    /// Sell price multiplier from quality (1★ = −20%, 5★ = +100%).
    /// </summary>
    public float QualitySellMultiplier
    {
        get
        {
            switch (quality)
            {
                case 1: return 0.80f;
                case 2: return 1.00f;
                case 3: return 1.25f;
                case 4: return 1.60f;
                case 5: return 2.00f;
                default: return 1.00f;
            }
        }
    }

    /// <summary>
    /// Sell price multiplier from flavor profile.
    /// </summary>
    public float FlavorSellMultiplier
    {
        get
        {
            switch (flavor)
            {
                case FlavorProfile.Sweet:      return 1.00f;
                case FlavorProfile.Spicy:      return 1.15f;
                case FlavorProfile.Smooth:     return 1.00f;
                case FlavorProfile.Rich:       return 1.25f;
                case FlavorProfile.SweetApple: return 1.10f;
                default:                       return 1.00f;
            }
        }
    }

    /// <summary>
    /// Sell price multiplier from temperature profile.
    /// </summary>
    public float TemperatureSellMultiplier
    {
        get
        {
            switch (temperature)
            {
                case TemperatureProfile.Cold: return 1.00f;
                case TemperatureProfile.Warm: return 1.00f;
                case TemperatureProfile.Hot:  return 1.20f;
                default:                      return 1.00f;
            }
        }
    }

    /// <summary>
    /// Fame earned per unit sold. Higher quality and rare flavors yield more fame.
    /// </summary>
    public int FamePerSale
    {
        get
        {
            int fame = quality - 1; // 0–4 from quality

            switch (flavor)
            {
                case FlavorProfile.Spicy:      fame += 1; break;
                case FlavorProfile.Rich:       fame += 2; break;
                case FlavorProfile.SweetApple: fame += 1; break;
                default: break;
            }

            return UnityEngine.Mathf.Max(1, fame);
        }
    }

    /// <summary>
    /// Meme Score contribution from whiskey properties alone (0–45 points).
    /// Combined with BottleCustomizationManager's bonus for the full Meme Score.
    /// </summary>
    public int WhiskeyMemeScore
    {
        get
        {
            int score = 0;

            // Quality contribution (max 20 pts)
            score += (quality - 1) * 5;

            // Flavor distinctiveness (max 25 pts)
            switch (flavor)
            {
                case FlavorProfile.Sweet:      score += 10; break;
                case FlavorProfile.Smooth:     score += 12; break;
                case FlavorProfile.Spicy:      score += 20; break;
                case FlavorProfile.SweetApple: score += 18; break;
                case FlavorProfile.Rich:       score += 25; break;
            }

            // Temperature contribution (max 10 pts)
            switch (temperature)
            {
                case TemperatureProfile.Hot:  score += 10; break;
                case TemperatureProfile.Warm: score += 5;  break;
                case TemperatureProfile.Cold: score += 3;  break;
            }

            return UnityEngine.Mathf.Clamp(score, 0, 55);
        }
    }

    // ========================================================================
    // 🖨️ DEBUG
    // ========================================================================

    public override string ToString()
    {
        return $"{quality}★ {flavor} ({temperature}) — Batch ×{batchSize} | " +
               $"Sell ×{QualitySellMultiplier * FlavorSellMultiplier * TemperatureSellMultiplier:F2} | " +
               $"Fame +{FamePerSale}/sale | Meme {WhiskeyMemeScore}/55";
    }
}
