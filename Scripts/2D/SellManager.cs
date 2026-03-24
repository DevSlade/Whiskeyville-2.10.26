// ============================================================================
// SELLMANAGER.CS
// ============================================================================
// PURPOSE:      Converts Aged Whiskey to Cash with visual feedback
// VERSION:      v2 — Added sell popup feedback
// UPDATED:      February 16, 2026
// DEPENDENCIES: InventoryManager, AudioManager, ProductionPopupPool
// ============================================================================

using UnityEngine;

public class SellManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static SellManager Instance { get; private set; }

    // ========================================================================
    // 💰 INSPECTOR SETTINGS
    // ========================================================================

    [Header("Sell Settings")]
    [Tooltip("Cash received per Aged Whiskey sold")]
    [SerializeField] private int _sellPrice = 50;

    [Header("Popup Settings")]
    [SerializeField] private Color _sellPopupColor = new Color(0.0f, 0.8f, 0.0f, 1f);

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log($"[SellManager] ✅ Initialized. Sell price: {_sellPrice} Cash per Aged Whiskey.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 💰 SELL METHODS
    // ========================================================================

    public bool SellOne()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[SellManager] ❌ InventoryManager not found!");
            return false;
        }

        if (!InventoryManager.Instance.HasResource(InventoryManager.RESOURCE_AGED_WHISKEY, 1))
        {
            Debug.Log("[SellManager] ❌ No Aged Whiskey to sell.");
            return false;
        }

        // Consume Aged Whiskey
        InventoryManager.Instance.AddResource(InventoryManager.RESOURCE_AGED_WHISKEY, -1);

        // Add Cash
        InventoryManager.Instance.AddResource(InventoryManager.RESOURCE_CASH, _sellPrice);

        Debug.Log($"[SellManager] 💰 Sold 1 Aged Whiskey for {_sellPrice} Cash.");

        // Popup
        ShowSellPopup(_sellPrice);

        // SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_COLLECT);
        }

        return true;
    }

    public int SellAll()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[SellManager] ❌ InventoryManager not found!");
            return 0;
        }

        int stock = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY);

        if (stock <= 0)
        {
            Debug.Log("[SellManager] ❌ No Aged Whiskey to sell.");
            return 0;
        }

        int totalCash = stock * _sellPrice;

        // Consume all Aged Whiskey
        InventoryManager.Instance.AddResource(InventoryManager.RESOURCE_AGED_WHISKEY, -stock);

        // Add Cash
        InventoryManager.Instance.AddResource(InventoryManager.RESOURCE_CASH, totalCash);

        Debug.Log($"[SellManager] 💰 Sold {stock} Aged Whiskey for {totalCash} Cash.");

        // Popup
        ShowSellPopup(totalCash);

        // SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_COLLECT);
        }

        return stock;
    }

    // ========================================================================
    // 🎉 SELL POPUP
    // ========================================================================

    private void ShowSellPopup(int cashAmount)
    {
        if (ProductionPopupPool.Instance == null) return;

        // Show popup at screen center (world position of camera)
        Vector3 popupPos = Camera.main != null
            ? Camera.main.transform.position + new Vector3(0f, 0f, 10f)
            : Vector3.zero;

        string text = $"+${cashAmount} Cash";
        ProductionPopupPool.Instance.ShowPopup(text, popupPos, _sellPopupColor);
    }

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public int SellPrice => _sellPrice;
}