// ============================================================================
// RESEARCHDATA.CS
// ============================================================================
// PURPOSE:      ScriptableObject defining a single research node in the
//               Research Lab upgrade tree
// USED BY:      ResearchManager, Research Panel UI
// ============================================================================

using UnityEngine;

public enum ResearchCategory
{
    WhiskeyMastery,
    BottleCrafting,
    LocalMarketing,
    FinancialOutlook,
    RetentionGrowth
}

[CreateAssetMenu(fileName = "NewResearch", menuName = "Whiskeyville/ResearchData")]
public class ResearchData : ScriptableObject
{
    // ========================================================================
    // 🔑 IDENTITY
    // ========================================================================

    [Header("Identity")]
    [Tooltip("Unique string ID used by ResearchManager.HasResearch()")]
    public string nodeId = "research_node";

    public string nodeName = "Research Node";

    [TextArea(2, 4)]
    public string description = "Describes what this research unlocks.";

    // ========================================================================
    // 🌿 TREE PLACEMENT
    // ========================================================================

    [Header("Tree")]
    public ResearchCategory category = ResearchCategory.WhiskeyMastery;

    [Tooltip("nodeId of the required prerequisite, or leave empty for root nodes")]
    public string prerequisiteNodeId = "";

    // ========================================================================
    // 💎 COST
    // ========================================================================

    [Header("Cost")]
    [Min(1)]
    public int researchPointCost = 5;

    // ========================================================================
    // ⚡ EFFECT
    // ========================================================================

    [Header("Effect")]
    [Tooltip("Short key string used by other systems to query effects, e.g. 'flavor_bonus'")]
    public string effectKey = "";

    [Tooltip("Numeric multiplier or value for the effect (e.g. 1.5 for +50%)")]
    public float effectValue = 1.0f;

    // ========================================================================
    // 🎨 UI
    // ========================================================================

    [Header("UI")]
    public Sprite icon;
    public Color  categoryColor = Color.white;
}
