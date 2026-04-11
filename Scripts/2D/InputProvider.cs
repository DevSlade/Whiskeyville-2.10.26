// ============================================================================
// INPUTPROVIDER.CS
// ============================================================================
// PURPOSE:      Unified input abstraction — mouse AND touch work identically
//               Every script reads from InputProvider instead of Input directly
// VERSION:      v1 — Foundation (mouse + single/multi-touch)
// CREATED:      April 4, 2026
// ============================================================================
// DEV GUIDE:
//   BEFORE (fragile, desktop-only):
//     if (Input.GetMouseButtonDown(0)) { ... }
//     Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//
//   AFTER (works on all platforms):
//     if (InputProvider.TapDown) { ... }
//     Vector2 pos = InputProvider.WorldTapPosition;
//
//   GESTURES:
//     InputProvider.IsPanning      — one-finger drag or right-click drag
//     InputProvider.PanDelta       — how far the pan moved this frame
//     InputProvider.IsPinching     — two-finger pinch (mobile) or scroll (desktop)
//     InputProvider.PinchDelta     — zoom amount (positive = zoom in)
//     InputProvider.TapDown        — single tap/click this frame
//     InputProvider.TapHeld        — finger/button held down
//     InputProvider.TapUp          — finger/button released this frame
//     InputProvider.ScreenPosition — current pointer position in screen space
//     InputProvider.WorldTapPosition — current pointer position in world space
//
//   This script auto-detects platform. No #if UNITY_IOS / UNITY_ANDROID needed.
// ============================================================================

using UnityEngine;

public class InputProvider : MonoBehaviour
{
    // ========================================================================
    // SINGLETON (lightweight — not BaseSingleton to avoid circular deps)
    // ========================================================================

    public static InputProvider Instance { get; private set; }

    // ========================================================================
    // PUBLIC STATE — Read these from any script
    // ========================================================================

    /// <summary>True on the frame the user taps/clicks.</summary>
    public static bool TapDown { get; private set; }

    /// <summary>True while the user is holding a tap/click.</summary>
    public static bool TapHeld { get; private set; }

    /// <summary>True on the frame the user releases a tap/click.</summary>
    public static bool TapUp { get; private set; }

    /// <summary>True when the user is dragging (one-finger on mobile, right-click on desktop).</summary>
    public static bool IsPanning { get; private set; }

    /// <summary>World-space delta of pan movement this frame.</summary>
    public static Vector2 PanDelta { get; private set; }

    /// <summary>True when user is pinching (mobile) or scrolling (desktop).</summary>
    public static bool IsPinching { get; private set; }

    /// <summary>Zoom delta: positive = zoom in, negative = zoom out.</summary>
    public static float PinchDelta { get; private set; }

    /// <summary>Current pointer position in screen space.</summary>
    public static Vector2 ScreenPosition { get; private set; }

    /// <summary>Current pointer position in world space (uses main camera).</summary>
    public static Vector2 WorldTapPosition { get; private set; }

    /// <summary>Number of active touches (0 on desktop unless touching).</summary>
    public static int TouchCount { get; private set; }

    /// <summary>True if running on a touch device.</summary>
    public static bool IsTouchDevice { get; private set; }

    // ========================================================================
    // PRIVATE STATE
    // ========================================================================

    private Camera _mainCamera;
    private Vector2 _lastPanPosition;
    private bool _isPanActive;
    private float _lastPinchDistance;

    // ========================================================================
    // CONFIGURATION
    // ========================================================================

    [Header("Pan Settings")]
    [SerializeField] private float _panDeadzone = 5f;       // pixels before pan starts
    [SerializeField] private float _panSmoothTime = 0.05f;  // smoothing

    [Header("Pinch Settings")]
    [SerializeField] private float _pinchSensitivity = 0.02f;

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _mainCamera = Camera.main;

        // Detect touch capability
        IsTouchDevice = Input.touchSupported && !Application.isEditor;

        // In editor, force mouse mode for testing (hold Shift+Alt to simulate touch)
        #if UNITY_EDITOR
        IsTouchDevice = false;
        #endif

