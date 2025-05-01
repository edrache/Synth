using UnityEngine;
using UnityEngine.Playables;

public class SamplerPianoRollBehaviour : PlayableBehaviour
{
    public int midiNote;
    public float velocity = 1f;
    public float duration;
    public float startTime;

    protected bool hasTriggered = false;
    private Sampler sampler;

    public override void OnPlayableCreate(Playable playable)
    {
        sampler = playable.GetGraph().GetResolver() as Sampler;
        if (sampler != null)
        {
            Debug.Log($"SamplerPianoRollBehaviour created with velocity {velocity}");
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!hasTriggered && sampler != null)
        {
            Debug.Log($"Playing note {midiNote} with velocity {velocity}");
            sampler.PlayNote(midiNote, velocity);
            hasTriggered = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (hasTriggered && sampler != null)
        {
            Debug.Log($"Stopping note {midiNote}");
            sampler.StopNote(midiNote);
            hasTriggered = false;
        }
    }
} 