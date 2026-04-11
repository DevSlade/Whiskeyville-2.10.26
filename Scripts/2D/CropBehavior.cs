// ============================================================================
// CROPBEHAVIOR.CS
// ============================================================================
// PURPOSE:      Handles crop growth stages and click-and-hold to harvest.
//               Requires Sickle tool + ToolChargeSystem hold mechanic.
// VERSION:      v4 — Charge system integration for harvest
// UPDATED:      April 9, 2026
// DEPENDENCIES: InventoryManager, AudioManager, ProductionPopupPool,
//               ParticleManager, ToolManager, ToolChargeSystem
// ============================================================================

using UnityEngine;
using System.Collections;

public class CropBehavior : MonoBehaviour
{
    // ========================================================================
    // 🌱 INSPECTOR — GROWTH STAGES
    // ========================================================================

    [Header("Growth Stage GameObjects")]
    [Tooltip("Array of child GameObjects representing each growth stage (0 = seedling, last = ready to harvest)")]
    [SerializeField] private GameObject[] _growthStages;

    [Header("Growth Settings")]
    [Tooltip("Seconds between each growth stage advance")]
    [SerializeField] private float _growthInterval = 5f;

    [Header("Harvest Settings")]
    [Tooltip("Resource name added to inventory on harvest")]
    [SerializeField] private string _harvestResource = "Corn";

    [Tooltip("Amount of resource added per harvest")]
    [SerializeField] private int _harvestAmount = 1;

    [Tooltip("Color of the floating harvest popup")]
    [SerializeField] private Color _popupColor = Color.yellow;

    // ========================================================================
    // 💾 SAVE DATA
    // ========================================================================

