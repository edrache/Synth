using UnityEngine;

public class ADSREnvelopeComponent : BaseEnvelopeComponent
{
    private float currentAmplitude = 0f;
    private float elapsedTime = 0f;
    private float startAmplitude = 0f;
    private EnvelopeStage currentStage = EnvelopeStage.Idle;

    private enum EnvelopeStage
    {
        Idle,
        Attack,
        Decay,
        Sustain,
        Release
    }

    protected override void Start()
    {
        base.Start();
        OnValidate();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    public override void NoteOn()
    {
        startAmplitude = currentAmplitude;
        elapsedTime = 0f;
        currentStage = EnvelopeStage.Attack;
        isNoteOn = true;
        isReleasing = false;
    }

    public override void NoteOff()
    {
        startAmplitude = currentAmplitude;
        elapsedTime = 0f;
        currentStage = EnvelopeStage.Release;
        isNoteOn = false;
        isReleasing = true;
    }

    public override float GetAmplitude(float deltaTime)
    {
        elapsedTime += deltaTime;
        float t;

        switch (currentStage)
        {
            case EnvelopeStage.Attack:
                t = Mathf.Clamp01(elapsedTime / attack);
                currentAmplitude = Mathf.Lerp(startAmplitude, 1f, t);
                if (t >= 1f)
                {
                    currentStage = EnvelopeStage.Decay;
                    startAmplitude = currentAmplitude;
                    elapsedTime = 0f;
                }
                break;

            case EnvelopeStage.Decay:
                t = Mathf.Clamp01(elapsedTime / decay);
                currentAmplitude = Mathf.Lerp(1f, sustain, t);
                if (t >= 1f)
                {
                    currentStage = EnvelopeStage.Sustain;
                    startAmplitude = currentAmplitude;
                    elapsedTime = 0f;
                }
                break;

            case EnvelopeStage.Sustain:
                currentAmplitude = sustain;
                break;

            case EnvelopeStage.Release:
                t = Mathf.Clamp01(elapsedTime / release);
                currentAmplitude = Mathf.Lerp(startAmplitude, 0f, t);
                if (t >= 1f)
                {
                    currentStage = EnvelopeStage.Idle;
                    currentAmplitude = 0f;
                }
                break;

            case EnvelopeStage.Idle:
                currentAmplitude = 0f;
                break;
        }

        return currentAmplitude;
    }
} 