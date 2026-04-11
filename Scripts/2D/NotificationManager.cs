// ============================================================================
// NOTIFICATIONMANAGER.CS
// ============================================================================
// PURPOSE:      Toast notification system — non-blocking popups for events,
//               achievements, fame gains, errors, and info messages
// VERSION:      v2 — Auto-creates UI at runtime (no prefab/container needed)
// UPDATED:      April 5, 2026
// ARCHITECTURE: Singleton (BaseSingleton), queue-based, auto-dismiss
// ============================================================================
// USAGE:
//   NotificationManager.Instance.Show("Corn harvested!", NotificationType.Info);
//   NotificationManager.Instance.Show("5-Star Whiskey!", NotificationType.Success);
//   NotificationManager.Instance.ShowError("Not enough cash!");
//   NotificationManager.Instance.ShowFame(50, "Sold premium whiskey");
//   NotificationManager.Instance.ShowAchievement("First Batch", "Distill whiskey");
// ============================================================================
// NO SETUP REQUIRED — UI is auto-created at runtime.
// If you WANT manual control, assign _notificationContainer in inspector.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum NotificationType
{
    Info,       // Blue
    Success,    // Green
    Warning,    // Orange
    Error,      // Red
    Fame,       // Gold
    Achievement // Purple
}

public class NotificationManager : BaseSingleton<NotificationManager>
{
    // ========================================================================
    // INSPECTOR
    // ========================================================================

    [Header("Settings")]
    [Tooltip("How long each notification stays visible")]
    [SerializeField] private float _displayDuration = 3.5f;

    [Tooltip("Fade in/out duration")]
    [SerializeField] private float _fadeDuration = 0.3f;

    [Tooltip("Maximum notifications visible at once")]
    [SerializeField] private int _maxVisible = 3;

    [Tooltip("Notification width in pixels")]
    [SerializeField] private float _notificationWidth = 380f;

    [Tooltip("Notification height in pixels")]
    [SerializeField] private float _notificationHeight = 70f;

    // ========================================================================
    // STATE
    // ========================================================================

    private readonly Queue<NotificationData> _queue = new Queue<NotificationData>();
    private readonly List<GameObject> _activeNotifications = new List<GameObject>();
    private bool _isProcessing = false;

    // Auto-created UI
    private Canvas _notifCanvas;
    private RectTransform _container;

    private struct NotificationData
    {
        public string Title;
        public string Message;
        public NotificationType Type;
    }

    // ========================================================================
    // SINGLETON
    // ========================================================================

    protected override bool Persistent => true;

    protected override void OnSingletonAwake()
    {
        CreateNotificationUI();
    }

    // ========================================================================
    // PUBLIC API
    // ========================================================================

    public void Show(string message, NotificationType type = NotificationType.Info)
    {
        Show("", message, type);
    }

    public void Show(string title, string message, NotificationType type)
    {
        _queue.Enqueue(new NotificationData { Title = title, Message = message, Type = type });

        if (!_isProcessing)
            StartCoroutine(ProcessQueue());
    }

    public void ShowFame(int amount, string reason = "")
    {
        string msg = $"+{amount} Fame";
        if (!string.IsNullOrEmpty(reason)) msg += $" — {reason}";
        Show("Fame Gained", msg, NotificationType.Fame);
    }

    public void ShowAchievement(string achievementName, string description)
    {
        Show($"Achievement: {achievementName}", description, NotificationType.Achievement);
    }

    public void ShowError(string message)
    {
        Show("", message, NotificationType.Error);
    }

    // ========================================================================
    // QUEUE PROCESSING
    // ========================================================================

    private IEnumerator ProcessQueue()
    {
        _isProcessing = true;

        while (_queue.Count > 0)
        {
            // Remove oldest if at max
            while (_activeNotifications.Count >= _maxVisible)
            {
                if (_activeNotifications.Count > 0 && _activeNotifications[0] != null)
                {
                    StartCoroutine(DismissNotification(_activeNotifications[0]));
                }
                _activeNotifications.RemoveAt(0);
                yield return new WaitForSecondsRealtime(0.1f);
            }

            NotificationData data = _queue.Dequeue();
            SpawnNotification(data);

            yield return new WaitForSecondsRealtime(0.3f);
        }

        _isProcessing = false;
    }

