using UnityEngine;

public class SawOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        return 2f * (phase / (2f * Mathf.PI)) - 1f;
    }
}