// ============================================================================
// BUILDINGANIMATOR.CS
// ============================================================================
// PURPOSE:       Animates buildings on three events only:
//                  1. Placement  — pop-in from scale 0 (EaseOutBack)
//                  2. Production — scale punch on every resource fire
//                  3. Demolish   — shrink to zero, then Destroy fires
//                Drop-on component — add to each building prefab.
//                BuildingPlacementManager routes Destroy through PlayDemolish().
// VERSION:       v2 — Simplified
// CREATED:       April 10, 2026
// ATTACHED TO:   Same GameObject as BuildingBehavior
// ============================================================================

using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(BuildingBehavior))]
public class BuildingAnimator : MonoBehaviour
{
    // =========================================================================
    // INSPECTOR
    // =========================================================================

    [Header("Spawn")]
    [Tooltip("Duration of the pop-in animation when the building is placed.")]
    [SerializeField] private float _spawnDuration = 0.25f;

    [Header("Production Punch")]
    [Tooltip("Scale overshoot on each production fire. 0.15 = 15% bigger at peak.")]
    [SerializeField] private float _punchStrength = 0.15f;

    [Tooltip("Duration of the production scale punch in seconds.")]
    [SerializeField] private float _punchDuration = 0.18f;

    [Header("Demolish Shrink")]
    [Tooltip("Duration of the shrink-to-zero animation before the building is destroyed.")]
    [SerializeField] private float _demolishDuration = 0.15f;

    // =========================================================================
    // PRIVATE STATE
    // =========================================================================

    private BuildingBehavior _building;

    // Previous frame's ProductionProgress — detects the 1→0 wrap event
    private float _prevProgress = 0f;

    // Single active coroutine handle — animations are mutually exclusive
    private Coroutine _activeCoroutine;

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

    private void Start()
    {
        _building = GetComponent<BuildingBehavior>();

        // Play pop-in animation on first frame
        _activeCoroutine = StartCoroutine(SpawnAnimation());
    }

    private void Update()
    {
        if (_building == null) return;

        float progress = _building.ProductionProgress;

        // Production event: progress wraps from ~1 back to ~0
        // Thresholds avoid false positives near 0 and 1
        bool justProduced = _prevProgress > 0.85f
                         && progress    < 0.15f
                         && _building.IsProducing;

        _prevProgress = progress;

        if (justProduced) TriggerProductionPunch();
    }

    // =========================================================================
    // PUBLIC API — DEMOLISH
    // =========================================================================

    /// <summary>
    /// Called by BuildingPlacementManager.ConfirmDemolish() instead of Destroy().
    /// Shrinks the building to zero scale, then invokes onComplete.
    /// onComplete should contain the Destroy() call and any post-demolish logic.
    /// </summary>
    public void PlayDemolish(Action onComplete)
    {
        // Stop any running animation (e.g. mid-spawn, mid-punch) before demolishing
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(DemolishAnimation(onComplete));
    }

    // =========================================================================
    // ANIMATIONS
    // =========================================================================

    /// <summary>
    /// Pop-in from scale 0 to 1 with EaseOutBack overshoot.
    /// Gives placement a satisfying "thunk into place" feel.
    /// </summary>
    private IEnumerator SpawnAnimation()
    {
        transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < _spawnDuration)
        {
            t += Time.deltaTime;
            float n     = Mathf.Clamp01(t / _spawnDuration);
            float eased = EaseOutBack(n);
            transform.localScale = Vector3.one * eased;
            yield return null;
        }

        transform.localScale = Vector3.one;
        _activeCoroutine = null;
    }

    /// <summary>
    /// Scale punch on production fire. Stops any previous punch first.
    /// </summary>
    private void TriggerProductionPunch()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            transform.localScale = Vector3.one; // Reset before re-punching
        }

        _activeCoroutine = StartCoroutine(TweenHelper.ScalePunch(transform, _punchStrength, _punchDuration));
    }

    /// <summary>
    /// Shrinks building from current scale to zero, then fires onComplete.
    /// BuildingPlacementManager calls Destroy() inside onComplete.
    /// </summary>
    private IEnumerator DemolishAnimation(Action onComplete)
    {
        Vector3 startScale = transform.localScale;

        float t = 0f;
        while (t < _demolishDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / _demolishDuration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, n);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        onComplete?.Invoke(); // Destroy fires here
    }

    // =========================================================================
    // EASING
    // =========================================================================

    /// <summary>Ease-out-back: overshoots then settles. Used for the spawn pop.</summary>
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
