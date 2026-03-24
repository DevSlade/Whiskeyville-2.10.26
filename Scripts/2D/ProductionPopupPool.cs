// ============================================================================
// PRODUCTIONPOPUPPOOL.CS
// ============================================================================
// PURPOSE:      Object pool for production popups (performance optimization)
// ARCHITECTURE: Singleton, reuses popup objects instead of Instantiate/Destroy
// ============================================================================

using UnityEngine;
using System.Collections.Generic;

public class ProductionPopupPool :  MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static ProductionPopupPool Instance { get; private set; }

    // ========================================================================
    // 🎨 INSPECTOR SETTINGS
    // ========================================================================

    [Header("🎨 Pool Settings")]
    [SerializeField] private int _initialPoolSize = 10;

    // ========================================================================
    // 🔒 PRIVATE DATA
    // ========================================================================

    private List<ProductionPopup> _pool = new List<ProductionPopup>();
    private Transform _poolContainer;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
            Debug.Log("[ProductionPopupPool] ✅ Singleton initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 INITIALIZATION
    // ========================================================================

    private void InitializePool()
    {
        _poolContainer = new GameObject("PopupPool").transform;
        _poolContainer.SetParent(transform);

        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreatePooledPopup();
        }

        Debug.Log($"[ProductionPopupPool] 📦 Created {_initialPoolSize} pooled popups.");
    }

    private ProductionPopup CreatePooledPopup()
    {
        GameObject go = new GameObject("Popup");
        go.transform.SetParent(_poolContainer);
        go.SetActive(false);

        ProductionPopup popup = go.AddComponent<ProductionPopup>();
        _pool.Add(popup);

        return popup;
    }

    // ========================================================================
    // 🚀 PUBLIC METHODS
    // ========================================================================

    public void ShowPopup(string text, Vector3 position, Color color)
    {
        ProductionPopup popup = GetAvailablePopup();
        popup.Show(text, position, color);
    }

    public void ShowPopup(string text, Vector3 position)
    {
        ShowPopup(text, position, Color.yellow);
    }

    // ========================================================================
    // 🔧 POOL MANAGEMENT
    // ========================================================================

    private ProductionPopup GetAvailablePopup()
    {
        foreach (ProductionPopup popup in _pool)
        {
            if (!popup.gameObject.activeInHierarchy)
            {
                return popup;
            }
        }

        Debug.Log("[ProductionPopupPool] 📦 Pool exhausted, creating new popup.");
        return CreatePooledPopup();
    }
}