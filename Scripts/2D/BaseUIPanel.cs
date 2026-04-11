// ============================================================================
// BASEUIPANEL.CS
// ============================================================================
// PURPOSE:      Base class for all UI panels — provides show/hide with
//               optional animation, audio feedback, and state tracking
// VERSION:      v1 — Foundation
// CREATED:      April 4, 2026
// ============================================================================
// DEV GUIDE:
//   To create a new UI panel:
//
//   public class MyNewPanelUI : BaseUIPanel
//   {
//       protected override void OnPanelShown()
//       {
//           // Refresh data, bind listeners, etc.
//       }
//
//       protected override void OnPanelHidden()
//       {
//           // Unbind listeners, cleanup, etc.
//       }
//   }
//
//   Then in UIManager or wherever:
//     myPanel.Show();    // plays SFX, animates in
//     myPanel.Hide();    // plays SFX, animates out
//     myPanel.Toggle();  // smart toggle
//
//   ANIMATION:
//     Set _animatePanel = true in inspector and assign a CanvasGroup.
//     The panel will fade in/out + scale. Or override AnimateShow/AnimateHide
//     for custom animations.
// ============================================================================

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseUIPanel : MonoBehaviour
{
    // ========================================================================
    // INSPECTOR
    // ========================================================================

    [Header("Panel Settings")]
    [Tooltip("Animate show/hide transitions")]
    [SerializeField] protected bool _animatePanel = true;

    [Tooltip("Animation duration in seconds")]
    [SerializeField] protected float _animDuration = 0.2f;

    [Tooltip("Play SFX on show/hide")]
    [SerializeField] protected bool _playSFX = true;

    // ========================================================================
    // STATE
    // ========================================================================

    private CanvasGroup _canvasGroup;
    private Coroutine _animCoroutine;

    /// <summary>True if panel is currently visible.</summary>
    public bool IsVisible { get; private set; }

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Start hidden
        if (!gameObject.activeSelf)
        {
            _canvasGroup.alpha = 0f;
            IsVisible = false;
        }
        else
        {
            IsVisible = true;
        }
    }

    // ========================================================================
    // PUBLIC API
    // ========================================================================

    /// <summary>Shows the panel with optional animation.</summary>
    public virtual void Show()
    {
        if (IsVisible) return;

        gameObject.SetActive(true);
        IsVisible = true;

        if (_playSFX && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_OPEN);

        if (_animatePanel && _canvasGroup != null)
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(AnimateShow());
        }
        else
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        OnPanelShown();
    }

    /// <summary>Hides the panel with optional animation.</summary>
    public virtual void Hide()
    {
        if (!IsVisible) return;

        IsVisible = false;

        if (_playSFX && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_CLOSE);

        if (_animatePanel && _canvasGroup != null)
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(AnimateHide());
        }
        else
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        OnPanelHidden();
    }

    /// <summary>Toggle visibility.</summary>
    public void Toggle()
    {
        if (IsVisible) Hide();
        else Show();
    }

    // ========================================================================
    // VIRTUAL HOOKS — Override in subclass
    // ========================================================================

    /// <summary>Called after the panel is shown. Refresh data here.</summary>
    protected virtual void OnPanelShown() { }

    /// <summary>Called after the panel is hidden. Cleanup here.</summary>
    protected virtual void OnPanelHidden() { }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    protected virtual IEnumerator AnimateShow()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.9f;
        Vector3 endScale = Vector3.one;
        transform.localScale = startScale;

        while (elapsed < _animDuration)
        {
            elapsed += Time.unscaledDeltaTime;  // unscaled so it works during pause
            float t = elapsed / _animDuration;
            float eased = EaseOutBack(t);

            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, eased);

            yield return null;
        }

        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    protected virtual IEnumerator AnimateHide()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 0.9f;

        while (elapsed < _animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _animDuration;

            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    // ========================================================================
    // EASING
    // ========================================================================

    /// <summary>Ease-out-back for snappy UI pop-in.</summary>
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
