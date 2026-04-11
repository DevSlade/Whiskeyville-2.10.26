// ============================================================================
// BASESINGLETON.CS
// ============================================================================
// PURPOSE:      Generic singleton base class for all managers
//               Eliminates copy-paste singleton boilerplate
// VERSION:      v1 — Foundation
// CREATED:      April 4, 2026
// ============================================================================
// DEV GUIDE:
//   To create a new singleton manager:
//
//   public class MyNewManager : BaseSingleton<MyNewManager>
//   {
//       protected override void OnSingletonAwake()
//       {
//           // Your init code here (replaces Awake)
//       }
//   }
//
//   That's it. No need to write Instance property, null checks, or Destroy logic.
//
//   For persistent managers (survive scene loads), add:
//   [PersistAcrossScenes] attribute or override Persistent => true
// ============================================================================

using UnityEngine;

/// <summary>
/// Generic singleton MonoBehaviour. Inherit from this to get auto-singleton pattern.
/// Override <see cref="Persistent"/> to true for DontDestroyOnLoad behavior.
/// Override <see cref="OnSingletonAwake"/> instead of using Awake().
/// </summary>
public abstract class BaseSingleton<T> : MonoBehaviour where T : BaseSingleton<T>
{
    // ========================================================================
    // SINGLETON INSTANCE
    // ========================================================================

    /// <summary>Global singleton instance. Null if not yet initialized.</summary>
    public static T Instance { get; private set; }

    /// <summary>True if a valid instance exists and hasn't been destroyed.</summary>
    public static bool HasInstance => Instance != null;

    // ========================================================================
    // CONFIGURATION — Override in subclass
    // ========================================================================

    /// <summary>
    /// Override to true to make this manager persist across scene loads.
    /// Default: false (destroyed on scene change).
    /// </summary>
    protected virtual bool Persistent => false;

    // ========================================================================
    // UNITY LIFECYCLE — Do NOT override Awake in subclasses
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = (T)this;

            if (Persistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnSingletonAwake();
            Debug.Log($"[{typeof(T).Name}] Initialized.{(Persistent ? " (Persistent)" : "")}");
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"[{typeof(T).Name}] Duplicate destroyed on {gameObject.name}.");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            OnSingletonDestroy();
            Instance = null;
        }
    }

    // ========================================================================
    // VIRTUAL HOOKS — Override these instead of Awake/OnDestroy
    // ========================================================================

    /// <summary>Called once when the singleton is first created. Use instead of Awake().</summary>
    protected virtual void OnSingletonAwake() { }

    /// <summary>Called when the singleton is destroyed. Use instead of OnDestroy().</summary>
    protected virtual void OnSingletonDestroy() { }
}
