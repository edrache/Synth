using UnityEngine;

public class SquareOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        return Mathf.Sin(phase) >= 0f ? 1f : -1f;
    }

    public void Reset()
    {
        // Square oscillator doesn't need to reset any state
    }
}