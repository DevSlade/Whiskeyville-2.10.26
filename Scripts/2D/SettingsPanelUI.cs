// ============================================================================
// SETTINGSPANELUI.CS
// ============================================================================
// PURPOSE:      Settings panel with volume sliders, persisted via PlayerPrefs
// VERSION:      v1 — Music / SFX / Ambient sliders + close button
// CREATED:      April 1, 2026
// ATTACHED TO:  MainMenu → SettingsPanel  AND  GameScene → PauseMenu → SettingsPanel
// DEPENDENCIES: AudioManager
// ============================================================================
// UI STRUCTURE:
//   SettingsPanel [GameObject — toggled by MainMenuManager / UIManager]
//   ├── TitleText "Settings"
//   ├── MusicLabel + MusicSlider      [assign _musicSlider]
//   ├── SFXLabel   + SFXSlider        [assign _sfxSlider]
//   ├── AmbientLabel + AmbientSlider  [assign _ambientSlider]
//   └── CloseButton                   [assign _closeButton]
// ============================================================================
// PLAYERPREFS KEYS (also used by AudioManager):
//   "MusicVolume"    (float 0-1)
//   "SFXVolume"      (float 0-1)
//   "AmbientVolume"  (float 0-1)
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelUI : MonoBehaviour
{
    // ========================================================================
    // ⚙️ INSPECTOR — SLIDERS
    // ========================================================================

    [Header("🎚️ Volume Sliders")]
    [Tooltip("Slider controlling music volume (0-1)")]
    [SerializeField] private Slider _musicSlider;

    [Tooltip("Slider controlling SFX volume (0-1)")]
    [SerializeField] private Slider _sfxSlider;

    [Tooltip("Slider controlling ambient volume (0-1)")]
    [SerializeField] private Slider _ambientSlider;

    // ========================================================================
    // ⚙️ INSPECTOR — LABELS (optional, show current value %)
    // ========================================================================

    [Header("📋 Value Labels (Optional)")]
    [Tooltip("TextMeshProUGUI showing current music volume %")]
    [SerializeField] private TextMeshProUGUI _musicValueLabel;

    [Tooltip("TextMeshProUGUI showing current SFX volume %")]
    [SerializeField] private TextMeshProUGUI _sfxValueLabel;

    [Tooltip("TextMeshProUGUI showing current ambient volume %")]
    [SerializeField] private TextMeshProUGUI _ambientValueLabel;

    // ========================================================================
    // ⚙️ INSPECTOR — CLOSE BUTTON
    // ========================================================================

    [Header("🔘 Close")]
    [Tooltip("Button to close the settings panel")]
    [SerializeField] private Button _closeButton;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        SetupSliders();
        BindListeners();
    }

    private void OnEnable()
    {
        // Refresh slider values every time the panel is opened
        RefreshSliderValues();
    }

    private void OnDestroy()
    {
        UnbindListeners();
    }

    // ========================================================================
    // 🔧 SETUP
    // ========================================================================

    private void SetupSliders()
    {
        // Configure slider ranges
        ConfigureSlider(_musicSlider);
        ConfigureSlider(_sfxSlider);
        ConfigureSlider(_ambientSlider);

        RefreshSliderValues();
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void RefreshSliderValues()
    {
        if (AudioManager.Instance == null) return;

        // Set slider values without triggering callbacks (remove then re-add)
        if (_musicSlider != null)   _musicSlider.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
        if (_sfxSlider != null)     _sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());
        if (_ambientSlider != null) _ambientSlider.SetValueWithoutNotify(AudioManager.Instance.GetAmbientVolume());

        // Update labels
        UpdateLabels();
    }

    private void BindListeners()
    {
        if (_musicSlider != null)   _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (_sfxSlider != null)     _sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        if (_ambientSlider != null) _ambientSlider.onValueChanged.AddListener(OnAmbientChanged);
        if (_closeButton != null)   _closeButton.onClick.AddListener(ClosePanel);
    }

    private void UnbindListeners()
    {
        if (_musicSlider != null)   _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (_sfxSlider != null)     _sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        if (_ambientSlider != null) _ambientSlider.onValueChanged.RemoveListener(OnAmbientChanged);
        if (_closeButton != null)   _closeButton.onClick.RemoveListener(ClosePanel);
    }

    // ========================================================================
    // 🎚️ SLIDER CALLBACKS
    // ========================================================================

    private void OnMusicChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);

        UpdateLabels();
    }

    private void OnSFXChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);

        // Play a brief SFX so the player can hear the new volume
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);

        UpdateLabels();
    }

    private void OnAmbientChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetAmbientVolume(value);

        UpdateLabels();
    }

    // ========================================================================
    // 📋 LABEL UPDATES
    // ========================================================================

    private void UpdateLabels()
    {
        if (_musicValueLabel != null && _musicSlider != null)
            _musicValueLabel.text = $"{Mathf.RoundToInt(_musicSlider.value * 100)}%";

        if (_sfxValueLabel != null && _sfxSlider != null)
            _sfxValueLabel.text = $"{Mathf.RoundToInt(_sfxSlider.value * 100)}%";

        if (_ambientValueLabel != null && _ambientSlider != null)
            _ambientValueLabel.text = $"{Mathf.RoundToInt(_ambientSlider.value * 100)}%";
    }

    // ========================================================================
    // 🚪 CLOSE
    // ========================================================================

    /// <summary>
    /// Closes this settings panel. Saves PlayerPrefs.
    /// </summary>
    public void ClosePanel()
    {
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);

        gameObject.SetActive(false);
        Debug.Log("[SettingsPanelUI] ⚙️ Settings closed and saved.");
    }
}
