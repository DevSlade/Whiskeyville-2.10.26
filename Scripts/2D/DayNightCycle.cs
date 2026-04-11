// ============================================================================
// DAYNIGHTCYCLE.CS
// ============================================================================
// PURPOSE:      Tints camera background between day/night colors
//               Fires audio transitions and public events on phase change
//               Auto-creates screen overlay for night darkness
// VERSION:      v4 — Continuous overlay curve, no phase discontinuities
// UPDATED:      April 8, 2026
// ATTACHED TO:  Main Camera
// DEPENDENCIES: AudioManager (optional — null-safe)
// ============================================================================
// EVENTS (subscribe from any script):
//   DayNightCycle.OnDayStart  → fires once when day begins
//   DayNightCycle.OnNightStart → fires once when night begins
// ============================================================================
// AUDIO TRIGGERS:
//   Day   → AudioManager.Instance.TransitionToDay()
//   Night → AudioManager.Instance.TransitionToNight()
// ============================================================================

using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    // ========================================================================
    // 📡 STATIC EVENTS
    // ========================================================================

    /// <summary>Fires once when the cycle transitions into daytime.</summary>
    public static event System.Action OnDayStart;

    /// <summary>Fires once when the cycle transitions into nighttime.</summary>
    public static event System.Action OnNightStart;

    // ========================================================================
    // ⚙️ CONFIGURABLE
    // ========================================================================

    [Header("Cycle Settings")]
    [Tooltip("Full day-night cycle length in seconds")]
    [SerializeField] private float cycleDuration = 120f;

    [Header("Day Colors")]
    [Tooltip("Camera background color at midday")]
    [SerializeField] private Color dayColor = new Color(0.53f, 0.81f, 0.92f, 1f);

    [Header("Sunset Colors")]
    [Tooltip("Camera background color at sunset/sunrise")]
    [SerializeField] private Color sunsetColor = new Color(1f, 0.55f, 0.2f, 1f);

    [Header("Night Colors")]
    [Tooltip("Camera background color at midnight")]
    [SerializeField] private Color nightColor = new Color(0.05f, 0.05f, 0.2f, 1f);

    [Header("Ambient Light / Night Overlay")]
    [Tooltip("If true, adjusts global light tint via a screen overlay")]
    [SerializeField] private bool tintSprites = true;

    [Tooltip("SpriteRenderer used as full-screen overlay for tinting. If empty, one is auto-created.")]
    [SerializeField] private SpriteRenderer screenOverlay;

    [Tooltip("If true, auto-creates a screen overlay if none is assigned")]
    [SerializeField] private bool _autoCreateOverlay = true;

    [Tooltip("Day tint — applied to overlay at midday (transparent = no tint)")]
    [SerializeField] private Color dayTint = new Color(1f, 1f, 1f, 0f);

    [Tooltip("Night tint — applied to overlay at midnight (blue-dark, visible)")]
    [SerializeField] private Color nightTint = new Color(0.05f, 0.05f, 0.25f, 0.45f);

    [Header("🔊 Audio")]
    [Tooltip("If false, audio transitions are skipped (useful for main menu scenes)")]
    [SerializeField] private bool _triggerAudio = true;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Camera _camera;
    private float _cycleTimer = 0f;

    // Phase tracking for transition events
    // t = 0.00–0.50 = DAY phase (sunrise through sunset)
    // t = 0.50–1.00 = NIGHT phase (sunset through midnight to next sunrise)
    private bool _isNight = false;

    // Hysteresis tolerance to prevent rapid re-triggering near phase boundaries
    private const float TRANSITION_TOLERANCE = 0.02f;

    // t thresholds — NIGHT_AUDIO_START is LATER than visual sunset (0.50)
    // This means crickets/night music wait until the sky is actually dark
    // Visual sunset starts at 0.50, sky is dark by ~0.65
    private const float NIGHT_START = 0.50f;        // Visual: night colors begin
    private const float NIGHT_AUDIO_START = 0.65f;   // Audio: crickets + night music
    private const float DAY_START   = 0.00f;          // wraps from 1.0
    private const float DAY_AUDIO_START = 0.05f;      // Audio: day music + birds

    // ========================================================================
    // 🚀 INITIALIZATION
    // ========================================================================

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        if (_camera == null)
        {
            Debug.LogError("[DayNightCycle] ❌ No Camera component found! Attach this to Main Camera.");
            enabled = false;
            return;
        }

        // Auto-create screen overlay if none assigned and tinting is enabled
        if (tintSprites && screenOverlay == null && _autoCreateOverlay)
        {
            CreateScreenOverlay();
        }

        // Start at morning (25% through cycle = midday)
        _cycleTimer = cycleDuration * 0.25f;
        _isNight = false;

        Debug.Log($"[DayNightCycle] 🌅 Initialized. Duration: {cycleDuration}s, Overlay: {(screenOverlay != null ? "active" : "none")}");
    }

    /// <summary>
    /// Auto-creates a full-screen SpriteRenderer overlay as a child of the camera.
    /// This renders a semi-transparent dark layer over everything during nighttime.
    /// The overlay scales to cover the camera's full viewport at any zoom level.
    /// </summary>
    private void CreateScreenOverlay()
    {
        // Create a child GameObject for the overlay
        GameObject overlayObj = new GameObject("NightOverlay");
        overlayObj.transform.SetParent(_camera.transform);
        overlayObj.transform.localPosition = new Vector3(0f, 0f, 1f); // In front of camera's near plane

        // Create a simple white 1x1 sprite texture (will be tinted by color)
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        screenOverlay = overlayObj.AddComponent<SpriteRenderer>();
        screenOverlay.sprite = sprite;
        screenOverlay.sortingLayerName = "UI";
        screenOverlay.sortingOrder = 9999; // Always on top
        screenOverlay.color = dayTint; // Start transparent

        Debug.Log("[DayNightCycle] 🌙 Auto-created night screen overlay.");
    }

    private void Start()
    {
        // Trigger initial day audio without playing the sting
        // (game already starts at daytime — no sting needed)
        if (_triggerAudio && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.MUSIC_DAY);
            AudioManager.Instance.PlayAmbient(AudioManager.AMBIENT_DAY);
        }
    }

    // ========================================================================
    // 🔄 UPDATE — LERP COLORS + AUDIO PHASE DETECTION
    // ========================================================================

    private void Update()
    {
        // ---- ⏱️ ADVANCE TIMER ----
        _cycleTimer += Time.deltaTime;

        if (_cycleTimer >= cycleDuration)
        {
            _cycleTimer -= cycleDuration;
        }

        float t = _cycleTimer / cycleDuration;

        // ====================================================================
        // 🎨 COLOR CALCULATION
        // t = 0.00 → Sunrise
        // t = 0.25 → Midday  (brightest)
        // t = 0.50 → Sunset  ← NIGHT begins here
        // t = 0.75 → Midnight (darkest)
        // t = 1.00 → Sunrise again ← DAY begins here
        // ====================================================================

        Color bgColor;
        Color overlayColor;

        // ====================================================================
        // OVERLAY CURVE DESIGN (continuous, no discontinuities at boundaries):
        //
        //   t=0.00 (sunrise start)  → nightTint  (still dark from night)
        //   t=0.25 (midday)         → dayTint    (fully transparent, bright)
        //   t=0.50 (sunset)         → dayTint    (still bright — day lingers)
        //   t=0.75 (midnight)       → nightTint  (fully dark)
        //   t=1.00 (sunrise again)  → nightTint  (→ connects back to t=0)
        //
        // Each boundary is continuous: every phase ends exactly where the
        // next phase begins — no jumps, no abrupt darkening at sunset.
        // The darkness descends between sunset (t=0.50) and midnight (t=0.75),
        // then stays dark from midnight through the full midnight→sunrise leg
        // so the "dawn break" moment (sunrise→midday) feels intentional.
        // ====================================================================

        if (t < 0.25f)
        {
            // Sunrise → Midday: darkness fades as the sun rises
            float phase = t / 0.25f;
            bgColor = Color.Lerp(sunsetColor, dayColor, phase);
            overlayColor = Color.Lerp(nightTint, dayTint, phase);
        }
        else if (t < 0.50f)
        {
            // Midday → Sunset: sky warms, overlay stays transparent (still daytime)
            float phase = (t - 0.25f) / 0.25f;
            bgColor = Color.Lerp(dayColor, sunsetColor, phase);
            overlayColor = dayTint; // No darkness yet — it's still day
        }
        else if (t < 0.75f)
        {
            // Sunset → Midnight: darkness descends smoothly
            float phase = (t - 0.50f) / 0.25f;
            bgColor = Color.Lerp(sunsetColor, nightColor, phase);
            overlayColor = Color.Lerp(dayTint, nightTint, phase);
        }
        else
        {
            // Midnight → Sunrise: stays fully dark until the sun actually rises
            // (Sunrise→Midday phase handles the brightening at t=0)
            float phase = (t - 0.75f) / 0.25f;
            bgColor = Color.Lerp(nightColor, sunsetColor, phase);
            overlayColor = nightTint; // Still night — dawn handled by next phase
        }

        // Apply camera background color
        _camera.backgroundColor = bgColor;

        // Apply overlay tint if enabled
        if (tintSprites && screenOverlay != null)
        {
            screenOverlay.color = overlayColor;

            // Scale overlay to cover the full camera viewport (handles zoom changes)
            if (_autoCreateOverlay)
            {
                float height = _camera.orthographicSize * 2f;
                float width = height * _camera.aspect;
                screenOverlay.transform.localScale = new Vector3(width + 2f, height + 2f, 1f);
            }
        }

        // ====================================================================
        // 🔊 PHASE CHANGE DETECTION
        // ====================================================================

        DetectPhaseTransition(t);
    }

    // ========================================================================
    // 🔊 PHASE TRANSITION DETECTION
    // ========================================================================

    /// <summary>
    /// Detects when t crosses the night/day boundary and fires transitions once.
    /// Uses _isNight flag to avoid firing repeatedly per frame.
    /// Audio transitions use NIGHT_AUDIO_START (0.65) and DAY_AUDIO_START (0.05)
    /// so crickets wait until the sky is actually dark, not just sunset.
    /// </summary>
    private void DetectPhaseTransition(float t)
    {
        // ---- 🌙 DAY → NIGHT (audio at 0.65 when sky is dark, not 0.50 sunset) ----
        if (!_isNight && t >= NIGHT_AUDIO_START - TRANSITION_TOLERANCE)
        {
            _isNight = true;
            FireNightTransition();
        }
        // ---- 🌅 NIGHT → DAY (audio at 0.05 when sky is brightening) ----
        else if (_isNight && t >= DAY_AUDIO_START && t < NIGHT_AUDIO_START - TRANSITION_TOLERANCE && t < 0.15f)
        {
            _isNight = false;
            FireDayTransition();
        }
    }

    private void FireDayTransition()
    {
        Debug.Log("[DayNightCycle] 🌅 DAY begins.");

        // Fire event for any subscribers
        OnDayStart?.Invoke();

        // Trigger audio transition
        if (_triggerAudio && AudioManager.Instance != null)
        {
            AudioManager.Instance.TransitionToDay();
        }
    }

    private void FireNightTransition()
    {
        Debug.Log("[DayNightCycle] 🌙 NIGHT begins.");

        // Fire event for any subscribers
        OnNightStart?.Invoke();

        // Trigger audio transition
        if (_triggerAudio && AudioManager.Instance != null)
        {
            AudioManager.Instance.TransitionToNight();
        }
    }

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    /// <summary>Returns true if currently in the night phase.</summary>
    public bool IsNight => _isNight;

    /// <summary>Returns normalized cycle progress (0-1).</summary>
    public float CycleProgress => _cycleTimer / cycleDuration;
}
