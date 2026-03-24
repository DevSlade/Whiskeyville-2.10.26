// ============================================================================
// BUILDPANELUI.CS
// ============================================================================
// PURPOSE:      Auto-generates building buttons from BuildingDatabase
// VERSION:      v3 — Fully automatic, no Inspector button wiring
// UPDATED:      February 12, 2026
// DEPENDENCIES: BuildingSelector, BuildingDatabase, InventoryManager
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildPanelUI : MonoBehaviour
{
    // ========================================================================
    // 🎨 INSPECTOR REFERENCES
    // ========================================================================

    [Header("Button Template")]
    [Tooltip("Drag ONE button from the panel to use as the template. It will be duplicated for each building.")]
    [SerializeField] private GameObject buttonTemplate;

    [Header("Button Container")]
    [Tooltip("The parent transform buttons spawn under. Should have a HorizontalLayoutGroup.")]
    [SerializeField] private Transform buttonContainer;

    [Header("Close Button (Optional)")]
    [SerializeField] private Button closeButton;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private List<Button> _generatedButtons = new List<Button>();
    private List<TextMeshProUGUI> _generatedLabels = new List<TextMeshProUGUI>();

    // ========================================================================
    // 🚀 INITIALIZATION
    // ========================================================================

    private void Start()
    {
        GenerateButtons();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ToggleBuildPanel();
                }
            });
        }
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnResourceChanged += OnResourceChanged;
        }

        RefreshButtons();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnResourceChanged -= OnResourceChanged;
        }
    }

    // ========================================================================
    // 🔧 AUTO-GENERATE BUTTONS
    // ========================================================================

    private void GenerateButtons()
    {
        if (BuildingDatabase.Instance == null)
        {
            Debug.LogError("[BuildPanelUI] ❌ BuildingDatabase not found!");
            return;
        }

        if (buttonTemplate == null)
        {
            Debug.LogError("[BuildPanelUI] ❌ Button template not assigned!");
            return;
        }

        if (buttonContainer == null)
        {
            Debug.LogError("[BuildPanelUI] ❌ Button container not assigned!");
            return;
        }

        BuildingData[] buildings = BuildingDatabase.Instance.buildings;

        // Hide template
        buttonTemplate.SetActive(false);

        // Clear any old generated buttons
        foreach (Button btn in _generatedButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        _generatedButtons.Clear();
        _generatedLabels.Clear();

        // Generate one button per building
        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] == null) continue;

            BuildingData data = buildings[i];
            int index = i;

            // Clone template
            GameObject btnObj = Instantiate(buttonTemplate, buttonContainer);
            btnObj.name = $"{data.buildingName}Button";
            btnObj.SetActive(true);

            // Setup button click
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectBuilding(index));
                _generatedButtons.Add(btn);
            }

            // Find and set label text
            // Looks for a child TextMeshProUGUI to use as the cost/name label
            TextMeshProUGUI[] texts = btnObj.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 2)
            {
                // First text = building name, second text = cost
                texts[0].text = data.buildingName;
                texts[1].text = $"${data.cost}";
                _generatedLabels.Add(texts[1]);
            }
            else if (texts.Length == 1)
            {
                // Single text = show both
                texts[0].text = $"{data.buildingName}\n${data.cost}";
                _generatedLabels.Add(texts[0]);
            }

            Debug.Log($"[BuildPanelUI] 🔧 Generated button: {data.buildingName} (${data.cost})");
        }

        // Move close button to end if it exists
        if (closeButton != null)
        {
            closeButton.transform.SetAsLastSibling();
        }

        RefreshButtons();

        Debug.Log($"[BuildPanelUI] ✅ Generated {_generatedButtons.Count} building buttons.");
    }

    // ========================================================================
    // 🖱️ BUTTON ACTIONS
    // ========================================================================

    private void SelectBuilding(int index)
    {
        if (BuildingSelector.Instance == null) return;

        BuildingSelector.Instance.SelectBuilding(index);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
        }

        Debug.Log($"[BuildPanelUI] 🏗️ Selected building index {index}.");
    }

    // ========================================================================
    // 🔄 REFRESH — Affordability Check
    // ========================================================================

    private void RefreshButtons()
    {
        if (BuildingDatabase.Instance == null) return;

        BuildingData[] buildings = BuildingDatabase.Instance.buildings;

        for (int i = 0; i < _generatedButtons.Count; i++)
        {
            if (_generatedButtons[i] == null) continue;
            if (i >= buildings.Length || buildings[i] == null) continue;

            bool canAfford = InventoryManager.Instance != null &&
                             InventoryManager.Instance.HasResource("Cash", buildings[i].cost);

            _generatedButtons[i].interactable = canAfford;
        }
    }

    // ========================================================================
    // 🔄 EVENT HANDLER
    // ========================================================================

    private void OnResourceChanged(string resource, int amount)
    {
        RefreshButtons();
    }
}