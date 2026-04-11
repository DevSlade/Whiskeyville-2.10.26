// ============================================================================
// TERRAINTYPE.CS
// ============================================================================
// PURPOSE:      Defines terrain types for tile-based placement validation
// USED BY:      TileBehavior, BuildingPlacementManager, ToolManager
// MVP RULE:     Only Grass AND Farm terrain allows building placement
// ============================================================================

public enum TerrainType
{
    /// 🌿 Standard buildable terrain.
    Grass,

    /// 💧 Water terrain. Completely blocks all placement.
    Water,

    /// 🟤 Dirt/path terrain. Blocks building placement.
    Dirt,

    /// 🌾 Farm terrain. Created by hoeing Grass. Allows crop placement.
    Farm
}