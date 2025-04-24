using UnityEngine;

public class SineOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        return Mathf.Sin(phase);
    }

    public void Reset()
    {
        // Sine oscillator doesn't need to reset any state
    }
}