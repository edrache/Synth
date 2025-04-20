using UnityEngine;

public class Envelope
{
    private float attack;
    private float release;
    private float amplitude = 0f;
    private float target = 0f;

    public Envelope(float attack, float release)
    {
        this.attack = attack;
        this.release = release;
    }

    public void NoteOn() => target = 1f;
    public void NoteOff() => target = 0f;

    public float Next(float step)
    {
        if (target > amplitude)
            amplitude += step / attack;
        else
            amplitude -= step / release;

        amplitude = Mathf.Clamp01(amplitude);
        return amplitude;
    }

    public bool IsSilent() => amplitude <= 0.001f && target == 0f;

    public void SetSettings(EnvelopeSettings settings)
    {
        this.attack = settings.Attack;
        this.release = settings.Release;
    }

    public EnvelopeSettings GetSettings()
    {
        return new EnvelopeSettings
        {
            Attack = this.attack,
            Release = this.release
        };
    }
}