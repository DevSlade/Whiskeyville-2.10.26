// ============================================================================
// INVENTORYMANAGER.CS
// ============================================================================
// PURPOSE:      Single Source of Truth (SSOT) for all player resources
// ATTACHED TO:  GameManager or persistent GameObject
// ARCHITECTURE: Singleton, event-driven, no silent failures
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static InventoryManager Instance { get; private set; }

    // ========================================================================
    // 📦 RESOURCE CONSTANTS
    // ========================================================================

    public const string RESOURCE_CASH = "Cash";
    public const string RESOURCE_CORN = "Corn";
    public const string RESOURCE_MASH = "Mash";
    public const string RESOURCE_WHISKEY = "Whiskey";
    public const string RESOURCE_AGED_WHISKEY = "AgedWhiskey";
    public const string RESOURCE_WOOD = "Wood";
    public const string RESOURCE_BARREL = "Barrel";

    // ========================================================================
    // 📢 EVENTS
    // ========================================================================

    public event Action<string, int> OnResourceChanged;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Dictionary<string, int> _resources = new Dictionary<string, int>();

    // ========================================================================
    // 🎮 INSPECTOR - STARTING VALUES
    // ========================================================================

    [Header("Starting Resources")]
    [SerializeField] private int _startingCash = 200;
    [SerializeField] private int _startingCorn = 0;
    [SerializeField] private int _startingMash = 0;
    [SerializeField] private int _startingWhiskey = 0;
    [SerializeField] private int _startingAgedWhiskey = 0;
    [SerializeField] private int _startingWood = 0;
    [SerializeField] private int _startingBarrel = 0;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeResources();
            Debug.Log("[InventoryManager] ✅ Singleton initialized.");
        }
        else
        {
            Debug.LogWarning("[InventoryManager] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 INITIALIZATION
    // ========================================================================

    private void InitializeResources()
    {
        _resources.Clear();
        _resources[RESOURCE_CASH] = _startingCash;
        _resources[RESOURCE_CORN] = _startingCorn;
        _resources[RESOURCE_MASH] = _startingMash;
        _resources[RESOURCE_WHISKEY] = _startingWhiskey;
        _resources[RESOURCE_AGED_WHISKEY] = _startingAgedWhiskey;
        _resources[RESOURCE_WOOD] = _startingWood;
        _resources[RESOURCE_BARREL] = _startingBarrel;

        Debug.Log("[InventoryManager] 📦 Resources initialized.");
    }

    // ========================================================================
    // 📊 PUBLIC API
    // ========================================================================

    public int GetResource(string resourceName)
    {
        if (_resources.TryGetValue(resourceName, out int value))
        {
            return value;
        }

        Debug.LogWarning($"[InventoryManager] ⚠️ Unknown resource: {resourceName}");
        return 0;
    }

    public void AddResource(string resourceName, int amount)
    {
        if (!_resources.ContainsKey(resourceName))
        {
            Debug.LogWarning($"[InventoryManager] ⚠️ Unknown resource: {resourceName}");
            return;
        }

        _resources[resourceName] += amount;

        if (_resources[resourceName] < 0)
        {
            _resources[resourceName] = 0;
        }

        Debug.Log($"[InventoryManager] 📦 {resourceName}: {_resources[resourceName]} ({(amount >= 0 ? "+" : "")}{amount})");

        OnResourceChanged?.Invoke(resourceName, _resources[resourceName]);
    }

    public void SetResource(string resourceName, int value)
    {
        if (!_resources.ContainsKey(resourceName))
        {
            Debug.LogWarning($"[InventoryManager] ⚠️ Unknown resource: {resourceName}");
            return;
        }

        _resources[resourceName] = Mathf.Max(0, value);

        Debug.Log($"[InventoryManager] 📦 {resourceName} set to: {_resources[resourceName]}");

        OnResourceChanged?.Invoke(resourceName, _resources[resourceName]);
    }

    public bool HasResource(string resourceName, int amount)
    {
        return GetResource(resourceName) >= amount;
    }

    public void ResetResources()
    {
        InitializeResources();

        foreach (var kvp in _resources)
        {
            OnResourceChanged?.Invoke(kvp.Key, kvp.Value);
        }

        Debug.Log("[InventoryManager] 🔄 Resources reset.");
    }

    public void LogAllResources()
    {
        Debug.Log("[InventoryManager] 📋 Current Resources:");
        foreach (var kvp in _resources)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
}