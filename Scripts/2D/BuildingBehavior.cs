// ============================================================================
// BUILDINGBEHAVIOR.CS
// ============================================================================
// PURPOSE:      Produces resources, optionally consumes one or two inputs.
//               Non-production buildings (Saloon) open Sell Panel on click.
// VERSION:      v6 — Per-frame production timer for progress bar support
// UPDATED:      April 9, 2026
// DEPENDENCIES: InventoryManager, ProductionPopupPool, UIManager,
//               AudioManager, ParticleManager
// ============================================================================

using UnityEngine;
using System.Collections;

public class BuildingBehavior : MonoBehaviour
{
    // ========================================================================
    // 🏭 RUNTIME DATA
    // ========================================================================

    private string _outputResource   = "Corn";
    private int    _outputAmount     = 1;
    private float  _productionInterval = 5f;

    private bool   _requiresInput    = false;
    private string _inputResource    = "";
    private int    _inputAmount      = 0;

    // Dual input support
    private bool   _requiresDualInput    = false;
    private string _secondInputResource  = "";
    private int    _secondInputAmount    = 0;

    private Color  _popupColor = Color.yellow;
    private string _buildingName = "Building";

    // ========================================================================
    // 💾 SAVE DATA
    // ========================================================================

    private int _buildingIndex = -1;
    private Vector2Int _gridPosition;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private bool  _isInitialized     = false;
    private bool  _isProducer        = false;
    private bool  _isWaitingForInput = false;

    // Per-frame production timer — used by BuildingProgressBar for the fill bar
    private float _productionTimer = 0f;

    // ========================================================================
    // 📊 PUBLIC PROPERTIES
    // ========================================================================

    public int        BuildingIndex    => _buildingIndex;
    public Vector2Int GridPosition    => _gridPosition;
    public string     BuildingName    => _buildingName;

    /// <summary>True if this building runs a production loop (has output defined).</summary>
    public bool IsProducing => _isProducer && _isInitialized;

    /// <summary>True if this building is blocked waiting for input resources.</summary>
    public bool IsWaitingForInput => _isWaitingForInput;

    /// <summary>
    /// Production cycle progress from 0 (just started) to 1 (about to produce).
    /// Returns 0 if building is not a producer. Used by BuildingProgressBar.
    /// </summary>
    public float ProductionProgress =>
        (_isProducer && _productionInterval > 0f)
            ? Mathf.Clamp01(_productionTimer / _productionInterval)
            : 0f;

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

        _buildingName        = data.buildingName;
        _outputResource      = data.outputResource;
        _outputAmount        = data.outputAmount;
        _productionInterval  = data.productionInterval;

        _requiresInput       = data.requiresInput;
        _inputResource       = data.inputResource;
        _inputAmount         = data.inputAmount;

        _requiresDualInput   = data.RequiresDualInput;
        _secondInputResource = data.secondInputResource;
        _secondInputAmount   = data.secondInputAmount;

        _popupColor          = data.popupColor;

        _isInitialized = true;

        _isProducer = _outputAmount > 0
                   && _productionInterval > 0
                   && !string.IsNullOrEmpty(_outputResource);

