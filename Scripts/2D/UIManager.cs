// ============================================================================
// UIMANAGER.CS
// ============================================================================
// PURPOSE:       Central manager for UI panels, pause state, and panel toggles
// VERSION:       v4 — Animated panel open/close via TweenHelper (fade + scale)
// UPDATED:       April 10, 2026
// ATTACHED TO:   GameScene → UIManager GameObject
// NOTES:
//   Panels animate in (FadeIn: alpha 0→1, scale 0.92→1.0 EaseOutBack)
//   and out (FadeOut: alpha 1→0, scale 1.0→0.92) using TweenHelper coroutines.
//   CanvasGroups are added automatically to panels that don't have one.
//   Bool flags (_buildPanelOpen, etc.) update immediately — visual is delayed.
//   All animations use unscaled time so they work correctly during pause.
// ============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // =========================================================================
    // SINGLETON
    // =========================================================================

    public static UIManager Instance { get; private set; }

    // =========================================================================
    // INSPECTOR — PANELS
    // =========================================================================

    [Header("UI Panels")]
    [SerializeField] private GameObject _resourcePanel;
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private GameObject _sellPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _selectedBuildingPanel;

    [Header("Demolish Confirm Panel")]
    [SerializeField] private GameObject _demolishConfirmPanel;

    [Header("Toggle Buttons (Optional)")]
    [SerializeField] private GameObject _buildToggleButton;
    [SerializeField] private GameObject _sellToggleButton;

    [Header("HUD Elements to Hide on Pause")]
    [Tooltip("Assign always-on HUD GameObjects here (Resource Panel, Fame Panel, etc.). " +
             "All are hidden when the game pauses and restored on resume.")]
    [SerializeField] private GameObject[] _hudElements;

    // =========================================================================
    // PRIVATE STATE — LOGIC
    // =========================================================================

    private bool _isPaused       = false;
    private bool _buildPanelOpen = false;
    private bool _sellPanelOpen  = false;

    // =========================================================================
    // PRIVATE STATE — ANIMATION
    // =========================================================================

    // CanvasGroups for animated panels (auto-added if missing)
    private CanvasGroup _buildPanelCG;
    private CanvasGroup _sellPanelCG;
    private CanvasGroup _pausePanelCG;

    // One coroutine handle per animated panel — allows stopping mid-animation
    private Coroutine _buildPanelAnim;
    private Coroutine _sellPanelAnim;
    private Coroutine _pausePanelAnim;

    // =========================================================================
    // PUBLIC PROPERTIES
    // =========================================================================

    public bool IsPaused       => _isPaused;
    public bool BuildPanelOpen => _buildPanelOpen;
    public bool SellPanelOpen  => _sellPanelOpen;

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

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
            if (Input.GetKeyDown(KeyCode.Escape)) ResumeGame();
            return;
        }

        HandleInput();
    }

    // =========================================================================
    // INPUT
    // =========================================================================

    private void HandleInput()
    {
        // ESC — close open panels first, then pause if nothing is open
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_buildPanelOpen || _sellPanelOpen)
            {
                CloseAllPanels();
                if (ToolManager.Instance != null) ToolManager.Instance.ClearTool();
            }
            else
            {
                PauseGame();
            }
        }

        // B — toggle build panel
        if (Input.GetKeyDown(KeyCode.B)) ToggleBuildPanel();

        // Tool hotkeys
        if (Input.GetKeyDown(KeyCode.H) && ToolManager.Instance != null)
            ToolManager.Instance.SetTool(ToolType.Hoe);

        if (Input.GetKeyDown(KeyCode.X) && ToolManager.Instance != null)
            ToolManager.Instance.SetTool(ToolType.Axe);

        if (Input.GetKeyDown(KeyCode.D) && ToolManager.Instance != null)
            ToolManager.Instance.SetTool(ToolType.Demolish);

        if (Input.GetKeyDown(KeyCode.S) && ToolManager.Instance != null)
            ToolManager.Instance.SetTool(ToolType.Sickle);
    }

    // =========================================================================
    // INITIALIZATION
    // =========================================================================

    private void InitializePanels()
    {
        // Cache CanvasGroups (auto-add if missing) before any SetActive calls
        _buildPanelCG = GetOrAddCanvasGroup(_buildPanel);
        _sellPanelCG  = GetOrAddCanvasGroup(_sellPanel);
        _pausePanelCG = GetOrAddCanvasGroup(_pausePanel);

        // Set initial alpha to 0 on hidden panels so first FadeIn starts correctly
        if (_buildPanelCG != null) ResetCanvasGroup(_buildPanelCG);
        if (_sellPanelCG  != null) ResetCanvasGroup(_sellPanelCG);
        if (_pausePanelCG != null) ResetCanvasGroup(_pausePanelCG);

        // Set active states
        if (_resourcePanel        != null) _resourcePanel.SetActive(true);
        if (_buildPanel           != null) _buildPanel.SetActive(false);
        if (_sellPanel            != null) _sellPanel.SetActive(false);
        if (_pausePanel           != null) _pausePanel.SetActive(false);
        if (_selectedBuildingPanel != null) _selectedBuildingPanel.SetActive(false);
        if (_demolishConfirmPanel  != null) _demolishConfirmPanel.SetActive(false);
        if (_buildToggleButton     != null) _buildToggleButton.SetActive(true);
        if (_sellToggleButton      != null) _sellToggleButton.SetActive(true);

        _buildPanelOpen = false;
        _sellPanelOpen  = false;

        Debug.Log("[UIManager] 📋 Panels initialized.");
    }

    // =========================================================================
    // BUILD PANEL
    // =========================================================================

    public void ToggleBuildPanel()
    {
        if (_buildPanelOpen) CloseBuildPanel();
        else                 OpenBuildPanel();
    }

    public void OpenBuildPanel()
    {
        if (_sellPanelOpen) CloseSellPanel();

        _buildPanelOpen = true;

        // Companion panel (no animation — follows build panel instantly)
        if (_selectedBuildingPanel != null) _selectedBuildingPanel.SetActive(true);

        // Animated open
        ShowPanelAnimated(_buildPanel, _buildPanelCG, ref _buildPanelAnim);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_OPEN);

        Debug.Log("[UIManager] 🏗️ Build panel OPENED.");
    }

    public void CloseBuildPanel()
    {
        _buildPanelOpen = false;

        // Companion panel closes instantly
        if (_selectedBuildingPanel != null) _selectedBuildingPanel.SetActive(false);

        // Animated close
        HidePanelAnimated(_buildPanel, _buildPanelCG, ref _buildPanelAnim);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_CLOSE);

        Debug.Log("[UIManager] 🏗️ Build panel CLOSED.");
    }

    // =========================================================================
    // SELL PANEL
    // =========================================================================

    public void ToggleSellPanel()
    {
        if (_sellPanelOpen) CloseSellPanel();
        else                OpenSellPanel();
    }

    public void OpenSellPanel()
    {
        if (_buildPanelOpen) CloseBuildPanel();

        _sellPanelOpen = true;

        ShowPanelAnimated(_sellPanel, _sellPanelCG, ref _sellPanelAnim);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_OPEN);

        Debug.Log("[UIManager] 💰 Sell panel OPENED.");
    }

    public void CloseSellPanel()
    {
        _sellPanelOpen = false;

        HidePanelAnimated(_sellPanel, _sellPanelCG, ref _sellPanelAnim);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_CLOSE);

        Debug.Log("[UIManager] 💰 Sell panel CLOSED.");
    }

    // =========================================================================
    // DEMOLISH CONFIRM PANEL (no animation — simple confirm dialog)
    // =========================================================================

    public void ShowDemolishConfirm()
    {
        if (_demolishConfirmPanel != null) _demolishConfirmPanel.SetActive(true);
        Debug.Log("[UIManager] 🔨 Demolish confirm shown.");
    }

    public void HideDemolishConfirm()
    {
        if (_demolishConfirmPanel != null) _demolishConfirmPanel.SetActive(false);
        Debug.Log("[UIManager] 🔨 Demolish confirm hidden.");
    }

    // =========================================================================
    // CLOSE ALL
    // =========================================================================

    public void CloseAllPanels()
    {
        CloseBuildPanel();
        CloseSellPanel();
        HideDemolishConfirm();
        Debug.Log("[UIManager] 📋 All panels closed.");
    }

    // =========================================================================
    // PAUSE SYSTEM
    // =========================================================================

    public void TogglePause()
    {
        if (_isPaused) ResumeGame();
        else           PauseGame();
    }

    public void PauseGame()
    {
        CloseAllPanels();

        _isPaused      = true;
        Time.timeScale = 0f;

        // Hide always-on HUD before showing pause panel
        SetHudElementsVisible(false);

        ShowPanelAnimated(_pausePanel, _pausePanelCG, ref _pausePanelAnim);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_OPEN);

        Debug.Log("[UIManager] ⏸️ Game PAUSED.");
    }

    public void ResumeGame()
    {
        _isPaused      = false;
        Time.timeScale = 1f;

        // Restore HUD after panel animates out
        SetHudElementsVisible(true);

        HidePanelAnimated(_pausePanel, _pausePanelCG, ref _pausePanelAnim);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.SFX_PANEL_CLOSE);

        Debug.Log("[UIManager] ▶️ Game RESUMED.");
    }

    // =========================================================================
    // NAVIGATION
    // =========================================================================

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        Debug.Log("[UIManager] 🚪 Returning to MainMenu.");
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("[UIManager] 🚪 Quitting game.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =========================================================================
    // HUD VISIBILITY
    // =========================================================================

    /// <summary>
    /// Shows or hides all GameObjects in the _hudElements array.
    /// Called on pause (false) and resume (true). Null-safe.
    /// </summary>
    private void SetHudElementsVisible(bool visible)
    {
        if (_hudElements == null) return;
        foreach (GameObject hud in _hudElements)
            if (hud != null) hud.SetActive(visible);
    }

    // =========================================================================
    // ANIMATION HELPERS
    // =========================================================================

    /// <summary>
    /// Shows a panel with a FadeIn animation. Stops any running animation first.
    /// CanvasGroup must be pre-cached (InitializePanels handles this).
    /// </summary>
    private void ShowPanelAnimated(GameObject panel, CanvasGroup cg, ref Coroutine handle)
    {
        if (panel == null) return;

        // Stop mid-animation if needed, snap alpha ready for FadeIn
        if (handle != null)
        {
            StopCoroutine(handle);
            handle = null;
        }

        panel.SetActive(true);

        if (cg != null)
            handle = StartCoroutine(TweenHelper.FadeIn(cg, panel.transform));
        else
            Debug.LogWarning($"[UIManager] ⚠️ No CanvasGroup on {panel.name} — showing without animation.");
    }

    /// <summary>
    /// Hides a panel with a FadeOut animation. SetActive(false) fires in the callback.
    /// Stops any running animation first.
    /// </summary>
    private void HidePanelAnimated(GameObject panel, CanvasGroup cg, ref Coroutine handle)
    {
        if (panel == null) return;

        // Stop mid-animation if needed
        if (handle != null)
        {
            StopCoroutine(handle);
            handle = null;
        }

        if (cg != null)
        {
            // Keep panel active during animation; disable in callback
            handle = StartCoroutine(TweenHelper.FadeOut(cg, panel.transform,
                onDone: () => panel.SetActive(false)));
        }
        else
        {
            // No CanvasGroup — instant hide
            panel.SetActive(false);
        }
    }

    // =========================================================================
    // UTILITY
    // =========================================================================

    /// <summary>Returns the CanvasGroup on a GameObject, adding one if missing.</summary>
    private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        if (go == null) return null;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    /// <summary>Resets a CanvasGroup to a fully hidden, non-interactive state.</summary>
    private static void ResetCanvasGroup(CanvasGroup cg)
    {
        cg.alpha          = 0f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;
    }
}
