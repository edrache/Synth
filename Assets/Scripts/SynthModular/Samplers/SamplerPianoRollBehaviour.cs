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
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!hasTriggered && sampler != null)
        {
            sampler.PlayNote(midiNote);
            hasTriggered = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (hasTriggered && sampler != null)
        {
            sampler.StopNote(midiNote);
            hasTriggered = false;
        }
    }
} 