using UnityEngine;

public class HalfSineOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        float s = Mathf.Sin(phase);
        return s > 0f ? s : 0f;
    }

    public void Reset()
    {
        // HalfSine oscillator doesn't need to reset any state
    }
}