// ============================================================================
// TILEBEHAVIOR.CS
// ============================================================================
// PURPOSE:     Manages tile state (terrain, occupancy) and COLOR-BASED highlight
// ATTACHED TO: Every tile prefab (via GridManager at runtime)
// DEPENDENCIES: SpriteRenderer component on same GameObject
// ============================================================================
// HIGHLIGHT SYSTEM:
//   ✅ Uses SpriteRenderer. color tinting (simple, no child objects needed)
//   ✅ Stores original color → applies highlight → restores original
//   ✅ Green = valid placement, Red = invalid placement
// ============================================================================
// PROPERTIES:
//   📍 GridPosition   = (x,y) coordinates in grid
//   🌿 TerrainType    = Grass/Water/Dirt
//   🏠 IsOccupied     = true if building/obstacle on tile
// ============================================================================
// METHODS:
//   🎨 Highlight(color)    = Apply color tint
//   🎨 ResetHighlight()    = Restore original color
//   🏗️ CanPlaceBuilding()  = Returns true if Grass AND not occupied
//   🔒 SetOccupied(bool)   = Mark tile as occupied/free
// ============================================================================

using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    // ========================================================================
    // 📊 PUBLIC PROPERTIES (Read by external systems)
    // ========================================================================

    /// <summary>
    /// 🌿 Terrain type determines if buildings can be placed.
    /// Only TerrainType.Grass allows building placement in MVP.
    /// Set via Initialize() method.
    /// </summary>
    public TerrainType TerrainType { get; private set; } = TerrainType. Grass;

    /// <summary>
    /// 🏠 True if a building or obstacle occupies this tile.
    /// When true, CanPlaceBuilding() returns false.
    /// Set via SetOccupied() method.
    /// </summary>
    public bool IsOccupied { get; private set; } = false;

    /// <summary>
    /// 📍 Grid coordinates of this tile (x,y).
    /// Set via Initialize() method.
    /// Used for debugging and future pathfinding.
    /// </summary>
    public Vector2Int GridPosition { get; private set; }

    // ========================================================================
    // 🔒 PRIVATE FIELDS (Internal state)
    // ========================================================================

    /// <summary>
    /// 🎨 Cached reference to SpriteRenderer for color changes.
    /// Cached in Awake() to avoid GetComponent calls every frame.
    /// </summary>
    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// 🎨 Original sprite color before any highlighting.
    /// Stored so we can restore it when highlight ends.
    /// </summary>
    private Color _originalColor = Color.white;

    /// <summary>
    /// ✅ Flag to track if Initialize() was called.
    /// Prevents double-initialization bugs.
    /// </summary>
    private bool _isInitialized = false;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    /// <summary>
    /// Awake runs when script loads (before Start).
    /// We cache component references and ensure required components exist.
    /// </summary>
    private void Awake()
    {
        // ---- 🎨 CACHE SPRITE RENDERER ----
        // GetComponent<T>() returns null if component missing
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            // Store original color for highlight reset
            _originalColor = _spriteRenderer.color;
        }
        else
        {
            // ❌ CRITICAL: No SpriteRenderer = tile won't render or highlight
            Debug.LogError($"[TileBehavior] ❌ No SpriteRenderer on {gameObject.name}!  Add SpriteRenderer to prefab.");
        }

        // ---- 📦 ENSURE BOXCOLLIDER2D EXISTS ----
        // Required for Physics2D.Raycast to detect this tile
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            // Add collider at runtime if prefab doesn't have one
            col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            Debug.Log($"[TileBehavior] 📦 Added BoxCollider2D to {gameObject.name}");
        }

        // ---- 🏷️ SET TAG ----
        // Tag "Tile" is required for raycast filtering in TileHighlightController
        gameObject.tag = "Tile";
    }

    // ========================================================================
    // 🚀 INITIALIZATION (Called by GridManager)
    // ========================================================================

    /// <summary>
    /// Initializes tile with grid position and terrain type.
    /// Called by GridManager immediately after Instantiate().
    /// 
    /// WHAT THIS DOES:
    /// 1. Sets GridPosition (x,y) for reference
    /// 2. Sets TerrainType for placement validation
    /// 3. Sets "Tile" tag for raycast detection
    /// 4. Marks tile as initialized
    /// </summary>
    /// <param name="gridX">X coordinate in grid (0 to gridWidth-1)</param>
    /// <param name="gridY">Y coordinate in grid (0 to gridHeight-1)</param>
    /// <param name="terrain">Terrain type (Grass, Dirt, Water)</param>
    public void Initialize(int gridX, int gridY, TerrainType terrain)
    {
        // ---- ⚠️ PREVENT DOUBLE INIT ----
        if (_isInitialized)
        {
            Debug.LogWarning($"[TileBehavior] ⚠️ Tile ({gridX},{gridY}) already initialized.  Skipping.");
            return;
        }

        // ---- 📍 STORE GRID POSITION ----
        GridPosition = new Vector2Int(gridX, gridY);

        // ---- 🌿 STORE TERRAIN TYPE ----
        TerrainType = terrain;

        // ---- 🏷️ SET TAG ----
        gameObject.tag = "Tile";

        // ---- ✅ MARK INITIALIZED ----
        _isInitialized = true;

        // Debug log (comment out in production)
        // Debug.Log($"[TileBehavior] ✅ Initialized tile ({gridX},{gridY}) as {terrain}");
    }

    // ========================================================================
    // 🏠 OCCUPANCY MANAGEMENT
    // ========================================================================

    /// <summary>
    /// Marks tile as occupied or unoccupied.
    /// Called by BuildingPlacementManager after placing building.
    /// Called by GridManager when spawning obstacles (trees, rocks).
    /// </summary>
    /// <param name="occupied">True if something now occupies this tile</param>
    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
        Debug.Log($"[TileBehavior] 🏠 Tile ({GridPosition.x},{GridPosition.y}) occupied: {occupied}");
    }

    /// <summary>
    /// Returns true if a building can be placed on this tile. 
    /// Combines terrain check AND occupancy check.
    /// 
    /// PLACEMENT RULES:
    /// ✅ TerrainType must be Grass
    /// ✅ IsOccupied must be false
    /// </summary>
    /// <returns>True if placement allowed</returns>
    public bool CanPlaceBuilding()
    {
        // Both conditions must be true
        bool isGrass = (TerrainType == TerrainType.Grass);
        bool isFree = !IsOccupied;
        return isGrass && isFree;
    }

    // ========================================================================
    // 🎨 HIGHLIGHT SYSTEM (Color Tinting)
    // ========================================================================

    /// <summary>
    /// Applies highlight color to tile sprite.
    /// Called by TileHighlightController on mouse hover.
    /// </summary>
    /// <param name="color">Color to apply (green=valid, red=invalid)</param>
    public void Highlight(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Resets tile sprite to original color.
    /// Called by TileHighlightController when mouse exits tile.
    /// </summary>
    public void ResetHighlight()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    // ========================================================================
    // 🔄 LEGACY SUPPORT (Backwards compatibility)
    // ========================================================================

    /// <summary>
    /// Legacy method - calls Highlight() with validity-based color.
    /// Kept for backwards compatibility with old code.
    /// </summary>
    public void ShowHighlight()
    {
        Color color = CanPlaceBuilding() ? Color.green : Color. red;
        Highlight(color);
    }

    /// <summary>
    /// Legacy method - calls ResetHighlight().
    /// Kept for backwards compatibility with old code.
    /// </summary>
    public void HideHighlight()
    {
        ResetHighlight();
    }
}