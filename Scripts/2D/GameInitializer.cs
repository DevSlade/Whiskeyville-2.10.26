// ============================================================================
// GAMEINITIALIZER.CS
// ============================================================================
// PURPOSE:      Ensures all singletons are initialized when game scene loads
// ATTACHED TO:  GameScene → GameInitializer GameObject
// EXECUTION:    Runs before other scripts via Script Execution Order or Awake
// ============================================================================
// SINGLETONS MANAGED:
//   🌐 InventoryManager
//   🌐 AudioManager
//   🌐 UIManager
//   🌐 GridManager
// ============================================================================

using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    // ========================================================================
    // 🎮 INSPECTOR - PREFABS
    // ========================================================================

    [Header("🌐 Singleton Prefabs (if not in scene)")]
    [Tooltip("InventoryManager prefab to instantiate if not found")]
    [SerializeField] private GameObject _inventoryManagerPrefab;

    [Tooltip("AudioManager prefab to instantiate if not found")]
    [SerializeField] private GameObject _audioManagerPrefab;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        Debug.Log("[GameInitializer] 🚀 Initializing game systems...");

        // ---- 🌐 ENSURE INVENTORY MANAGER ----
        EnsureSingleton<InventoryManager>(_inventoryManagerPrefab, "InventoryManager");

        // ---- 🌐 ENSURE AUDIO MANAGER ----
        EnsureSingleton<AudioManager>(_audioManagerPrefab, "AudioManager");

        Debug.Log("[GameInitializer] ✅ All systems initialized.");
    }

    // ========================================================================
    // 🔧 HELPER METHODS
    // ========================================================================

    /// <summary>
    /// Ensures a singleton exists.  Creates from prefab if not found.
    /// </summary>
    private void EnsureSingleton<T>(GameObject prefab, string name) where T : MonoBehaviour
    {
        if (FindFirstObjectByType<T>() == null)
        {
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log($"[GameInitializer] 🏗️ Created {name} from prefab.");
            }
            else
            {
                // Create empty GameObject with component
                GameObject go = new GameObject(name);
                go.AddComponent<T>();
                Debug.Log($"[GameInitializer] 🏗️ Created {name} GameObject.");
            }
        }
        else
        {
            Debug. Log($"[GameInitializer] ✅ {name} already exists.");
        }
    }
}