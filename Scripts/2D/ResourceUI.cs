// ============================================================================
// RESOURCEUI.CS
// ============================================================================
// PURPOSE:      Displays all resource counts in the HUD.
//               Supports icon sprites alongside text labels.
//               When an icon is assigned, text shows just the number.
//               When no icon, falls back to "Label: N" plain text.
// VERSION:      v2 — Icon support + formatted number display
// UPDATED:      April 9, 2026
// ATTACHED TO:  Canvas → ResourcePanel
// DEPENDENCIES: InventoryManager
// ============================================================================
// INSPECTOR SETUP:
//   Text fields (required): _cashText, _cornText, etc. — TMP labels
//   Icon fields (optional): _cashIcon, _cornIcon, etc. — UI Image components
//   _useIconMode: when true, text shows "500" or "$500" (no "Cash:" prefix)
//   _animateOnChange: briefly pulses text white on resource gain
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResourceUI : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR — TEXT LABELS
    // ========================================================================

    [Header("Resource Text Labels")]
    [Tooltip("TextMeshProUGUI showing cash amount")]
    [SerializeField] private TextMeshProUGUI _cashText;

    [Tooltip("TextMeshProUGUI showing corn amount")]
    [SerializeField] private TextMeshProUGUI _cornText;

    [Tooltip("TextMeshProUGUI showing mash amount")]
    [SerializeField] private TextMeshProUGUI _mashText;

    [Tooltip("TextMeshProUGUI showing whiskey amount")]
    [SerializeField] private TextMeshProUGUI _whiskeyText;

    [Tooltip("TextMeshProUGUI showing aged whiskey amount")]
    [SerializeField] private TextMeshProUGUI _agedWhiskeyText;

    [Tooltip("TextMeshProUGUI showing wood amount")]
    [SerializeField] private TextMeshProUGUI _woodText;

    [Tooltip("TextMeshProUGUI showing barrel amount")]
    [SerializeField] private TextMeshProUGUI _barrelText;

    // ========================================================================
    // 🖼️ INSPECTOR — ICON IMAGES (optional)
    // ========================================================================

    [Header("Resource Icons (optional — assign to enable icon mode per slot)")]
    [Tooltip("UI Image for cash icon (e.g. coin sprite). Leave null to use text-only mode.")]
    [SerializeField] private Image _cashIcon;

    [Tooltip("UI Image for corn icon")]
    [SerializeField] private Image _cornIcon;

    [Tooltip("UI Image for mash icon")]
    [SerializeField] private Image _mashIcon;

    [Tooltip("UI Image for whiskey icon")]
    [SerializeField] private Image _whiskeyIcon;

    [Tooltip("UI Image for aged whiskey icon")]
    [SerializeField] private Image _agedWhiskeyIcon;

    [Tooltip("UI Image for wood icon")]
    [SerializeField] private Image _woodIcon;

    [Tooltip("UI Image for barrel icon")]
    [SerializeField] private Image _barrelIcon;

    // ========================================================================
    // ⚙️ DISPLAY OPTIONS
    // ========================================================================

    [Header("Display Options")]
    [Tooltip("When enabled: text shows only the number (icon mode). When disabled: 'Resource: N' (text-only mode).")]
    [SerializeField] private bool _useIconMode = true;

    [Tooltip("Briefly flashes the text white when a resource value increases")]
    [SerializeField] private bool _animateOnChange = true;

    [Tooltip("Duration of the gain flash animation in seconds")]
    [SerializeField] private float _flashDuration = 0.25f;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    // Track previous values to detect gain vs loss for animation
    private int _prevCash          = -1;
    private int _prevCorn          = -1;
    private int _prevMash          = -1;
    private int _prevWhiskey       = -1;
    private int _prevAgedWhiskey   = -1;
    private int _prevWood          = -1;
    private int _prevBarrel        = -1;

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
    // 🔧 EVENT SUBSCRIPTION
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
    // 🔄 FULL REFRESH
    // ========================================================================

    private void RefreshAllDisplays()
    {
        if (InventoryManager.Instance == null) return;

        UpdateDisplay(InventoryManager.RESOURCE_CASH,         InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CASH));
        UpdateDisplay(InventoryManager.RESOURCE_CORN,         InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CORN));
        UpdateDisplay(InventoryManager.RESOURCE_MASH,         InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_MASH));
        UpdateDisplay(InventoryManager.RESOURCE_WHISKEY,      InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WHISKEY));
        UpdateDisplay(InventoryManager.RESOURCE_AGED_WHISKEY, InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY));
        UpdateDisplay(InventoryManager.RESOURCE_WOOD,         InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WOOD));
        UpdateDisplay(InventoryManager.RESOURCE_BARREL,       InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_BARREL));
    }

    // ========================================================================
    // 🔧 PER-RESOURCE DISPLAY UPDATE
    // ========================================================================

    private void UpdateDisplay(string resourceName, int value)
    {
        switch (resourceName)
        {
            case InventoryManager.RESOURCE_CASH:
                SetSlot(_cashText, _cashIcon, value, ref _prevCash, cash: true);
                break;

            case InventoryManager.RESOURCE_CORN:
                SetSlot(_cornText, _cornIcon, value, ref _prevCorn);
                break;

            case InventoryManager.RESOURCE_MASH:
                SetSlot(_mashText, _mashIcon, value, ref _prevMash);
                break;

            case InventoryManager.RESOURCE_WHISKEY:
                SetSlot(_whiskeyText, _whiskeyIcon, value, ref _prevWhiskey);
                break;

            case InventoryManager.RESOURCE_AGED_WHISKEY:
                SetSlot(_agedWhiskeyText, _agedWhiskeyIcon, value, ref _prevAgedWhiskey);
                break;

            case InventoryManager.RESOURCE_WOOD:
                SetSlot(_woodText, _woodIcon, value, ref _prevWood);
                break;

            case InventoryManager.RESOURCE_BARREL:
                SetSlot(_barrelText, _barrelIcon, value, ref _prevBarrel);
                break;
        }
    }

    // ========================================================================
    // 🔧 SLOT HELPER — Sets text + icon, handles both modes
    // ========================================================================

    /// <summary>
    /// Updates a single resource slot. Handles icon vs text-only mode.
    /// Triggers flash animation on gain if _animateOnChange is enabled.
    /// </summary>
    /// <param name="label">The TMP text element for this resource.</param>
    /// <param name="icon">The Image icon for this resource (may be null).</param>
    /// <param name="value">The new resource value.</param>
    /// <param name="prevValue">Ref to previous value — used for gain/loss detection.</param>
    /// <param name="cash">If true, formats value with $ prefix and thousands separator.</param>
    private void SetSlot(TextMeshProUGUI label, Image icon, int value, ref int prevValue, bool cash = false)
    {
        if (label == null) return;

        // ---- BUILD DISPLAY STRING ----
        bool hasIcon = icon != null;
        string displayText;

        if (_useIconMode && hasIcon)
        {
            // Icon mode: show only the number (icon provides the label context)
            displayText = cash ? $"${value:N0}" : value.ToString("N0");
        }
        else
        {
            // Text-only mode: show "Resource: N" fallback
            string prefix = cash ? "Cash" : label.name.Replace("Text", "").Replace("_", "");
            displayText = cash
                ? $"${value:N0}"
                : $"{value:N0}";

            // If no icon at all, show a prefix so it's still legible
            if (!hasIcon && !_useIconMode)
            {
                displayText = cash
                    ? $"Cash: ${value:N0}"
                    : $"{label.gameObject.name.Replace("_", " ").Replace("Text", "")}: {value:N0}";
            }
        }

        label.text = displayText;

        // ---- FLASH ON GAIN ----
        if (_animateOnChange && value > prevValue && prevValue >= 0)
        {
            StartCoroutine(FlashGain(label));
        }

        prevValue = value;
    }

    // ========================================================================
    // ✨ GAIN FLASH ANIMATION
    // ========================================================================

    /// <summary>
    /// Briefly flashes text to white on resource gain, then returns to original color.
    /// Uses Time.unscaledTime so pause menu doesn't freeze the animation.
    /// </summary>
    private IEnumerator FlashGain(TextMeshProUGUI label)
    {
        Color originalColor = label.color;
        Color gainColor     = Color.white;

        float elapsed = 0f;

        // Flash in: original → white
        while (elapsed < _flashDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (_flashDuration * 0.5f);
            label.color = Color.Lerp(originalColor, gainColor, t);
            yield return null;
        }

        // Flash out: white → original
        elapsed = 0f;
        while (elapsed < _flashDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (_flashDuration * 0.5f);
            label.color = Color.Lerp(gainColor, originalColor, t);
            yield return null;
        }

        label.color = originalColor;
    }
}
