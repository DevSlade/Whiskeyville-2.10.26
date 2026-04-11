// ============================================================================
// TWEENHELPER.CS
// ============================================================================
// PURPOSE:       Static coroutine-based tween utilities. No DOTween required.
//                Returns IEnumerators — callers must use StartCoroutine().
// VERSION:       v1
// CREATED:       April 10, 2026
// USAGE:
//   StartCoroutine(TweenHelper.ScalePunch(transform));
//   StartCoroutine(TweenHelper.FadeIn(canvasGroup, panelTransform));
//   StartCoroutine(TweenHelper.FadeOut(canvasGroup, panelTransform, () => go.SetActive(false)));
//   StartCoroutine(TweenHelper.ShakeH(transform));
// ============================================================================

using UnityEngine;
using System;
using System.Collections;

public static class TweenHelper
{
    // =========================================================================
    // SCALE PUNCH
    // =========================================================================

    /// <summary>
    /// Overshoot scale then snap back to original. 60% expand, 40% settle.
    /// Use for button clicks, production pops, harvest confirmations.
    /// Uses unscaled time — works during pause.
    /// </summary>
    /// <param name="target">Transform to punch.</param>
    /// <param name="strength">Scale overshoot factor (0.12 = 12% bigger at peak).</param>
    /// <param name="duration">Total animation time in seconds.</param>
    public static IEnumerator ScalePunch(Transform target, float strength = 0.12f, float duration = 0.14f)
    {
        if (target == null) yield break;

        Vector3 original = target.localScale;
        Vector3 peak     = original * (1f + strength);

        // Expand phase — 60% of total duration
        float expandTime = duration * 0.6f;
        float t = 0f;
        while (t < expandTime)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / expandTime);
            target.localScale = Vector3.LerpUnclamped(original, peak, n);
            yield return null;
        }

        // Settle phase — 40% of total duration
        t = 0f;
        float settleTime = duration * 0.4f;
        while (t < settleTime)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / settleTime);
            target.localScale = Vector3.LerpUnclamped(peak, original, n);
            yield return null;
        }

        target.localScale = original;
    }

    // =========================================================================
    // HORIZONTAL SHAKE
    // =========================================================================

    /// <summary>
    /// Shakes a transform left-right and returns to origin.
    /// Use for invalid actions: no cash, wrong tool, can't afford, etc.
    /// Uses unscaled time — works during pause.
    /// </summary>
    /// <param name="target">Transform to shake.</param>
    /// <param name="duration">How long the shake lasts.</param>
    /// <param name="amplitude">Max pixel offset at peak (tapers to zero).</param>
    public static IEnumerator ShakeH(Transform target, float duration = 0.3f, float amplitude = 5f)
    {
        if (target == null) yield break;

        Vector3 origin  = target.localPosition;
        float   elapsed = 0f;
        float   freq    = 30f; // oscillation speed

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float decay = 1f - (elapsed / duration);                         // 1→0 over duration
            float x     = Mathf.Sin(elapsed * freq) * amplitude * decay;
            target.localPosition = origin + new Vector3(x, 0f, 0f);
            yield return null;
        }

        target.localPosition = origin; // Snap back to exact origin
    }

    // =========================================================================
    // PANEL FADE IN
    // =========================================================================

    /// <summary>
    /// Fades a CanvasGroup from 0→1 alpha with a subtle scale pop (EaseOutBack).
    /// Call AFTER SetActive(true).
    /// Sets interactable = true when complete.
    /// Uses unscaled time — safe during pause.
    /// </summary>
    /// <param name="cg">CanvasGroup on the panel.</param>
    /// <param name="panel">Panel transform for scale animation (can be null for alpha-only).</param>
    /// <param name="duration">Animation duration in seconds.</param>
    public static IEnumerator FadeIn(CanvasGroup cg, Transform panel, float duration = 0.18f)
    {
        if (cg == null) yield break;

        // Lock interaction during animation
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        cg.alpha          = 0f;

        Vector3 startScale = Vector3.one * 0.92f;
        if (panel != null) panel.localScale = startScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            cg.alpha = n;
            if (panel != null)
                panel.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, EaseOutBack(n));
            yield return null;
        }

        // Snap to final state
        cg.alpha = 1f;
        if (panel != null) panel.localScale = Vector3.one;
        cg.interactable   = true;
        cg.blocksRaycasts = true;
    }

    // =========================================================================
    // PANEL FADE OUT
    // =========================================================================

    /// <summary>
    /// Fades a CanvasGroup from current alpha→0 with subtle scale-down.
    /// Calls onDone when complete (typically () => gameObject.SetActive(false)).
    /// Uses unscaled time — safe during pause.
    /// </summary>
    /// <param name="cg">CanvasGroup on the panel.</param>
    /// <param name="panel">Panel transform for scale animation (can be null for alpha-only).</param>
    /// <param name="duration">Animation duration in seconds.</param>
    /// <param name="onDone">Action invoked on completion. Use to SetActive(false).</param>
    public static IEnumerator FadeOut(CanvasGroup cg, Transform panel,
                                      float duration = 0.14f, Action onDone = null)
    {
        if (cg == null)
        {
            onDone?.Invoke();
            yield break;
        }

        // Lock interaction immediately
        cg.interactable   = false;
        cg.blocksRaycasts = false;

        float startAlpha  = cg.alpha;
        Vector3 startScale = panel != null ? panel.localScale : Vector3.one;
        Vector3 endScale   = Vector3.one * 0.92f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            cg.alpha = Mathf.Lerp(startAlpha, 0f, n);
            if (panel != null) panel.localScale = Vector3.Lerp(startScale, endScale, n);
            yield return null;
        }

        // Snap to final state, then call callback
        cg.alpha = 0f;
        if (panel != null) panel.localScale = Vector3.one; // Reset for next show
        onDone?.Invoke();
    }

    // =========================================================================
    // EASING
    // =========================================================================

    /// <summary>
    /// Ease-out-back: overshoots slightly then settles. Gives UI a snappy pop feel.
    /// </summary>
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
