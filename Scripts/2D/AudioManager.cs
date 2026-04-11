// ============================================================================
// AUDIOMANAGER.CS
// ============================================================================
// PURPOSE:      Singleton for ALL audio in Whiskeyville
//               SFX, music, ambience, crossfade, day/night transitions
// VERSION:      v3 — SFX cooldown (anti-spam), pitch variance, variant clip support
// UPDATED:      April 8, 2026
// ATTACHED TO:  AudioManager GameObject (persists across scenes)
// ============================================================================
// AUDIO SOURCES:
//   🎵 _musicSource   = Looping background music (crossfade-capable)
//   🔊 _sfxSource     = One-shot sound effects
//   🌿 _ambientSource = Looping ambient soundscape (day/night)
// ============================================================================
// USAGE:
//   AudioManager.Instance.PlaySFX(AudioManager.SFX_CHOP);
//   AudioManager.Instance.PlayMusic(AudioManager.MUSIC_MENU);
//   AudioManager.Instance.TransitionToDay();
//   AudioManager.Instance.TransitionToNight();
//   AudioManager.Instance.SetMusicVolume(0.5f);
// ============================================================================
// INSPECTOR HOOKUP — Audio clip → slot mapping:
//   _sfxClick     → button-20.mp3
//   _sfxError     → error.ogg
//   _sfxBuild     → hammering-1.mp3
//   _sfxDemolish  → cassette-out-1.mp3
//   _sfxChop      → wood-chop-axe-hit-01.mp3
//   _sfxHoe       → marker-1.mp3
//   _sfxHarvest   → typewriter-backspace-1.mp3
//   _sfxCash      → writing-signature-1.mp3
//   _sfxCollect   → button-22.mp3
//   _sfxSuccess   → button-30.mp3
//   _sfxPlace     → button-21.mp3
//   _musicMenu    → gone_fishin_by_memoraphile_CC0.mp3
//   _musicDay     → Tambul.mp3
//   (remaining slots: source via Suno / ElevenLabs — leave empty for now)
// ============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Defines a set of variant audio clips for a single SFX name.
/// When variants exist, PlaySFX picks one randomly for natural sound.
/// Add entries in the Inspector under "SFX Variants".
/// </summary>
[System.Serializable]
public class SFXVariantSet
{
    [Tooltip("Must match an SFX constant name (e.g. Click, Chop, Build, Harvest)")]
    public string sfxName;

    [Tooltip("Multiple clips to randomly select from. Adds natural variation.")]
    public AudioClip[] clips;
}

public class AudioManager : MonoBehaviour
{
    // ========================================================================
    // 🌐 SINGLETON
    // ========================================================================

    public static AudioManager Instance { get; private set; }

    // ========================================================================
    // 🎵 INSPECTOR — AUDIO SOURCES
    // ========================================================================

    [Header("🎵 Audio Sources")]
    [Tooltip("Loops background music — supports crossfade")]
    [SerializeField] private AudioSource _musicSource;

    [Tooltip("Plays one-shot sound effects")]
    [SerializeField] private AudioSource _sfxSource;

    [Tooltip("Loops ambient soundscape (birds, crickets, rain)")]
    [SerializeField] private AudioSource _ambientSource;

    // ========================================================================
    // 🔊 INSPECTOR — SFX CLIPS
    // ========================================================================

    [Header("🔊 Core SFX")]
    [Tooltip("SFX: UI button click")]
    [SerializeField] private AudioClip _sfxClick;

    [Tooltip("SFX: Invalid action / error")]
    [SerializeField] private AudioClip _sfxError;

    [Tooltip("SFX: Generic resource collect (fallback)")]
    [SerializeField] private AudioClip _sfxCollect;

    [Tooltip("SFX: Generic success / positive feedback")]
    [SerializeField] private AudioClip _sfxSuccess;

    [Tooltip("SFX: Building placement (alias: _sfxBuild)")]
    [SerializeField] private AudioClip _sfxPlace;

    [Header("🏗️ Construction SFX")]
    [Tooltip("SFX: Building placed — hammering, thud")]
    [SerializeField] private AudioClip _sfxBuild;

    [Tooltip("SFX: Building demolished — crumble, crash")]
    [SerializeField] private AudioClip _sfxDemolish;

    [Tooltip("SFX: Hoe tilling soil — earthy scrape")]
    [SerializeField] private AudioClip _sfxHoe;

    [Header("🌽 Farm & Production SFX")]
    [Tooltip("SFX: Crop harvested — snap, rustle")]
    [SerializeField] private AudioClip _sfxHarvest;

