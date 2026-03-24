// ============================================================================
// RESOURCEUI.CS
// ============================================================================
// PURPOSE:      Displays all resource counts in the HUD
// ATTACHED TO:  Canvas → ResourcePanel
// DEPENDENCIES: InventoryManager
// ============================================================================

using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    // ========================================================================
    // 🎮 INSPECTOR
    // ========================================================================

    [Header("Resource Text Elements")]
    [SerializeField] private TextMeshProUGUI _cashText;
    [SerializeField] private TextMeshProUGUI _cornText;
    [SerializeField] private TextMeshProUGUI _mashText;
    [SerializeField] private TextMeshProUGUI _whiskeyText;
    [SerializeField] private TextMeshProUGUI _agedWhiskeyText;
    [SerializeField] private TextMeshProUGUI _woodText;
    [SerializeField] private TextMeshProUGUI _barrelText;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        SubscribeToEvents();
        RefreshAllDisplays();
        Debug.Log("[ResourceUI] ✅ Initialized.");
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
    }

    private void UnsubscribeFromEvents()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnResourceChanged -= OnResourceChanged;
        }
    }

    private void OnResourceChanged(string resourceName, int newValue)
    {
        UpdateDisplay(resourceName, newValue);
    }

    // ========================================================================
    // 🔄 DISPLAY UPDATES
    // ========================================================================

    private void RefreshAllDisplays()
    {
        if (InventoryManager.Instance == null) return;

        UpdateDisplay(InventoryManager.RESOURCE_CASH, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CASH));
        UpdateDisplay(InventoryManager.RESOURCE_CORN, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CORN));
        UpdateDisplay(InventoryManager.RESOURCE_MASH, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_MASH));
        UpdateDisplay(InventoryManager.RESOURCE_WHISKEY, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WHISKEY));
        UpdateDisplay(InventoryManager.RESOURCE_AGED_WHISKEY, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY));
        UpdateDisplay(InventoryManager.RESOURCE_WOOD, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WOOD));
        UpdateDisplay(InventoryManager.RESOURCE_BARREL, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_BARREL));
    }

    private void UpdateDisplay(string resourceName, int value)
    {
        switch (resourceName)
        {
            case InventoryManager.RESOURCE_CASH:
                if (_cashText != null)
                    _cashText.text = "Cash: " + value;
                break;

            case InventoryManager.RESOURCE_CORN:
                if (_cornText != null)
                    _cornText.text = "Corn: " + value;
                break;

            case InventoryManager.RESOURCE_MASH:
                if (_mashText != null)
                    _mashText.text = "Mash: " + value;
                break;

            case InventoryManager.RESOURCE_WHISKEY:
                if (_whiskeyText != null)
                    _whiskeyText.text = "Whiskey: " + value;
                break;

            case InventoryManager.RESOURCE_AGED_WHISKEY:
                if (_agedWhiskeyText != null)
                    _agedWhiskeyText.text = "Aged: " + value;
                break;

            case InventoryManager.RESOURCE_WOOD:
                if (_woodText != null)
                    _woodText.text = "Wood: " + value;
                break;

            case InventoryManager.RESOURCE_BARREL:
                if (_barrelText != null)
                    _barrelText.text = "Barrel: " + value;
                break;
        }
    }
}