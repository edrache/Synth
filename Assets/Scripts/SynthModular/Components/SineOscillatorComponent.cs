using UnityEngine;

public class SineOscillatorComponent : BaseOscillatorComponent
{
    public override float GetSample(float phase)
    {
        return Mathf.Sin(phase);
    }
} 