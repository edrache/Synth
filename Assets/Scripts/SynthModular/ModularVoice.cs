using UnityEngine;

public class ModularVoice
{
    public int id;
    public float frequency;
    private float phase;
    private float sampleRate;

    private Envelope envelope;
    private IOscillator oscillator;
    private ILowPassFilter filter;
    private IDistortion distortion;
    private IGrainGate grainGate;

    public bool IsActive => !envelope.IsSilent();

    public ModularVoice(
        int id,
        float frequency,
        float sampleRate,
        IOscillator oscillator,
        Envelope envelope,
        ILowPassFilter filter,
        IDistortion distortion,
        IGrainGate grainGate = null)
    {
        this.id = id;
        this.frequency = frequency;
        this.sampleRate = sampleRate;
        this.oscillator = oscillator;
        this.envelope = envelope;
        this.filter = filter;
        this.distortion = distortion;
        this.grainGate = grainGate;

        envelope.NoteOn();
    }

    public void NoteOff() => envelope.NoteOff();

    public float NextSample(float step)
    {
        float increment = 2f * Mathf.PI * frequency / sampleRate;
        phase += increment;
        if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI;

        float raw = oscillator.GetSample(phase);
        float shaped = distortion.Apply(raw);
        float filtered = filter.Apply(shaped);
        float amp = envelope.Next(step);

        float final = filtered * amp * 0.5f;
        return grainGate != null ? grainGate.Apply(final, step) : final;
    }
}