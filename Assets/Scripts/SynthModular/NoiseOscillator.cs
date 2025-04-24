using UnityEngine;

public class NoiseOscillator : IOscillator
{
    private System.Random rand = new System.Random();

    public float GetSample(float phase)
    {
        return (float)(rand.NextDouble() * 2.0 - 1.0);
    }

    public void Reset()
    {
        // Noise oscillator doesn't need to reset any state
    }
}