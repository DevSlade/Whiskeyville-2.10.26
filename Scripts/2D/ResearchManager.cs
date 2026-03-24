// ============================================================================
// RESEARCHMANAGER.CS
// ============================================================================
// PURPOSE:      Manages the Research Lab system: Research Points balance,
//               unlocked research nodes, and querying active effects.
//               Opened by the Research Lab building.
// ATTACHED TO:  GameManager or persistent GameObject
// ARCHITECTURE: Singleton, event-driven
// DEPENDENCIES: InventoryManager, UIManager, ResearchData ScriptableObjects
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

public class ResearchManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static ResearchManager Instance { get; private set; }

    // ========================================================================
    // 📢 EVENTS
    // ========================================================================

    public event Action<string>  OnResearchUnlocked;
    public event Action<int>     OnResearchPointsChanged;

    // ========================================================================
    // 🎮 INSPECTOR
    // ========================================================================

    [Header("Research Nodes (assign all ResearchData assets here)")]
    [SerializeField] private ResearchData[] _allResearchNodes;

    [Header("Starting Values")]
    [SerializeField] private int _startingResearchPoints = 0;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private int _researchPoints = 0;
    private HashSet<string> _unlockedNodes = new HashSet<string>();
    private Dictionary<string, ResearchData> _nodeMap = new Dictionary<string, ResearchData>();
    private bool _researchLabBuilt = false;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _researchPoints = _startingResearchPoints;
            BuildNodeMap();
            Debug.Log($"[ResearchManager] ✅ Singleton initialized. {_nodeMap.Count} research nodes loaded.");
        }
        else
        {
            Debug.LogWarning("[ResearchManager] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 INITIALIZATION
    // ========================================================================

    private void BuildNodeMap()
    {
        _nodeMap.Clear();

        if (_allResearchNodes == null) return;

        foreach (var node in _allResearchNodes)
        {
            if (node == null || string.IsNullOrEmpty(node.nodeId)) continue;

            if (_nodeMap.ContainsKey(node.nodeId))
            {
                Debug.LogWarning($"[ResearchManager] ⚠️ Duplicate nodeId: {node.nodeId}. Skipping.");
                continue;
            }

            _nodeMap[node.nodeId] = node;
        }
    }

    // ========================================================================
    // 🏗️ RESEARCH LAB STATE
    // ========================================================================

    /// <summary>
    /// Called by BuildingPlacementManager when a Research Lab is placed.
    /// </summary>
    public void NotifyResearchLabBuilt()
    {
        _researchLabBuilt = true;
        Debug.Log("[ResearchManager] 🏗️ Research Lab built — passive RP generation active.");
    }

    /// <summary>
    /// Called by BuildingPlacementManager if the Research Lab is demolished.
    /// </summary>
    public void NotifyResearchLabRemoved()
    {
        _researchLabBuilt = false;
        Debug.Log("[ResearchManager] ❌ Research Lab removed — passive RP generation paused.");
    }

    public bool IsResearchLabBuilt => _researchLabBuilt;

    // ========================================================================
    // 💎 RESEARCH POINTS
    // ========================================================================

    public int ResearchPoints => _researchPoints;

    /// <summary>
    /// Adds Research Points (e.g. from Rickhouse cycle, Saloon sales, Critical Batch).
    /// </summary>
    public void AddResearchPoints(int amount)
    {
        if (amount <= 0) return;

        _researchPoints += amount;

        Debug.Log($"[ResearchManager] 🔬 Research Points: {_researchPoints} (+{amount})");
        OnResearchPointsChanged?.Invoke(_researchPoints);
    }

    // ========================================================================
    // 🔬 RESEARCH OPERATIONS
    // ========================================================================

    /// <summary>
    /// Returns true if the player has unlocked the given research node.
    /// Queried by WhiskeyPropertyManager, BottleCustomizationManager, etc.
    /// </summary>
    public bool HasResearch(string nodeId)
    {
        return _unlockedNodes.Contains(nodeId);
    }

    /// <summary>
    /// Returns true if the node exists and all conditions to purchase it are met.
    /// </summary>
    public bool CanPurchase(string nodeId)
    {
        if (!_nodeMap.TryGetValue(nodeId, out ResearchData node)) return false;
        if (_unlockedNodes.Contains(nodeId)) return false; // Already purchased
        if (_researchPoints < node.researchPointCost) return false;

        // Check prerequisite
        if (!string.IsNullOrEmpty(node.prerequisiteNodeId))
        {
            if (!_unlockedNodes.Contains(node.prerequisiteNodeId)) return false;
        }

        return true;
    }

    /// <summary>
    /// Purchases a research node if all conditions are met. Returns true on success.
    /// </summary>
    public bool PurchaseResearch(string nodeId)
    {
        if (!CanPurchase(nodeId))
        {
            Debug.LogWarning($"[ResearchManager] ⚠️ Cannot purchase: {nodeId}");
            return false;
        }

        ResearchData node = _nodeMap[nodeId];

        _researchPoints -= node.researchPointCost;
        _unlockedNodes.Add(nodeId);

        Debug.Log($"[ResearchManager] ✅ Purchased: {node.nodeName} (−{node.researchPointCost} RP). Remaining: {_researchPoints} RP");

        OnResearchPointsChanged?.Invoke(_researchPoints);
        OnResearchUnlocked?.Invoke(nodeId);

        return true;
    }

    // ========================================================================
    // ⚡ EFFECT QUERIES
    // ========================================================================

    /// <summary>
    /// Returns the effectValue of a research node if unlocked, otherwise 1.0.
    /// Used as a multiplier: e.g. GetResearchMultiplier("flavor_bonus") returns 1.5
    /// if Flavor Mastery is purchased.
    /// </summary>
    public float GetResearchMultiplier(string effectKey)
    {
        foreach (var nodeId in _unlockedNodes)
        {
            if (_nodeMap.TryGetValue(nodeId, out ResearchData node))
            {
                if (node.effectKey == effectKey)
                {
                    return node.effectValue;
                }
            }
        }

        return 1.0f; // Default: no modifier
    }

    // ========================================================================
    // 📋 PUBLIC QUERY HELPERS
    // ========================================================================

    /// <summary>
    /// Returns all research nodes for the given category, sorted by cost.
    /// Used by the Research Panel UI to render the tree.
    /// </summary>
    public List<ResearchData> GetNodesForCategory(ResearchCategory category)
    {
        var result = new List<ResearchData>();

        foreach (var node in _nodeMap.Values)
        {
            if (node.category == category)
            {
                result.Add(node);
            }
        }

        result.Sort((a, b) => a.researchPointCost.CompareTo(b.researchPointCost));
        return result;
    }

    /// <summary>
    /// Returns a snapshot of all unlocked node IDs (for save serialization).
    /// </summary>
    public List<string> GetUnlockedNodeIds()
    {
        return new List<string>(_unlockedNodes);
    }

    // ========================================================================
    // 💾 SAVE / LOAD
    // ========================================================================

    public void LoadFromSaveData(int researchPoints, List<string> unlockedNodes)
    {
        _researchPoints = Mathf.Max(0, researchPoints);
        _unlockedNodes.Clear();

        if (unlockedNodes != null)
        {
            foreach (var nodeId in unlockedNodes)
            {
                if (_nodeMap.ContainsKey(nodeId))
                {
                    _unlockedNodes.Add(nodeId);
                }
                else
                {
                    Debug.LogWarning($"[ResearchManager] ⚠️ Save references unknown nodeId: {nodeId}. Skipping.");
                }
            }
        }

        Debug.Log($"[ResearchManager] 💾 Loaded {_unlockedNodes.Count} research nodes. RP: {_researchPoints}");
        OnResearchPointsChanged?.Invoke(_researchPoints);
    }

    // ========================================================================
    // 🔄 PRESTIGE RESET
    // ========================================================================

    /// <summary>
    /// Resets all research for a Prestige run. Optionally carries one node forward.
    /// </summary>
    public void PrestigeReset(string carryOverNodeId = null)
    {
        _researchPoints = 0;
        _unlockedNodes.Clear();

        if (!string.IsNullOrEmpty(carryOverNodeId) && _nodeMap.ContainsKey(carryOverNodeId))
        {
            _unlockedNodes.Add(carryOverNodeId);
            Debug.Log($"[ResearchManager] 🔄 Prestige reset. Carried over: {carryOverNodeId}");
        }
        else
        {
            Debug.Log("[ResearchManager] 🔄 Prestige reset. All research cleared.");
        }

        OnResearchPointsChanged?.Invoke(_researchPoints);
    }
}
