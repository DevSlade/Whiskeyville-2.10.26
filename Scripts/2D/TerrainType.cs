// ============================================================================
// TERRAINTYPE.CS
// ============================================================================
// PURPOSE:      Defines terrain types for tile-based placement validation
// USED BY:     TileBehavior, BuildingPlacementManager
// MVP RULE:     Only Grass terrain allows building placement
// ============================================================================
// TERRAIN TYPES:
//   🌿 Grass  = Buildable (fields, buildings)
//   💧 Water  = Blocked (future: fishing, mills)
//   🟤 Dirt   = Blocked (visual variety)
// ============================================================================

/// <summary>
/// Enumeration of all terrain types in Whiskeyville.  
/// Each tile has exactly one TerrainType that determines what can be placed on it.
/// </summary>
public enum TerrainType
{
    /// <summary>
    /// 🌿 Standard buildable terrain.  Fields, barns, stills can be placed here.
    /// This is the default terrain type for the playable area.
    /// </summary>
    Grass,

    /// <summary>
    /// 💧 Water terrain. Completely blocks all placement.
    /// Future use: Fishing, water mills, irrigation. 
    /// </summary>
    Water,

    /// <summary>
    /// 🟤 Dirt/path terrain.  Blocks building placement. 
    /// Visual variety only in MVP.  Future: Walking paths, roads. 
    /// </summary>
    Dirt
}