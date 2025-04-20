using UnityEngine;

public class SubOscillator : IOscillator
{
    private IOscillator baseOsc;

    public SubOscillator(IOscillator baseOsc)
    {
        this.baseOsc = baseOsc;
    }

    public float GetSample(float phase)
    {
        return baseOsc.GetSample(phase * 0.5f); // oktawa niżej
    }
}