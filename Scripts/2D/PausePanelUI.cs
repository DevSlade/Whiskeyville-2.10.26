// ============================================================================
// PAUSEPANELUI.CS
// ============================================================================
// PURPOSE:      Pause menu buttons including Save/Load
// ATTACHED TO:  Canvas → PausePanel
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

public class PausePanelUI : MonoBehaviour
{
    // ========================================================================
    // 🎮 INSPECTOR
    // ========================================================================

    [Header("Navigation")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _quitButton;

    [Header("Persistence")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;

    // ========================================================================
    // ⚙️ LIFECYCLE
    // ========================================================================

    private void Start()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(OnResume);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(OnMainMenu);

        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuit);

        if (_saveButton != null)
            _saveButton.onClick.AddListener(OnSave);

        if (_loadButton != null)
            _loadButton.onClick.AddListener(OnLoad);

        Debug.Log("[PausePanelUI] ✅ Initialized.");
    }

    private void OnDestroy()
    {
        if (_resumeButton != null) _resumeButton.onClick.RemoveAllListeners();
        if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveAllListeners();
        if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
        if (_saveButton != null) _saveButton.onClick.RemoveAllListeners();
        if (_loadButton != null) _loadButton.onClick.RemoveAllListeners();
    }

    // ========================================================================
    // 🎮 HANDLERS
    // ========================================================================

    private void OnResume()
    {
        UIManager.Instance?.ResumeGame();
        AudioManager.Instance?.PlaySFX(AudioManager.SFX_CLICK);
    }

    private void OnMainMenu()
    {
        UIManager.Instance?.QuitToMainMenu();
    }

    private void OnQuit()
    {
        UIManager.Instance?.QuitGame();
    }

    private void OnSave()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            AudioManager.Instance?.PlaySFX(AudioManager.SFX_SUCCESS);
            Debug.Log("[PausePanelUI] 💾 Saved.");
        }
    }

    private void OnLoad()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.LoadGame())
        {
            AudioManager.Instance?.PlaySFX(AudioManager.SFX_SUCCESS);
            UIManager.Instance?.ResumeGame();
            Debug.Log("[PausePanelUI] 📂 Loaded.");
        }
        else
        {
            AudioManager.Instance?.PlaySFX(AudioManager.SFX_ERROR);
            Debug.Log("[PausePanelUI] ⚠️ No save found.");
        }
    }
}