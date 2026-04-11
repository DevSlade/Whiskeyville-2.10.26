// ============================================================================
// DEVTOOLS.CS — LEGACY WRAPPER
// ============================================================================
// PURPOSE:      Backward-compatible stub. All functionality moved to DevConsole.cs
// VERSION:      v2 — Now just a passthrough to DevConsole
// UPDATED:      April 5, 2026
// NOTE:         You can safely remove this script. DevConsole handles everything.
//               Left in place so existing GameObjects with DevTools don't break.
// ============================================================================
// MIGRATION:
//   Old: DevTools GameObject with F-key cheats (editor-only)
//   New: DevConsole — works in ALL builds, has full UI, 40+ commands
//        Press ` (backtick) or F12 to open
// ============================================================================

using UnityEngine;

public class DevTools : MonoBehaviour
{
    [Header("DEPRECATED — Use DevConsole instead")]
    [Tooltip("This script is now a stub. All dev tools are in DevConsole.cs")]
    [SerializeField] private bool _showDeprecationWarning = true;

    private void Start()
    {
        if (_showDeprecationWarning)
        {
            Debug.LogWarning("[DevTools] This script is DEPRECATED. Use DevConsole instead. Press ` or F12 to open the dev console.");
        }

        // If no DevConsole exists, create one automatically
        if (DevConsole.Instance == null)
        {
            GameObject consoleObj = new GameObject("DevConsole");
            consoleObj.AddComponent<DevConsole>();
            Debug.Log("[DevTools] Auto-created DevConsole (migration from legacy DevTools).");
        }
    }
}
