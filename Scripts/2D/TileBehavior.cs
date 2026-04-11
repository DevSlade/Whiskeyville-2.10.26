// ============================================================================
// TILEBEHAVIOR.CS
// ============================================================================
// PURPOSE:     Manages tile state (terrain, occupancy) and COLOR-BASED highlight
// VERSION:     v2 — Added SetTerrainType for Hoe, building reference for Demolish
// ATTACHED TO: Every tile prefab (via GridManager at runtime)
// DEPENDENCIES: SpriteRenderer component on same GameObject
// ============================================================================

using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    // ========================================================================
    // 📊 PUBLIC PROPERTIES (Read by external systems)
    // ========================================================================

    public TerrainType TerrainType { get; private set; } = TerrainType.Grass;
    public bool IsOccupied { get; private set; } = false;
    public Vector2Int GridPosition { get; private set; }

    /// <summary>
    /// Reference to the building GameObject on this tile. Null if empty.
    /// Set by BuildingPlacementManager on place, cleared on demolish.
    /// </summary>
    public GameObject OccupyingBuilding { get; private set; }

    // ========================================================================
    // 🔒 PRIVATE FIELDS
    // ========================================================================

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor = Color.white;
    private bool _isInitialized = false;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
        else
        {
            Debug.LogError($"[TileBehavior] ❌ No SpriteRenderer on {gameObject.name}!");
        }

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
        }

        gameObject.tag = "Tile";
    }

    // ========================================================================
    // 🚀 INITIALIZATION (Called by GridManager)
    // ========================================================================

    public void Initialize(int gridX, int gridY, TerrainType terrain)
    {
        if (_isInitialized)
        {
            Debug.LogWarning($"[TileBehavior] ⚠️ Tile ({gridX},{gridY}) already initialized.");
            return;
        }

        GridPosition = new Vector2Int(gridX, gridY);
        TerrainType = terrain;
        gameObject.tag = "Tile";
        _isInitialized = true;
    }

    // ========================================================================
    // 🏠 OCCUPANCY MANAGEMENT
    // ========================================================================

    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;

        if (!occupied)
        {
            OccupyingBuilding = null;
        }

        Debug.Log($"[TileBehavior] 🏠 Tile ({GridPosition.x},{GridPosition.y}) occupied: {occupied}");
    }

    /// <summary>
    /// Sets occupied AND stores reference to the building GameObject.
    /// Called by BuildingPlacementManager after placing.
    /// </summary>
    public void SetOccupied(bool occupied, GameObject building)
    {
        IsOccupied = occupied;
        OccupyingBuilding = occupied ? building : null;
        Debug.Log($"[TileBehavior] 🏠 Tile ({GridPosition.x},{GridPosition.y}) occupied: {occupied}, Building: {(building != null ? building.name : "null")}");
    }

    public bool CanPlaceBuilding()
    {
        bool isBuildable = (TerrainType == TerrainType.Grass || TerrainType == TerrainType.Farm);
        bool isFree = !IsOccupied;
        return isBuildable && isFree;
    }

    // ========================================================================
    // 🌾 TERRAIN MODIFICATION (Hoe Tool)
    // ========================================================================

    /// <summary>
    /// Changes terrain type at runtime. Used by Hoe tool (Grass → Farm).
    /// Updates the original color so highlight resets work correctly.
    /// </summary>
    public void SetTerrainType(TerrainType newTerrain, Sprite newSprite = null)
    {
        TerrainType oldTerrain = TerrainType;
        TerrainType = newTerrain;

        if (newSprite != null && _spriteRenderer != null)
        {
            _spriteRenderer.sprite = newSprite;
        }

        // Update stored original color from current sprite
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }

        Debug.Log($"[TileBehavior] 🌾 Tile ({GridPosition.x},{GridPosition.y}) terrain changed: {oldTerrain} → {newTerrain}");
    }

    // ========================================================================
    // 🎨 HIGHLIGHT SYSTEM
    // ========================================================================

    public void Highlight(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
    }

    public void ResetHighlight()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    public void ShowHighlight()
    {
        Color color = CanPlaceBuilding() ? Color.green : Color.red;
        Highlight(color);
    }

    public void HideHighlight()
    {
        ResetHighlight();
    }
}