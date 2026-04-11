// ============================================================================
// TOOLCURSORUI.CS
// ============================================================================
// PURPOSE:      Shows a tool sprite icon that follows the mouse cursor
// ATTACHED TO:  Canvas → ToolCursorIcon (UI Image)
// DEPENDENCIES: ToolManager, ToolType
// VERSION:      v2 — Fixed: Image was disabling before ToolManager existed
// UPDATED:      March 25, 2026
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

public class ToolCursorUI : MonoBehaviour
{
    // ========================================================================
    // 🔧 INSPECTOR — TOOL SPRITES
    // ========================================================================

    [Header("🔧 Tool Sprites")]
    [Tooltip("Sprite shown when Build tool is active")]
    [SerializeField] private Sprite _buildSprite;

    [Tooltip("Sprite shown when Hoe tool is active")]
    [SerializeField] private Sprite _hoeSprite;

    [Tooltip("Sprite shown when Axe tool is active")]
    [SerializeField] private Sprite _axeSprite;

    [Tooltip("Sprite shown when Demolish tool is active")]
    [SerializeField] private Sprite _demolishSprite;

    [Tooltip("Sprite shown when Sickle tool is active")]
    [SerializeField] private Sprite _sickleSprite;

    // ========================================================================
    // ⚙️ INSPECTOR — SETTINGS
    // ========================================================================

    [Header("⚙️ Cursor Settings")]
    [Tooltip("Offset from mouse position in screen pixels")]
    [SerializeField] private Vector2 _offset = new Vector2(16f, -16f);

    [Tooltip("Size of the cursor icon")]
    [SerializeField] private Vector2 _iconSize = new Vector2(32f, 32f);

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Image _image;
    private RectTransform _rectTransform;
    private Canvas _parentCanvas;
    private Camera _canvasCamera;
    private bool _subscribed = false;
    private ToolType _lastKnownTool = ToolType.None;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        // ---- 🔧 CACHE COMPONENTS ----
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();

        if (_image == null)
        {
            Debug.LogError("[ToolCursorUI] ❌ No Image component found!");
            return;
        }

        if (_rectTransform == null)
        {
            Debug.LogError("[ToolCursorUI] ❌ No RectTransform found!");
            return;
        }

        // ---- 📐 SET SIZE ----
        _rectTransform.sizeDelta = _iconSize;

        // ---- 🚫 DISABLE RAYCAST SO IT DOESN'T BLOCK CLICKS ----
        _image.raycastTarget = false;

        // ---- 📷 FIND CANVAS FOR COORDINATE CONVERSION ----
        _parentCanvas = GetComponentInParent<Canvas>();

        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            _canvasCamera = _parentCanvas.worldCamera;
        }

        // ---- 👻 START HIDDEN (no sprite, fully transparent) ----
        _image.sprite = null;
        _image.color = new Color(1f, 1f, 1f, 0f);

        Debug.Log("[ToolCursorUI] ✅ Awake complete.");
    }

    private void Start()
    {
        // ---- 🔗 SUBSCRIBE TO TOOL MANAGER ----
        TrySubscribe();

        // ---- 🔄 SYNC TO CURRENT TOOL STATE ----
        if (ToolManager.Instance != null)
        {
            ApplyTool(ToolManager.Instance.ActiveTool);
        }
        else
        {
            Debug.LogWarning("[ToolCursorUI] ⚠️ ToolManager.Instance is null in Start. Will retry in Update.");
        }
    }

    private void Update()
    {
        // ---- 🔗 RETRY SUBSCRIBE IF FAILED ----
        if (!_subscribed)
        {
            TrySubscribe();
            if (_subscribed && ToolManager.Instance != null)
            {
                ApplyTool(ToolManager.Instance.ActiveTool);
            }
        }

        // ---- 🛡️ SAFETY POLL: catch missed events ----
        if (ToolManager.Instance != null)
        {
            ToolType currentTool = ToolManager.Instance.ActiveTool;
            if (currentTool != _lastKnownTool)
            {
                ApplyTool(currentTool);
            }
        }

        // ---- 🖱️ FOLLOW MOUSE ----
        if (_image != null && _image.sprite != null)
        {
            FollowMouse();
        }
    }

    private void OnDestroy()
    {
        // ---- 🔗 UNSUBSCRIBE ----
        if (_subscribed && ToolManager.Instance != null)
        {
            ToolManager.Instance.OnToolChanged -= OnToolChanged;
            _subscribed = false;
        }
    }

    // ========================================================================
    // 🔗 SUBSCRIPTION
    // ========================================================================

    private void TrySubscribe()
    {
        if (_subscribed) return;

        if (ToolManager.Instance != null)
        {
            ToolManager.Instance.OnToolChanged += OnToolChanged;
            _subscribed = true;
            Debug.Log("[ToolCursorUI] ✅ Subscribed to ToolManager.OnToolChanged.");
        }
    }

    // ========================================================================
    // 🖱️ FOLLOW MOUSE
    // ========================================================================

    private void FollowMouse()
    {
        if (_parentCanvas == null) return;

        Vector2 screenPos = Input.mousePosition;
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform,
            screenPos,
            _canvasCamera,
            out localPoint))
        {
            _rectTransform.localPosition = localPoint + _offset;
        }
    }

    // ========================================================================
    // 🔧 TOOL CHANGED HANDLER (event callback)
    // ========================================================================

    private void OnToolChanged(ToolType newTool)
    {
        ApplyTool(newTool);
    }

    // ========================================================================
    // 🎨 APPLY TOOL — SINGLE SOURCE OF TRUTH FOR VISIBILITY
    // ========================================================================

    private void ApplyTool(ToolType tool)
    {
        if (_image == null) return;

        _lastKnownTool = tool;

        Sprite toolSprite = GetSpriteForTool(tool);

        if (toolSprite != null)
        {
            // ---- ✅ SHOW ICON ----
            _image.sprite = toolSprite;
            _image.color = Color.white;
            Debug.Log($"[ToolCursorUI] 🔧 Showing cursor: {tool}");
        }
        else
        {
            // ---- 👻 HIDE ICON (transparent, no sprite) ----
            _image.sprite = null;
            _image.color = new Color(1f, 1f, 1f, 0f);
            Debug.Log("[ToolCursorUI] 🔧 Cursor hidden (no tool).");
        }
    }

    // ========================================================================
    // 🎨 SPRITE LOOKUP
    // ========================================================================

    private Sprite GetSpriteForTool(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.Build:
                return _buildSprite;
            case ToolType.Hoe:
                return _hoeSprite;
            case ToolType.Axe:
                return _axeSprite;
            case ToolType.Demolish:
                return _demolishSprite;
            case ToolType.Sickle:
                return _sickleSprite;
            case ToolType.None:
            default:
                return null;
        }
    }
}