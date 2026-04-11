// ============================================================================
// TOOLMANAGER.CS
// ============================================================================
// PURPOSE:      Singleton that owns the active tool state
// VERSION:      v1 — MVP Tool System
// UPDATED:      March 25, 2026
// DEPENDENCIES: ToolType enum
// ============================================================================

using UnityEngine;
using System;

public class ToolManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static ToolManager Instance { get; private set; }

    // ========================================================================
    // 📢 EVENTS
    // ========================================================================

    /// <summary>
    /// Fired when active tool changes. Listeners: TileHighlightController, UI
    /// </summary>
    public event Action<ToolType> OnToolChanged;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private ToolType _activeTool = ToolType.None;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public ToolType ActiveTool => _activeTool;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ToolManager] ✅ Singleton initialized.");
        }
        else
        {
            Debug.LogWarning("[ToolManager] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 PUBLIC API
    // ========================================================================

    /// <summary>
    /// Sets the active tool. If same tool is selected again, deselects (toggle).
    /// </summary>
    public void SetTool(ToolType tool)
    {
        if (_activeTool == tool)
        {
            // Toggle off
            _activeTool = ToolType.None;
            Debug.Log("[ToolManager] 🔧 Tool deselected.");
        }
        else
        {
            _activeTool = tool;
            Debug.Log($"[ToolManager] 🔧 Tool selected: {_activeTool}");
        }

        OnToolChanged?.Invoke(_activeTool);
    }

    /// <summary>
    /// Force-sets tool without toggle behavior. Used by BuildingSelector.
    /// </summary>
    public void ForceSetTool(ToolType tool)
    {
        _activeTool = tool;
        Debug.Log($"[ToolManager] 🔧 Tool forced: {_activeTool}");
        OnToolChanged?.Invoke(_activeTool);
    }

    /// <summary>
    /// Clears active tool to None.
    /// </summary>
    public void ClearTool()
    {
        _activeTool = ToolType.None;
        Debug.Log("[ToolManager] 🔧 Tool cleared.");
        OnToolChanged?.Invoke(_activeTool);
    }
}