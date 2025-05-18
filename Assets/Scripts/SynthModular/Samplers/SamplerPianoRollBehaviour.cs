using UnityEngine;
using UnityEngine.Playables;

public class SamplerPianoRollBehaviour : PlayableBehaviour
{
    public int midiNote;
    public float velocity = 1f;
    public float duration;
    public float startTime;
    public GameObject sourceObject;
    public TimelineType timelineType;

    protected bool hasTriggered = false;
    private Sampler sampler;
    private CityNote cityNote;

    public override void OnPlayableCreate(Playable playable)
    {
        sampler = playable.GetGraph().GetResolver() as Sampler;
        if (sourceObject != null)
        {
            cityNote = sourceObject.GetComponent<CityNote>();
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // Debug.Log($"[SamplerPianoRollBehaviour] OnBehaviourPlay called for note {midiNote} with velocity {velocity} and timeline type {timelineType}");
        if (sourceObject != null)
        {
            var cityNote = sourceObject.GetComponent<CityNote>();
            if (cityNote != null)
            {
                // Debug.Log($"[SamplerPianoRollBehaviour] Found CityNote component, notifying NoteEventManager");
                var eventManager = sourceObject.GetComponent<NoteEventManager>();
                if (eventManager != null)
                {
                    eventManager.NotifyNoteStart(cityNote, velocity, timelineType);
                }
                else
                {
                    Debug.LogWarning($"[SamplerPianoRollBehaviour] No NoteEventManager found on source object");
                }
            }
            else
            {
                Debug.LogWarning($"[SamplerPianoRollBehaviour] No CityNote component found on source object");
            }
        }
        else
        {
            Debug.LogWarning($"[SamplerPianoRollBehaviour] Source object is null");
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        // Debug.Log($"[SamplerPianoRollBehaviour] OnBehaviourPause called for note {midiNote} with timeline type {timelineType}");
        if (sourceObject != null)
        {
            var cityNote = sourceObject.GetComponent<CityNote>();
            if (cityNote != null)
            {
                // Debug.Log($"[SamplerPianoRollBehaviour] Found CityNote component, notifying NoteEventManager");
                var eventManager = sourceObject.GetComponent<NoteEventManager>();
                if (eventManager != null)
                {
                    eventManager.NotifyNoteEnd(cityNote, timelineType);
                }
                else
                {
                    Debug.LogWarning($"[SamplerPianoRollBehaviour] No NoteEventManager found on source object");
                }
            }
            else
            {
                Debug.LogWarning($"[SamplerPianoRollBehaviour] No CityNote component found on source object");
            }
        }
        else
        {
            Debug.LogWarning($"[SamplerPianoRollBehaviour] Source object is null");
        }
    }
} 