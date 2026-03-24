// ============================================================================
// TILEHIGHLIGHTCONTROLLER.CS
// ============================================================================
// PURPOSE:     Highlights tile under mouse cursor using COLOR TINTING
// ATTACHED TO:  GameManager or any always-active GameObject
// DEPENDENCIES: TileBehavior on tiles, tiles must have "Tile" tag + BoxCollider2D
// ============================================================================
// HOW IT WORKS:
//   1️⃣ Every frame, raycast from mouse position
//   2️⃣ If hit tile:  apply highlight color (green=valid, red=invalid)
//   3️⃣ If hit different tile: reset old tile, highlight new tile
//   4️⃣ If hit nothing: reset current tile
// ============================================================================
// HIGHLIGHT COLORS:
//   🟢 validColor   = Can place building (Grass + not occupied)
//   🔴 invalidColor = Cannot place building (wrong terrain or occupied)
// ============================================================================

using UnityEngine;

public class TileHighlightController : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR SETTINGS
    // ========================================================================

    [Header("🎨 Highlight Colors")]
    [Tooltip("Color for tiles where buildings CAN be placed (green)")]
    public Color validColor = new Color(0.5f, 1f, 0.5f, 1f);    // Light green

    [Tooltip("Color for tiles where buildings CANNOT be placed (red)")]
    public Color invalidColor = new Color(1f, 0.5f, 0.5f, 1f);  // Light red

    [Header("🔧 Debug Settings")]
    [Tooltip("Enable debug logs for troubleshooting")]
    public bool enableDebugLogs = false;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    /// <summary>
    /// 📍 Currently highlighted tile.  Null if not hovering any tile.
    /// Used to reset previous tile when moving to new tile.
    /// </summary>
    private TileBehavior _currentTile;

    /// <summary>
    /// 📷 Cached camera reference for performance.
    /// Avoids Camera.main lookup every frame.
    /// </summary>
    private Camera _mainCamera;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    /// <summary>
    /// Start caches camera reference.
    /// </summary>
    private void Start()
    {
        // ---- 📷 CACHE MAIN CAMERA ----
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError("[TileHighlightController] ❌ No main camera found!  Ensure camera is tagged 'MainCamera'.");
        }
        else
        {
            Debug. Log("[TileHighlightController] ✅ Initialized.  Camera found.");
        }
    }

    /// <summary>
    /// Update runs every frame.  We check mouse position and update highlight.
    /// </summary>
    private void Update()
    {
        // ---- ⚠️ SAFETY CHECK ----
        if (_mainCamera == null) return;

        // ---- 🎯 UPDATE HIGHLIGHT ----
        UpdateHighlight();
    }

    // ========================================================================
    // 🎯 HIGHLIGHT LOGIC
    // ========================================================================

    /// <summary>
    /// Main highlight update loop.
    /// Raycasts from mouse, highlights tile if found, clears if not.
    /// </summary>
    private void UpdateHighlight()
    {
        // ---- 🖱️ GET MOUSE WORLD POSITION ----
        // Convert screen coordinates to world coordinates
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // ---- 🎯 RAYCAST TO FIND TILE ----
        // Vector2.zero = point cast (not directional)
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // ---- 🔍 CHECK RAYCAST RESULT ----
        if (hit.collider != null)
        {
            // Debug:  Log what we hit
            if (enableDebugLogs)
            {
                Debug.Log($"[TileHighlightController] 🎯 Hit:  {hit.collider.gameObject.name}, Tag: {hit.collider.tag}");
            }

            // ---- 🏷️ CHECK IF HIT A TILE ----
            if (hit.collider.CompareTag("Tile"))
            {
                // Get TileBehavior component
                TileBehavior tile = hit.collider.GetComponent<TileBehavior>();

                if (tile != null)
                {
                    // ---- 🔄 CHECK IF NEW TILE ----
                    if (tile != _currentTile)
                    {
                        // Reset previous tile
                        if (_currentTile != null)
                        {
                            _currentTile.ResetHighlight();
                        }

                        // Determine highlight color based on placement validity
                        Color highlightColor = tile.CanPlaceBuilding() ? validColor : invalidColor;

                        // Apply highlight to new tile
                        tile.Highlight(highlightColor);

                        // Store reference
                        _currentTile = tile;

                        if (enableDebugLogs)
                        {
                            Debug. Log($"[TileHighlightController] 🎨 Highlighting:  {tile.gameObject.name}, Valid: {tile. CanPlaceBuilding()}");
                        }
                    }
                }
                else
                {
                    // ❌ Tagged as Tile but missing TileBehavior
                    Debug.LogWarning($"[TileHighlightController] ⚠️ Object tagged 'Tile' missing TileBehavior:  {hit.collider.gameObject.name}");
                    ClearHighlight();
                }
            }
            else
            {
                // Hit something but not a tile
                ClearHighlight();
            }
        }
        else
        {
            // Hit nothing
            ClearHighlight();
        }
    }

    /// <summary>
    /// Clears current highlight if any.
    /// </summary>
    private void ClearHighlight()
    {
        if (_currentTile != null)
        {
            _currentTile. ResetHighlight();
            _currentTile = null;
        }
    }
}