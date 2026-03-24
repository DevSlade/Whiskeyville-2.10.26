// ============================================================================
// HUDBUTTONSUI. CS
// ============================================================================
// PURPOSE:       Handles HUD buttons for opening Build/Sell panels
// ATTACHED TO:  Canvas → HUDButtons panel
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

public class HUDButtonsUI : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR
    // ========================================================================

    [Header("Toggle Buttons")]
    [SerializeField] private Button _buildButton;
    [SerializeField] private Button _sellButton;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        if (_buildButton != null)
        {
            _buildButton.onClick.AddListener(OnBuildClicked);
        }

        if (_sellButton != null)
        {
            _sellButton.onClick.AddListener(OnSellClicked);
        }

        Debug.Log("[HUDButtonsUI] ✅ Initialized.");
    }

    private void OnDestroy()
    {
        if (_buildButton != null)
        {
            _buildButton.onClick.RemoveListener(OnBuildClicked);
        }

        if (_sellButton != null)
        {
            _sellButton.onClick.RemoveListener(OnSellClicked);
        }
    }

    // ========================================================================
    // 🎮 BUTTON HANDLERS
    // ========================================================================

    private void OnBuildClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleBuildPanel();
        }
    }

    private void OnSellClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleSellPanel();
        }
    }
}