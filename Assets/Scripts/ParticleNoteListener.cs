using UnityEngine;

public class ParticleNoteListener : MonoBehaviour, INoteEventListener
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem bassParticles;
    [SerializeField] private ParticleSystem leadParticles;
    [SerializeField] private ParticleSystem drumParticles;
    [SerializeField] private ParticleSystem ambientParticles;
    [SerializeField] private ParticleSystem fxParticles;

    [Header("Timeline Configurations")]
    [SerializeField] private TimelineParticleConfig bassConfig = new TimelineParticleConfig();
    [SerializeField] private TimelineParticleConfig leadConfig = new TimelineParticleConfig();
    [SerializeField] private TimelineParticleConfig drumConfig = new TimelineParticleConfig();
    [SerializeField] private TimelineParticleConfig ambientConfig = new TimelineParticleConfig();
    [SerializeField] private TimelineParticleConfig fxConfig = new TimelineParticleConfig();

    private void Start()
    {
        // Debug.Log("[ParticleNoteListener] Starting initialization");
        var eventManager = GetComponent<NoteEventManager>();
        if (eventManager == null)
        {
            // Debug.Log("[ParticleNoteListener] No NoteEventManager found, adding one");
            eventManager = gameObject.AddComponent<NoteEventManager>();
        }
        eventManager.RegisterListener(this);
        // Debug.Log("[ParticleNoteListener] Registered as listener");
    }

    private void OnDestroy()
    {
        var eventManager = GetComponent<NoteEventManager>();
        if (eventManager != null)
        {
            eventManager.UnregisterListener(this);
        }
    }

    public void OnNoteStart(CityNote note, float velocity, TimelineType timelineType)
    {
        // Debug.Log($"[ParticleNoteListener] OnNoteStart called for note {note.pitch} with velocity {velocity} and timeline type {timelineType}");
        ParticleSystem particles = GetParticlesForTimeline(timelineType);
        TimelineParticleConfig config = GetConfigForTimeline(timelineType);
        
        if (particles != null && config != null)
        {
            config.ApplyToParticleSystem(particles, velocity);
            particles.Play();
            // Debug.Log($"[ParticleNoteListener] Playing particles for timeline type {timelineType}");
        }
        else
        {
            Debug.LogWarning($"[ParticleNoteListener] No particles or config found for timeline type {timelineType}");
        }
    }

    public void OnNoteEnd(CityNote note, TimelineType timelineType)
    {
        // Nie zatrzymujemy cząsteczek przy końcu nuty
        // Debug.Log($"[ParticleNoteListener] OnNoteEnd called for note {note.pitch} with timeline type {timelineType} - particles will continue playing");
    }

    private ParticleSystem GetParticlesForTimeline(TimelineType timelineType)
    {
        switch(timelineType)
        {
            case TimelineType.Bass:
                return bassParticles;
            case TimelineType.Lead:
                return leadParticles;
            case TimelineType.Drums:
                return drumParticles;
            case TimelineType.Ambient:
                return ambientParticles;
            case TimelineType.FX:
                return fxParticles;
            default:
                return null;
        }
    }

    private TimelineParticleConfig GetConfigForTimeline(TimelineType timelineType)
    {
        switch(timelineType)
        {
            case TimelineType.Bass:
                return bassConfig;
            case TimelineType.Lead:
                return leadConfig;
            case TimelineType.Drums:
                return drumConfig;
            case TimelineType.Ambient:
                return ambientConfig;
            case TimelineType.FX:
                return fxConfig;
            default:
                return null;
        }
    }

    // Helper method to copy configuration from one timeline to another
    public void CopyConfiguration(TimelineType sourceType, TimelineType targetType)
    {
        TimelineParticleConfig sourceConfig = GetConfigForTimeline(sourceType);
        TimelineParticleConfig targetConfig = GetConfigForTimeline(targetType);
        
        if (sourceConfig != null && targetConfig != null)
        {
            targetConfig.CopyFrom(sourceConfig);
        }
    }
} 