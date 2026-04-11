// ============================================================================
// TOOLCHARGESYSTEM.CS
// ============================================================================
// PURPOSE:      Manages the "hold-to-charge" interaction for all tools.
//               Draws the charge ring via OnGUI — bypasses Canvas/URP entirely.
//               OnGUI renders on top of everything in every Unity config.
//               Ring fills clockwise as player holds mouse on a valid target.
// VERSION:      v3 — OnGUI arc ring (no Canvas, no SpriteRenderer, no URP issues)
// UPDATED:      April 9, 2026
// ARCHITECTURE: BaseSingleton (non-persistent)
// ATTACHED TO:  Any manager GameObject in GameScene
// DEPENDENCIES: AudioManager (optional), BaseSingleton
// ============================================================================
// HOW IT WORKS:
//   Draws 32 small squares arranged in a circle around the cursor.
//   Dark squares show the full ring path. Amber squares fill clockwise.
//   Color shifts amber → bright white at 100% to signal completion.
// ============================================================================

using UnityEngine;

public class ToolChargeSystem : BaseSingleton<ToolChargeSystem>
{
    // ========================================================================
    // ⚙️ CONFIGURABLE
    // ========================================================================

    [Header("Charge Settings")]
    [Tooltip("Seconds the player must hold to complete a charge")]
    [SerializeField] private float _chargeDuration = 0.6f;

    [Header("Ring Visuals")]
    [Tooltip("Radius of the ring in screen pixels")]
    [SerializeField] private float _ringRadius = 28f;

    [Tooltip("Size of each dot segment in screen pixels")]
    [SerializeField] private float _dotSize = 6.5f;

    [Tooltip("Number of dot segments in the ring (more = smoother)")]
    [SerializeField] private int _segments = 24;

    [Tooltip("Vertical offset above the cursor in screen pixels")]
    [SerializeField] private float _cursorOffset = 38f;

    [Header("Colors")]
    [Tooltip("Inactive segment color — dark ring track")]
    [SerializeField] private Color _trackColor = new Color(0.18f, 0.12f, 0.05f, 0.85f);

    [Tooltip("Active fill color — whiskey amber")]
    [SerializeField] private Color _fillColorStart = new Color(0.95f, 0.60f, 0.08f, 1f);

    [Tooltip("Fill color at 100% — bright white burst")]
    [SerializeField] private Color _fillColorEnd = new Color(1f, 0.95f, 0.75f, 1f);

    // ========================================================================
    // 🔒 SINGLETON CONFIG
    // ========================================================================

    protected override bool Persistent => false;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private bool          _isCharging  = false;
    private float         _chargeTimer = 0f;
    private System.Action _onCompleteCallback;
    private System.Action _onCancelCallback;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public bool  IsCharging     => _isCharging;
    public float ChargeProgress => _isCharging
        ? Mathf.Clamp01(_chargeTimer / _chargeDuration)
        : 0f;

    // ========================================================================
    // 🔄 UPDATE — Tick charge timer
    // ========================================================================

    private void Update()
    {
        if (!_isCharging) return;

        // ---- Cancel on mouse release ----
        if (!Input.GetMouseButton(0))
        {
            CancelCharge();
            return;
        }

        // ---- Advance ----
        _chargeTimer += Time.deltaTime;

        if (_chargeTimer >= _chargeDuration)
            FireCharge();
    }

    // ========================================================================
    // 🖥️ OnGUI — Draws the charge ring directly to the screen
    // ========================================================================

    // OnGUI is called by Unity's rendering pipeline regardless of render mode,
    // camera stack, or URP setup. It always draws on top of the game view.

    private void OnGUI()
    {
        if (!_isCharging) return;

        float progress     = ChargeProgress;
        int   activeCount  = Mathf.RoundToInt(progress * _segments);

        // Convert mouse from Unity screen coords (Y flipped for GUI)
        float mx = Input.mousePosition.x;
        float my = Screen.height - Input.mousePosition.y - _cursorOffset;

        for (int i = 0; i < _segments; i++)
        {
            // Start from top (–90°), advance clockwise
            float angleDeg = -90f + (360f / _segments) * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float px = mx + Mathf.Cos(angleRad) * _ringRadius;
            float py = my + Mathf.Sin(angleRad) * _ringRadius;

            bool active = i < activeCount;

            if (active)
            {
                // Lerp amber → white as charge nears 100%
                GUI.color = Color.Lerp(_fillColorStart, _fillColorEnd, progress * progress);
            }
            else
            {
                GUI.color = _trackColor;
            }

            GUI.DrawTexture(
                new Rect(px - _dotSize * 0.5f, py - _dotSize * 0.5f, _dotSize, _dotSize),
                Texture2D.whiteTexture
            );
        }

        // ---- Reset GUI color (required — other GUI will inherit otherwise) ----
        GUI.color = Color.white;
    }

    // ========================================================================
    // 📢 PUBLIC API
    // ========================================================================

    /// <summary>
    /// Starts a charge. Returns false if already charging (one at a time).
    /// onComplete fires when the ring fills. onCancel fires if released early.
    /// </summary>
    public bool BeginCharge(Vector3 worldPos, System.Action onComplete, System.Action onCancel = null)
    {
        if (_isCharging) return false;

        _isCharging         = true;
        _chargeTimer        = 0f;
        _onCompleteCallback = onComplete;
        _onCancelCallback   = onCancel;

        return true;
    }

    /// <summary>Cancels the current charge. Safe to call when not charging.</summary>
    public void CancelCharge()
    {
        if (!_isCharging) return;

        _isCharging = false;

        System.Action cancel = _onCancelCallback;
        _onCompleteCallback  = null;
        _onCancelCallback    = null;

        cancel?.Invoke();
    }

    // ========================================================================
    // 🔒 INTERNAL — Complete
    // ========================================================================

    private void FireCharge()
    {
        _isCharging = false;

        AudioManager.Instance?.PlaySFX(AudioManager.SFX_CLICK);

        System.Action complete = _onCompleteCallback;
        _onCompleteCallback    = null;
        _onCancelCallback      = null;

        complete?.Invoke();
    }
}
