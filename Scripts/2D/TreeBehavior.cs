// ============================================================================
// TREEBEHAVIOR.CS
// ============================================================================
// PURPOSE:      Click-and-hold to chop tree. Yields Wood on completion.
//               Requires Axe tool + ToolChargeSystem hold mechanic.
//               Permanent chop by default — trees do not respawn.
// VERSION:      v7 — Charge system integration + permanent chop flag
// UPDATED:      April 9, 2026
// DEPENDENCIES: InventoryManager, AudioManager, ProductionPopupPool,
//               ToolManager, ToolChargeSystem
// ============================================================================

using UnityEngine;
using System.Collections;

public class TreeBehavior : MonoBehaviour
{
    // ========================================================================
    // ⚙️ CONFIGURABLE
    // ========================================================================

    [Header("Harvest Settings")]
    [Tooltip("Wood yielded per chop")]
    [SerializeField] private int woodYield = 1;

    [Header("Cooldown Settings")]
    [Tooltip("Seconds after a chop before the tree can be clicked again (only relevant in respawn mode)")]
    [SerializeField] private float chopCooldown = 1.5f;

    [Header("Chop Behavior")]
    [Tooltip("When true: tree stays permanently chopped (no respawn). When false: tree respawns after respawnTime.")]
    [SerializeField] private bool _permanentChop = true;

    [Tooltip("Seconds before tree respawns. Only used when _permanentChop is false.")]
    [SerializeField] private float respawnTime = 30f;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    // All renderers in the prefab (tree sprite + shadow + any children)
    private SpriteRenderer[] _allRenderers;
    private Collider2D       _collider;

    private bool _isChopped          = false;
    private bool _isOnCooldown       = false;
    private bool _isWaitingForCharge = false; // True while charge ring is filling

    // ========================================================================
    // 🚀 INITIALIZATION
    // ========================================================================

    private void Awake()
    {
        // Grab ALL SpriteRenderers (tree + shadow + any child sprites)
        _allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        _collider     = GetComponent<Collider2D>();
    }

    // ========================================================================
    // 🖱️ INPUT — Hold to charge, release to cancel
    // ========================================================================

    private void OnMouseDown()
    {
        // ---- Guard checks ----
        if (_isChopped)          return;
        if (_isOnCooldown)       return;
        if (_isWaitingForCharge) return; // Already mid-charge on this tree

        // ---- Require Axe tool ----
        if (ToolManager.Instance == null) return;
        if (ToolManager.Instance.ActiveTool != ToolType.Axe)
        {
            Debug.Log("[TreeBehavior] ⚠️ Need Axe tool selected to chop trees.");
            return;
        }

        // ---- Begin charge ----
        if (ToolChargeSystem.Instance == null)
        {
            // No charge system present — execute immediately as a safe fallback
            PerformChop();
            return;
        }

        bool started = ToolChargeSystem.Instance.BeginCharge(
            worldPos:   transform.position,
            onComplete: OnChargeComplete,
            onCancel:   OnChargeCancelled
        );

        if (started)
        {
            _isWaitingForCharge = true;
            Debug.Log("[TreeBehavior] ⏳ Charge started — hold to chop.");
        }
    }

    private void OnMouseUp()
    {
        // Mouse released before ring filled → cancel charge
        if (_isWaitingForCharge)
        {
            ToolChargeSystem.Instance?.CancelCharge();
            // _isWaitingForCharge is cleared in OnChargeCancelled callback
        }
    }

    // ========================================================================
    // ⚡ CHARGE CALLBACKS
    // ========================================================================

    /// <summary>Called by ToolChargeSystem when the hold completes successfully.</summary>
    private void OnChargeComplete()
    {
        _isWaitingForCharge = false;

        // Re-validate before executing — tool or state may have changed during hold
        if (_isChopped)    return;
        if (_isOnCooldown) return;
        if (ToolManager.Instance == null || ToolManager.Instance.ActiveTool != ToolType.Axe) return;

        PerformChop();
    }

    /// <summary>Called by ToolChargeSystem when mouse was released before charge filled.</summary>
    private void OnChargeCancelled()
    {
        _isWaitingForCharge = false;
        Debug.Log("[TreeBehavior] ❌ Chop cancelled — released too early.");
    }

    // ========================================================================
    // 🪓 CHOP EXECUTION
    // ========================================================================

    /// <summary>
    /// Executes the actual chop — adds wood, plays SFX and popup, starts hide coroutine.
    /// Extracted so both the charge callback and the instant-fallback can call it.
    /// </summary>
    private void PerformChop()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[TreeBehavior] ❌ InventoryManager not found!");
            return;
        }

        InventoryManager.Instance.AddResource(GameConstants.Resources.WOOD, woodYield);
        _isChopped = true;

        Debug.Log($"[TreeBehavior] 🪓 Tree chopped! +{woodYield} Wood.");

        // ---- Popup ----
        if (ProductionPopupPool.Instance != null)
        {
            ProductionPopupPool.Instance.ShowPopup(
                $"+{woodYield} Wood",
                transform.position,
                new Color(0.55f, 0.27f, 0.07f)
            );
        }

        // ---- SFX ----
        AudioManager.Instance?.PlaySFX(AudioManager.SFX_CHOP);
        StartCoroutine(PlayTreeFallSFX());

        // ---- Cooldown + hide ----
        StartCoroutine(CooldownCoroutine());
        StartCoroutine(ChopCoroutine());
    }

    // ========================================================================
    // 🌳 TREE FALL SFX
    // ========================================================================

    private IEnumerator PlayTreeFallSFX()
    {
        // Short delay so the chop-impact SFX plays first, then the falling thud
        yield return new WaitForSeconds(0.3f);
        AudioManager.Instance?.PlaySFX(AudioManager.SFX_TREE_FALL);
    }

    // ========================================================================
    // ⏱️ COOLDOWN
    // ========================================================================

    private IEnumerator CooldownCoroutine()
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(chopCooldown);
        _isOnCooldown = false;
    }

    // ========================================================================
    // 🌲 CHOP COROUTINE — Hide tree, optionally respawn
    // ========================================================================

    /// <summary>
    /// Disables all sprites and the collider immediately after chop.
    ///
    /// _permanentChop = true  (default): tree stays hidden. Done.
    /// _permanentChop = false (legacy):  re-enables after respawnTime seconds.
    /// The respawn block is preserved and reachable via Inspector toggle.
    /// </summary>
    private IEnumerator ChopCoroutine()
    {
        // ---- Disable all renderers (tree body + shadow + any child sprites) ----
        if (_allRenderers != null)
        {
            foreach (SpriteRenderer sr in _allRenderers)
            {
                if (sr != null) sr.enabled = false;
            }
        }

        if (_collider != null)
            _collider.enabled = false;

        // ---- PERMANENT CHOP MODE (default) ----
        if (_permanentChop)
        {
            Debug.Log("[TreeBehavior] 🪓 Tree permanently removed.");
            yield break; // Stay gone — _isChopped remains true
        }

        // ---- RESPAWN MODE (legacy — enable via Inspector: _permanentChop = false) ----
        Debug.Log($"[TreeBehavior] 🌱 Tree will respawn in {respawnTime}s.");
        yield return new WaitForSeconds(respawnTime);

        // Re-enable all renderers
        if (_allRenderers != null)
        {
            foreach (SpriteRenderer sr in _allRenderers)
            {
                if (sr != null) sr.enabled = true;
            }
        }

        if (_collider != null)
            _collider.enabled = true;

        _isChopped = false;
        Debug.Log("[TreeBehavior] 🌲 Tree respawned.");
    }
}
