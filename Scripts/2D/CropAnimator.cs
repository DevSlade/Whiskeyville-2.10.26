// ============================================================================
// CROPANIMATOR.CS
// ============================================================================
// PURPOSE:       Adds idle animations to crops. Drop-on component — zero wiring
//                required. Polls CropBehavior state each frame.
// VERSION:       v1
// CREATED:       April 10, 2026
// ATTACHED TO:   Same GameObject as CropBehavior (add to farm prefab)
// ANIMATIONS:
//   Growth advance   — scale punch on the whole crop when stage increments
//   Fully grown      — gentle continuous Y bob ("ready to harvest" signal)
//   Harvest complete — scale punch when IsFullyGrown flips true→false
//   Idle (growing)   — no movement while crop is still developing
// NOTES:
//   Anchor captured on Start. All offsets are relative to it.
//   Stage advance detection: CurrentStage != _prevStage (increments by 1).
//   Harvest detection: _wasFullyGrown && !IsFullyGrown (true→false flip).
//   Uses Time.deltaTime — freezes correctly when paused.
// ============================================================================

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CropBehavior))]
public class CropAnimator : MonoBehaviour
{
    // =========================================================================
    // INSPECTOR
    // =========================================================================

    [Header("Ready Bob (Fully Grown)")]
    [Tooltip("Speed of the Y-axis sine bob in radians per second.")]
    [SerializeField] private float _bobSpeed     = 1.4f;

    [Tooltip("Max Y offset in world units at bob peak. Slightly more urgent than buildings.")]
    [SerializeField] private float _bobAmplitude = 0.035f;

    [Header("Growth Punch")]
    [Tooltip("Scale overshoot when the crop advances a growth stage.")]
    [SerializeField] private float _growthPunchStrength = 0.2f;

    [Tooltip("Duration of the growth stage punch in seconds.")]
    [SerializeField] private float _growthPunchDuration = 0.16f;

    [Header("Harvest Punch")]
    [Tooltip("Scale overshoot on harvest confirmation (before crop resets to stage 0).")]
    [SerializeField] private float _harvestPunchStrength = 0.25f;

    [Tooltip("Duration of the harvest punch in seconds.")]
    [SerializeField] private float _harvestPunchDuration = 0.2f;

    // =========================================================================
    // PRIVATE STATE
    // =========================================================================

    private CropBehavior _crop;

    // Anchor local position — bob offsets applied relative to this
    private Vector3 _anchorLocalPos;

    // Running accumulator for the ready-to-harvest bob
    private float _animTime = 0f;

    // Previous frame values — used for change detection
    private int  _prevStage      = -1;
    private bool _wasFullyGrown  = false;

    // Active punch coroutine — stopped before starting a new one
    private Coroutine _punchCoroutine;

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

    private void Start()
    {
        _crop = GetComponent<CropBehavior>();

        // Capture resting position
        _anchorLocalPos = transform.localPosition;

        // Sync initial state so we don't trigger a false growth event on frame 1
        _prevStage     = _crop.CurrentStage;
        _wasFullyGrown = _crop.IsFullyGrown;
    }

    private void Update()
    {
        // Scaled time — freezes during pause
        _animTime += Time.deltaTime;

        bool isFullyGrown = _crop.IsFullyGrown;
        int  currentStage = _crop.CurrentStage;

        // ── Growth Stage Advance Detection ────────────────────────────────────
        // CurrentStage increments by 1 each time GrowthLoop advances.
        // Skip on the very first frame (_prevStage was seeded from current).
        if (currentStage != _prevStage && _prevStage != -1)
        {
            TriggerPunch(_growthPunchStrength, _growthPunchDuration);
        }

        // ── Harvest Detection ─────────────────────────────────────────────────
        // IsFullyGrown flips true→false when Harvest() fires ResetGrowth().
        // The punch fires on the same frame as the sprite reset — gives a "pop" feel.
        if (_wasFullyGrown && !isFullyGrown)
        {
            TriggerPunch(_harvestPunchStrength, _harvestPunchDuration);
        }

        // Update tracked state
        _prevStage     = currentStage;
        _wasFullyGrown = isFullyGrown;

        // ── Position Animation ────────────────────────────────────────────────
        ApplyPositionAnimation(isFullyGrown);
    }

    // =========================================================================
    // POSITION ANIMATION
    // =========================================================================

    private void ApplyPositionAnimation(bool isFullyGrown)
    {
        if (isFullyGrown)
        {
            // Gentle bob — "I'm ready, come harvest me!"
            float yOffset = Mathf.Sin(_animTime * _bobSpeed) * _bobAmplitude;
            transform.localPosition = _anchorLocalPos + new Vector3(0f, yOffset, 0f);
        }
        else
        {
            // Growing — snap back to anchor, no movement
            transform.localPosition = _anchorLocalPos;
        }
    }

    // =========================================================================
    // SCALE PUNCH
    // =========================================================================

    /// <summary>
    /// Fires a scale punch with given parameters.
    /// Stops any in-progress punch first to prevent stacking.
    /// </summary>
    private void TriggerPunch(float strength, float duration)
    {
        if (_punchCoroutine != null)
        {
            StopCoroutine(_punchCoroutine);
            transform.localScale = Vector3.one;
        }

        _punchCoroutine = StartCoroutine(TweenHelper.ScalePunch(transform, strength, duration));
    }

    // =========================================================================
    // CLEANUP
    // =========================================================================

    private void OnDisable()
    {
        // Reset transform state on disable — prevents ghost offset if GameObject is pooled
        transform.localPosition = _anchorLocalPos;
        transform.localScale    = Vector3.one;
    }
}
