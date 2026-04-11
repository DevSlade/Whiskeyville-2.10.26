// ============================================================================
// AMBIENTCLOUDSYSTEM.CS
// ============================================================================
// PURPOSE:       Spawns cloud sprites that drift slowly across the sky.
//                Clouds wrap relative to the camera viewport — they always fill
//                the visible area regardless of how far the player pans.
// VERSION:       v1
// CREATED:       April 10, 2026
// ATTACHED TO:   A dedicated "AmbientSystems" GameObject in GameScene
// SETUP:
//   1. Assign at least one sprite to _cloudSprites
//   2. Set _yMin / _yMax to the Y range in world space where clouds should appear
//   3. Adjust _sortingOrder so clouds sit above sky but below buildings
//      (recommend: "Background" layer, sortingOrder = 10)
// ============================================================================

using UnityEngine;

public class AmbientCloudSystem : MonoBehaviour
{
    // =========================================================================
    // INSPECTOR
    // =========================================================================

    [Header("Cloud Sprites")]
    [Tooltip("Pool of cloud sprites to randomly pick from. Assign 2-4 variants for variety.")]
    [SerializeField] private Sprite[] _cloudSprites;

    [Header("Count & Speed")]
    [Tooltip("Number of cloud GameObjects to spawn. More = denser sky.")]
    [SerializeField] private int _cloudCount = 5;

    [Tooltip("Base drift speed in world units per second (positive = left to right).")]
    [SerializeField] private float _speedMin = 0.3f;

    [Tooltip("Max drift speed — each cloud gets a random value in [speedMin, speedMax].")]
    [SerializeField] private float _speedMax = 0.7f;

    [Header("Position")]
    [Tooltip("Min Y world position for cloud spawning. Set this to the top of your sky zone.")]
    [SerializeField] private float _yMin = 6f;

    [Tooltip("Max Y world position for cloud spawning.")]
    [SerializeField] private float _yMax = 10f;

    [Tooltip("Extra buffer beyond the camera edge before wrapping (world units).")]
    [SerializeField] private float _wrapBuffer = 2f;

    [Header("Scale")]
    [Tooltip("Min random scale for each cloud. Variance makes the sky feel natural.")]
    [SerializeField] private float _scaleMin = 0.8f;

    [Tooltip("Max random scale.")]
    [SerializeField] private float _scaleMax = 1.6f;

    [Header("Rendering")]
    [Tooltip("Sorting layer name. Use 'Background' to keep clouds behind buildings.")]
    [SerializeField] private string _sortingLayerName = "Background";

    [Tooltip("Sorting order within the layer. Higher = in front.")]
    [SerializeField] private int _sortingOrder = 10;

    [Tooltip("Base cloud color. Set alpha < 1 for a softer, dreamy look.")]
    [SerializeField] private Color _cloudColor = new Color(1f, 1f, 1f, 0.75f);

    // =========================================================================
    // PRIVATE STATE
    // =========================================================================

    // One entry per cloud: the transform + its individual drift speed
    private Transform[]  _cloudTransforms;
    private float[]      _cloudSpeeds;
    private Camera       _cam;

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

    private void Start()
    {
        _cam = Camera.main;

        if (_cloudSprites == null || _cloudSprites.Length == 0)
        {
            Debug.LogWarning("[AmbientCloudSystem] ⚠️ No cloud sprites assigned — clouds disabled.");
            enabled = false;
            return;
        }

        SpawnClouds();
    }

    private void Update()
    {
        if (_cam == null) return;

        // Camera-relative bounds recalculated each frame — follows camera pan
        float halfWidth  = _cam.orthographicSize * _cam.aspect;
        float rightBound = _cam.transform.position.x + halfWidth + _wrapBuffer;
        float leftBound  = _cam.transform.position.x - halfWidth - _wrapBuffer;

        for (int i = 0; i < _cloudTransforms.Length; i++)
        {
            if (_cloudTransforms[i] == null) continue;

            // Move cloud rightward
            _cloudTransforms[i].position += Vector3.right * (_cloudSpeeds[i] * Time.deltaTime);

            // Wrap: when the cloud exits the right edge, reset it to the left edge
            if (_cloudTransforms[i].position.x > rightBound)
            {
                ResetCloud(i, leftBound);
            }
        }
    }

    // =========================================================================
    // SPAWN
    // =========================================================================

    private void SpawnClouds()
    {
        _cloudTransforms = new Transform[_cloudCount];
        _cloudSpeeds     = new float[_cloudCount];

        float halfWidth = _cam.orthographicSize * _cam.aspect;

        for (int i = 0; i < _cloudCount; i++)
        {
            // Spread clouds across the full camera width initially so they
            // don't all arrive from the left at the same time
            float startX = _cam.transform.position.x
                         - halfWidth
                         + (i / (float)_cloudCount) * (halfWidth * 2f);

            float startY = Random.Range(_yMin, _yMax);

            GameObject cloud = new GameObject($"Cloud_{i}");
            cloud.transform.SetParent(transform); // Child of this manager
            cloud.transform.position = new Vector3(startX, startY, 0f);

            // SpriteRenderer setup
            SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite           = _cloudSprites[Random.Range(0, _cloudSprites.Length)];
            sr.sortingLayerName = _sortingLayerName;
            sr.sortingOrder     = _sortingOrder;
            sr.color            = _cloudColor;

            // Random scale
            float scale = Random.Range(_scaleMin, _scaleMax);
            cloud.transform.localScale = Vector3.one * scale;

            // Random speed per cloud
            _cloudSpeeds[i]     = Random.Range(_speedMin, _speedMax);
            _cloudTransforms[i] = cloud.transform;
        }

        Debug.Log($"[AmbientCloudSystem] ☁️ Spawned {_cloudCount} clouds.");
    }

    // =========================================================================
    // WRAP
    // =========================================================================

    /// <summary>
    /// Resets a cloud to the left edge with new random Y, scale, and sprite.
    /// Called when the cloud drifts past the right camera bound.
    /// </summary>
    private void ResetCloud(int index, float leftBound)
    {
        Transform t = _cloudTransforms[index];

        t.position = new Vector3(
            leftBound,
            Random.Range(_yMin, _yMax),
            0f
        );

        float scale = Random.Range(_scaleMin, _scaleMax);
        t.localScale = Vector3.one * scale;

        // Randomize sprite and speed for variety on each wrap
        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = _cloudSprites[Random.Range(0, _cloudSprites.Length)];

        _cloudSpeeds[index] = Random.Range(_speedMin, _speedMax);
    }
}
