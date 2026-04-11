// ============================================================================
// AMBIENTFIREFYLSYSTEM.CS
// ============================================================================
// PURPOSE:       Firefly particles that fade in at dusk and fade out at dawn.
//                Emission rate scales with DayNightCycle.CycleProgress so
//                fireflies are brightest at midnight and absent at midday.
//                Configures the ParticleSystem entirely in code — zero
//                Inspector particle setup required. Just add the component.
// VERSION:       v1
// CREATED:       April 10, 2026
// ATTACHED TO:   A dedicated "AmbientSystems" GameObject in GameScene
// SETUP:
//   1. Add this component to a GameObject in GameScene
//   2. Assign _dayNightCycle reference in Inspector
//   3. Optionally adjust _spawnAreaSize to match your map size
//   That's it. Particle system is fully configured via code.
// ============================================================================

using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AmbientFireflySystem : MonoBehaviour
{
    // =========================================================================
    // INSPECTOR
    // =========================================================================

    [Header("References")]
    [Tooltip("Assign the DayNightCycle component from Main Camera.")]
    [SerializeField] private DayNightCycle _dayNightCycle;

    [Header("Emission")]
    [Tooltip("Max particles emitted per second at peak (midnight).")]
    [SerializeField] private float _maxEmissionRate = 12f;

    [Header("Spawn Area")]
    [Tooltip("Width and height of the world-space box that fireflies spawn in. " +
             "Set this to cover your entire playable map.")]
    [SerializeField] private Vector2 _spawnAreaSize = new Vector2(30f, 15f);

    [Header("Colors")]
    [Tooltip("Warm amber — firefly glow at start of lifetime.")]
    [SerializeField] private Color _colorStart = new Color(1f, 0.75f, 0.2f, 0.9f);

    [Tooltip("Soft yellow — firefly glow at end of lifetime.")]
    [SerializeField] private Color _colorEnd = new Color(1f, 0.95f, 0.5f, 0f); // Fades to transparent

    // =========================================================================
    // PRIVATE STATE
    // =========================================================================

    private ParticleSystem           _ps;
    private ParticleSystem.EmissionModule _emission;

    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();

        if (_dayNightCycle == null)
        {
            // Attempt auto-find — DayNightCycle is on Main Camera
            _dayNightCycle = FindObjectOfType<DayNightCycle>();
            if (_dayNightCycle == null)
            {
                Debug.LogWarning("[AmbientFireflySystem] ⚠️ DayNightCycle not found — fireflies disabled.");
                enabled = false;
                return;
            }
        }

        ConfigureParticleSystem();
    }

    private void Update()
    {
        // Scale emission rate based on how deep into night we are
        float nightBlend = CalculateNightBlend(_dayNightCycle.CycleProgress);

        var emission = _ps.emission;
        emission.rateOverTime = nightBlend * _maxEmissionRate;

        // Start/stop the system to save performance during full day
        if (nightBlend > 0f && !_ps.isPlaying)
            _ps.Play();
        else if (nightBlend <= 0f && _ps.isPlaying)
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); // Let existing particles finish
    }

    // =========================================================================
    // EMISSION CURVE
    // =========================================================================

    /// <summary>
    /// Returns a 0-1 blend value representing how "night" the current time is.
    /// 0 = full day (no fireflies), 1 = midnight (max fireflies).
    ///
    /// CycleProgress timeline:
    ///   0.00 = dawn         (no fireflies)
    ///   0.25 = midday       (no fireflies)
    ///   0.50 = dusk         (fireflies start)
    ///   0.75 = midnight     (max fireflies)
    ///   1.00 = dawn again   (no fireflies)
    /// </summary>
    private static float CalculateNightBlend(float t)
    {
        if (t < 0.5f)
        {
            // Full day — no fireflies
            return 0f;
        }
        else if (t < 0.75f)
        {
            // Dusk → midnight: ramp up 0→1
            return Mathf.InverseLerp(0.5f, 0.75f, t);
        }
        else
        {
            // Midnight → dawn: ramp down 1→0
            return 1f - Mathf.InverseLerp(0.75f, 1.0f, t);
        }
    }

    // =========================================================================
    // PARTICLE SYSTEM SETUP
    // =========================================================================

    /// <summary>
    /// Configures the ParticleSystem entirely in code.
    /// Called once on Awake — no Inspector particle setup required.
    /// </summary>
    private void ConfigureParticleSystem()
    {
        // ── Main Module ───────────────────────────────────────────────────────
        var main = _ps.main;
        main.loop             = true;
        main.playOnAwake      = false;        // We control Play/Stop in Update
        main.simulationSpace  = ParticleSystemSimulationSpace.World; // World-space particles
        main.maxParticles     = 80;

        // Lifetime: 3-5 seconds per firefly
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);

        // Speed: slow drift
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);

        // Size: small glowing dots
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);

        // Color: warm amber to soft yellow (randomized per particle)
        main.startColor = new ParticleSystem.MinMaxGradient(_colorStart, _colorEnd);

        // ── Emission Module ───────────────────────────────────────────────────
        var emission = _ps.emission;
        emission.enabled       = true;
        emission.rateOverTime  = 0f; // Controlled each frame in Update

        // ── Shape: spawn across a box covering the map ────────────────────────
        var shape = _ps.shape;
        shape.enabled    = true;
        shape.shapeType  = ParticleSystemShapeType.Box;
        shape.scale      = new Vector3(_spawnAreaSize.x, _spawnAreaSize.y, 0.1f);

        // ── Velocity Over Lifetime: slow upward drift + sideways wobble ───────
        var vel = _ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;

        // All three axes must use the same MinMaxCurve mode (TwoConstants here).
        // If any axis is left at the default Constant mode Unity throws a warning.
        vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f); // Gentle sideways drift
        vel.y = new ParticleSystem.MinMaxCurve(0.08f,  0.25f); // Slow upward float
        vel.z = new ParticleSystem.MinMaxCurve(0f,     0f);    // No Z movement (2D)

        // ── Color Over Lifetime: fade in then fade out ────────────────────────
        var colorOverLife = _ps.colorOverLifetime;
        colorOverLife.enabled = true;

        Gradient fadeInOut = new Gradient();
        fadeInOut.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 0.5f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f,   0f),    // Start transparent (fade in)
                new GradientAlphaKey(1f,   0.2f),  // Reach full alpha quickly
                new GradientAlphaKey(1f,   0.75f), // Hold bright through midlife
                new GradientAlphaKey(0f,   1f)     // Fade out at end of life
            }
        );
        colorOverLife.color = new ParticleSystem.MinMaxGradient(fadeInOut);

        // ── Size Over Lifetime: slight pulse for a "blinking" effect ──────────
        var sizeOverLife = _ps.sizeOverLifetime;
        sizeOverLife.enabled = true;

        AnimationCurve pulse = new AnimationCurve();
        pulse.AddKey(0f,    0.3f);  // Small at spawn
        pulse.AddKey(0.25f, 1.0f);  // Grow to full
        pulse.AddKey(0.5f,  0.7f);  // Slight dim
        pulse.AddKey(0.75f, 1.0f);  // Brighten again
        pulse.AddKey(1.0f,  0f);    // Shrink at end
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, pulse);

        // ── Renderer: "Particles" sorting layer, above world/below UI ─────────
        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "Particles";
        renderer.sortingOrder     = 5;
        renderer.renderMode       = ParticleSystemRenderMode.Billboard;

        Debug.Log("[AmbientFireflySystem] 🐛 Firefly particle system configured.");
    }
}
