using UnityEngine;
using UnityEngine.Playables;

public class DrumRackPianoRollBehaviour : PlayableBehaviour
{
    public IDrumRackSampler sampler;
    public Note note;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (sampler != null)
        {
            sampler.PlayNote(note);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (sampler != null)
        {
            sampler.StopNote(note);
        }
    }
} 