// ============================================================================
// SELLPANELUI.CS
// ============================================================================
// PURPOSE:      Player-facing sell button and stock display
// ATTACHED TO:   Canvas → SellPanel
// DEPENDENCIES: SellManager, InventoryManager
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellPanelUI :  MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR
    // ========================================================================

    [Header("UI Elements")]
    [SerializeField] private Button _sellButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _stockText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private TextMeshProUGUI _buttonText;

    [Header("Visual Feedback")]
    [SerializeField] private Color _canSellColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _cantSellColor = new Color(0.5f, 0.5f, 0.5f);

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Image _buttonImage;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        if (_sellButton != null)
        {
            _buttonImage = _sellButton.GetComponent<Image>();
            _sellButton.onClick.AddListener(OnSellClicked);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        UpdateDisplay();
        SubscribeToEvents();

        Debug.Log("[SellPanelUI] ✅ Initialized.");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        if (_sellButton != null)
        {
            _sellButton.onClick.RemoveListener(OnSellClicked);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }

    private void OnEnable()
    {
        UpdateDisplay();
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

    private void OnResourceChanged(string resource, int amount)
    {
        if (resource == InventoryManager.RESOURCE_AGED_WHISKEY ||
            resource == InventoryManager.RESOURCE_CASH)
        {
            UpdateDisplay();
        }
    }

    // ========================================================================
    // 🎮 BUTTON ACTIONS
    // ========================================================================

    private void OnSellClicked()
    {
        if (SellManager.Instance == null)
        {
            Debug.LogError("[SellPanelUI] ❌ SellManager not found!");
            return;
        }

        int stock = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY);

        if (stock <= 0)
        {
            Debug.Log("[SellPanelUI] ❌ No Aged Whiskey to sell.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
            }
            return;
        }

        int sold = SellManager.Instance.SellAll();

        if (sold > 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance. PlaySFX(AudioManager. SFX_COLLECT);
        }

        UpdateDisplay();
    }

    private void OnCloseClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseSellPanel();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
        }
    }

    // ========================================================================
    // 🔄 DISPLAY UPDATE
    // ========================================================================

    private void UpdateDisplay()
    {
        int stock = 0;
        int sellPrice = 50;

        if (InventoryManager.Instance != null)
        {
            stock = InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY);
        }

        if (SellManager.Instance != null)
        {
            sellPrice = SellManager.Instance.SellPrice;
        }

        // Update stock text
        if (_stockText != null)
        {
            _stockText.text = $"Stock: {stock}";
        }

        // Update price text
        if (_priceText != null)
        {
            int totalValue = stock * sellPrice;
            _priceText.text = $"Value: ${totalValue}";
        }

        // Update button text
        if (_buttonText != null)
        {
            _buttonText.text = stock > 0 ? $"SELL ALL (${stock * sellPrice})" : "NO STOCK";
        }

        // Update button color
        if (_buttonImage != null)
        {
            _buttonImage.color = stock > 0 ? _canSellColor : _cantSellColor;
        }

        // Update button interactable
        if (_sellButton != null)
        {
            _sellButton.interactable = stock > 0;
        }
    }
}