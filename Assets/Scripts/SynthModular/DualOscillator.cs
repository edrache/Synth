using UnityEngine;

public class DualOscillator : IOscillator
{
    private IOscillator oscA;
    private IOscillator oscB;
    private float mix; // 0 = tylko A, 1 = tylko B

    public DualOscillator(IOscillator a, IOscillator b, float mix)
    {
        oscA = a;
        oscB = b;
        this.mix = Mathf.Clamp01(mix);
    }

    public float GetSample(float phase)
    {
        return Mathf.Lerp(oscA.GetSample(phase), oscB.GetSample(phase), mix);
    }

    public void Reset()
    {
        oscA.Reset();
        oscB.Reset();
    }
}