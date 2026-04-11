// ============================================================================
// TUTORIALMANAGER.CS
// ============================================================================
// PURPOSE:      8-step guided tutorial for new players
// VERSION:      v1 — Foundation: steps, skip, PlayerPrefs, event system
// CREATED:      April 1, 2026
// ATTACHED TO:  GameScene → TutorialManager GameObject (singleton)
// ============================================================================
// TUTORIAL STEPS (8 total):
//   Step 0: Welcome + select Build tool
//   Step 1: Place a Corn Field
//   Step 2: Wait for corn to grow, then harvest
//   Step 3: Build a Mash Tun and assign corn
//   Step 4: Build a Still
//   Step 5: Build a Cooperage to make barrels
//   Step 6: Build a Rickhouse to age whiskey
//   Step 7: Build a Saloon and sell your first whiskey!
// ============================================================================
// UI STRUCTURE:
//   GameScene → TutorialPanel [GameObject — assign _tutorialPanel]
//   ├── TutorialText        [TextMeshProUGUI — assign _tutorialText]
//   ├── StepCounter         [TextMeshProUGUI — assign _stepCounterText, e.g. "Step 1 of 8"]
//   ├── ArrowIndicator      [Image — assign _arrowImage]
//   ├── NextButton          [Button — assign _nextButton]  (manual advance)
//   └── SkipButton          [Button — assign _skipButton]
// ============================================================================
// PLAYERPREFS:
//   "TutorialComplete" (int) 1 = done, 0 = not done
//   "TutorialStep"     (int) current step index (for resume on reload)
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static TutorialManager Instance { get; private set; }

    // ========================================================================
    // 📡 EVENTS
    // ========================================================================

    /// <summary>Fires when tutorial step changes. Parameter = new step index.</summary>
    public static event Action<int> OnStepChanged;

    /// <summary>Fires when tutorial is completed or skipped.</summary>
    public static event Action OnTutorialComplete;

    // ========================================================================
    // ⚙️ INSPECTOR — UI REFERENCES
    // ========================================================================

    [Header("📋 UI References")]
    [Tooltip("Root panel GameObject — shown during tutorial, hidden after")]
    [SerializeField] private GameObject _tutorialPanel;

    [Tooltip("TextMeshProUGUI displaying the current step instruction")]
    [SerializeField] private TextMeshProUGUI _tutorialText;

    [Tooltip("TextMeshProUGUI showing step progress e.g. 'Step 2 of 8'")]
    [SerializeField] private TextMeshProUGUI _stepCounterText;

    [Tooltip("Arrow image pointing to relevant UI element (optional)")]
    [SerializeField] private Image _arrowImage;

    [Tooltip("Button to manually advance to next step")]
    [SerializeField] private Button _nextButton;

    [Tooltip("Button to skip the entire tutorial")]
    [SerializeField] private Button _skipButton;

    // ========================================================================
    // ⚙️ INSPECTOR — STEP CONFIGURATION
    // ========================================================================

    [Header("📝 Tutorial Steps")]
    [Tooltip("Tutorial step instructions (one string per step)")]
    [TextArea(2, 4)]
    [SerializeField] private string[] _steps = new string[]
    {
        "🥃 Welcome to Whiskeyville! You're about to build a distillery empire.\n\nPress [B] or tap the Build button to open your building menu.",
        "🌽 Place a Corn Field on the green land.\n\nSelect 'Corn Field' from the build panel, then tap a green tile to plant it.",
        "🌾 Your corn is growing! Come back when it's fully grown — then click it to harvest.\n\nLook for the golden shimmer when it's ready.",
        "🪣 Build a Mash Tun to process your corn into mash.\n\nThe Mash Tun automatically uses your corn. Just build it and watch it work!",
        "🔥 Build a Still to distill your mash into whiskey.\n\nThe Still needs mash from your Mash Tun. Place it and let the chain flow.",
        "🪵 Build a Cooperage to make barrels.\n\nYou'll need wood (chop trees with the Axe tool) to make barrels for aging.",
        "🏚️ Build a Rickhouse to age your whiskey.\n\nWhiskey + Barrel go in → Aged Whiskey comes out. Patience makes it better.",
        "🍺 Build a Saloon and sell your first whiskey!\n\nClick the Saloon to open the sell panel. This is what it's all about.\n\nYou're a distiller now. Welcome to the craft. 🥃"
    };

    [Header("⏱️ Settings")]
    [Tooltip("If true, tutorial auto-starts on new game when not previously completed")]
    [SerializeField] private bool _autoStartOnNewGame = true;

    [Tooltip("If true, skip button is always visible")]
    [SerializeField] private bool _showSkipButton = true;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private int _currentStep = 0;
    private bool _isActive = false;

    private const string PREFS_COMPLETE = "TutorialComplete";
    private const string PREFS_STEP     = "TutorialStep";

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[TutorialManager] ✅ Initialized.");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // ---- BIND BUTTONS ----
        if (_nextButton != null) _nextButton.onClick.AddListener(AdvanceStep);
        if (_skipButton != null) _skipButton.onClick.AddListener(SkipTutorial);

        // ---- CONFIGURE SKIP BUTTON VISIBILITY ----
        if (_skipButton != null)
            _skipButton.gameObject.SetActive(_showSkipButton);

        // ---- HIDE PANEL INITIALLY ----
        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(false);

        // ---- AUTO-START ----
        if (_autoStartOnNewGame && !IsTutorialComplete())
        {
            // Resume from saved step if player quit mid-tutorial
            int savedStep = PlayerPrefs.GetInt(PREFS_STEP, 0);
            StartTutorial(savedStep);
        }
    }

    private void OnDestroy()
    {
        if (_nextButton != null) _nextButton.onClick.RemoveListener(AdvanceStep);
        if (_skipButton != null) _skipButton.onClick.RemoveListener(SkipTutorial);
    }

    // ========================================================================
    // 🎓 PUBLIC — TUTORIAL CONTROL
    // ========================================================================

    /// <summary>
    /// Starts the tutorial from a specific step (default 0 = beginning).
    /// </summary>
    public void StartTutorial(int fromStep = 0)
    {
        if (_steps == null || _steps.Length == 0)
        {
            Debug.LogWarning("[TutorialManager] ⚠️ No tutorial steps configured!");
            return;
        }

        _isActive = true;
        _currentStep = Mathf.Clamp(fromStep, 0, _steps.Length - 1);

        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(true);

        DisplayCurrentStep();

        Debug.Log($"[TutorialManager] 🎓 Tutorial started from step {_currentStep}.");
    }

    /// <summary>
    /// Advances to the next tutorial step. Completes tutorial if on last step.
    /// </summary>
    public void AdvanceStep()
    {
        if (!_isActive) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);

        _currentStep++;
        PlayerPrefs.SetInt(PREFS_STEP, _currentStep);

        if (_currentStep >= _steps.Length)
        {
            CompleteTutorial();
        }
        else
        {
            DisplayCurrentStep();
            OnStepChanged?.Invoke(_currentStep);
            Debug.Log($"[TutorialManager] 📖 Advanced to step {_currentStep + 1}/{_steps.Length}.");
        }
    }

    /// <summary>
    /// Skips the entire tutorial — marks complete and hides the panel.
    /// </summary>
    public void SkipTutorial()
    {
        Debug.Log("[TutorialManager] ⏭️ Tutorial skipped.");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);

        CompleteTutorial();
    }

    // ========================================================================
    // 🔧 PRIVATE — DISPLAY + COMPLETION
    // ========================================================================

    private void DisplayCurrentStep()
    {
        if (_steps == null || _currentStep >= _steps.Length) return;

        // ---- UPDATE TEXT ----
        if (_tutorialText != null)
            _tutorialText.text = _steps[_currentStep];

        // ---- UPDATE COUNTER ----
        if (_stepCounterText != null)
            _stepCounterText.text = $"Step {_currentStep + 1} of {_steps.Length}";

        // ---- SHOW/HIDE NEXT BUTTON ----
        // On last step, change button text to "Finish"
        if (_nextButton != null)
        {
            TextMeshProUGUI btnText = _nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = (_currentStep == _steps.Length - 1) ? "Finish!" : "Got it >";
        }
    }

    private void CompleteTutorial()
    {
        _isActive = false;

        // ---- MARK AS COMPLETE ----
        PlayerPrefs.SetInt(PREFS_COMPLETE, 1);
        PlayerPrefs.SetInt(PREFS_STEP, 0);
        PlayerPrefs.Save();

        // ---- HIDE PANEL ----
        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(false);

        // ---- FIRE EVENT ----
        OnTutorialComplete?.Invoke();

        Debug.Log("[TutorialManager] ✅ Tutorial complete!");
    }

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    /// <summary>Returns true if the tutorial has been completed previously.</summary>
    public static bool IsTutorialComplete()
    {
        return PlayerPrefs.GetInt(PREFS_COMPLETE, 0) == 1;
    }

    /// <summary>Resets tutorial completion (useful for testing).</summary>
    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(PREFS_COMPLETE);
        PlayerPrefs.DeleteKey(PREFS_STEP);
        Debug.Log("[TutorialManager] 🔄 Tutorial reset.");
    }

    public bool IsActive => _isActive;
    public int CurrentStep => _currentStep;
    public int TotalSteps => _steps != null ? _steps.Length : 0;
}