    [Tooltip("SFX: Axe chop on tree — wood crack")]
    [SerializeField] private AudioClip _sfxChop;

    [Tooltip("SFX: Cooperage output — barrel roll, seal")]
    [SerializeField] private AudioClip _sfxBarrel;

    [Tooltip("SFX: Still output — whiskey pour, drip")]
    [SerializeField] private AudioClip _sfxPour;

    [Tooltip("SFX: Rickhouse aging complete — shimmer reveal")]
    [SerializeField] private AudioClip _sfxAge;

    [Tooltip("SFX: Saloon sale — cash register, coins")]
    [SerializeField] private AudioClip _sfxCash;

    [Header("🌅 Transition SFX")]
    [Tooltip("SFX: Day begins — rooster crow or chime")]
    [SerializeField] private AudioClip _sfxDay;

    [Tooltip("SFX: Night begins — crickets swell or chime")]
    [SerializeField] private AudioClip _sfxNight;

    [Header("📋 UI Panel SFX")]
    [Tooltip("SFX: UI panel opens (fallback: _sfxClick if empty)")]
    [SerializeField] private AudioClip _sfxPanelOpen;

    [Tooltip("SFX: UI panel closes (fallback: _sfxClick if empty)")]
    [SerializeField] private AudioClip _sfxPanelClose;

    [Header("🌳 Additional SFX")]
    [Tooltip("SFX: Tree falling after chop (Phase 2 — leave empty for now)")]
    [SerializeField] private AudioClip _sfxTreeFall;

    [Tooltip("SFX: Dev tools activated (Phase 2 — leave empty for now)")]
    [SerializeField] private AudioClip _sfxDev;

    // ========================================================================
    // 🎵 INSPECTOR — MUSIC CLIPS
    // ========================================================================

    [Header("🎵 Music Tracks")]
    [Tooltip("Main menu music — plays on startup")]
    [SerializeField] private AudioClip _musicMenu;

    [Tooltip("Daytime gameplay music — warm, acoustic, productive")]
    [SerializeField] private AudioClip _musicDay;

    [Tooltip("Nighttime gameplay music — mellow, peaceful")]
    [SerializeField] private AudioClip _musicNight;

    [Tooltip("Alias: legacy gameplay music (used if _musicDay is unassigned)")]
    [SerializeField] private AudioClip _musicGameplay;

    // ========================================================================
    // 🌿 INSPECTOR — AMBIENT CLIPS
    // ========================================================================

    [Header("🌿 Ambient Soundscapes")]
    [Tooltip("Daytime ambient loop — birds, wind, distant rooster")]
    [SerializeField] private AudioClip _ambientDay;

    [Tooltip("Nighttime ambient loop — crickets, frogs, owl")]
    [SerializeField] private AudioClip _ambientNight;

    // ========================================================================
    // 🔊 INSPECTOR — VOLUME SETTINGS
    // ========================================================================

