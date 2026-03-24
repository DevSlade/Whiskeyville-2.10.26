// ============================================================================
// INTROVIDEOCONTROLLER.CS
// ============================================================================
// PURPOSE:       Plays intro video and transitions to MainMenu when done
// ATTACHED TO:  IntroScene → VideoPlayer GameObject
// DEPENDENCIES: UnityEngine.Video, UnityEngine.SceneManagement
// ============================================================================

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroVideoController : MonoBehaviour
{
    // ========================================================================
    // 🎬 INSPECTOR SETTINGS
    // ========================================================================

    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load after video")]
    [SerializeField] private string _nextSceneName = "MainMenu";

    [Header("Skip Settings")]
    [Tooltip("Allow skipping video with any key/click")]
    [SerializeField] private bool _allowSkip = true;

    [Tooltip("Delay before skip is allowed (seconds)")]
    [SerializeField] private float _skipDelay = 1f;

    // ========================================================================
    // 🔒 PRIVATE STATE
    // ========================================================================

    private VideoPlayer _videoPlayer;
    private bool _isTransitioning = false;
    private bool _canSkip = false;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Start()
    {
        _videoPlayer = GetComponent<VideoPlayer>();

        if (_videoPlayer == null)
        {
            Debug.LogError("[IntroVideoController] ❌ No VideoPlayer found!  Loading next scene.");
            LoadNextScene();
            return;
        }

        // Subscribe to video end event
        _videoPlayer.loopPointReached += OnVideoEnd;

        // Start video
        _videoPlayer.Play();

        Debug.Log("[IntroVideoController] 🎬 Video started.");

        // Enable skip after delay
        if (_allowSkip)
        {
            StartCoroutine(EnableSkipAfterDelay());
        }
    }

    private void Update()
    {
        // Skip video on any input
        if (_allowSkip && _canSkip && !_isTransitioning)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                Debug.Log("[IntroVideoController] ⏭️ Video skipped.");
                LoadNextScene();
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }

    // ========================================================================
    // 🎬 VIDEO EVENTS
    // ========================================================================

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("[IntroVideoController] ✅ Video finished.");
        LoadNextScene();
    }

    // ========================================================================
    // 🔄 SCENE LOADING
    // ========================================================================

    private void LoadNextScene()
    {
        if (_isTransitioning) return;

        _isTransitioning = true;

        Debug.Log($"[IntroVideoController] 🚀 Loading scene: {_nextSceneName}");
        SceneManager.LoadScene(_nextSceneName);
    }

    // ========================================================================
    // ⏳ SKIP DELAY
    // ========================================================================

    private IEnumerator EnableSkipAfterDelay()
    {
        yield return new WaitForSeconds(_skipDelay);
        _canSkip = true;
        Debug.Log("[IntroVideoController] ⏭️ Skip enabled.");
    }
}