// ============================================================================
// BUTTONANIMATOR.CS
// ============================================================================
// PURPOSE:       Adds a scale-punch animation to any Button on click.
//                Drop onto any Button GameObject — zero wiring required.
//                Hooks into the Button's onClick automatically.
// VERSION:       v1
// CREATED:       April 10, 2026
// INSPECTOR:     _strength  — how much to overshoot (0.12 = 12% bigger at peak)
//                _duration  — total punch time in seconds
//                _playSFX   — play a click SFX alongside the punch (opt-in)
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAnimator : MonoBehaviour
{
    // =========================================================================
    // INSPECTOR
    // =========================================================================

    [Header("Punch")]
    [Tooltip("How much the button overshoots its normal scale at peak. 0.12 = 12% bigger.")]
    [SerializeField] private float _strength = 0.12f;

    [Tooltip("Total duration of the punch animation in seconds.")]
    [SerializeField] private float _duration = 0.14f;

    [Header("Audio (Optional)")]
    [Tooltip("Play AudioManager.SFX_CLICK when the button is clicked.")]
    [SerializeField] private bool _playSFX = false;

    // =========================================================================
    // PRIVATE STATE
    // =========================================================================

    private Button    _button;
    private Coroutine _punchCoroutine;

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        // Clean up listener to prevent memory leaks on panel destruction
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }

    // =========================================================================
    // CLICK HANDLER
    // =========================================================================

    private void OnClick()
    {
        // Stop any mid-animation punch before starting a new one
        if (_punchCoroutine != null)
        {
            StopCoroutine(_punchCoroutine);
            transform.localScale = Vector3.one; // Reset to avoid stuck scale
        }

        _punchCoroutine = StartCoroutine(TweenHelper.ScalePunch(transform, _strength, _duration));

        if (_playSFX && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
    }
}