    // ========================================================================
    // SPAWN & DISMISS — Creates notification UI inline (no prefab needed)
    // ========================================================================

    private void SpawnNotification(NotificationData data)
    {
        if (_container == null)
        {
            Debug.Log($"[Notification] [{data.Type}] {data.Title} — {data.Message}");
            return;
        }

        // ---- CREATE NOTIFICATION GAMEOBJECT ----
        GameObject notifObj = new GameObject($"Notif_{data.Type}");
        notifObj.transform.SetParent(_container, false);

        RectTransform notifRT = notifObj.AddComponent<RectTransform>();
        notifRT.sizeDelta = new Vector2(_notificationWidth, _notificationHeight);

        // Background
        Image bg = notifObj.AddComponent<Image>();
        bg.color = GetTypeColor(data.Type);

        // Round corners via sprite slicing isn't available at runtime,
        // so we use a slight gradient effect by adding a darker border
        GameObject border = new GameObject("Border");
        border.transform.SetParent(notifObj.transform, false);
        RectTransform borderRT = border.AddComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(3, 3);
        borderRT.offsetMax = new Vector2(-3, -3);
        Image borderImg = border.AddComponent<Image>();
        Color innerColor = GetTypeColor(data.Type);
        innerColor.r *= 0.85f; innerColor.g *= 0.85f; innerColor.b *= 0.85f;
        borderImg.color = innerColor;

        // Type icon/emoji text (left side)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(notifObj.transform, false);
        RectTransform iconRT = iconObj.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0);
        iconRT.anchorMax = new Vector2(0, 1);
        iconRT.offsetMin = new Vector2(8, 5);
        iconRT.offsetMax = new Vector2(40, -5);
        iconRT.pivot = new Vector2(0, 0.5f);
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = GetTypeIcon(data.Type);
        iconText.fontSize = 24;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = Color.white;

        // Title text
        bool hasTitle = !string.IsNullOrEmpty(data.Title);
        if (hasTitle)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(notifObj.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(45, 2);
            titleRT.offsetMax = new Vector2(-10, -5);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = data.Title;
            titleText.fontSize = 15;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = Color.white;
            titleText.enableWordWrapping = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
        }

        // Message text
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(notifObj.transform, false);
        RectTransform msgRT = msgObj.AddComponent<RectTransform>();
        if (hasTitle)
        {
            msgRT.anchorMin = new Vector2(0, 0);
            msgRT.anchorMax = new Vector2(1, 0.5f);
            msgRT.offsetMin = new Vector2(45, 5);
            msgRT.offsetMax = new Vector2(-10, -2);
        }
        else
        {
            msgRT.anchorMin = new Vector2(0, 0);
            msgRT.anchorMax = new Vector2(1, 1);
            msgRT.offsetMin = new Vector2(45, 5);
            msgRT.offsetMax = new Vector2(-10, -5);
        }
        TextMeshProUGUI msgText = msgObj.AddComponent<TextMeshProUGUI>();
        msgText.text = data.Message;
        msgText.fontSize = hasTitle ? 13 : 14;
        msgText.alignment = hasTitle ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left;
        msgText.color = new Color(1f, 1f, 1f, 0.9f);
        msgText.enableWordWrapping = true;
        msgText.overflowMode = TextOverflowModes.Ellipsis;

        // ---- CANVAS GROUP FOR FADE ----
        CanvasGroup cg = notifObj.AddComponent<CanvasGroup>();

        // ---- ANIMATE IN ----
        _activeNotifications.Add(notifObj);
        StartCoroutine(AnimateIn(notifObj, cg));
        StartCoroutine(AutoDismiss(notifObj, _displayDuration));

        // ---- SFX ----
        PlayTypeSFX(data.Type);

