// ============================================================================
// FAMEUI.CS
// ============================================================================
// PURPOSE:      HUD display for Fame total, current tier, and progress bar
// VERSION:      v1 — Foundation
// CREATED:      April 8, 2026
// ATTACHED TO:  Canvas → FamePanel (or wherever the HUD fame widget lives)
// DEPENDENCIES: FameManager, TMPro
// ============================================================================
// INSPECTOR SETUP:
//   _fameText       — TextMeshProUGUI showing "⭐ 1,500" (fame total)
//   _tierNameText   — TextMeshProUGUI showing "Known Distiller"
//   _nextTierText   — TextMeshProUGUI (optional) showing "500 fame to Respected"
//   _progressBar    — Image (fill type) showing progress to next tier (0–1)
//   _tierUpFlash    — GameObject (optional) briefly shown on tier-up
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FameUI : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR REFERENCES
    // ========================================================================

    [Header("Text Labels")]
    [Tooltip("Displays the total fame number, e.g. '⭐ 1,500'")]
    [SerializeField] private TextMeshProUGUI _fameText;

    [Tooltip("Displays the current tier name, e.g. 'Known Distiller'")]
    [SerializeField] private TextMeshProUGUI _tierNameText;

    [Tooltip("(Optional) Displays fame needed to next tier, e.g. '500 fame to Respected'")]
    [SerializeField] private TextMeshProUGUI _nextTierText;

    [Header("Progress Bar")]
    [Tooltip("Image with Image Type = Filled, Fill Method = Horizontal. Fill Amount is set at runtime.")]
    [SerializeField] private Image _progressBar;

    [Header("Tier-Up Effect")]
    [Tooltip("(Optional) GameObject briefly shown on tier-up (flash panel, particles, etc.)")]
    [SerializeField] private GameObject _tierUpFlash;

    [Tooltip("How long the tier-up flash stays visible in seconds")]
    [SerializeField] private float _tierUpFlashDuration = 2.0f;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Coroutine _flashCoroutine;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void OnEnable()
    {
        // Subscribe to FameManager events
        FameManager.OnFameChanged += OnFameChanged;
        FameManager.OnTierUp     += OnTierUp;

        // Sync immediately to current state (handles panel re-enabling mid-game)
        RefreshDisplay();
    }

    private void OnDisable()
    {
        FameManager.OnFameChanged -= OnFameChanged;
        FameManager.OnTierUp     -= OnTierUp;
    }

    // ========================================================================
    // 🔄 EVENT HANDLERS
    // ========================================================================

    /// <summary>Called by FameManager when fame total changes.</summary>
    private void OnFameChanged(int newTotal, int delta)
    {
        RefreshDisplay();
    }

    /// <summary>Called by FameManager when player reaches a new tier.</summary>
    private void OnTierUp(FameTier newTier)
    {
        RefreshDisplay();
        PlayTierUpFlash();
    }

    // ========================================================================
    // 🔧 DISPLAY UPDATE
    // ========================================================================

    /// <summary>Reads current FameManager state and updates all UI elements.</summary>
    private void RefreshDisplay()
    {
        if (FameManager.Instance == null)
        {
            // Hide gracefully if FameManager hasn't spawned yet
            if (_fameText    != null) _fameText.text    = "⭐ 0";
            if (_tierNameText != null) _tierNameText.text = "Unknown Distiller";
            if (_nextTierText != null) _nextTierText.text = "";
            if (_progressBar  != null) _progressBar.fillAmount = 0f;
            return;
        }

        // ---- 💫 FAME TOTAL ----
        if (_fameText != null)
        {
            // Format with thousand separators: "⭐ 1,500"
            _fameText.text = $"⭐ {FameManager.Instance.TotalFame:N0}";
        }

        // ---- 🏅 TIER NAME ----
        if (_tierNameText != null)
        {
            _tierNameText.text = FameManager.Instance.TierName;
        }

        // ---- 📈 PROGRESS BAR ----
        if (_progressBar != null)
        {
            _progressBar.fillAmount = FameManager.Instance.TierProgress;
        }

        // ---- 🔜 NEXT TIER LABEL ----
        if (_nextTierText != null)
        {
            int needed = FameManager.Instance.FameToNextTier;

            if (needed < 0)
            {
                // At max tier
                _nextTierText.text = "MAX TIER";
            }
            else
            {
                // Show next tier name and required fame
                FameTier nextTier = FameManager.Instance.CurrentTier + 1;
                string nextName = GetTierName(nextTier);
                _nextTierText.text = $"{needed:N0} fame → {nextName}";
            }
        }
    }

    // ========================================================================
    // ✨ TIER-UP FLASH EFFECT
    // ========================================================================

    /// <summary>Briefly shows the tier-up flash GameObject, then hides it.</summary>
    private void PlayTierUpFlash()
    {
        if (_tierUpFlash == null) return;

        // Stop any existing flash before starting a new one
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(TierUpFlashCoroutine());
    }

    private IEnumerator TierUpFlashCoroutine()
    {
        _tierUpFlash.SetActive(true);
        yield return new WaitForSecondsRealtime(_tierUpFlashDuration);
        _tierUpFlash.SetActive(false);
        _flashCoroutine = null;
    }

    // ========================================================================
    // 🔧 HELPER — Tier Name Lookup
    // ========================================================================

    /// <summary>Returns a short display-friendly name for any FameTier.</summary>
    private static string GetTierName(FameTier tier)
    {
        return tier switch
        {
            FameTier.Unknown   => "Unknown",
            FameTier.Known     => "Known",
            FameTier.Respected => "Respected",
            FameTier.Famous    => "Famous",
            FameTier.Legendary => "Legendary",
            FameTier.Iconic    => "Iconic",
            _                  => "???"
        };
    }
}
