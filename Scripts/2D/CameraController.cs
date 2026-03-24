// ============================================================================
// CAMERACONTROLLER.CS
// ============================================================================
// PURPOSE:     Handles camera pan and zoom for top-down 2D view
// CONTROLS:    Right-click drag = pan, Scroll wheel = zoom
// ============================================================================

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // ========================================================================
    // 🔍 INSPECTOR SETTINGS
    // ========================================================================

    [Header("🔍 Zoom Settings")]
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minZoom = 3f;
    [SerializeField] private float _maxZoom = 15f;

    [Header("🖱️ Pan Settings")]
    [SerializeField] private float _panSpeed = 1f;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private Vector3 _dragOrigin;
    private Camera _camera;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("[CameraController] ❌ No main camera found!");
        }
    }

    private void Update()
    {
        HandlePan();
        HandleZoom();
    }

    // ========================================================================
    // 🎮 CONTROLS
    // ========================================================================

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _dragOrigin = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 currentPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = _dragOrigin - currentPos;
            _camera.transform.position += delta;
        }
    }

    private void HandleZoom()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            float newZoom = _camera.orthographicSize - (scrollDelta * _zoomSpeed);
            _camera.orthographicSize = Mathf.Clamp(newZoom, _minZoom, _maxZoom);
        }
    }
}