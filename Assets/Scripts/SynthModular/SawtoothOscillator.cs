using UnityEngine;

public class SawtoothOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        return 2f * (phase / (2f * Mathf.PI)) - 1f;
    }

    public void Reset()
    {
        // Sawtooth oscillator doesn't need to reset any state
    }
} 