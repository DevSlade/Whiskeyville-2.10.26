// ============================================================================
// AUDIOMANAGER.CS
// ============================================================================
// PURPOSE:      Singleton for playing SFX and Music throughout the game
// ATTACHED TO:   AudioManager GameObject (persists across scenes)
// ARCHITECTURE: Singleton pattern, DontDestroyOnLoad
// ============================================================================
// AUDIO SOURCES:
//   🎵 _musicSource = Looping background music
//   🔊 _sfxSource   = One-shot sound effects
// ============================================================================
// USAGE:
//   AudioManager.Instance.PlaySFX(clipName);
//   AudioManager. Instance.PlayMusic(clipName);
//   AudioManager.Instance.SetMusicVolume(0.5f);
//   AudioManager.Instance.SetSFXVolume(0.8f);
// ============================================================================

using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static AudioManager Instance { get; private set; }

    // ========================================================================
    // 🎵 INSPECTOR - AUDIO SOURCES
    // ========================================================================

    [Header("🎵 Audio Sources")]
    [Tooltip("AudioSource for background music (looping)")]
    [SerializeField] private AudioSource _musicSource;

    [Tooltip("AudioSource for sound effects (one-shot)")]
    [SerializeField] private AudioSource _sfxSource;

    // ========================================================================
    // 🔊 INSPECTOR - AUDIO CLIPS
    // ========================================================================

    [Header("🔊 Sound Effects")]
    [Tooltip("SFX:  Building placed")]
    [SerializeField] private AudioClip _sfxPlace;

    [Tooltip("SFX: Resource collected/produced")]
    [SerializeField] private AudioClip _sfxCollect;

    [Tooltip("SFX: Button click")]
    [SerializeField] private AudioClip _sfxClick;

    [Tooltip("SFX: Error/invalid action")]
    [SerializeField] private AudioClip _sfxError;

    [Tooltip("SFX: Success/positive feedback")]
    [SerializeField] private AudioClip _sfxSuccess;

    [Header("🎵 Music Tracks")]
    [Tooltip("Main menu music")]
    [SerializeField] private AudioClip _musicMenu;

    [Tooltip("Gameplay music")]
    [SerializeField] private AudioClip _musicGameplay;

    // ========================================================================
    // 🔊 INSPECTOR - VOLUME SETTINGS
    // ========================================================================

    [Header("🔊 Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _musicVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 1f;

    // ========================================================================
    // 🔒 PRIVATE DATA
    // ========================================================================

    /// <summary>
    /// Dictionary for quick SFX lookup by name. 
    /// </summary>
    private Dictionary<string, AudioClip> _sfxClips = new Dictionary<string, AudioClip>();

    /// <summary>
    /// Dictionary for quick Music lookup by name.
    /// </summary>
    private Dictionary<string, AudioClip> _musicClips = new Dictionary<string, AudioClip>();

    // ========================================================================
    // 📛 AUDIO CLIP NAMES (Constants)
    // ========================================================================

    public const string SFX_PLACE = "Place";
    public const string SFX_COLLECT = "Collect";
    public const string SFX_CLICK = "Click";
    public const string SFX_ERROR = "Error";
    public const string SFX_SUCCESS = "Success";

    public const string MUSIC_MENU = "Menu";
    public const string MUSIC_GAMEPLAY = "Gameplay";

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        // ---- 🌐 SINGLETON SETUP ----
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            RegisterAudioClips();
            Debug.Log("[AudioManager] 🌐 Singleton initialized.");
        }
        else
        {
            Debug.LogWarning("[AudioManager] ⚠️ Duplicate destroyed.");
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // 🔧 INITIALIZATION
    // ========================================================================

    /// <summary>
    /// Creates AudioSources if not assigned in Inspector.
    /// </summary>
    private void InitializeAudioSources()
    {
        // ---- 🎵 MUSIC SOURCE ----
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            Debug.Log("[AudioManager] 🎵 Created Music AudioSource.");
        }
        _musicSource.volume = _musicVolume;

        // ---- 🔊 SFX SOURCE ----
        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource. playOnAwake = false;
            Debug.Log("[AudioManager] 🔊 Created SFX AudioSource.");
        }
        _sfxSource.volume = _sfxVolume;
    }

    /// <summary>
    /// Registers all audio clips into dictionaries for name-based lookup.
    /// </summary>
    private void RegisterAudioClips()
    {
        // ---- 🔊 REGISTER SFX ----
        if (_sfxPlace != null) _sfxClips[SFX_PLACE] = _sfxPlace;
        if (_sfxCollect != null) _sfxClips[SFX_COLLECT] = _sfxCollect;
        if (_sfxClick != null) _sfxClips[SFX_CLICK] = _sfxClick;
        if (_sfxError != null) _sfxClips[SFX_ERROR] = _sfxError;
        if (_sfxSuccess != null) _sfxClips[SFX_SUCCESS] = _sfxSuccess;

        // ---- 🎵 REGISTER MUSIC ----
        if (_musicMenu != null) _musicClips[MUSIC_MENU] = _musicMenu;
        if (_musicGameplay != null) _musicClips[MUSIC_GAMEPLAY] = _musicGameplay;

        Debug.Log($"[AudioManager] 📂 Registered {_sfxClips.Count} SFX, {_musicClips.Count} Music tracks.");
    }

    // ========================================================================
    // 🔊 PUBLIC METHODS - SFX
    // ========================================================================

    /// <summary>
    /// Plays a sound effect by name.
    /// </summary>
    /// <param name="sfxName">Name of SFX (use constants)</param>
    public void PlaySFX(string sfxName)
    {
        if (_sfxClips.TryGetValue(sfxName, out AudioClip clip))
        {
            _sfxSource.PlayOneShot(clip, _sfxVolume);
            Debug.Log($"[AudioManager] 🔊 Playing SFX: {sfxName}");
        }
        else
        {
            Debug.LogWarning($"[AudioManager] ⚠️ SFX not found: {sfxName}");
        }
    }

    /// <summary>
    /// Plays a sound effect directly from AudioClip reference.
    /// </summary>
    /// <param name="clip">AudioClip to play</param>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }
    }

    // ========================================================================
    // 🎵 PUBLIC METHODS - MUSIC
    // ========================================================================

    /// <summary>
    /// Plays background music by name.  Stops current music first.
    /// </summary>
    /// <param name="musicName">Name of music track (use constants)</param>
    public void PlayMusic(string musicName)
    {
        if (_musicClips.TryGetValue(musicName, out AudioClip clip))
        {
            // ---- 🛑 STOP CURRENT ----
            if (_musicSource. isPlaying)
            {
                _musicSource.Stop();
            }

            // ---- ▶️ PLAY NEW ----
            _musicSource.clip = clip;
            _musicSource.Play();
            Debug.Log($"[AudioManager] 🎵 Playing Music:  {musicName}");
        }
        else
        {
            Debug.LogWarning($"[AudioManager] ⚠️ Music not found: {musicName}");
        }
    }

    /// <summary>
    /// Stops currently playing music.
    /// </summary>
    public void StopMusic()
    {
        if (_musicSource.isPlaying)
        {
            _musicSource. Stop();
            Debug.Log("[AudioManager] 🛑 Music stopped.");
        }
    }

    /// <summary>
    /// Pauses currently playing music.
    /// </summary>
    public void PauseMusic()
    {
        if (_musicSource.isPlaying)
        {
            _musicSource.Pause();
            Debug. Log("[AudioManager] ⏸️ Music paused.");
        }
    }

    /// <summary>
    /// Resumes paused music.
    /// </summary>
    public void ResumeMusic()
    {
        _musicSource.UnPause();
        Debug.Log("[AudioManager] ▶️ Music resumed.");
    }

    // ========================================================================
    // 🔊 PUBLIC METHODS - VOLUME
    // ========================================================================

    /// <summary>
    /// Sets music volume (0-1).
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        _musicSource. volume = _musicVolume;
        Debug.Log($"[AudioManager] 🎵 Music volume:  {_musicVolume}");
    }

    /// <summary>
    /// Sets SFX volume (0-1).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        _sfxSource.volume = _sfxVolume;
        Debug. Log($"[AudioManager] 🔊 SFX volume: {_sfxVolume}");
    }

    /// <summary>
    /// Gets current music volume.
    /// </summary>
    public float GetMusicVolume() => _musicVolume;

    /// <summary>
    /// Gets current SFX volume.
    /// </summary>
    public float GetSFXVolume() => _sfxVolume;
}