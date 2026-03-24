// ============================================================================
// MAINMENUMANAGER.CS
// ============================================================================
// PURPOSE:       Controls main menu UI and scene transitions
// ATTACHED TO:  MainMenu scene → MainMenuCanvas
// DEPENDENCIES: AudioManager (for menu music and button SFX)
// ============================================================================
// UI STRUCTURE:
//   MainMenuCanvas
//   ├── TitleText ("Whiskeyville")
//   ├── PlayButton → OnClick:  PlayGame()
//   ├── SettingsButton → OnClick: OpenSettings() (future)
//   └── QuitButton → OnClick: QuitGame()
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
    [Tooltip("Button to start the game")]
    [SerializeField] private Button _playButton;

    [Tooltip("Button to open settings (future)")]
    [SerializeField] private Button _settingsButton;

    [Tooltip("Button to quit the game")]
    [SerializeField] private Button _quitButton;

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
        if (_playButton != null)
        {
            _playButton.onClick.AddListener(PlayGame);
        }

        if (_settingsButton != null)
        {
            _settingsButton.onClick. AddListener(OpenSettings);
        }

        if (_quitButton != null)
        {
            _quitButton.onClick.AddListener(QuitGame);
        }

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
        if (_playButton != null)
        {
            _playButton.onClick.RemoveListener(PlayGame);
        }

        if (_settingsButton != null)
        {
            _settingsButton.onClick.RemoveListener(OpenSettings);
        }

        if (_quitButton != null)
        {
            _quitButton.onClick.RemoveListener(QuitGame);
        }
    }

    // ========================================================================
    // 🎮 BUTTON HANDLERS
    // ========================================================================

    /// <summary>
    /// Called when Play button is clicked.
    /// Loads the gameplay scene. 
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("[MainMenuManager] ▶️ Play button clicked.  Loading game...");

        // ---- 🔊 PLAY CLICK SFX ----
        if (AudioManager.Instance != null)
        {
            AudioManager. Instance.PlaySFX(AudioManager.SFX_CLICK);
        }

        // ---- 🎬 LOAD GAME SCENE ----
        SceneManager.LoadScene(_gameSceneName);
    }

    /// <summary>
    /// Called when Settings button is clicked.
    /// Opens settings panel (future implementation).
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("[MainMenuManager] ⚙️ Settings button clicked. (Not implemented yet)");

        // ---- 🔊 PLAY CLICK SFX ----
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
        }

        // TODO: Open settings panel
    }

    /// <summary>
    /// Called when Quit button is clicked. 
    /// Exits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MainMenuManager] 🚪 Quit button clicked. Exiting game...");

        // ---- 🔊 PLAY CLICK SFX ----
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
        }

        // ---- 🚪 QUIT APPLICATION ----
#if UNITY_EDITOR
        UnityEditor.EditorApplication. isPlaying = false;
#else
        Application.Quit();
#endif
    }
}