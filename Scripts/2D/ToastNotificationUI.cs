// ============================================================================
// TOASTNOTIFICATIONUI.CS
// ============================================================================
// PURPOSE:      Shows brief "Game Saved ✓" (and other) toasts on screen
// VERSION:      v1 — CanvasGroup fade, event-driven, queued messages
// CREATED:      April 1, 2026
// ATTACHED TO:  GameScene → HUD Canvas → ToastPanel
// ============================================================================
// UI STRUCTURE:
//   HUD Canvas
//   └── ToastPanel          [CanvasGroup — assign _toastGroup]
//       └── ToastText       [TextMeshProUGUI — assign _toastText]
//
// POSITION:    Anchor to bottom-center of screen, ~150px above bottom edge
// SIZE:        ~400w × 60h, flexible height preferred
// ============================================================================
// SUBSCRIBES TO:
//   SaveManager.OnGameSaved → shows "Game Saved ✓"
//   Call ShowToast() directly from any script for custom messages
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ToastNotificationUI : MonoBehaviour
{
    // ========================================================================
    // ⚙️ INSPECTOR
    // ========================================================================

    [Header("📋 UI References")]
    [Tooltip("CanvasGroup on the toast panel (controls alpha)")]
    [SerializeField] private CanvasGroup _toastGroup;

    [Tooltip("TextMeshProUGUI for the toast message")]
    [SerializeField] private TextMeshProUGUI _toastText;

    [Header("⏱️ Timing")]
    [Tooltip("Seconds the toast is fully visible")]
    [SerializeField] private float _holdDuration = 2f;

    [Tooltip("Seconds for fade in")]
    [SerializeField] private float _fadeInDuration = 0.25f;

    [Tooltip("Seconds for fade out")]
    [SerializeField] private float _fadeOutDuration = 0.5f;

    [Header("🎨 Appearance")]
    [Tooltip("Default text color for toast messages")]
    [SerializeField] private Color _defaultColor = new Color(0.95f, 0.90f, 0.80f, 1f); // warm cream

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Queue<ToastMessage> _queue = new Queue<ToastMessage>();
    private bool _isShowing = false;
    private Coroutine _showCoroutine;

    private struct ToastMessage
    {
        public string text;
        public Color color;
    }

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        // Start hidden
        if (_toastGroup != null)
        {
            _toastGroup.alpha = 0f;
            _toastGroup.interactable = false;
            _toastGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        // Subscribe to save events
        SaveManager.OnGameSaved += OnGameSaved;
    }

    private void OnDisable()
    {
        SaveManager.OnGameSaved -= OnGameSaved;
    }

    // ========================================================================
    // 📡 EVENT HANDLERS
    // ========================================================================

    private void OnGameSaved()
    {
        ShowToast("Game Saved ✓", new Color(0.4f, 0.9f, 0.5f, 1f)); // soft green
    }

    // ========================================================================
    // 📢 PUBLIC — SHOW TOAST
    // ========================================================================

    /// <summary>
    /// Queues a toast message with default color.
    /// </summary>
    public void ShowToast(string message)
    {
        ShowToast(message, _defaultColor);
    }

    /// <summary>
    /// Queues a toast message with custom color.
    /// Messages queue up and display sequentially.
    /// </summary>
    public void ShowToast(string message, Color color)
    {
        _queue.Enqueue(new ToastMessage { text = message, color = color });

        if (!_isShowing)
        {
            if (_showCoroutine != null) StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(ProcessQueue());
        }
    }

    // ========================================================================
    // 🔄 COROUTINE — PROCESS QUEUE
    // ========================================================================

    private IEnumerator ProcessQueue()
    {
        _isShowing = true;

        while (_queue.Count > 0)
        {
            ToastMessage msg = _queue.Dequeue();
            yield return StartCoroutine(ShowSingle(msg.text, msg.color));

            // Brief gap between sequential toasts
            if (_queue.Count > 0)
                yield return new WaitForSeconds(0.1f);
        }

        _isShowing = false;
    }

    private IEnumerator ShowSingle(string message, Color color)
    {
        // ---- SET CONTENT ----
        if (_toastText != null)
        {
            _toastText.text = message;
            _toastText.color = color;
        }

        // ---- FADE IN ----
        float t = 0f;
        while (t < _fadeInDuration)
        {
            t += Time.deltaTime;
            if (_toastGroup != null)
                _toastGroup.alpha = Mathf.Clamp01(t / _fadeInDuration);
            yield return null;
        }
        if (_toastGroup != null) _toastGroup.alpha = 1f;

        // ---- HOLD ----
        yield return new WaitForSeconds(_holdDuration);

        // ---- FADE OUT ----
        t = 0f;
        while (t < _fadeOutDuration)
        {
            t += Time.deltaTime;
            if (_toastGroup != null)
                _toastGroup.alpha = Mathf.Clamp01(1f - (t / _fadeOutDuration));
            yield return null;
        }
        if (_toastGroup != null) _toastGroup.alpha = 0f;
    }
}
