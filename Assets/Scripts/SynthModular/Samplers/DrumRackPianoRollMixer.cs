using UnityEngine;
using UnityEngine.Playables;

public class DrumRackPianoRollMixer : PlayableBehaviour
{
    public IDrumRackSampler sampler;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (sampler == null) return;

        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float weight = playable.GetInputWeight(i);
            ScriptPlayable<DrumRackPianoRollBehaviour> inputPlayable = (ScriptPlayable<DrumRackPianoRollBehaviour>)playable.GetInput(i);
            DrumRackPianoRollBehaviour input = inputPlayable.GetBehaviour();

            if (input != null)
            {
                input.sampler = sampler;
            }
        }
    }
} 