        Debug.Log($"[InputProvider] Initialized. Touch device: {IsTouchDevice}");
    }

    private void Update()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;
        }

        // Reset per-frame states
        TapDown = false;
        TapUp = false;
        PanDelta = Vector2.zero;
        PinchDelta = 0f;
        IsPinching = false;

        if (IsTouchDevice || Input.touchCount > 0)
        {
            ProcessTouchInput();
        }
        else
        {
            ProcessMouseInput();
        }

        // Always update world position
        WorldTapPosition = _mainCamera.ScreenToWorldPoint(ScreenPosition);
    }

    // ========================================================================
    // MOUSE INPUT (Desktop / Editor)
    // ========================================================================

    private void ProcessMouseInput()
    {
        TouchCount = 0;
        ScreenPosition = Input.mousePosition;

        // --- TAP (Left Click) ---
        TapDown = Input.GetMouseButtonDown(0);
        TapHeld = Input.GetMouseButton(0);
        TapUp   = Input.GetMouseButtonUp(0);

        // --- PAN (Right Click Drag) ---
        if (Input.GetMouseButtonDown(1))
        {
            _lastPanPosition = ScreenPosition;
            _isPanActive = true;
        }

        if (Input.GetMouseButton(1) && _isPanActive)
        {
            Vector2 currentPos = ScreenPosition;
            Vector2 screenDelta = currentPos - _lastPanPosition;

            if (screenDelta.magnitude > _panDeadzone || IsPanning)
            {
                // Convert screen delta to world delta
                Vector2 worldCurrent = _mainCamera.ScreenToWorldPoint(currentPos);
                Vector2 worldLast    = _mainCamera.ScreenToWorldPoint(_lastPanPosition);
                PanDelta = worldLast - worldCurrent;
                IsPanning = true;
            }

            _lastPanPosition = currentPos;
        }

        if (Input.GetMouseButtonUp(1))
        {
            IsPanning = false;
            _isPanActive = false;
        }

        // --- ZOOM (Scroll Wheel) ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            PinchDelta = scroll;
            IsPinching = true;
        }
    }

    // ========================================================================
    // TOUCH INPUT (Mobile)
    // ========================================================================

    private void ProcessTouchInput()
    {
        TouchCount = Input.touchCount;

        if (TouchCount == 0)
        {
            TapHeld = false;
            IsPanning = false;
            _isPanActive = false;
            return;
        }

        Touch firstTouch = Input.GetTouch(0);
        ScreenPosition = firstTouch.position;

        // --- SINGLE TOUCH ---
        if (TouchCount == 1)
        {
            switch (firstTouch.phase)
            {
                case TouchPhase.Began:
                    TapDown = true;
                    TapHeld = true;
                    _lastPanPosition = firstTouch.position;
                    _isPanActive = false;  // Don't start panning immediately
                    break;

                case TouchPhase.Moved:
                    TapHeld = true;
                    Vector2 moveDelta = firstTouch.position - _lastPanPosition;

                    // Only start panning after deadzone
                    if (!_isPanActive && moveDelta.magnitude > _panDeadzone)
                    {
                        _isPanActive = true;
                    }

                    if (_isPanActive)
                    {
                        IsPanning = true;
                        Vector2 worldCurrent = _mainCamera.ScreenToWorldPoint(firstTouch.position);
                        Vector2 worldLast    = _mainCamera.ScreenToWorldPoint(_lastPanPosition);
                        PanDelta = worldLast - worldCurrent;
                    }

                    _lastPanPosition = firstTouch.position;
                    break;

                case TouchPhase.Stationary:
                    TapHeld = true;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    TapUp = true;
                    TapHeld = false;
                    IsPanning = false;
                    _isPanActive = false;
                    break;
            }
        }

        // --- TWO-FINGER PINCH ---
        if (TouchCount >= 2)
        {
            Touch secondTouch = Input.GetTouch(1);
            IsPanning = false;  // Cancel pan during pinch

            float currentDistance = Vector2.Distance(firstTouch.position, secondTouch.position);

            if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
            {
                _lastPinchDistance = currentDistance;
            }
            else
            {
                float delta = currentDistance - _lastPinchDistance;
                PinchDelta = delta * _pinchSensitivity;
                IsPinching = Mathf.Abs(delta) > 1f;  // minimum threshold
                _lastPinchDistance = currentDistance;
            }

            // Center point between two fingers for screen position
            ScreenPosition = (firstTouch.position + secondTouch.position) * 0.5f;
        }
    }

    // ========================================================================
    // PUBLIC HELPERS
    // ========================================================================

    /// <summary>
    /// Returns true if the tap this frame was a "clean tap" (not a pan drag).
    /// Use this for building placement — ignores accidental taps during panning.
    /// </summary>
    public static bool IsCleanTap => TapDown && !IsPanning;

    /// <summary>
    /// Performs a 2D raycast at the current world tap position.
    /// Returns the hit result. Check hit.collider != null.
    /// </summary>
    public static RaycastHit2D RaycastAtPointer()
    {
        return Physics2D.Raycast(WorldTapPosition, Vector2.zero);
    }

    /// <summary>
    /// Forces a camera reference refresh. Call after scene loads.
    /// </summary>
    public void RefreshCamera()
    {
        _mainCamera = Camera.main;
    }
}
