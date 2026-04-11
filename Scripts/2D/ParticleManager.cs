// ============================================================================
// PARTICLEMANAGER.CS
// ============================================================================
// PURPOSE:      Singleton for triggering pooled particle effects at world positions
// VERSION:      v1 — 6 effect slots, object pool per effect type
// CREATED:      April 1, 2026
// ATTACHED TO:  ParticleManager GameObject (persists across scenes)
// ============================================================================
// USAGE:
//   ParticleManager.Instance.PlayParticle(ParticleManager.PARTICLE_HARVEST, transform.position);
//   ParticleManager.Instance.PlayParticle(ParticleManager.PARTICLE_CASH, transform.position);
//   ParticleManager.Instance.StopAllParticles();
// ============================================================================
// INSPECTOR HOOKUP:
//   Create 6 ParticleSystem prefabs in Unity, assign to the matching slots below.
//   Each prefab should have "Stop Action: Disable" set on its main module,
//   and Play On Awake = false, Loop = false.
//
//   PARTICLE PREFAB SETTINGS GUIDE:
//   _particleHarvest   → Gold dust burst: Shape=Cone, Color=golden, 0.3s duration
//   _particleCash      → Coin spray: Shape=Sphere, Color=gold, 0.5s duration
//   _particleSteam     → Steam wisps: Shape=Cone up, Color=white-gray, 1.5s loop
//   _particleBarrelGlow → Amber shimmer: Shape=Circle, Color=amber glow, 0.8s
//   _particleBuild     → Star burst: Shape=Sphere, Color=white/yellow, 0.4s
//   _particleDemolish  → Rubble scatter: Shape=Box, Color=brown/gray, 0.6s
// ============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static ParticleManager Instance { get; private set; }

    // ========================================================================
    // ✨ INSPECTOR — PARTICLE PREFABS
    // ========================================================================

    [Header("✨ Particle Prefabs")]
    [Tooltip("Gold dust burst — plays on crop harvest")]
    [SerializeField] private ParticleSystem _particleHarvest;

    [Tooltip("Coin spray — plays when income is received (Saloon sale)")]
    [SerializeField] private ParticleSystem _particleCash;

    [Tooltip("Steam wisps — plays on Still production")]
    [SerializeField] private ParticleSystem _particleSteam;

    [Tooltip("Amber shimmer — plays when Rickhouse aging completes")]
    [SerializeField] private ParticleSystem _particleBarrelGlow;

    [Tooltip("Star burst — plays when a building is placed")]
    [SerializeField] private ParticleSystem _particleBuild;

    [Tooltip("Rubble scatter — plays when a building is demolished")]
    [SerializeField] private ParticleSystem _particleDemolish;

    [Header("⚙️ Pool Settings")]
    [Tooltip("Number of instances to pre-pool per effect type")]
    [SerializeField] private int _poolSizePerEffect = 3;

    // ========================================================================
    // 📛 CONSTANTS — PARTICLE NAMES
    // ========================================================================

    public const string PARTICLE_HARVEST     = "Harvest";
    public const string PARTICLE_CASH        = "Cash";
    public const string PARTICLE_STEAM       = "Steam";
    public const string PARTICLE_BARREL_GLOW = "BarrelGlow";
    public const string PARTICLE_BUILD       = "Build";
    public const string PARTICLE_DEMOLISH    = "Demolish";

    // ========================================================================
    // 🔒 PRIVATE — POOL
    // ========================================================================

    private Dictionary<string, Queue<ParticleSystem>> _pool
        = new Dictionary<string, Queue<ParticleSystem>>();

    private Dictionary<string, ParticleSystem> _prefabs
        = new Dictionary<string, ParticleSystem>();

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterPrefabs();
            BuildPool();
            Debug.Log("[ParticleManager] ✅ Initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 INITIALIZATION
    // ========================================================================

    private void RegisterPrefabs()
    {
        if (_particleHarvest  != null) _prefabs[PARTICLE_HARVEST]     = _particleHarvest;
        if (_particleCash     != null) _prefabs[PARTICLE_CASH]        = _particleCash;
        if (_particleSteam    != null) _prefabs[PARTICLE_STEAM]       = _particleSteam;
        if (_particleBarrelGlow != null) _prefabs[PARTICLE_BARREL_GLOW] = _particleBarrelGlow;
        if (_particleBuild    != null) _prefabs[PARTICLE_BUILD]       = _particleBuild;
        if (_particleDemolish != null) _prefabs[PARTICLE_DEMOLISH]    = _particleDemolish;

        Debug.Log($"[ParticleManager] 📂 Registered {_prefabs.Count} particle types.");
    }

    private void BuildPool()
    {
        foreach (var kvp in _prefabs)
        {
            string key = kvp.Key;
            ParticleSystem prefab = kvp.Value;

            Queue<ParticleSystem> queue = new Queue<ParticleSystem>();

            for (int i = 0; i < _poolSizePerEffect; i++)
            {
                ParticleSystem instance = Instantiate(prefab, transform);
                instance.gameObject.SetActive(false);
                queue.Enqueue(instance);
            }

            _pool[key] = queue;
        }

        Debug.Log($"[ParticleManager] 🎱 Pool built: {_prefabs.Count} types × {_poolSizePerEffect} = {_prefabs.Count * _poolSizePerEffect} instances.");
    }

    // ========================================================================
    // ✨ PUBLIC — PLAY PARTICLE
    // ========================================================================

    /// <summary>
    /// Plays a pooled particle effect at a world position.
    /// Effect auto-returns to pool after it finishes playing.
    /// </summary>
    /// <param name="particleName">Use ParticleManager.PARTICLE_* constants</param>
    /// <param name="worldPosition">World space position to spawn the effect</param>
    public void PlayParticle(string particleName, Vector3 worldPosition)
    {
        if (!_pool.ContainsKey(particleName))
        {
            Debug.LogWarning($"[ParticleManager] ⚠️ No pool for: {particleName}. Assign prefab in Inspector.");
            return;
        }

        Queue<ParticleSystem> queue = _pool[particleName];

        if (queue.Count == 0)
        {
            // Pool exhausted — expand by 1 (rare, only happens in burst scenarios)
            if (_prefabs.TryGetValue(particleName, out ParticleSystem prefab))
            {
                ParticleSystem extra = Instantiate(prefab, transform);
                extra.gameObject.SetActive(false);
                queue.Enqueue(extra);
                Debug.LogWarning($"[ParticleManager] ⚠️ Pool expanded for: {particleName}");
            }
            else
            {
                return;
            }
        }

        ParticleSystem ps = queue.Dequeue();
        ps.transform.position = worldPosition;
        ps.gameObject.SetActive(true);
        ps.Play();

        // Return to pool after playback
        StartCoroutine(ReturnToPool(ps, particleName));
    }

    // ========================================================================
    // 🔄 POOL RETURN
    // ========================================================================

    private IEnumerator ReturnToPool(ParticleSystem ps, string key)
    {
        // Wait for the particle system to finish playing
        yield return new WaitUntil(() => !ps.isPlaying);

        ps.gameObject.SetActive(false);

        if (_pool.ContainsKey(key))
        {
            _pool[key].Enqueue(ps);
        }
    }

    // ========================================================================
    // 🛑 UTILITY
    // ========================================================================

    /// <summary>
    /// Stops all active particle effects immediately.
    /// </summary>
    public void StopAllParticles()
    {
        foreach (Transform child in transform)
        {
            ParticleSystem ps = child.GetComponent<ParticleSystem>();
            if (ps != null && ps.isPlaying)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            }
        }

        // Rebuild all queues
        _pool.Clear();
        BuildPool();

        Debug.Log("[ParticleManager] 🛑 All particles stopped and pool rebuilt.");
    }
}