    [Header("🔊 Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _musicVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float _ambientVolume = 0.35f;

    [Header("🎚️ Crossfade")]
    [Tooltip("Default duration in seconds for music crossfades (longer = smoother day/night)")]
    [SerializeField] private float _defaultCrossfadeDuration = 6f;

    // ========================================================================
    // 🛡️ INSPECTOR — SFX ANTI-SPAM & VARIATION
    // ========================================================================

    [Header("🛡️ SFX Anti-Spam")]
    [Tooltip("Minimum seconds between plays of the SAME SFX (prevents rapid-fire spam)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float _sfxCooldown = 0.05f;

    [Tooltip("Random pitch variance for natural sound variation (0 = none, 0.1 = +/-10%)")]
    [Range(0f, 0.2f)]
    [SerializeField] private float _sfxPitchVariance = 0.05f;

    [Header("🎲 SFX Variants (optional)")]
    [Tooltip("Add variant clips for any SFX. System picks one randomly each play for variety.")]
    [SerializeField] private SFXVariantSet[] _sfxVariantSets;

    // ========================================================================
    // 📛 CONSTANTS — SFX NAMES
    // ========================================================================

    // Core
    public const string SFX_CLICK    = "Click";
    public const string SFX_ERROR    = "Error";
    public const string SFX_COLLECT  = "Collect";
    public const string SFX_SUCCESS  = "Success";
    public const string SFX_PLACE    = "Place";   // kept for backward compat

    // Construction
    public const string SFX_BUILD    = "Build";
    public const string SFX_DEMOLISH = "Demolish";
    public const string SFX_HOE      = "Hoe";

    // Farm & Production
    public const string SFX_HARVEST  = "Harvest";
    public const string SFX_CHOP     = "Chop";
    public const string SFX_BARREL   = "Barrel";
    public const string SFX_POUR     = "Pour";
    public const string SFX_AGE      = "Age";
    public const string SFX_CASH     = "Cash";

    // Transitions
    public const string SFX_DAY      = "Day";
    public const string SFX_NIGHT    = "Night";

    // UI Panels
    public const string SFX_PANEL_OPEN  = "PanelOpen";
    public const string SFX_PANEL_CLOSE = "PanelClose";

    // Phase 2 — assign clips when ready, fallback to existing
    public const string SFX_TREE_FALL = "TreeFall";
    public const string SFX_DEV       = "Dev";

    // ========================================================================
    // 📛 CONSTANTS — MUSIC NAMES
    // ========================================================================

    public const string MUSIC_MENU     = "Menu";
    public const string MUSIC_DAY      = "MusicDay";
    public const string MUSIC_NIGHT    = "MusicNight";
    public const string MUSIC_GAMEPLAY = "Gameplay"; // legacy alias → MusicDay

    // ========================================================================
    // 📛 CONSTANTS — AMBIENT NAMES
    // ========================================================================

    public const string AMBIENT_DAY   = "AmbientDay";
    public const string AMBIENT_NIGHT = "AmbientNight";

    // ========================================================================
    // 🔒 PRIVATE — DICTIONARIES
    // ========================================================================

    private Dictionary<string, AudioClip> _sfxClips     = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _musicClips   = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _ambientClips = new Dictionary<string, AudioClip>();

    // Anti-spam: tracks last play time per SFX name
    private Dictionary<string, float> _sfxLastPlayTime = new Dictionary<string, float>();

    // Variant clips: multiple options per SFX for random selection
    private Dictionary<string, AudioClip[]> _sfxVariants = new Dictionary<string, AudioClip[]>();

    private Coroutine _crossfadeCoroutine;

    // ========================================================================
    // ⚙️ UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            LoadVolumeFromPrefs();
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

    private void InitializeAudioSources()
    {
        // ---- 🎵 MUSIC SOURCE ----
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
        }
        _musicSource.volume = _musicVolume;

        // ---- 🔊 SFX SOURCE ----
        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }
        _sfxSource.volume = _sfxVolume;

        // ---- 🌿 AMBIENT SOURCE ----
        if (_ambientSource == null)
        {
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
        }
        _ambientSource.volume = _ambientVolume;

        Debug.Log("[AudioManager] 🔊 Audio sources ready.");
    }

    private void LoadVolumeFromPrefs()
    {
        _musicVolume   = PlayerPrefs.GetFloat("MusicVolume",   _musicVolume);
        _sfxVolume     = PlayerPrefs.GetFloat("SFXVolume",     _sfxVolume);
        _ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", _ambientVolume);
    }

    private void RegisterAudioClips()
    {
        // ---- 🔊 SFX ----
        Register(_sfxClips, SFX_CLICK,    _sfxClick);
        Register(_sfxClips, SFX_ERROR,    _sfxError);
        Register(_sfxClips, SFX_COLLECT,  _sfxCollect);
        Register(_sfxClips, SFX_SUCCESS,  _sfxSuccess);
        Register(_sfxClips, SFX_PLACE,    _sfxPlace);
        Register(_sfxClips, SFX_BUILD,    _sfxBuild    != null ? _sfxBuild : _sfxPlace);
        Register(_sfxClips, SFX_DEMOLISH, _sfxDemolish);
        Register(_sfxClips, SFX_HOE,      _sfxHoe);
        Register(_sfxClips, SFX_HARVEST,  _sfxHarvest);
        Register(_sfxClips, SFX_CHOP,     _sfxChop);
        Register(_sfxClips, SFX_BARREL,   _sfxBarrel);
        Register(_sfxClips, SFX_POUR,     _sfxPour);
        Register(_sfxClips, SFX_AGE,      _sfxAge);
        Register(_sfxClips, SFX_CASH,     _sfxCash);
        Register(_sfxClips, SFX_DAY,      _sfxDay);
        Register(_sfxClips, SFX_NIGHT,    _sfxNight);

        // UI Panel SFX — fallback to click if no dedicated clip assigned
        Register(_sfxClips, SFX_PANEL_OPEN,  _sfxPanelOpen  != null ? _sfxPanelOpen  : _sfxClick);
        Register(_sfxClips, SFX_PANEL_CLOSE, _sfxPanelClose != null ? _sfxPanelClose : _sfxClick);

        // Phase 2 SFX — fallback to existing
        Register(_sfxClips, SFX_TREE_FALL, _sfxTreeFall != null ? _sfxTreeFall : _sfxChop);
        Register(_sfxClips, SFX_DEV,       _sfxDev      != null ? _sfxDev      : _sfxSuccess);

        // ---- 🎵 MUSIC ----
        Register(_musicClips, MUSIC_MENU,     _musicMenu);
        Register(_musicClips, MUSIC_DAY,      _musicDay != null ? _musicDay : _musicGameplay);
        Register(_musicClips, MUSIC_NIGHT,    _musicNight);
        Register(_musicClips, MUSIC_GAMEPLAY, _musicDay != null ? _musicDay : _musicGameplay);

        // ---- 🌿 AMBIENT ----
        Register(_ambientClips, AMBIENT_DAY,   _ambientDay);
        Register(_ambientClips, AMBIENT_NIGHT, _ambientNight);

        // ---- 🎲 SFX VARIANTS ----
        if (_sfxVariantSets != null)
        {
            foreach (SFXVariantSet variantSet in _sfxVariantSets)
            {
                if (!string.IsNullOrEmpty(variantSet.sfxName) &&
                    variantSet.clips != null && variantSet.clips.Length > 0)
                {
                    _sfxVariants[variantSet.sfxName] = variantSet.clips;
                }
            }
        }

        Debug.Log($"[AudioManager] 📂 Registered: {_sfxClips.Count} SFX | {_musicClips.Count} Music | {_ambientClips.Count} Ambient | {_sfxVariants.Count} Variant sets");
    }

    private void Register(Dictionary<string, AudioClip> dict, string key, AudioClip clip)
    {
        if (clip != null) dict[key] = clip;
    }

    // ========================================================================
    // 🔊 PUBLIC — SFX
    // ========================================================================

    /// <summary>
    /// Plays a sound effect by name constant with anti-spam cooldown,
    /// random variant selection, and pitch variance for natural sound.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        // ---- COOLDOWN CHECK — prevent rapid-fire audio spam ----
        if (_sfxCooldown > 0f && _sfxLastPlayTime.TryGetValue(sfxName, out float lastTime))
        {
            if (Time.unscaledTime - lastTime < _sfxCooldown) return;
        }

        AudioClip clip = null;

        // ---- VARIANT CHECK — pick random clip if variants exist ----
        if (_sfxVariants.TryGetValue(sfxName, out AudioClip[] variants) && variants.Length > 0)
        {
            clip = variants[Random.Range(0, variants.Length)];
        }
        // ---- FALLBACK — use single registered clip ----
        else if (_sfxClips.TryGetValue(sfxName, out AudioClip singleClip))
        {
            clip = singleClip;
        }

        if (clip != null)
        {
            // Apply pitch variance for natural sound (±5% default)
            if (_sfxPitchVariance > 0f)
                _sfxSource.pitch = 1f + Random.Range(-_sfxPitchVariance, _sfxPitchVariance);
            else
                _sfxSource.pitch = 1f;

            _sfxSource.PlayOneShot(clip);
            _sfxLastPlayTime[sfxName] = Time.unscaledTime;
        }
        else
        {
            Debug.LogWarning($"[AudioManager] ⚠️ SFX not found: {sfxName}");
        }
    }

    /// <summary>Plays a sound effect directly from an AudioClip reference. No cooldown.</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            _sfxSource.PlayOneShot(clip);
    }

