// ============================================================================
// DAYNIGHTCYCLE.CS
// ============================================================================
// PURPOSE:      Tints the camera background between day and night colors
// VERSION:      v1
// CREATED:      February 12, 2026
// DEPENDENCIES: None
// NOTES:        Attach to Main Camera. Pure visual — no gameplay impact.
// ============================================================================

using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
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

    [Header("Ambient Light (Optional)")]
    [Tooltip("If true, also adjusts global light tint via a screen overlay")]
    [SerializeField] private bool tintSprites = true;

    [Tooltip("SpriteRenderer used as a full-screen overlay for tinting (optional)")]
    [SerializeField] private SpriteRenderer screenOverlay;

    [Tooltip("Day tint — applied to overlay at midday")]
    [SerializeField] private Color dayTint = new Color(1f, 1f, 1f, 0f);

    [Tooltip("Night tint — applied to overlay at midnight")]
    [SerializeField] private Color nightTint = new Color(0.1f, 0.1f, 0.3f, 0.35f);

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Camera _camera;
    private float _cycleTimer = 0f;

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

        // Start at morning (25% through cycle)
        _cycleTimer = cycleDuration * 0.25f;

        Debug.Log($"[DayNightCycle] 🌅 Day/Night cycle initialized. Duration: {cycleDuration}s");
    }

    // ========================================================================
    // 🔄 UPDATE — LERP COLORS
    // ========================================================================

    private void Update()
    {
        // Advance timer
        _cycleTimer += Time.deltaTime;

        if (_cycleTimer >= cycleDuration)
        {
            _cycleTimer -= cycleDuration;
        }

        // Normalize to 0-1 range
        float t = _cycleTimer / cycleDuration;

        // ====================================================================
        // COLOR CALCULATION
        // ====================================================================
        // t = 0.00 → Sunrise
        // t = 0.25 → Midday (brightest)
        // t = 0.50 → Sunset
        // t = 0.75 → Midnight (darkest)
        // t = 1.00 → Sunrise again
        // ====================================================================

        Color bgColor;
        Color overlayColor;

        if (t < 0.25f)
        {
            // Sunrise → Midday (0.00 - 0.25)
            float phase = t / 0.25f;
            bgColor = Color.Lerp(sunsetColor, dayColor, phase);
            overlayColor = Color.Lerp(nightTint, dayTint, phase);
        }
        else if (t < 0.50f)
        {
            // Midday → Sunset (0.25 - 0.50)
            float phase = (t - 0.25f) / 0.25f;
            bgColor = Color.Lerp(dayColor, sunsetColor, phase);
            overlayColor = Color.Lerp(dayTint, nightTint, phase * 0.5f);
        }
        else if (t < 0.75f)
        {
            // Sunset → Midnight (0.50 - 0.75)
            float phase = (t - 0.50f) / 0.25f;
            bgColor = Color.Lerp(sunsetColor, nightColor, phase);
            overlayColor = Color.Lerp(nightTint * 0.5f, nightTint, phase);
        }
        else
        {
            // Midnight → Sunrise (0.75 - 1.00)
            float phase = (t - 0.75f) / 0.25f;
            bgColor = Color.Lerp(nightColor, sunsetColor, phase);
            overlayColor = Color.Lerp(nightTint, nightTint * 0.5f, phase);
        }

        // Apply background color
        _camera.backgroundColor = bgColor;

        // Apply overlay tint if enabled
        if (tintSprites && screenOverlay != null)
        {
            screenOverlay.color = overlayColor;
        }
    }
}