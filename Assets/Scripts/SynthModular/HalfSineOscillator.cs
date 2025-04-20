using UnityEngine;

public class HalfSineOscillator : IOscillator
{
    public float GetSample(float phase)
    {
        float s = Mathf.Sin(phase);
        return s > 0f ? s : 0f;
    }
}