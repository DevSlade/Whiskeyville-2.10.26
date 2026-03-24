// ============================================================================
// CROPBEHAVIOR.CS
// ============================================================================
// PURPOSE:      Handles crop growth stages and click-to-harvest
// ============================================================================

using UnityEngine;
using System.Collections;

public class CropBehavior : MonoBehaviour
{
    // ========================================================================
    // 🌱 INSPECTOR - GROWTH STAGES
    // ========================================================================

    [Header("Growth Stage GameObjects")]
    [SerializeField] private GameObject[] _growthStages;

    [Header("Growth Settings")]
    [SerializeField] private float _growthInterval = 5f;

    [Header("Harvest Settings")]
    [SerializeField] private string _harvestResource = "Corn";
    [SerializeField] private int _harvestAmount = 1;
    [SerializeField] private Color _popupColor = Color.yellow;

    // ========================================================================
    // 💾 SAVE DATA
    // ========================================================================

    private int _buildingIndex = 0;
    private Vector2Int _gridPosition;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private int _currentStage = -1;
    private bool _isFullyGrown = false;
    private Coroutine _growthCoroutine;
    private bool _isInitialized = false;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public bool IsFullyGrown => _isFullyGrown;
    public int CurrentStage => _currentStage;
    public int BuildingIndex => _buildingIndex;
    public Vector2Int GridPosition => _gridPosition;

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

    private void OnMouseDown()
    {
        TryHarvest();
    }

    // ========================================================================
    // 💾 SAVE/LOAD METHODS
    // ========================================================================

    public void SetBuildingIndex(int index)
    {
        _buildingIndex = index;
    }

    public void SetGridPosition(Vector2Int pos)
    {
        _gridPosition = pos;
    }

    public void RestoreGrowthState(int stage, bool fullyGrown)
    {
        _isInitialized = true;

        if (_growthStages == null || _growthStages. Length == 0) return;

        // Disable all stages
        foreach (GameObject stageObj in _growthStages)
        {
            if (stageObj != null) stageObj.SetActive(false);
        }

        // Clamp stage
        stage = Mathf.Clamp(stage, 0, _growthStages.Length - 1);
        _currentStage = stage;
        _isFullyGrown = fullyGrown;

        // Enable current stage
        if (_growthStages[_currentStage] != null)
        {
            _growthStages[_currentStage].SetActive(true);
        }

        // Resume growth if not fully grown
        if (!_isFullyGrown)
        {
            _growthCoroutine = StartCoroutine(GrowthLoop());
        }

        Debug.Log($"[CropBehavior] 📂 Restored to stage {_currentStage + 1}, fullyGrown:  {_isFullyGrown}");
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

        foreach (GameObject stage in _growthStages)
        {
            if (stage != null) stage.SetActive(false);
        }

        _currentStage = 0;
        _isFullyGrown = false;

        if (_growthStages[0] != null)
        {
            _growthStages[0].SetActive(true);
            Debug.Log("[CropBehavior] 🌱 Stage 1 active.  Crop planted.");
        }

        if (_growthCoroutine != null) StopCoroutine(_growthCoroutine);
        _growthCoroutine = StartCoroutine(GrowthLoop());
    }

    private IEnumerator GrowthLoop()
    {
        int totalStages = _growthStages.Length;
        int lastStageIndex = totalStages - 1;

        while (_currentStage < lastStageIndex)
        {
            yield return new WaitForSeconds(_growthInterval);

            if (_growthStages[_currentStage] != null)
            {
                _growthStages[_currentStage].SetActive(false);
            }

            _currentStage++;

            if (_growthStages[_currentStage] != null)
            {
                _growthStages[_currentStage].SetActive(true);
            }

            Debug.Log($"[CropBehavior] 🌱 Advanced to stage {_currentStage + 1}/{totalStages}");
        }

        _isFullyGrown = true;
        Debug.Log("[CropBehavior] 🌾 Crop FULLY GROWN!  Click to harvest.");
    }

    // ========================================================================
    // 🌾 HARVEST SYSTEM
    // ========================================================================

    private void TryHarvest()
    {
        if (! _isFullyGrown)
        {
            Debug.Log($"[CropBehavior] ⏳ Not ready.  Stage {_currentStage + 1}/{_growthStages.Length}");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_ERROR);
            }
            return;
        }

        Harvest();
    }

    private void Harvest()
    {
        if (InventoryManager. Instance != null)
        {
            InventoryManager.Instance.AddResource(_harvestResource, _harvestAmount);
            Debug.Log($"[CropBehavior] 🌾 Harvested {_harvestAmount} {_harvestResource}!");
        }

        if (ProductionPopupPool.Instance != null)
        {
            string text = $"+{_harvestAmount} {_harvestResource}";
            ProductionPopupPool. Instance.ShowPopup(text, transform.position, _popupColor);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_COLLECT);
        }

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
        {
            _growthStages[0].SetActive(true);
        }

        Debug.Log("[CropBehavior] 🌱 Crop replanted. Growing again.. .");

        _growthCoroutine = StartCoroutine(GrowthLoop());
    }

    public void Initialize(string harvestResource, int harvestAmount, float growthInterval, Color popupColor)
    {
        _harvestResource = harvestResource;
        _harvestAmount = harvestAmount;
        _growthInterval = growthInterval;
        _popupColor = popupColor;
        _isInitialized = false;
    }
}