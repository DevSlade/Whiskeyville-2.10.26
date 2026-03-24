// ============================================================================
// BUILDINGBEHAVIOR.CS
// ============================================================================
// PURPOSE:      Produces resources, optionally consumes one or two inputs
//               Non-production buildings (Saloon) open Sell Panel on click
// VERSION:      v4 — Added click-to-open for non-production buildings
// UPDATED:      February 12, 2026
// DEPENDENCIES: InventoryManager, ProductionPopupPool, UIManager
// ============================================================================

using UnityEngine;
using System.Collections;

public class BuildingBehavior : MonoBehaviour
{
    // ========================================================================
    // 🏭 RUNTIME DATA
    // ========================================================================

    private string _outputResource = "Corn";
    private int _outputAmount = 1;
    private float _productionInterval = 5f;

    private bool _requiresInput = false;
    private string _inputResource = "";
    private int _inputAmount = 0;

    // 🆕 DUAL INPUT
    private bool _requiresDualInput = false;
    private string _secondInputResource = "";
    private int _secondInputAmount = 0;

    private Color _popupColor = Color.yellow;
    private string _buildingName = "Building";

    // ========================================================================
    // 💾 SAVE DATA
    // ========================================================================

    private int _buildingIndex = -1;
    private Vector2Int _gridPosition;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private bool _isInitialized = false;
    private bool _isProducer = false;
    private bool _isWaitingForInput = false;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public int BuildingIndex => _buildingIndex;
    public Vector2Int GridPosition => _gridPosition;
    public string BuildingName => _buildingName;

    // ========================================================================
    // 🚀 INITIALIZATION
    // ========================================================================

    public void Initialize(BuildingData data)
    {
        if (data == null)
        {
            Debug.LogError("[BuildingBehavior] ❌ BuildingData is null!");
            return;
        }

        _buildingName = data.buildingName;
        _outputResource = data.outputResource;
        _outputAmount = data.outputAmount;
        _productionInterval = data.productionInterval;

        _requiresInput = data.requiresInput;
        _inputResource = data.inputResource;
        _inputAmount = data.inputAmount;

        // 🆕 Dual input setup
        _requiresDualInput = data.RequiresDualInput;
        _secondInputResource = data.secondInputResource;
        _secondInputAmount = data.secondInputAmount;

        _popupColor = data.popupColor;

        _isInitialized = true;

        // ================================================================
        // 🛡️ GUARD — Only start production if this building actually produces
        // ================================================================
        _isProducer = _outputAmount > 0
                   && _productionInterval > 0
                   && !string.IsNullOrEmpty(_outputResource);

        if (_isProducer)
        {
            StartCoroutine(ProductionLoop());

            if (_requiresDualInput)
            {
                Debug.Log($"[BuildingBehavior] 🏭 {_buildingName} initialized. DUAL INPUT: {_inputAmount} {_inputResource} + {_secondInputAmount} {_secondInputResource} → {_outputAmount} {_outputResource}");
            }
            else
            {
                Debug.Log($"[BuildingBehavior] 🏭 {_buildingName} initialized. Produces {_outputAmount} {_outputResource} every {_productionInterval}s.");
            }
        }
        else
        {
            Debug.Log($"[BuildingBehavior] 🏠 {_buildingName} initialized. Non-production building (clickable).");
        }
    }

    public void SetBuildingIndex(int index)
    {
        _buildingIndex = index;
    }

    public void SetGridPosition(Vector2Int pos)
    {
        _gridPosition = pos;
    }

    // ========================================================================
    // 🖱️ CLICK HANDLER — Non-production buildings open Sell Panel
    // ========================================================================

    private void OnMouseDown()
    {
        if (!_isInitialized) return;
        if (_isProducer) return;

        // Non-production building clicked (Saloon)
        if (_buildingName == "Saloon")
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ToggleSellPanel();
                Debug.Log("[BuildingBehavior] 🍺 Saloon clicked — toggling Sell Panel.");
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SFX_CLICK);
            }
        }
    }

    // ========================================================================
    // 🔄 PRODUCTION LOOP
    // ========================================================================

    private IEnumerator ProductionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_productionInterval);

            if (!_isInitialized) continue;

            TryProduce();
        }
    }

    // ========================================================================
    // 🏭 PRODUCTION — SINGLE + DUAL INPUT
    // ========================================================================

    private void TryProduce()
    {
        if (InventoryManager.Instance == null) return;
        if (!_isProducer) return;

        if (_requiresInput)
        {
            // ============================================================
            // DUAL INPUT — Atomic check, atomic consume
            // ============================================================
            if (_requiresDualInput)
            {
                bool hasFirst = InventoryManager.Instance.HasResource(_inputResource, _inputAmount);
                bool hasSecond = InventoryManager.Instance.HasResource(_secondInputResource, _secondInputAmount);

                if (!hasFirst || !hasSecond)
                {
                    if (!_isWaitingForInput)
                    {
                        string missing = "";
                        if (!hasFirst) missing += $"{_inputAmount} {_inputResource}";
                        if (!hasFirst && !hasSecond) missing += " + ";
                        if (!hasSecond) missing += $"{_secondInputAmount} {_secondInputResource}";

                        Debug.Log($"[BuildingBehavior] ⏳ {_buildingName} waiting for {missing}...");
                        _isWaitingForInput = true;
                    }
                    return;
                }

                InventoryManager.Instance.AddResource(_inputResource, -_inputAmount);
                InventoryManager.Instance.AddResource(_secondInputResource, -_secondInputAmount);
                _isWaitingForInput = false;
            }
            // ============================================================
            // SINGLE INPUT
            // ============================================================
            else
            {
                if (!InventoryManager.Instance.HasResource(_inputResource, _inputAmount))
                {
                    if (!_isWaitingForInput)
                    {
                        Debug.Log($"[BuildingBehavior] ⏳ {_buildingName} waiting for {_inputAmount} {_inputResource}...");
                        _isWaitingForInput = true;
                    }
                    return;
                }

                InventoryManager.Instance.AddResource(_inputResource, -_inputAmount);
                _isWaitingForInput = false;
            }
        }

        // ================================================================
        // OUTPUT
        // ================================================================

        InventoryManager.Instance.AddResource(_outputResource, _outputAmount);

        if (ProductionPopupPool.Instance != null)
        {
            string text = $"+{_outputAmount} {_outputResource}";
            ProductionPopupPool.Instance.ShowPopup(text, transform.position, _popupColor);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SFX_COLLECT);
        }

        Debug.Log($"[BuildingBehavior] 🌽 {_buildingName} produced {_outputAmount} {_outputResource}.");
    }
}