    // ========================================================================
    // 🎵 PUBLIC — MUSIC
    // ========================================================================

    /// <summary>Immediately switches to a music track (no crossfade).</summary>
    public void PlayMusic(string musicName)
    {
        if (_musicClips.TryGetValue(musicName, out AudioClip clip))
        {
            if (_musicSource.isPlaying) _musicSource.Stop();
            _musicSource.clip = clip;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
            Debug.Log($"[AudioManager] 🎵 Playing: {musicName}");
        }
        else
        {
            Debug.LogWarning($"[AudioManager] ⚠️ Music not found: {musicName}");
        }
    }

    /// <summary>
    /// Crossfades from current music to a new track over [duration] seconds.
    /// If duration <= 0, switches immediately.
    /// </summary>
    public void CrossfadeMusic(string newTrack, float duration = -1f)
    {
        if (duration < 0f) duration = _defaultCrossfadeDuration;

        if (!_musicClips.TryGetValue(newTrack, out AudioClip clip))
        {
            Debug.LogWarning($"[AudioManager] ⚠️ CrossfadeMusic: track not found: {newTrack}");
            return;
        }

        if (duration <= 0f)
        {
            PlayMusic(newTrack);
            return;
        }

        if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
        _crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(clip, duration));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip, float duration)
    {
        float halfDuration = duration * 0.5f;
        float startVolume = _musicSource.volume;

        // ---- Fade out ----
        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
            yield return null;
        }

        // ---- Swap clip ----
        _musicSource.Stop();
        _musicSource.clip = newClip;
        _musicSource.Play();

        // ---- Fade in ----
        t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, _musicVolume, t / halfDuration);
            yield return null;
        }

        _musicSource.volume = _musicVolume;
        _crossfadeCoroutine = null;
        Debug.Log($"[AudioManager] 🎵 Crossfade complete: {newClip.name}");
    }

    public void StopMusic()
    {
        if (_musicSource.isPlaying)
        {
            _musicSource.Stop();
            Debug.Log("[AudioManager] 🛑 Music stopped.");
        }
    }

    public void PauseMusic()
    {
        if (_musicSource.isPlaying)
        {
            _musicSource.Pause();
            Debug.Log("[AudioManager] ⏸️ Music paused.");
        }
    }

    public void ResumeMusic()
    {
        _musicSource.UnPause();
        Debug.Log("[AudioManager] ▶️ Music resumed.");
    }

    // ========================================================================
    // 🌿 PUBLIC — AMBIENT
    // ========================================================================

    /// <summary>Plays an ambient loop by name (AMBIENT_DAY / AMBIENT_NIGHT).</summary>
    public void PlayAmbient(string ambientName)
    {
        if (_ambientClips.TryGetValue(ambientName, out AudioClip clip))
        {
            _ambientSource.clip = clip;
            _ambientSource.volume = _ambientVolume;
            _ambientSource.Play();
            Debug.Log($"[AudioManager] 🌿 Ambient: {ambientName}");
        }
        else
        {
            Debug.LogWarning($"[AudioManager] ⚠️ Ambient not found: {ambientName}");
        }
    }

    public void StopAmbient()
    {
        if (_ambientSource.isPlaying)
        {
            _ambientSource.Stop();
            Debug.Log("[AudioManager] 🌿 Ambient stopped.");
        }
    }

    // ========================================================================
    // 🌅 PUBLIC — DAY/NIGHT TRANSITIONS
    // ========================================================================

    /// <summary>
    /// Call when the game transitions to daytime.
    /// Plays day sting SFX + crossfades to day music + switches to day ambience.
    /// </summary>
    public void TransitionToDay()
    {
        Debug.Log("[AudioManager] 🌅 Transitioning to Day.");
        PlaySFX(SFX_DAY);
        CrossfadeMusic(MUSIC_DAY);
        PlayAmbient(AMBIENT_DAY);
    }

    /// <summary>
    /// Call when the game transitions to nighttime.
    /// Plays night sting SFX + crossfades to night music + switches to night ambience.
    /// </summary>
    public void TransitionToNight()
    {
        Debug.Log("[AudioManager] 🌙 Transitioning to Night.");
        PlaySFX(SFX_NIGHT);
        CrossfadeMusic(MUSIC_NIGHT);
        PlayAmbient(AMBIENT_NIGHT);
    }

    // ========================================================================
    // 🔊 PUBLIC — VOLUME CONTROL
    // ========================================================================

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        _musicSource.volume = _musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
        Debug.Log($"[AudioManager] 🎵 Music volume: {_musicVolume:F2}");
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        _sfxSource.volume = _sfxVolume;
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        Debug.Log($"[AudioManager] 🔊 SFX volume: {_sfxVolume:F2}");
    }

    public void SetAmbientVolume(float volume)
    {
        _ambientVolume = Mathf.Clamp01(volume);
        _ambientSource.volume = _ambientVolume;
        PlayerPrefs.SetFloat("AmbientVolume", _ambientVolume);
        Debug.Log($"[AudioManager] 🌿 Ambient volume: {_ambientVolume:F2}");
    }

    public float GetMusicVolume()   => _musicVolume;
    public float GetSFXVolume()     => _sfxVolume;
    public float GetAmbientVolume() => _ambientVolume;
}
