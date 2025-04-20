using UnityEngine;

public class SineOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        return Mathf.Sin(phase);
    }
}