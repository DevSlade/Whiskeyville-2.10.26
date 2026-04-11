// ============================================================================
// CAMERACONTROLLER.CS
// ============================================================================
// PURPOSE:     Handles camera pan and zoom for top-down 2D view
// VERSION:     v4 — Smooth pan with momentum + smooth zoom
// UPDATED:     April 5, 2026
// CONTROLS:    Desktop: Right-click drag = pan, Scroll wheel = zoom
//              Mobile:  One-finger drag = pan, Pinch = zoom
// DEPENDS ON:  InputProvider (must exist in scene)
// ============================================================================

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // ========================================================================
    // INSPECTOR SETTINGS
    // ========================================================================

    [Header("Zoom Settings")]
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minZoom = 3f;
    [SerializeField] private float _maxZoom = 15f;
    [Tooltip("How quickly zoom reaches its target (lower = smoother)")]
    [SerializeField] private float _zoomSmoothTime = 0.15f;

    [Header("Pan Settings")]
    [SerializeField] private float _panSpeed = 1f;
    [Tooltip("How quickly pan movement smooths (lower = smoother)")]
    [SerializeField] private float _panSmoothTime = 0.08f;
    [Tooltip("How quickly momentum decays after releasing (lower = longer glide)")]
    [SerializeField] private float _panDecayRate = 8f;
    [Tooltip("Minimum momentum speed before stopping")]
    [SerializeField] private float _panMomentumThreshold = 0.001f;

    [Header("Camera Bounds (Optional)")]
    [Tooltip("Clamp camera to world bounds")]
    [SerializeField] private bool _useBounds = false;
    [SerializeField] private Vector2 _boundsMin = new Vector2(-5f, -5f);
    [SerializeField] private Vector2 _boundsMax = new Vector2(25f, 20f);

    [Header("Follow Target (Optional)")]
    [Tooltip("If set, camera smoothly follows this transform")]
    [SerializeField] private Transform _followTarget;
    [SerializeField] private bool _useFollowMode = false;
    [SerializeField] private float _followSpeed = 5f;

    // ========================================================================
    // PRIVATE STATE
    // ========================================================================

    private Camera _camera;
    private bool _isUserPanning = false;

    // Smooth pan state
    private Vector3 _panMomentum = Vector3.zero;
    private Vector3 _smoothPanVelocity = Vector3.zero;
    private Vector3 _lastPanDelta = Vector3.zero;

    // Smooth zoom state
    private float _targetZoom;
    private float _zoomVelocity;

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("[CameraController] No main camera found!");
        }
        else
        {
            _targetZoom = _camera.orthographicSize;
        }
    }

    private void LateUpdate()
    {
        if (_camera == null) return;
        if (UIManager.Instance != null && UIManager.Instance.IsPaused) return;

        // Block camera if DevConsole is open
        if (DevConsole.Instance != null && DevConsole.Instance.IsOpen) return;

        HandlePan();
        HandleZoom();

        if (_useFollowMode && _followTarget != null && !_isUserPanning)
        {
            HandleFollow();
        }

        if (_useBounds)
        {
            ClampToBounds();
        }
    }

    // ========================================================================
    // PAN — Smooth drag with momentum/glide on release
    // ========================================================================

    private void HandlePan()
    {
        if (InputProvider.Instance != null)
        {
            HandleInputProviderPan();
        }
        else
        {
            HandleLegacyPan();
        }
    }

    private void HandleInputProviderPan()
    {
        _isUserPanning = InputProvider.IsPanning;

        if (InputProvider.IsPanning)
        {
            // Get raw delta from InputProvider
            Vector3 rawDelta = new Vector3(InputProvider.PanDelta.x, InputProvider.PanDelta.y, 0f) * _panSpeed;

            // Smooth the delta for buttery movement
            _lastPanDelta = Vector3.SmoothDamp(_lastPanDelta, rawDelta, ref _smoothPanVelocity, _panSmoothTime);

            // Apply to camera
            _camera.transform.position += _lastPanDelta;

            // Track momentum for glide effect
            _panMomentum = _lastPanDelta;
        }
        else
        {
            // Apply momentum glide when not panning
            if (_panMomentum.sqrMagnitude > _panMomentumThreshold * _panMomentumThreshold)
            {
                _camera.transform.position += _panMomentum;
                _panMomentum = Vector3.Lerp(_panMomentum, Vector3.zero, _panDecayRate * Time.deltaTime);
            }
            else
            {
                _panMomentum = Vector3.zero;
            }

            // Reset smoothing state
            _lastPanDelta = Vector3.zero;
            _smoothPanVelocity = Vector3.zero;
        }
    }

    private void HandleLegacyPan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _isUserPanning = true;
            _panMomentum = Vector3.zero;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 currentPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 prevPos = _camera.ScreenToWorldPoint(
                Input.mousePosition - new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f)
            );
            Vector3 rawDelta = prevPos - currentPos;

            // Smooth the delta
            _lastPanDelta = Vector3.SmoothDamp(_lastPanDelta, rawDelta, ref _smoothPanVelocity, _panSmoothTime);
            _camera.transform.position += _lastPanDelta;
            _panMomentum = _lastPanDelta;
        }

        if (Input.GetMouseButtonUp(1))
        {
            _isUserPanning = false;
        }

        // Momentum when not holding right-click
        if (!Input.GetMouseButton(1) && _panMomentum.sqrMagnitude > _panMomentumThreshold * _panMomentumThreshold)
        {
            _camera.transform.position += _panMomentum;
            _panMomentum = Vector3.Lerp(_panMomentum, Vector3.zero, _panDecayRate * Time.deltaTime);
        }
    }

    // ========================================================================
    // ZOOM — Smooth interpolation, no glitch at limits
    // ========================================================================

    private void HandleZoom()
    {
        float zoomDelta = 0f;

        if (InputProvider.Instance != null)
        {
            if (InputProvider.IsPinching)
            {
                zoomDelta = InputProvider.PinchDelta;
            }
        }
        else
        {
            zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        }

        if (Mathf.Abs(zoomDelta) > 0.001f)
        {
            _targetZoom -= zoomDelta * _zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        float currentZoom = _camera.orthographicSize;
        if (Mathf.Abs(currentZoom - _targetZoom) > 0.001f)
        {
            _camera.orthographicSize = Mathf.SmoothDamp(
                currentZoom, _targetZoom, ref _zoomVelocity, _zoomSmoothTime
            );
        }
        else
        {
            _camera.orthographicSize = _targetZoom;
            _zoomVelocity = 0f;
        }
    }

    // ========================================================================
    // FOLLOW MODE
    // ========================================================================

    private void HandleFollow()
    {
        if (_followTarget == null) return;

        Vector3 target = new Vector3(
            _followTarget.position.x,
            _followTarget.position.y,
            _camera.transform.position.z
        );

        _camera.transform.position = Vector3.Lerp(
            _camera.transform.position, target, _followSpeed * Time.deltaTime
        );
    }

    // ========================================================================
    // BOUNDS CLAMPING
    // ========================================================================

    private void ClampToBounds()
    {
        Vector3 pos = _camera.transform.position;
        float vertExtent = _camera.orthographicSize;
        float horizExtent = vertExtent * _camera.aspect;

        float minX = _boundsMin.x + horizExtent;
        float maxX = _boundsMax.x - horizExtent;
        float minY = _boundsMin.y + vertExtent;
        float maxY = _boundsMax.y - vertExtent;

        // Handle case where camera is wider than bounds
        if (minX > maxX) { float mid = (_boundsMin.x + _boundsMax.x) * 0.5f; minX = maxX = mid; }
        if (minY > maxY) { float mid = (_boundsMin.y + _boundsMax.y) * 0.5f; minY = maxY = mid; }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Kill momentum if hitting bounds
        if (Mathf.Approximately(pos.x, minX) || Mathf.Approximately(pos.x, maxX))
            _panMomentum.x = 0f;
        if (Mathf.Approximately(pos.y, minY) || Mathf.Approximately(pos.y, maxY))
            _panMomentum.y = 0f;

        _camera.transform.position = pos;
    }

    // ========================================================================
    // PUBLIC API
    // ========================================================================

    /// <summary>Smoothly move camera to a world position.</summary>
    public void FocusOn(Vector2 worldPosition, float duration = 0.5f)
    {
        _panMomentum = Vector3.zero;
        StopAllCoroutines();
        StartCoroutine(SmoothMoveTo(worldPosition, duration));
    }

    /// <summary>Set zoom level directly (clamped to min/max).</summary>
    public void SetZoom(float orthographicSize)
    {
        if (_camera != null)
        {
            _targetZoom = Mathf.Clamp(orthographicSize, _minZoom, _maxZoom);
            _camera.orthographicSize = _targetZoom;
        }
    }

    /// <summary>Set camera bounds at runtime (e.g. when grid size changes).</summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        _boundsMin = min;
        _boundsMax = max;
        _useBounds = true;
    }

    /// <summary>Stop all momentum immediately.</summary>
    public void StopMomentum()
    {
        _panMomentum = Vector3.zero;
        _lastPanDelta = Vector3.zero;
        _smoothPanVelocity = Vector3.zero;
    }

    private System.Collections.IEnumerator SmoothMoveTo(Vector2 target, float duration)
    {
        Vector3 start = _camera.transform.position;
        Vector3 end = new Vector3(target.x, target.y, start.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _camera.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        _camera.transform.position = end;
    }
}
