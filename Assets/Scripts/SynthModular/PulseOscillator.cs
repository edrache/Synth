using UnityEngine;

public class PulseOscillator : IOscillator
{
    private float pulseWidth;

    public PulseOscillator(float pulseWidth = 0.5f)
    {
        this.pulseWidth = Mathf.Clamp01(pulseWidth);
    }

    public float GetSample(float phase)
    {
        return (phase / (2f * Mathf.PI)) % 1f < pulseWidth ? 1f : -1f;
    }

    public void SetPulseWidth(float width)
    {
        pulseWidth = Mathf.Clamp01(width);
    }

    public void Reset()
    {
        // Pulse oscillator doesn't need to reset any state
    }
}