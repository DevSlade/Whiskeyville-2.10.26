// ============================================================================
// TOOLTYPE.CS
// ============================================================================
// PURPOSE:      Defines all player tools for click-intent disambiguation
// USED BY:      ToolManager, BuildingPlacementManager, TileHighlightController
// ============================================================================

public enum ToolType
{
    /// No tool selected — default state, clicks do nothing on tiles
    None,

    /// Build mode — clicks place the selected building
    Build,

    /// Hoe — clicks convert Grass tiles into Farm tiles
    Hoe,

    /// Axe — clicks chop trees (with cooldown)
    Axe,

    /// Demolish — clicks remove buildings (with confirm dialog)
    Demolish,

    /// Sickle — clicks harvest fully-grown crops
    Sickle
}