        Debug.Log($"[NotificationManager] Spawned: [{data.Type}] {data.Title} {data.Message}");
    }

    private IEnumerator AnimateIn(GameObject obj, CanvasGroup cg)
    {
        if (cg == null || obj == null) yield break;

        RectTransform rt = obj.GetComponent<RectTransform>();
        cg.alpha = 0f;

        // Slide in from right + fade
        float startX = 100f;
        Vector2 startPos = rt.anchoredPosition + new Vector2(startX, 0);
        Vector2 endPos = rt.anchoredPosition;

        float elapsed = 0f;
        float duration = _fadeDuration;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Ease out cubic
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            cg.alpha = ease;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
            yield return null;
        }

        cg.alpha = 1f;
        rt.anchoredPosition = endPos;
    }

    private IEnumerator AutoDismiss(GameObject instance, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (instance != null)
        {
            _activeNotifications.Remove(instance);
            yield return StartCoroutine(DismissNotification(instance));
        }
    }

    private IEnumerator DismissNotification(GameObject instance)
    {
        if (instance == null) yield break;

        CanvasGroup cg = instance.GetComponent<CanvasGroup>();
        RectTransform rt = instance.GetComponent<RectTransform>();
        Vector2 startPos = rt != null ? rt.anchoredPosition : Vector2.zero;

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            if (instance == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _fadeDuration;
            if (cg != null) cg.alpha = 1f - t;
            if (rt != null) rt.anchoredPosition = startPos + new Vector2(50f * t, 0);
            yield return null;
        }

        if (instance != null)
            Destroy(instance);
    }

    // ========================================================================
    // AUTO-CREATE UI — No prefab or manual setup needed
    // ========================================================================

    private void CreateNotificationUI()
    {
        // Create a dedicated canvas for notifications
        GameObject canvasObj = new GameObject("NotificationCanvas");
        canvasObj.transform.SetParent(transform);
        _notifCanvas = canvasObj.AddComponent<Canvas>();
        _notifCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _notifCanvas.sortingOrder = 9000; // Above game, below dev console

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Container — top-right, vertical layout, grows downward
        GameObject containerObj = new GameObject("NotificationContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);
        _container = containerObj.AddComponent<RectTransform>();
        _container.anchorMin = new Vector2(1, 1);
        _container.anchorMax = new Vector2(1, 1);
        _container.pivot = new Vector2(1, 1);
        _container.anchoredPosition = new Vector2(-15, -15);
        _container.sizeDelta = new Vector2(_notificationWidth, 0);

        VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperRight;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        ContentSizeFitter fitter = containerObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Debug.Log("[NotificationManager] UI auto-created (top-right corner).");
    }

    // ========================================================================
    // TYPE COLORS, ICONS & SFX
    // ========================================================================

    private static Color GetTypeColor(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info       => new Color(0.15f, 0.35f, 0.6f, 0.95f),
            NotificationType.Success    => new Color(0.15f, 0.5f, 0.15f, 0.95f),
            NotificationType.Warning    => new Color(0.7f, 0.5f, 0.1f, 0.95f),
            NotificationType.Error      => new Color(0.6f, 0.15f, 0.15f, 0.95f),
            NotificationType.Fame       => new Color(0.7f, 0.55f, 0.0f, 0.95f),
            NotificationType.Achievement => new Color(0.45f, 0.15f, 0.65f, 0.95f),
            _ => new Color(0.25f, 0.25f, 0.3f, 0.95f)
        };
    }

    private static string GetTypeIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info       => "i",
            NotificationType.Success    => "OK",
            NotificationType.Warning    => "!",
            NotificationType.Error      => "X",
            NotificationType.Fame       => "F",
            NotificationType.Achievement => "*",
            _ => ">"
        };
    }

    private static void PlayTypeSFX(NotificationType type)
    {
        if (AudioManager.Instance == null) return;

        switch (type)
        {
            case NotificationType.Success:
            case NotificationType.Achievement:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_SUCCESS);
                break;
            case NotificationType.Error:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
                break;
            case NotificationType.Fame:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_CASH);
                break;
            default:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
                break;
        }
    }
}
