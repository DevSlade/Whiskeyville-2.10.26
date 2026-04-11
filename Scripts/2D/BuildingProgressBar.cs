// ============================================================================
// BUILDINGPROGRESSBAR.CS
// ============================================================================
// PURPOSE:      World-space production progress bar floating above a building.
//               Uses SpriteRenderers (guaranteed visible — no Canvas/URP issues).
//               Fills left-to-right as production ticks.
//               Color shifts from dark amber → bright gold as completion nears.
//               Pulses red-orange when blocked waiting for input resources.
//               Flashes white on production complete.
// VERSION:      v3 — SpriteRenderer approach, improved color feedback
// UPDATED:      April 9, 2026
// ATTACHED TO:  Same GameObject as BuildingBehavior (add to each building prefab)
// DEPENDENCIES: BuildingBehavior (RequireComponent)
// ============================================================================
// HOW TO READ THE BAR:
//   Dark amber fill    = Production in progress
//   Bright gold fill   = Almost ready (>75%)
//   Red/orange fill    = Waiting for input resources (building starved)
//   White flash        = Production just fired — item created!
//   Bar hidden         = Non-production building (Saloon etc.)
// ============================================================================
// INSPECTOR:
//   _barWidth     — Width of the bar in world units (default 0.75)
//   _barHeight    — Height of the bar in world units (default 0.14)
//   _yOffset      — Vertical position above building pivot (default 0.9)
// ============================================================================

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BuildingBehavior))]
public class BuildingProgressBar : MonoBehaviour
{
    // ========================================================================
    // ⚙️ CONFIGURABLE
    // ========================================================================

    [Header("Bar Dimensions")]
    [Tooltip("Width of the full bar in world units")]
    [SerializeField] private float _barWidth = 0.75f;

    [Tooltip("Height of the bar in world units (make it chunky enough to see clearly)")]
    [SerializeField] private float _barHeight = 0.14f;

    [Tooltip("Vertical offset above the building pivot — increase for taller buildings")]
    [SerializeField] private float _yOffset = 0.9f;

    [Header("Colors")]
    [Tooltip("Background / track color")]
    [SerializeField] private Color _bgColor = new Color(0.10f, 0.07f, 0.04f, 0.90f);

    [Tooltip("Fill color at low progress (0–50%) — dark amber")]
    [SerializeField] private Color _fillColorLow = new Color(0.70f, 0.38f, 0.05f, 1f);

    [Tooltip("Fill color at high progress (75–100%) — bright gold")]
    [SerializeField] private Color _fillColorHigh = new Color(1.00f, 0.82f, 0.10f, 1f);

    [Tooltip("Fill color when blocked waiting for input resources — red-orange")]
    [SerializeField] private Color _waitingColor = new Color(0.80f, 0.22f, 0.05f, 1f);

    [Tooltip("Flash color on production complete — bright white")]
    [SerializeField] private Color _flashColor = new Color(1f, 1f, 1f, 1f);

    [Tooltip("Duration of the white completion flash in seconds")]
    [SerializeField] private float _flashDuration = 0.25f;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private BuildingBehavior _building;

    private SpriteRenderer _bgRenderer;
    private SpriteRenderer _fillRenderer;

    private bool  _isFlashing   = false;
    private float _lastProgress = 0f;

    // Left edge x of the fill bar (pivot is bottom-left of fill, so position = left edge)
    private float _leftEdge;

    // ========================================================================
    // 🚀 INITIALIZATION
    // ========================================================================

    private void Awake()
    {
        _building = GetComponent<BuildingBehavior>();

        if (_building == null)
        {
            Debug.LogError("[BuildingProgressBar] ❌ No BuildingBehavior found!");
            enabled = false;
            return;
        }

        CreateBarRenderers();
    }

    private void Start()
    {
        SetBarVisible(false); // Hidden until building initializes
    }

    // ========================================================================
    // 🖼️ BAR CREATION
    // ========================================================================

