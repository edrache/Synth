using UnityEngine;

public class TriangleOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        return 2f * Mathf.Abs(2f * (phase / (2f * Mathf.PI) - Mathf.Floor(phase / (2f * Mathf.PI) + 0.5f))) - 1f;
    }
}