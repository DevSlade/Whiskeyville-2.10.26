// ============================================================================
// BUILDINGPLACEMENTMANAGER.CS
// ============================================================================
// PURPOSE:      Places buildings, handles Hoe/Demolish tool actions on tiles
// VERSION:      v10 — Tilling costs money, tag constants, farm not in build menu
// UPDATED:      April 5, 2026
// DEPENDENCIES: BuildingSelector, BuildingDatabase, InventoryManager,
//               GridManager, ToolManager, UIManager, AudioManager,
//               ParticleManager, InputProvider
// ============================================================================

using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR — HOE TOOL
    // ========================================================================

    [Header("Hoe Tool")]
    [Tooltip("The farm/field prefab to spawn when tilling. Assign the same prefab used in the build menu.")]
    [SerializeField] private GameObject _farmPrefab;

    [Tooltip("LEGACY: Sprite fallback if no farm prefab assigned")]
    [SerializeField] private Sprite _farmTileSprite;

    [Tooltip("Cost in cash to till one tile (0 = free)")]
    [SerializeField] private int _tillCost = 10;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Camera _mainCamera;

    /// <summary>
    /// Tracks the building pending demolish confirmation.
    /// Null when no demolish is pending.
    /// </summary>
    private TileBehavior _pendingDemolishTile;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError("[BuildingPlacementManager] ❌ No main camera found!");
        }
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused) return;

        // Use InputProvider if available, otherwise fallback to legacy
        bool tapped = false;

        if (InputProvider.Instance != null)
        {
            // Only process clean taps (not drags/pans)
            tapped = InputProvider.TapDown && !InputProvider.IsPanning;
        }
        else
        {
            tapped = Input.GetMouseButtonDown(0);
        }

        if (tapped)
        {
            if (IsPointerOverUI()) return;
            HandleClick();
        }
    }

    // ========================================================================
    // 🖱️ CLICK ROUTER — Dispatches based on active tool
    // ========================================================================

    private void HandleClick()
    {
        if (_mainCamera == null) return;

        ToolType activeTool = ToolManager.Instance != null
            ? ToolManager.Instance.ActiveTool
            : ToolType.None;

        switch (activeTool)
        {
            case ToolType.Build:
                TryPlaceBuilding();
                break;

            case ToolType.Hoe:
                TryHoeTile();
                break;

            case ToolType.Demolish:
                TryDemolish();
                break;

            case ToolType.Axe:
                // Axe is handled by TreeBehavior.OnMouseDown()
                break;

            case ToolType.Sickle:
                // Sickle harvest is handled by CropBehavior.OnMouseDown()
                break;

            case ToolType.None:
            default:
                // No tool — no tile action
                break;
        }
    }

    // ========================================================================
    // 🏗️ BUILD TOOL — Place selected building
    // ========================================================================

    private void TryPlaceBuilding()
    {
        BuildingData buildingData = BuildingSelector.Instance?.SelectedBuilding;

        if (buildingData == null)
        {
            Debug.LogWarning("[BuildingPlacementManager] ⚠️ No building selected.");
            return;
        }

        if (buildingData.prefab == null)
        {
            Debug.LogError($"[BuildingPlacementManager] ❌ {buildingData.buildingName} has no prefab!");
            return;
        }

        // Skip cost check in creative mode
        bool isCreative = DevConsole.Instance != null && DevConsole.Instance.IsCreativeMode;

        if (!isCreative && !InventoryManager.Instance.HasResource(InventoryManager.RESOURCE_CASH, buildingData.cost))
        {
            Debug.Log($"[BuildingPlacementManager] ❌ Cannot afford {buildingData.buildingName}. Need {buildingData.cost} Cash.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
            }
            return;
        }

        // Use InputProvider world position if available
        Vector2 worldPos;
        if (InputProvider.Instance != null)
            worldPos = InputProvider.WorldTapPosition;
        else
            worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) return;
        if (!hit.collider.CompareTag(GameConstants.Tags.TILE)) return;

        TileBehavior tile = hit.collider.GetComponent<TileBehavior>();
        if (tile == null) return;

        if (!tile.CanPlaceBuilding())
        {
            Debug.Log("[BuildingPlacementManager] Tile occupied or invalid terrain.");
            AudioManager.Instance?.PlaySFX(AudioManager.SFX_ERROR);
            return;
        }

        PlaceBuilding(tile, buildingData);
    }

    private void PlaceBuilding(TileBehavior tile, BuildingData buildingData)
    {
        // Deduct cost (skip in creative mode)
        bool isCreative = DevConsole.Instance != null && DevConsole.Instance.IsCreativeMode;
        if (!isCreative)
            InventoryManager.Instance.AddResource(InventoryManager.RESOURCE_CASH, -buildingData.cost);

        GameObject building = Instantiate(
            buildingData.prefab,
            tile.transform.position,
            Quaternion.identity
        );

        int buildingIndex = BuildingSelector.Instance.SelectedIndex;
        Vector2Int gridPos = tile.GridPosition;

        if (buildingData.isCrop)
        {
            CropBehavior crop = building.GetComponent<CropBehavior>();
            if (crop != null)
            {
                crop.SetBuildingIndex(buildingIndex);
                crop.SetGridPosition(gridPos);
            }

            ApplyCropSorting(building, gridPos.y);
        }
        else
        {
            BuildingBehavior behavior = building.GetComponent<BuildingBehavior>();
            if (behavior != null)
            {
                behavior.Initialize(buildingData);
                behavior.SetBuildingIndex(buildingIndex);
                behavior.SetGridPosition(gridPos);
            }

            ApplySorting(building, gridPos.y);
        }

        // Store building reference on tile for demolish
        tile.SetOccupied(true, building);

        // ---- 🔊 BUILD SFX ----
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_BUILD);
        }

        // ---- ✨ BUILD PARTICLE ----
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayParticle(ParticleManager.PARTICLE_BUILD, tile.transform.position);
        }

        Debug.Log($"[BuildingPlacementManager] ✅ Placed {buildingData.buildingName} at ({gridPos.x}, {gridPos.y})");
    }

    // ========================================================================
    // 🌾 HOE TOOL — Convert Grass → Farm
    // ========================================================================

    private void TryHoeTile()
    {
        Vector2 worldPos = InputProvider.Instance != null
            ? InputProvider.WorldTapPosition
            : (Vector2)_mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) return;
        if (!hit.collider.CompareTag(GameConstants.Tags.TILE)) return;

        TileBehavior tile = hit.collider.GetComponent<TileBehavior>();
        if (tile == null) return;

        // Only Grass can be hoed
        if (tile.TerrainType != TerrainType.Grass)
        {
            Debug.Log("[BuildingPlacementManager] ⚠️ Can only hoe Grass tiles.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
            }
            return;
        }

        // Can't hoe occupied tile (tree, building on it)
        if (tile.IsOccupied)
        {
            Debug.Log("[BuildingPlacementManager] ⚠️ Tile is occupied. Clear it first.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
            }
            return;
        }

        // ---- 💰 CHECK TILLING COST (skip in creative mode) ----
        bool isCreativeTill = DevConsole.Instance != null && DevConsole.Instance.IsCreativeMode;

        if (!isCreativeTill && _tillCost > 0 && InventoryManager.Instance != null)
        {
            if (!InventoryManager.Instance.HasResource(InventoryManager.RESOURCE_CASH, _tillCost))
            {
                Debug.Log($"[BuildingPlacementManager] ❌ Cannot afford tilling. Need ${_tillCost} Cash.");
                AudioManager.Instance?.PlaySFX(AudioManager.SFX_ERROR);

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowError($"Need ${_tillCost} to till soil!");

                return;
            }

            // Deduct cost
            InventoryManager.Instance.AddResource(InventoryManager.RESOURCE_CASH, -_tillCost);
        }

        // ---- 🌾 SPAWN FARM PREFAB OR FALLBACK TO SPRITE SWAP ----
        if (_farmPrefab != null)
        {
            // Spawn the actual farm prefab (matches build menu placement)
            GameObject farmObj = Instantiate(_farmPrefab, tile.transform.position, Quaternion.identity);

            // Look up the farm's database index so save/load restores the correct building
            int farmIndex = BuildingDatabase.Instance != null
                ? BuildingDatabase.Instance.FindIndexByPrefab(_farmPrefab)
                : -1;

            if (farmIndex < 0)
                Debug.LogWarning("[BuildingPlacementManager] ⚠️ Farm prefab not found in BuildingDatabase — save/load may fail.");

            // Initialize as crop if it has CropBehavior
            CropBehavior crop = farmObj.GetComponent<CropBehavior>();
            if (crop != null)
            {
                crop.SetBuildingIndex(farmIndex);
                crop.SetGridPosition(tile.GridPosition);
            }

            // Initialize as building if it has BuildingBehavior
            BuildingBehavior beh = farmObj.GetComponent<BuildingBehavior>();
            if (beh != null)
            {
                beh.SetBuildingIndex(farmIndex);
                beh.SetGridPosition(tile.GridPosition);
            }

            // Apply sorting
            int gridY = tile.GridPosition.y;
            ApplyCropSorting(farmObj, gridY);

            // Mark tile as occupied with the farm prefab
            tile.SetOccupied(true, farmObj);
            tile.SetTerrainType(TerrainType.Farm);
        }
        else
        {
            // Fallback: just swap the sprite (legacy behavior)
            tile.SetTerrainType(TerrainType.Farm, _farmTileSprite);
        }

        // ---- 🔊 HOE SFX ----
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_HOE);
        }

        // ---- ✨ BUILD PARTICLE ----
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayParticle(ParticleManager.PARTICLE_BUILD, tile.transform.position);
        }

        if (ProductionPopupPool.Instance != null)
        {
            string costText = _tillCost > 0 && !isCreativeTill ? $"Tilled! -${_tillCost}" : "Tilled!";
            ProductionPopupPool.Instance.ShowPopup(costText, tile.transform.position, new Color(0.55f, 0.35f, 0.15f));
        }

        Debug.Log($"[BuildingPlacementManager] 🌾 Hoed tile at ({tile.GridPosition.x}, {tile.GridPosition.y}) → Farm. Cost: ${_tillCost}");
    }

    // ========================================================================
    // 🔨 DEMOLISH TOOL — Remove buildings with confirm
    // ========================================================================

    private void TryDemolish()
    {
        Vector2 worldPos = InputProvider.Instance != null
            ? InputProvider.WorldTapPosition
            : (Vector2)_mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) return;
        if (!hit.collider.CompareTag(GameConstants.Tags.TILE)) return;

        TileBehavior tile = hit.collider.GetComponent<TileBehavior>();
        if (tile == null) return;

        if (!tile.IsOccupied || tile.OccupyingBuilding == null)
        {
            Debug.Log("[BuildingPlacementManager] ⚠️ Nothing to demolish here.");
            return;
        }

        // Store pending and show confirm
        _pendingDemolishTile = tile;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDemolishConfirm();
        }

        Debug.Log($"[BuildingPlacementManager] 🔨 Demolish pending at ({tile.GridPosition.x}, {tile.GridPosition.y}).");
    }

    /// <summary>
    /// Called by UIManager confirm button. Actually destroys the building.
    /// </summary>
    public void ConfirmDemolish()
    {
        if (_pendingDemolishTile == null)
        {
            Debug.LogWarning("[BuildingPlacementManager] ⚠️ No pending demolish.");
            return;
        }

        GameObject building = _pendingDemolishTile.OccupyingBuilding;

        // Free tile and close UI immediately — visual destruction is handled below
        _pendingDemolishTile.SetOccupied(false);
        _pendingDemolishTile = null;

        AudioManager.Instance?.PlaySFX(AudioManager.SFX_DEMOLISH);
        UIManager.Instance?.HideDemolishConfirm();

        if (building == null) return;

        // Capture world position before any async animation in case the ref goes stale
        Vector3 buildingPos = building.transform.position;

        // If BuildingAnimator is present, play shrink-out then destroy.
        // Otherwise destroy immediately (original behavior).
        BuildingAnimator animator = building.GetComponent<BuildingAnimator>();
        if (animator != null)
        {
            animator.PlayDemolish(() =>
            {
                // Particle fires at captured position after the building has shrunk away
                ParticleManager.Instance?.PlayParticle(ParticleManager.PARTICLE_DEMOLISH, buildingPos);
                if (building != null) Destroy(building);
            });
        }
        else
        {
            ParticleManager.Instance?.PlayParticle(ParticleManager.PARTICLE_DEMOLISH, buildingPos);
            Destroy(building);
        }

        Debug.Log($"[BuildingPlacementManager] 🔨 Demolished building at {buildingPos}.");
    }

    /// <summary>
    /// Called by UIManager cancel button. Cancels pending demolish.
    /// </summary>
    public void CancelDemolish()
    {
        _pendingDemolishTile = null;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideDemolishConfirm();
        }

        Debug.Log("[BuildingPlacementManager] 🔨 Demolish cancelled.");
    }

    // ========================================================================
    // 🔧 SORTING — Buildings (all sprites on Buildings layer)
    // ========================================================================

    private void ApplySorting(GameObject obj, int gridY)
    {
        int baseSortOrder = GridManager.Instance != null
            ? GridManager.GetSortOrder(gridY, GridManager.Instance.GridHeight)
            : 0;

        SpriteRenderer[] allRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].sortingLayerName = GridManager.SORT_LAYER_OBJECTS;
            allRenderers[i].sortingOrder = baseSortOrder + allRenderers[i].sortingOrder;
        }
    }

    // ========================================================================
    // 🌾 SORTING — Crops (base soil on Ground, growth stages on Buildings)
    // ========================================================================

    private void ApplyCropSorting(GameObject cropObj, int gridY)
    {
        int baseSortOrder = GridManager.Instance != null
            ? GridManager.GetSortOrder(gridY, GridManager.Instance.GridHeight)
            : 0;

        SpriteRenderer rootSR = cropObj.GetComponent<SpriteRenderer>();
        if (rootSR != null)
        {
            rootSR.sortingLayerName = "Ground";
            rootSR.sortingOrder = 1;
        }

        SpriteRenderer[] childRenderers = cropObj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] == rootSR) continue;

            childRenderers[i].sortingLayerName = GridManager.SORT_LAYER_OBJECTS;
            childRenderers[i].sortingOrder = baseSortOrder + childRenderers[i].sortingOrder;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}