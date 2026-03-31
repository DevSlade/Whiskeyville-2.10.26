// ============================================================================
// BOTTLECUSTOMIZATIONDATA.CS
// ============================================================================
// PURPOSE:      Serializable data class holding all bottle design choices
// USED BY:      BottleCustomizationManager, SaveManager, GameData
// ARCHITECTURE: Pure data — no MonoBehaviour
// ============================================================================

using System;

[Serializable]
public class BottleCustomizationData
{
    // ========================================================================
    // 🍾 GLASS TYPE
    // ========================================================================

    /// <summary>
    /// 0=Standard, 1=HipFlask, 2=TallDecanter, 3=CeramicJug,
    /// 4=CrystalDecanter, 5=PersonalizedCrock
    /// </summary>
    public int glassTypeIndex = 0;

    // ========================================================================
    // 🎨 LABEL
    // ========================================================================

    /// <summary>
    /// 0=PlainWhite, 1=KraftBrown, 2=DeepNavy, 3=EmeraldGreen,
    /// 4=CrimsonRed, 5=CharcoalBlack, 6=AgedParchment, 7=CustomGradient
    /// </summary>
    public int labelColorIndex = 1; // Default: Kraft Brown (rustic look)

    // ========================================================================
    // 🦅 EMBLEM
    // ========================================================================

    /// <summary>
    /// 0=None, 1=SimpleStar, 2=WheatSheaf, 3=OakBarrel,
    /// 4=RunningFox, 5=EagleCrest, 6=BlackLabelSeal, 7=CustomEmblem
    /// </summary>
    public int emblemIndex = 0;

    // ========================================================================
    // 📝 TEXT FIELDS
    // ========================================================================

    public string distilleryName = "My Distillery";
    public string whiskeyName    = "House Whiskey";
    public string tagline        = "";
    public int    vintageYear    = 2026;

    // ========================================================================
    // 🔧 CONSTRUCTORS
    // ========================================================================

    public BottleCustomizationData() { }

    public BottleCustomizationData(
        int glassTypeIndex,
        int labelColorIndex,
        int emblemIndex,
        string distilleryName,
        string whiskeyName,
        string tagline,
        int vintageYear)
    {
        this.glassTypeIndex   = glassTypeIndex;
        this.labelColorIndex  = labelColorIndex;
        this.emblemIndex      = emblemIndex;
        this.distilleryName   = distilleryName ?? "My Distillery";
        this.whiskeyName      = whiskeyName    ?? "House Whiskey";
        this.tagline          = tagline        ?? "";
        this.vintageYear      = vintageYear;
    }

    // ========================================================================
    // 📊 COMPUTED VALUES
    // ========================================================================

    /// <summary>
    /// Sell price multiplier granted by the selected glass type.
    /// Standard = ×1.00, Crystal Decanter = ×1.30.
    /// </summary>
    public float GlassSellMultiplier
    {
        get
        {
            switch (glassTypeIndex)
            {
                case 0: return 1.00f; // Standard Bottle
                case 1: return 1.05f; // Hip Flask
                case 2: return 1.15f; // Tall Decanter
                case 3: return 1.10f; // Ceramic Jug
                case 4: return 1.30f; // Crystal Decanter
                case 5: return 1.20f; // Personalized Crock
                default: return 1.00f;
            }
        }
    }

    /// <summary>
    /// Meme Score contribution from this bottle design (0–40 pts).
    /// </summary>
    public int BottleMemeScore
    {
        get
        {
            int score = 0;

            // Glass type bonus (max 20 pts)
            switch (glassTypeIndex)
            {
                case 0: score += 0;  break; // Standard
                case 1: score += 5;  break; // Hip Flask
                case 2: score += 10; break; // Tall Decanter
                case 3: score += 8;  break; // Ceramic Jug
                case 4: score += 20; break; // Crystal Decanter
                case 5: score += 15; break; // Personalized Crock
            }

            // Label color bonus (max 12 pts)
            int[] labelBonuses = { 0, 2, 4, 4, 6, 8, 10, 12 };
            if (labelColorIndex >= 0 && labelColorIndex < labelBonuses.Length)
            {
                score += labelBonuses[labelColorIndex];
            }

            // Emblem bonus (max 12 pts)
            int[] emblemBonuses = { 0, 2, 4, 5, 6, 8, 12, 15 };
            if (emblemIndex >= 0 && emblemIndex < emblemBonuses.Length)
            {
                score += emblemBonuses[emblemIndex];
            }

            // Naming bonus (up to 10 pts)
            bool hasCustomDistilleryName = !string.IsNullOrEmpty(distilleryName)
                                          && distilleryName != "My Distillery";
            bool hasCustomWhiskeyName    = !string.IsNullOrEmpty(whiskeyName)
                                          && whiskeyName != "House Whiskey";

            if (hasCustomDistilleryName) score += 5;
            if (hasCustomWhiskeyName)    score += 5;

            return UnityEngine.Mathf.Clamp(score, 0, 59);
        }
    }

    // ========================================================================
    // 🖨️ DEBUG
    // ========================================================================

    public override string ToString()
    {
        return $"[{distilleryName}] \"{whiskeyName}\" {vintageYear} — " +
               $"Glass:{glassTypeIndex} Label:{labelColorIndex} Emblem:{emblemIndex} | " +
               $"Sell ×{GlassSellMultiplier:F2} | Meme +{BottleMemeScore}pts";
    }
}
