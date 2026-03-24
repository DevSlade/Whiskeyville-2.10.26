// ============================================================================
// BUILDINGPLACEMENTMANAGER.CS
// ============================================================================
// PURPOSE:      Places selected building, tracks position for save/load
// VERSION:      v6 — Crop base sprite stays on Ground layer
// UPDATED:      February 19, 2026
// DEPENDENCIES: BuildingSelector, BuildingDatabase, InventoryManager, GridManager
// ============================================================================

using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    private Camera _mainCamera;

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

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            TryPlaceBuilding();
        }
    }

    private void TryPlaceBuilding()
    {
        if (_mainCamera == null) return;

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

        if (!InventoryManager.Instance.HasResource(InventoryManager.RESOURCE_CASH, buildingData.cost))
        {
            Debug.Log($"[BuildingPlacementManager] ❌ Cannot afford {buildingData.buildingName}. Need {buildingData.cost} Cash.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
            }
            return;
        }

        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) return;
        if (!hit.collider.CompareTag("Tile")) return;

        TileBehavior tile = hit.collider.GetComponent<TileBehavior>();
        if (tile == null) return;

        if (!tile.CanPlaceBuilding())
        {
            Debug.Log($"[BuildingPlacementManager] ❌ Tile occupied or invalid terrain.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
            }
            return;
        }

        PlaceBuilding(tile, buildingData);
    }

    private void PlaceBuilding(TileBehavior tile, BuildingData buildingData)
    {
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

            // Crops: base soil on Ground, growth stages on Buildings
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

            // Buildings: everything on Buildings layer
            ApplySorting(building, gridPos.y);
        }

        tile.SetOccupied(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_PLACE);
        }

        Debug.Log($"[BuildingPlacementManager] ✅ Placed {buildingData.buildingName} at ({gridPos.x}, {gridPos.y})");
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

        // Root sprite (plowed soil) stays on Ground — renders flat with terrain
        SpriteRenderer rootSR = cropObj.GetComponent<SpriteRenderer>();
        if (rootSR != null)
        {
            rootSR.sortingLayerName = "Ground";
            rootSR.sortingOrder = 1;
        }

        // Child sprites (growth stages) go on Buildings — Y-sorted with trees
        SpriteRenderer[] childRenderers = cropObj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < childRenderers.Length; i++)
        {
            // Skip root — already handled
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