    /// <summary>
    /// Creates two child GameObjects with SpriteRenderers:
    /// a dark background track and an amber fill bar.
    /// Both use programmatic 1×1 white textures tinted by .color — no assets needed.
    /// Fill bar uses bottom-LEFT pivot so scaling X expands rightward from the left edge.
    /// </summary>
    private void CreateBarRenderers()
    {
        // Shared white texture (tinted per renderer)
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        // Center pivot for background
        Sprite bgSprite   = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        // Bottom-LEFT pivot for fill so scale-X expands rightward
        Sprite fillSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);

        // ---- BACKGROUND (slightly taller than fill to create border effect) ----
        GameObject bgObj = new GameObject("ProgressBar_BG");
        bgObj.transform.SetParent(transform, false);
        bgObj.transform.localPosition = new Vector3(0f, _yOffset, 0f);
        bgObj.transform.localScale    = new Vector3(_barWidth + 0.04f, _barHeight + 0.04f, 1f); // 2px border

        _bgRenderer              = bgObj.AddComponent<SpriteRenderer>();
        _bgRenderer.sprite       = bgSprite;
        _bgRenderer.color        = _bgColor;
        _bgRenderer.sortingOrder = 400; // Above buildings

        // ---- FILL BAR (left edge anchored, scales X by progress) ----
        _leftEdge = -(_barWidth * 0.5f); // World x of left edge

        GameObject fillObj = new GameObject("ProgressBar_Fill");
        fillObj.transform.SetParent(transform, false);
        fillObj.transform.localPosition = new Vector3(_leftEdge, _yOffset, 0f);
        fillObj.transform.localScale    = new Vector3(0f, _barHeight, 1f); // Starts empty

        _fillRenderer              = fillObj.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite       = fillSprite;
        _fillRenderer.color        = _fillColorLow;
        _fillRenderer.sortingOrder = 401; // One above background

        Debug.Log($"[BuildingProgressBar] ✅ Bar created for {gameObject.name}.");
    }

    // ========================================================================
    // 🔄 UPDATE — Drive bar width and color each frame
    // ========================================================================

    private void Update()
    {
        // Non-producing buildings (Saloon etc.) stay hidden
        if (!_building.IsProducing)
        {
            SetBarVisible(false);
            return;
        }

        SetBarVisible(true);

        float progress = _building.ProductionProgress;

        // ---- Detect production fire moment: progress wraps from ~1 → ~0 ----
        if (_lastProgress >= 0.95f && progress < 0.10f && !_isFlashing)
        {
            StartCoroutine(FlashComplete());
        }

        _lastProgress = progress;

        if (_isFlashing) return; // Flash coroutine owns the fill visuals during flash

        // ---- Scale fill bar width (X = progress * barWidth, pivot at left edge) ----
        Vector3 scale   = _fillRenderer.transform.localScale;
        scale.x         = progress * _barWidth;
        _fillRenderer.transform.localScale = scale;

        // ---- Color feedback ----
        if (_building.IsWaitingForInput)
        {
            // Red-orange: clearly communicates "I'm starved, need resources"
            _fillRenderer.color = _waitingColor;
        }
        else
        {
            // Lerp from dark amber (empty) → bright gold (full) as progress climbs
            // Progress < 0.5 → dark amber; progress > 0.75 → bright gold
            float colorT = Mathf.InverseLerp(0.5f, 0.85f, progress);
            _fillRenderer.color = Color.Lerp(_fillColorLow, _fillColorHigh, colorT);
        }
    }

    // ========================================================================
    // ✨ COMPLETION FLASH
    // ========================================================================

    /// <summary>
    /// Briefly flashes the bar white when production fires, then resets.
    /// White = something was made. Universal "success" signal.
    /// </summary>
    private IEnumerator FlashComplete()
    {
        _isFlashing = true;

        // Snap to full, white
        Vector3 scale   = _fillRenderer.transform.localScale;
        scale.x         = _barWidth;
        _fillRenderer.transform.localScale = scale;
        _fillRenderer.color                = _flashColor;

        yield return new WaitForSeconds(_flashDuration);

        _isFlashing = false;
    }

    // ========================================================================
    // 🔧 VISIBILITY
    // ========================================================================

    private void SetBarVisible(bool visible)
    {
        if (_bgRenderer   != null) _bgRenderer.enabled   = visible;
        if (_fillRenderer != null) _fillRenderer.enabled = visible;
    }
}
