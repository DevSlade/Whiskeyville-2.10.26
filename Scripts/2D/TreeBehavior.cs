// ============================================================================
// TREEBEHAVIOR.CS
// ============================================================================
// PURPOSE:      Click-to-chop tree that yields Wood and respawns after delay
// VERSION:      v2 — Added respawn system
// UPDATED:      February 12, 2026
// DEPENDENCIES: InventoryManager, AudioManager, ProductionPopupPool
// ============================================================================

using UnityEngine;
using System.Collections;

public class TreeBehavior : MonoBehaviour
{
    // ========================================================================
    // ⚙️ CONFIGURABLE
    // ========================================================================

    [Header("Harvest Settings")]
    [SerializeField] private int woodYield = 1;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnTime = 30f;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private bool _isChopped = false;

    // ========================================================================
    // 🚀 INITIALIZATION
    // ========================================================================

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    // ========================================================================
    // 🖱️ CLICK TO CHOP
    // ========================================================================

    private void OnMouseDown()
    {
        if (_isChopped) return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[TreeBehavior] ❌ InventoryManager not found!");
            return;
        }

        // ====================================================================
        // HARVEST — Add wood to inventory
        // ====================================================================

        InventoryManager.Instance.AddResource("Wood", woodYield);
        _isChopped = true;

        Debug.Log($"[TreeBehavior] 🪓 Tree chopped! +{woodYield} Wood.");

        // ====================================================================
        // FEEDBACK — Popup + Sound
        // ====================================================================

        if (ProductionPopupPool.Instance != null)
        {
            string text = $"+{woodYield} Wood";
            ProductionPopupPool.Instance.ShowPopup(text, transform.position, new Color(0.55f, 0.27f, 0.07f));
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_COLLECT);
        }

        // ====================================================================
        // RESPAWN — Hide tree, start timer, regrow
        // ====================================================================

        StartCoroutine(RespawnCoroutine());
    }

    // ========================================================================
    // 🌱 RESPAWN COROUTINE
    // ========================================================================

    private IEnumerator RespawnCoroutine()
    {
        // Hide the tree
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        if (_collider != null)
            _collider.enabled = false;

        Debug.Log($"[TreeBehavior] 🌱 Tree will respawn in {respawnTime} seconds...");

        yield return new WaitForSeconds(respawnTime);

        // Regrow the tree
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = true;

        if (_collider != null)
            _collider.enabled = true;

        _isChopped = false;

        Debug.Log("[TreeBehavior] 🌲 Tree has respawned!");
    }
}