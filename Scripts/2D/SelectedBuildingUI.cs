// ============================================================================
// SELECTEDBUILDINGUI.CS
// ============================================================================
// PURPOSE:       Displays currently selected building and cost
// ATTACHED TO:  Canvas → SelectedBuildingText (or any TMP text)
// DEPENDENCIES: BuildingSelector, BuildingDatabase, InventoryManager
// ============================================================================

using UnityEngine;
using TMPro;

public class SelectedBuildingUI : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR
    // ========================================================================

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _selectedText;

    [Header("Colors")]
    [SerializeField] private Color _canAffordColor = Color.white;
    [SerializeField] private Color _cantAffordColor = new Color(1f, 0.3f, 0.3f);

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        UpdateDisplay();
        SubscribeToEvents();

        Debug.Log("[SelectedBuildingUI] ✅ Initialized.");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    // ========================================================================
    // 🔧 EVENTS
    // ========================================================================

    private void SubscribeToEvents()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnResourceChanged += OnResourceChanged;
        }

        if (BuildingSelector.Instance != null)
        {
            BuildingSelector.Instance.OnSelectionChanged += OnSelectionChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnResourceChanged -= OnResourceChanged;
        }

        if (BuildingSelector. Instance != null)
        {
            BuildingSelector.Instance.OnSelectionChanged -= OnSelectionChanged;
        }
    }

    private void OnResourceChanged(string resource, int amount)
    {
        if (resource == InventoryManager.RESOURCE_CASH)
        {
            UpdateDisplay();
        }
    }

    private void OnSelectionChanged(int newIndex)
    {
        UpdateDisplay();
    }

    // ========================================================================
    // 🔄 DISPLAY UPDATE
    // ========================================================================

    private void UpdateDisplay()
    {
        if (_selectedText == null) return;

        if (BuildingSelector.Instance == null || BuildingSelector.Instance.SelectedBuilding == null)
        {
            _selectedText.text = "No Building Selected";
            _selectedText.color = _cantAffordColor;
            return;
        }

        BuildingData data = BuildingSelector.Instance.SelectedBuilding;
        int currentCash = InventoryManager. Instance != null 
            ? InventoryManager. Instance.GetResource(InventoryManager.RESOURCE_CASH) 
            : 0;

        bool canAfford = currentCash >= data.cost;

        if (canAfford)
        {
            _selectedText. text = $"Selected: {data.buildingName} (${data.cost})";
            _selectedText.color = _canAffordColor;
        }
        else
        {
            _selectedText.text = $"Selected: {data.buildingName} (${data.cost}) - CAN'T AFFORD! ";
            _selectedText.color = _cantAffordColor;
        }
    }
}