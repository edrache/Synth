using UnityEngine;

public class ParticleNoteListener : MonoBehaviour, INoteEventListener
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem bassParticles;
    [SerializeField] private ParticleSystem leadParticles;
    [SerializeField] private ParticleSystem drumParticles;
    [SerializeField] private ParticleSystem ambientParticles;
    [SerializeField] private ParticleSystem fxParticles;

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
        if (particles != null)
        {
            // Debug.Log($"[ParticleNoteListener] Found particle system for timeline type {timelineType}");
            particles.Play();
            // Debug.Log($"[ParticleNoteListener] Playing particles for timeline type {timelineType}");
        }
        else
        {
            Debug.LogWarning($"[ParticleNoteListener] No particles found for timeline type {timelineType}. Available systems: Bass: {bassParticles != null}, Lead: {leadParticles != null}, Drums: {drumParticles != null}, Ambient: {ambientParticles != null}, FX: {fxParticles != null}");
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
} 