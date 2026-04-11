// ============================================================================
// MAINMENUMANAGER.CS
// ============================================================================
// PURPOSE:       Controls main menu UI and scene transitions
// VERSION:       v2 — Added Continue button, NewGame confirm, Settings stub
// UPDATED:       April 1, 2026
// ATTACHED TO:   MainMenu scene → MainMenuCanvas
// DEPENDENCIES:  AudioManager, SaveManager
// ============================================================================
// UI STRUCTURE:
//   MainMenuCanvas
//   ├── TitleText ("Whiskeyville")
//   ├── ContinueButton  → ContinueGame()    [hidden if no save file]
//   ├── PlayButton      → StartNewGame()
//   ├── SettingsButton  → OpenSettings()
//   ├── QuitButton      → QuitGame()
//   ├── SettingsPanel   → SettingsPanelUI.cs
//   └── ConfirmNewGamePanel
//       ├── ConfirmText ("Start over? Your progress will be lost.")
//       ├── ConfirmYesButton → ConfirmNewGame()
//       └── ConfirmNoButton  → CancelNewGame()
// ============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // ========================================================================
    // 🎮 INSPECTOR - BUTTONS
    // ========================================================================

    [Header("🎮 Menu Buttons")]
    [Tooltip("Button to continue from a saved game (hidden if no save exists)")]
    [SerializeField] private Button _continueButton;

    [Tooltip("Button to start a new game")]
    [SerializeField] private Button _playButton;

    [Tooltip("Button to open settings panel")]
    [SerializeField] private Button _settingsButton;

    [Tooltip("Button to quit the game")]
    [SerializeField] private Button _quitButton;

    // ========================================================================
    // 🪟 INSPECTOR - PANELS
    // ========================================================================

    [Header("🪟 Panels")]
    [Tooltip("Panel shown when player clicks New Game but a save exists")]
    [SerializeField] private GameObject _confirmNewGamePanel;

    [Tooltip("Settings panel (controlled by SettingsPanelUI)")]
    [SerializeField] private GameObject _settingsPanel;

    [Tooltip("Confirm: Yes, start over")]
    [SerializeField] private Button _confirmYesButton;

    [Tooltip("Confirm: No, go back")]
    [SerializeField] private Button _confirmNoButton;

    // ========================================================================
    // 🎬 INSPECTOR - SCENE NAMES
    // ========================================================================

    [Header("🎬 Scene Names")]
    [Tooltip("Name of the gameplay scene to load")]
    [SerializeField] private string _gameSceneName = "GameScene";

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        // ---- 🔗 BIND BUTTON EVENTS ----
        if (_continueButton != null)  _continueButton.onClick.AddListener(ContinueGame);
        if (_playButton != null)      _playButton.onClick.AddListener(StartNewGame);
        if (_settingsButton != null)  _settingsButton.onClick.AddListener(OpenSettings);
        if (_quitButton != null)      _quitButton.onClick.AddListener(QuitGame);
        if (_confirmYesButton != null) _confirmYesButton.onClick.AddListener(ConfirmNewGame);
        if (_confirmNoButton != null)  _confirmNoButton.onClick.AddListener(CancelNewGame);

        // ---- 🔄 SETUP CONTINUE BUTTON VISIBILITY ----
        RefreshContinueButton();

        // ---- 🪟 ENSURE PANELS START CLOSED ----
        if (_confirmNewGamePanel != null) _confirmNewGamePanel.SetActive(false);
        if (_settingsPanel != null)       _settingsPanel.SetActive(false);

        // ---- 🎵 PLAY MENU MUSIC ----
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.MUSIC_MENU);
        }

        Debug.Log("[MainMenuManager] ✅ Main menu initialized.");
    }

    private void OnDestroy()
    {
        // ---- 🔗 UNBIND BUTTON EVENTS ----
        if (_continueButton != null)   _continueButton.onClick.RemoveListener(ContinueGame);
        if (_playButton != null)       _playButton.onClick.RemoveListener(StartNewGame);
        if (_settingsButton != null)   _settingsButton.onClick.RemoveListener(OpenSettings);
        if (_quitButton != null)       _quitButton.onClick.RemoveListener(QuitGame);
        if (_confirmYesButton != null) _confirmYesButton.onClick.RemoveListener(ConfirmNewGame);
        if (_confirmNoButton != null)  _confirmNoButton.onClick.RemoveListener(CancelNewGame);
    }

    // ========================================================================
    // 🔄 CONTINUE BUTTON VISIBILITY
    // ========================================================================

    /// <summary>
    /// Shows Continue button only if a save file exists.
    /// Called on Start and after save is deleted.
    /// </summary>
    private void RefreshContinueButton()
    {
        if (_continueButton == null) return;

        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();
        _continueButton.gameObject.SetActive(hasSave);

        Debug.Log($"[MainMenuManager] 💾 Save file found: {hasSave} — Continue button {(hasSave ? "shown" : "hidden")}.");
    }

    // ========================================================================
    // 🎮 BUTTON HANDLERS
    // ========================================================================

    /// <summary>
    /// Continues from the last saved game.
    /// Registers a callback so LoadGame() fires AFTER the scene finishes loading.
    /// </summary>
    public void ContinueGame()
    {
        Debug.Log("[MainMenuManager] Continue clicked. Loading saved game...");
        PlayClickSFX();

        // Flag SaveManager to do a full load after the game scene initializes
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.PendingLoad = true;
        }

        SceneManager.LoadScene(_gameSceneName);
    }

    /// <summary>
    /// Called when Play / New Game button is clicked.
    /// If a save exists, shows confirm panel. Otherwise starts immediately.
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("[MainMenuManager] 🆕 New Game clicked.");
        PlayClickSFX();

        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();

        if (hasSave)
        {
            // ---- ⚠️ SHOW CONFIRM DIALOG ----
            if (_confirmNewGamePanel != null)
            {
                _confirmNewGamePanel.SetActive(true);
                Debug.Log("[MainMenuManager] ⚠️ Showing New Game confirm dialog.");
            }
        }
        else
        {
            // ---- ▶️ NO SAVE — START IMMEDIATELY ----
            LoadGameScene();
        }
    }

    /// <summary>
    /// Player confirmed they want to erase save and start fresh.
    /// </summary>
    public void ConfirmNewGame()
    {
        Debug.Log("[MainMenuManager] ✅ New Game confirmed. Deleting save and starting fresh.");
        PlayClickSFX();

        // Delete existing save
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }

        if (_confirmNewGamePanel != null) _confirmNewGamePanel.SetActive(false);

        LoadGameScene();
    }

    /// <summary>
    /// Player cancelled — go back to main menu without starting new game.
    /// </summary>
    public void CancelNewGame()
    {
        Debug.Log("[MainMenuManager] ❌ New Game cancelled.");
        PlayClickSFX();

        if (_confirmNewGamePanel != null) _confirmNewGamePanel.SetActive(false);
    }

    /// <summary>
    /// Opens the settings panel.
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("[MainMenuManager] ⚙️ Settings opened.");
        PlayClickSFX();

        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Closes the settings panel (call from the panel's Close button).
    /// </summary>
    public void CloseSettings()
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MainMenuManager] 🚪 Quit clicked. Exiting...");
        PlayClickSFX();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ========================================================================
    // 🔧 HELPERS
    // ========================================================================

    private void LoadGameScene()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    private void PlayClickSFX()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
        }
    }
}
