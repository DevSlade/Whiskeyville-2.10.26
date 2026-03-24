// ============================================================================
// BUILDINGSELECTOR.CS
// ============================================================================
// PURPOSE:      Handles building selection via hotkeys and UI
// VERSION:      v2 — Added hotkey 6 for Saloon
// UPDATED:      February 13, 2026
// DEPENDENCIES: BuildingDatabase
// ============================================================================

using UnityEngine;
using System;

public class BuildingSelector : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static BuildingSelector Instance { get; private set; }

    // ========================================================================
    // 📢 EVENTS
    // ========================================================================

    public event Action<int> OnSelectionChanged;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private int _selectedIndex = -1;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public int SelectedIndex => _selectedIndex;

    public BuildingData SelectedBuilding
    {
        get
        {
            if (_selectedIndex < 0 || BuildingDatabase.Instance == null)
                return null;
            return BuildingDatabase.Instance.GetBuilding(_selectedIndex);
        }
    }

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[BuildingSelector] ✅ Singleton initialized.");
        }
        else
        {
            Debug.LogWarning("[BuildingSelector] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        HandleHotkeyInput();
    }

    // ========================================================================
    // 🎮 INPUT
    // ========================================================================

    private void HandleHotkeyInput()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectBuilding(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectBuilding(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectBuilding(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectBuilding(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SelectBuilding(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SelectBuilding(5);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
        }
    }

    // ========================================================================
    // 🔧 PUBLIC API
    // ========================================================================

    public void SelectBuilding(int index)
    {
        if (BuildingDatabase.Instance == null)
        {
            Debug.LogError("[BuildingSelector] ❌ BuildingDatabase not found!");
            return;
        }

        BuildingData building = BuildingDatabase.Instance.GetBuilding(index);

        if (building == null)
        {
            Debug.LogWarning($"[BuildingSelector] ⚠️ Invalid building index: {index}");
            return;
        }

        _selectedIndex = index;
        Debug.Log($"[BuildingSelector] 🏗️ Selected: {building.buildingName} (Index: {index})");

        OnSelectionChanged?.Invoke(_selectedIndex);
    }

    public void ClearSelection()
    {
        _selectedIndex = -1;
        Debug.Log("[BuildingSelector] 🚫 Selection cleared.");

        OnSelectionChanged?.Invoke(_selectedIndex);
    }
}