// ============================================================================
// TILEHIGHLIGHTCONTROLLER.CS
// ============================================================================
// PURPOSE:     Highlights tile under mouse cursor — TOOL AWARE
// VERSION:     v2 — Colors change based on active tool
// ATTACHED TO: GameManager or any always-active GameObject
// DEPENDENCIES: TileBehavior, ToolManager
// ============================================================================

using UnityEngine;

public class TileHighlightController : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR SETTINGS
    // ========================================================================

    [Header("Build Tool Colors")]
    public Color validColor = new Color(0.5f, 1f, 0.5f, 1f);
    public Color invalidColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("Hoe Tool Colors")]
    public Color hoeValidColor = new Color(0.72f, 0.53f, 0.26f, 1f);    // Brown
    public Color hoeInvalidColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("Axe Tool Colors")]
    public Color axeColor = new Color(1f, 0.65f, 0f, 1f);               // Orange

    [Header("Demolish Tool Colors")]
    public Color demolishValidColor = new Color(1f, 0.3f, 0.3f, 1f);    // Bright red
    public Color demolishInvalidColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Grey

    [Header("Debug Settings")]
    public bool enableDebugLogs = false;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private TileBehavior _currentTile;
    private Camera _mainCamera;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError("[TileHighlightController] ❌ No main camera found!");
        }
    }

    private void Update()
    {
        if (_mainCamera == null) return;
        UpdateHighlight();
    }

    // ========================================================================
    // 🎯 HIGHLIGHT LOGIC — TOOL AWARE
    // ========================================================================

    private void UpdateHighlight()
    {
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Tile"))
            {
                TileBehavior tile = hit.collider.GetComponent<TileBehavior>();

                if (tile != null)
                {
                    if (tile != _currentTile)
                    {
                        if (_currentTile != null)
                        {
                            _currentTile.ResetHighlight();
                        }

                        Color highlightColor = GetHighlightColor(tile);
                        tile.Highlight(highlightColor);
                        _currentTile = tile;
                    }
                }
                else
                {
                    ClearHighlight();
                }
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    /// <summary>
    /// Returns the correct highlight color based on active tool and tile state.
    /// </summary>
    private Color GetHighlightColor(TileBehavior tile)
    {
        ToolType activeTool = ToolManager.Instance != null
            ? ToolManager.Instance.ActiveTool
            : ToolType.None;

        switch (activeTool)
        {
            case ToolType.Build:
                return tile.CanPlaceBuilding() ? validColor : invalidColor;

            case ToolType.Hoe:
                bool canHoe = (tile.TerrainType == TerrainType.Grass && !tile.IsOccupied);
                return canHoe ? hoeValidColor : hoeInvalidColor;

            case ToolType.Axe:
                return axeColor;

            case ToolType.Demolish:
                bool canDemolish = (tile.IsOccupied && tile.OccupyingBuilding != null);
                return canDemolish ? demolishValidColor : demolishInvalidColor;

            case ToolType.None:
            default:
                // No tool — neutral highlight (placement colors as fallback)
                return tile.CanPlaceBuilding() ? validColor : invalidColor;
        }
    }

    private void ClearHighlight()
    {
        if (_currentTile != null)
        {
            _currentTile.ResetHighlight();
            _currentTile = null;
        }
    }
}