    private int _buildingIndex = 0;
    private Vector2Int _gridPosition;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private int       _currentStage       = -1;
    private bool      _isFullyGrown       = false;
    private Coroutine _growthCoroutine    = null;
    private bool      _isInitialized      = false;
    private bool      _isWaitingForCharge = false; // True while charge ring is filling

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public bool       IsFullyGrown  => _isFullyGrown;
    public int        CurrentStage  => _currentStage;
    public int        BuildingIndex => _buildingIndex;
    public Vector2Int GridPosition  => _gridPosition;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        if (!_isInitialized)
        {
            StartGrowth();
        }
    }

    // ========================================================================
    // 🖱️ INPUT — Hold to charge, release to cancel
    // ========================================================================

    private void OnMouseDown()
    {
        // ---- Require Sickle tool ----
        if (ToolManager.Instance == null) return;
        if (ToolManager.Instance.ActiveTool != ToolType.Sickle)
        {
            Debug.Log("[CropBehavior] ⚠️ Need Sickle tool selected to harvest crops.");
            return;
        }

        // ---- Must be fully grown ----
        if (!_isFullyGrown)
        {
            Debug.Log($"[CropBehavior] ⏳ Not ready. Stage {_currentStage + 1}/{_growthStages.Length}");
            AudioManager.Instance?.PlaySFX(AudioManager.SFX_ERROR);
            return;
        }

        // ---- Prevent double-charge ----
        if (_isWaitingForCharge) return;

        // ---- Begin charge ----
        if (ToolChargeSystem.Instance == null)
        {
            // No charge system present — harvest immediately as a safe fallback
            Harvest();
            return;
        }

        bool started = ToolChargeSystem.Instance.BeginCharge(
            worldPos:   transform.position,
            onComplete: OnChargeComplete,
            onCancel:   OnChargeCancelled
        );

        if (started)
        {
            _isWaitingForCharge = true;
            Debug.Log("[CropBehavior] ⏳ Harvest charge started — hold to reap.");
        }
    }

    private void OnMouseUp()
    {
        // Released before ring filled → cancel
        if (_isWaitingForCharge)
        {
            ToolChargeSystem.Instance?.CancelCharge();
            // _isWaitingForCharge is cleared in OnChargeCancelled callback
        }
    }

    // ========================================================================
    // ⚡ CHARGE CALLBACKS
    // ========================================================================

    /// <summary>Called by ToolChargeSystem when the hold completes successfully.</summary>
    private void OnChargeComplete()
    {
        _isWaitingForCharge = false;

        // Re-validate before executing — state may have changed during hold
        if (!_isFullyGrown) return;
        if (ToolManager.Instance == null || ToolManager.Instance.ActiveTool != ToolType.Sickle) return;

        Harvest();
    }

    /// <summary>Called by ToolChargeSystem when mouse released before charge filled.</summary>
    private void OnChargeCancelled()
    {
        _isWaitingForCharge = false;
        Debug.Log("[CropBehavior] ❌ Harvest cancelled — released too early.");
    }

    // ========================================================================
    // 💾 SAVE / LOAD API
    // ========================================================================

    public void SetBuildingIndex(int index)     => _buildingIndex = index;
    public void SetGridPosition(Vector2Int pos) => _gridPosition = pos;

    /// <summary>Called by SaveManager to restore crop to its saved growth state.</summary>
    public void RestoreGrowthState(int stage, bool fullyGrown)
    {
        _isInitialized = true;

        if (_growthStages == null || _growthStages.Length == 0) return;

        // Disable all stages first
        foreach (GameObject stageObj in _growthStages)
        {
            if (stageObj != null) stageObj.SetActive(false);
        }

        // Restore saved stage
        stage         = Mathf.Clamp(stage, 0, _growthStages.Length - 1);
        _currentStage = stage;
        _isFullyGrown = fullyGrown;

        if (_growthStages[_currentStage] != null)
            _growthStages[_currentStage].SetActive(true);

        // Resume growth coroutine if not yet at final stage
        if (!_isFullyGrown)
            _growthCoroutine = StartCoroutine(GrowthLoop());

        Debug.Log($"[CropBehavior] 📂 Restored stage {_currentStage + 1}, fullyGrown: {_isFullyGrown}");
    }

    // ========================================================================
    // 🌱 GROWTH SYSTEM
    // ========================================================================

    private void StartGrowth()
    {
        if (_growthStages == null || _growthStages.Length == 0)
        {
            Debug.LogError("[CropBehavior] ❌ No growth stages assigned!");
            return;
        }

        _isInitialized = true;

        // Disable all stages
        foreach (GameObject stage in _growthStages)
        {
            if (stage != null) stage.SetActive(false);
        }

        _currentStage = 0;
        _isFullyGrown = false;

        if (_growthStages[0] != null)
        {
            _growthStages[0].SetActive(true);
            Debug.Log("[CropBehavior] 🌱 Stage 1 active. Crop planted.");
        }

        if (_growthCoroutine != null) StopCoroutine(_growthCoroutine);
        _growthCoroutine = StartCoroutine(GrowthLoop());
    }

    private IEnumerator GrowthLoop()
    {
        int totalStages    = _growthStages.Length;
        int lastStageIndex = totalStages - 1;

        while (_currentStage < lastStageIndex)
        {
            yield return new WaitForSeconds(_growthInterval);

            if (_growthStages[_currentStage] != null)
                _growthStages[_currentStage].SetActive(false);

            _currentStage++;

            if (_growthStages[_currentStage] != null)
                _growthStages[_currentStage].SetActive(true);

            Debug.Log($"[CropBehavior] 🌱 Advanced to stage {_currentStage + 1}/{totalStages}");
        }

        _isFullyGrown = true;
        Debug.Log("[CropBehavior] 🌾 Crop FULLY GROWN! Use Sickle to harvest.");
    }

    // ========================================================================
    // 🌾 HARVEST
    // ========================================================================

    private void Harvest()
    {
        // ---- Add resource ----
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddResource(_harvestResource, _harvestAmount);
            Debug.Log($"[CropBehavior] 🌾 Harvested {_harvestAmount} {_harvestResource}!");
        }

        // ---- Popup ----
        ProductionPopupPool.Instance?.ShowPopup(
            $"+{_harvestAmount} {_harvestResource}",
            transform.position,
            _popupColor
        );

        // ---- SFX ----
        AudioManager.Instance?.PlaySFX(AudioManager.SFX_HARVEST);

        // ---- Particle ----
        ParticleManager.Instance?.PlayParticle(ParticleManager.PARTICLE_HARVEST, transform.position);

        // ---- Reset growth cycle ----
        ResetGrowth();
    }

    private void ResetGrowth()
    {
        if (_growthCoroutine != null)
        {
            StopCoroutine(_growthCoroutine);
            _growthCoroutine = null;
        }

        foreach (GameObject stage in _growthStages)
        {
            if (stage != null) stage.SetActive(false);
        }

        _currentStage = 0;
        _isFullyGrown = false;

        if (_growthStages[0] != null)
            _growthStages[0].SetActive(true);

        Debug.Log("[CropBehavior] 🌱 Crop replanted. Growing again.");

        _growthCoroutine = StartCoroutine(GrowthLoop());
    }

    // ========================================================================
    // ⚙️ EXTERNAL INITIALIZATION (called by BuildingPlacementManager)
    // ========================================================================

    public void Initialize(string harvestResource, int harvestAmount, float growthInterval, Color popupColor)
    {
        _harvestResource = harvestResource;
        _harvestAmount   = harvestAmount;
        _growthInterval  = growthInterval;
        _popupColor      = popupColor;
        _isInitialized   = false; // Causes Start() to call StartGrowth()
    }
}
