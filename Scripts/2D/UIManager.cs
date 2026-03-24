// ============================================================================
// UIMANAGER.CS
// ============================================================================
// PURPOSE:       Central manager for UI panels, pause state, and panel toggles
// ATTACHED TO:   GameScene → UIManager GameObject
// ============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static UIManager Instance { get; private set; }

    // ========================================================================
    // 🎨 INSPECTOR - PANELS
    // ========================================================================

    [Header("UI Panels")]
    [SerializeField] private GameObject _resourcePanel;
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _sellPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _selectedBuildingPanel;

    [Header("Toggle Buttons (Optional)")]
    [SerializeField] private GameObject _buildToggleButton;
    [SerializeField] private GameObject _sellToggleButton;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private bool _isPaused = false;
    private bool _buildPanelOpen = false;
    private bool _sellPanelOpen = false;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public bool IsPaused => _isPaused;
    public bool BuildPanelOpen => _buildPanelOpen;
    public bool SellPanelOpen => _sellPanelOpen;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[UIManager] ✅ Singleton initialized.");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializePanels();
    }

    private void Update()
    {
        if (_isPaused)
        {
            if (Input.GetKeyDown(KeyCode. Escape))
            {
                ResumeGame();
            }
            return;
        }

        HandleInput();
    }

    // ========================================================================
    // 🎮 INPUT HANDLING
    // ========================================================================

    private void HandleInput()
    {
        // ESC = Pause (closes other panels first)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_buildPanelOpen || _sellPanelOpen)
            {
                CloseAllPanels();
            }
            else
            {
                PauseGame();
            }
        }

        // B = Toggle Build Panel
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildPanel();
        }

        // S = Toggle Sell Panel REMOVED
        
    }

    // ========================================================================
    // 🔧 INITIALIZATION
    // ========================================================================

    private void InitializePanels()
    {
        // Always visible
        if (_resourcePanel != null)
        {
            _resourcePanel.SetActive(true);
        }

        // Hidden by default
        if (_buildPanel != null)
        {
            _buildPanel.SetActive(false);
            _buildPanelOpen = false;
        }

        if (_sellPanel != null)
        {
            _sellPanel.SetActive(false);
            _sellPanelOpen = false;
        }

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }

        if (_selectedBuildingPanel != null)
        {
            _selectedBuildingPanel.SetActive(false);
        }

        // Toggle buttons always visible
        if (_buildToggleButton != null)
        {
            _buildToggleButton.SetActive(true);
        }

        if (_sellToggleButton != null)
        {
            _sellToggleButton.SetActive(true);
        }

        Debug.Log("[UIManager] 📋 Panels initialized.");
    }

    // ========================================================================
    // 🏗️ BUILD PANEL
    // ========================================================================

    public void ToggleBuildPanel()
    {
        if (_buildPanelOpen)
        {
            CloseBuildPanel();
        }
        else
        {
            OpenBuildPanel();
        }
    }

    public void OpenBuildPanel()
    {
        // Close sell panel if open
        if (_sellPanelOpen)
        {
            CloseSellPanel();
        }

        _buildPanelOpen = true;

        if (_buildPanel != null)
        {
            _buildPanel.SetActive(true);
        }

        if (_selectedBuildingPanel != null)
        {
            _selectedBuildingPanel.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance. PlaySFX(AudioManager.SFX_CLICK);
        }

        Debug.Log("[UIManager] 🏗️ Build panel OPENED.");
    }

    public void CloseBuildPanel()
    {
        _buildPanelOpen = false;

        if (_buildPanel != null)
        {
            _buildPanel.SetActive(false);
        }

        if (_selectedBuildingPanel != null)
        {
            _selectedBuildingPanel.SetActive(false);
        }

        Debug.Log("[UIManager] 🏗️ Build panel CLOSED.");
    }

    // ========================================================================
    // 💰 SELL PANEL
    // ========================================================================

    public void ToggleSellPanel()
    {
        if (_sellPanelOpen)
        {
            CloseSellPanel();
        }
        else
        {
            OpenSellPanel();
        }
    }

    public void OpenSellPanel()
    {
        // Close build panel if open
        if (_buildPanelOpen)
        {
            CloseBuildPanel();
        }

        _sellPanelOpen = true;

        if (_sellPanel != null)
        {
            _sellPanel.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
        }

        Debug.Log("[UIManager] 💰 Sell panel OPENED.");
    }

    public void CloseSellPanel()
    {
        _sellPanelOpen = false;

        if (_sellPanel != null)
        {
            _sellPanel.SetActive(false);
        }

        Debug.Log("[UIManager] 💰 Sell panel CLOSED.");
    }

    // ========================================================================
    // 🔄 CLOSE ALL
    // ========================================================================

    public void CloseAllPanels()
    {
        CloseBuildPanel();
        CloseSellPanel();

        Debug.Log("[UIManager] 📋 All panels closed.");
    }

    // ========================================================================
    // ⏸️ PAUSE SYSTEM
    // ========================================================================

    public void TogglePause()
    {
        if (_isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        // Close other panels first
        CloseAllPanels();

        _isPaused = true;
        Time. timeScale = 0f;

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(true);
        }

        Debug.Log("[UIManager] ⏸️ Game PAUSED.");
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }

        Debug.Log("[UIManager] ▶️ Game RESUMED.");
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        Debug.Log("[UIManager] 🚪 Returning to MainMenu.");
        SceneManager. LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug. Log("[UIManager] 🚪 Quitting game.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication. isPlaying = false;
#else
        Application.Quit();
#endif
    }
}