        if (_isProducer)
        {
            StartCoroutine(ProductionLoop());

            if (_requiresDualInput)
                Debug.Log($"[BuildingBehavior] 🏭 {_buildingName}: DUAL INPUT {_inputAmount} {_inputResource} + {_secondInputAmount} {_secondInputResource} → {_outputAmount} {_outputResource}");
            else
                Debug.Log($"[BuildingBehavior] 🏭 {_buildingName}: {_outputAmount} {_outputResource} every {_productionInterval}s");
        }
        else
        {
            Debug.Log($"[BuildingBehavior] 🏠 {_buildingName}: non-production (clickable).");
        }
    }

    public void SetBuildingIndex(int index) => _buildingIndex = index;
    public void SetGridPosition(Vector2Int pos) => _gridPosition = pos;

    // ========================================================================
    // 🖱️ CLICK — Non-production buildings (Saloon)
    // ========================================================================

    private void OnMouseDown()
    {
        if (!_isInitialized) return;
        if (_isProducer) return;

        if (_buildingName == GameConstants.Buildings.SALOON)
        {
            UIManager.Instance?.ToggleSellPanel();

            // Cash sound when opening the saloon
            AudioManager.Instance?.PlaySFX(AudioManager.SFX_CASH);

            Debug.Log("[BuildingBehavior] 🍺 Saloon clicked — toggling Sell Panel.");
        }
    }

    // ========================================================================
    // 🔄 PRODUCTION LOOP
    // ========================================================================

    private IEnumerator ProductionLoop()
    {
        // Per-frame loop so _productionTimer is always current.
        // BuildingProgressBar reads ProductionProgress each frame for the fill bar.
        while (true)
        {
            // Reset timer at the start of each production cycle
            _productionTimer = 0f;

            // Count up to _productionInterval in real game time
            while (_productionTimer < _productionInterval)
            {
                _productionTimer += Time.deltaTime;
                yield return null;
            }

            // Clamp to interval so ProductionProgress hits exactly 1.0
            _productionTimer = _productionInterval;

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
            // ---- DUAL INPUT ----
            if (_requiresDualInput)
            {
                bool hasFirst  = InventoryManager.Instance.HasResource(_inputResource,       _inputAmount);
                bool hasSecond = InventoryManager.Instance.HasResource(_secondInputResource, _secondInputAmount);

                if (!hasFirst || !hasSecond)
                {
                    if (!_isWaitingForInput)
                    {
                        string missing = "";
                        if (!hasFirst)  missing += $"{_inputAmount} {_inputResource}";
                        if (!hasFirst && !hasSecond) missing += " + ";
                        if (!hasSecond) missing += $"{_secondInputAmount} {_secondInputResource}";
                        Debug.Log($"[BuildingBehavior] ⏳ {_buildingName} waiting for {missing}...");
                        _isWaitingForInput = true;
                    }
                    return;
                }

                InventoryManager.Instance.AddResource(_inputResource,       -_inputAmount);
                InventoryManager.Instance.AddResource(_secondInputResource, -_secondInputAmount);
                _isWaitingForInput = false;
            }
            // ---- SINGLE INPUT ----
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

        // ====================================================================
        // ✅ OUTPUT
        // ====================================================================

        InventoryManager.Instance.AddResource(_outputResource, _outputAmount);

        // ---- 💬 POPUP ----
        if (ProductionPopupPool.Instance != null)
        {
            string text = $"+{_outputAmount} {_outputResource}";
            ProductionPopupPool.Instance.ShowPopup(text, transform.position, _popupColor);
        }

        // ---- 🔊 SFX (per-building) ----
        PlayProductionSFX();

        // ---- ✨ PARTICLE (per-building) ----
        PlayProductionParticle();

        Debug.Log($"[BuildingBehavior] ✅ {_buildingName} produced {_outputAmount} {_outputResource}.");
    }

    // ========================================================================
    // 🔊 PER-BUILDING SFX DISPATCH
    // ========================================================================

    /// <summary>
    /// Routes production SFX based on building name.
    /// Each building in the chain has its own distinct audio identity.
    /// </summary>
    private void PlayProductionSFX()
    {
        if (AudioManager.Instance == null) return;

        switch (_buildingName)
        {
            case GameConstants.Buildings.COOPERAGE:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_BARREL);  // barrel rolling/sealing
                break;
            case GameConstants.Buildings.STILL:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_POUR);    // liquid drip
                break;
            case GameConstants.Buildings.RICKHOUSE:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_AGE);     // shimmer reveal
                break;
            case GameConstants.Buildings.SALOON:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_CASH);    // cash register
                break;
            case GameConstants.Buildings.MASH_TUN:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_COLLECT); // bubbling/general
                break;
            default:
                AudioManager.Instance.PlaySFX(AudioManager.SFX_COLLECT); // fallback
                break;
        }
    }

    // ========================================================================
    // ✨ PER-BUILDING PARTICLE DISPATCH
    // ========================================================================

    /// <summary>
    /// Triggers a particle effect appropriate for this building's production.
    /// </summary>
    private void PlayProductionParticle()
    {
        if (ParticleManager.Instance == null) return;

        switch (_buildingName)
        {
            case GameConstants.Buildings.STILL:
                ParticleManager.Instance.PlayParticle(ParticleManager.PARTICLE_STEAM, transform.position);
                break;
            case GameConstants.Buildings.RICKHOUSE:
                ParticleManager.Instance.PlayParticle(ParticleManager.PARTICLE_BARREL_GLOW, transform.position);
                break;
            case GameConstants.Buildings.SALOON:
                ParticleManager.Instance.PlayParticle(ParticleManager.PARTICLE_CASH, transform.position);
                break;
            // MashTun, Cooperage, CornField: no particle yet (add in Phase 2)
            default:
                break;
        }
    }
}
