// ============================================================================
// GAMEEVENTNOTIFIER.CS
// ============================================================================
// PURPOSE:      Bridges game events to the NotificationManager
//               Subscribes to SaveManager, FameManager, DayNightCycle events
//               and sends appropriate toast notifications
// VERSION:      v1 — Foundation
// CREATED:      April 5, 2026
// ATTACHED TO:  Same GameObject as NotificationManager (or any persistent obj)
// DEPENDENCIES: NotificationManager, SaveManager, FameManager, DayNightCycle
// ============================================================================
// DEV GUIDE:
//   To add a new notification trigger:
//     1. Subscribe to the event in OnEnable()
//     2. Unsubscribe in OnDisable()
//     3. Create a handler method that calls NotificationManager.Instance.Show()
// ============================================================================

using UnityEngine;

public class GameEventNotifier : MonoBehaviour
{
    // ========================================================================
    // SINGLETON
    // ========================================================================

    public static GameEventNotifier Instance { get; private set; }

    // ========================================================================
    // INSPECTOR
    // ========================================================================

    [Header("Notification Toggles")]
    [Tooltip("Show 'Game Saved' notification on auto-save")]
    [SerializeField] private bool _notifyOnSave = true;

    [Tooltip("Show notifications on fame tier ups")]
    [SerializeField] private bool _notifyOnTierUp = true;

    [Tooltip("Show day/night transition notifications")]
    [SerializeField] private bool _notifyOnDayNight = false; // Off by default — can be noisy

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Subscribe to game events
        SaveManager.OnGameSaved += OnGameSaved;
        FameManager.OnTierUp += OnFameTierUp;
        FameManager.OnFameChanged += OnFameChanged;
        DayNightCycle.OnDayStart += OnDayStart;
        DayNightCycle.OnNightStart += OnNightStart;

        Debug.Log("[GameEventNotifier] Subscribed to game events.");
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SaveManager.OnGameSaved -= OnGameSaved;
        FameManager.OnTierUp -= OnFameTierUp;
        FameManager.OnFameChanged -= OnFameChanged;
        DayNightCycle.OnDayStart -= OnDayStart;
        DayNightCycle.OnNightStart -= OnNightStart;
    }

    // ========================================================================
    // EVENT HANDLERS
    // ========================================================================

    private void OnGameSaved()
    {
        if (!_notifyOnSave) return;
        if (NotificationManager.Instance == null) return;

        NotificationManager.Instance.Show("Game Saved", "Your progress has been saved.", NotificationType.Success);
    }

    private void OnFameTierUp(FameTier newTier)
    {
        if (!_notifyOnTierUp) return;
        if (NotificationManager.Instance == null) return;

        string tierName = FameManager.Instance != null ? FameManager.Instance.TierName : newTier.ToString();
        NotificationManager.Instance.Show(
            "TIER UP!",
            $"You are now a {tierName}!",
            NotificationType.Achievement
        );
    }

    private void OnFameChanged(int totalFame, int delta)
    {
        // Only notify on gains (not on set operations or decreases)
        if (delta <= 0) return;
        if (NotificationManager.Instance == null) return;

        // Small gains (< 5) are too noisy — skip them
        // TierUp already handles the big celebration notification
        // This just shows the "+X Fame" toast
        if (delta >= 5)
        {
            NotificationManager.Instance.ShowFame(delta, "");
        }
    }

    private void OnDayStart()
    {
        if (!_notifyOnDayNight) return;
        if (NotificationManager.Instance == null) return;

        NotificationManager.Instance.Show("A new day dawns over Whiskeyville.", NotificationType.Info);
    }

    private void OnNightStart()
    {
        if (!_notifyOnDayNight) return;
        if (NotificationManager.Instance == null) return;

        NotificationManager.Instance.Show("Night falls. The crickets are singing.", NotificationType.Info);
    }
}
