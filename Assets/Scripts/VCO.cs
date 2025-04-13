using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class VCO : MonoBehaviour
{
    public enum Waveform { Sine, Square, Saw, Triangle, Noise, Sine2, SoftSquare, SoftSaw, Sine3, SoftPulse }

    [Range(20f, 2000f)]
    public float frequency = 440f;

    public Waveform waveform = Waveform.Sine;

    [Range(20f, 20000f)]
    public float baseCutoff = 1000f;

    [Range(0f, 1f)]
    public float resonance = 0.5f;

    [Range(0.01f, 2f)]
    public float decay = 0.3f;

    [Range(0.001f, 1f)]
    public float slideTime = 0.1f;

    [Range(1f, 2f)]
    public float accentStrength = 1.2f;

    [Range(40f, 300f)]
    public float bpm = 120f;

    [Range(0f, 1f)]
    public float volume = 0.5f; // Głośność sygnału

    public float gain = 1.0f; // Wzmocnienie sygnału

    // Sequence properties
    public VCOSequence currentSequence;
    private List<Step> sequence = new List<Step>();
    private int currentStep = 0;
    private float stepTimer = 0f;
    private float barTimer = 0f;
    private float barLength;

    private double phase;
    private float sampleRate;

    private float envelopeValue = 0f;
    private float envelopeDecayRate;
    private bool triggerEnvelope = false;

    private MoogFilter filter;
    private System.Random random = new System.Random();

    private float currentFrequency;
    private float targetFrequency;
    private bool isSliding = false;
    private float slideStartTime;

    public delegate void StepChangedHandler(float pitch, int stepNumber);
    public event StepChangedHandler OnStepChanged;

    [System.Serializable]
    public class Step
    {
        public float pitch = 440f;
        public bool slide = false;
        public bool accent = false;
        [Range(0.1f, 2f)]
        public float accentStrength = 1.2f;
        public bool useNote = false;
        public MusicUtils.Note note = MusicUtils.Note.A;
        public int octave = 4;
        [Range(0f, 4f)]
        public float duration = 2f; // 0 = no sound, 1 = 16th note, 2 = 8th note, 3 = dotted 8th note, 4 = quarter note

        public void UpdatePitchFromNote()
        {
            if (useNote)
            {
                pitch = MusicUtils.GetFrequency(note, octave);
            }
        }
    }

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        envelopeDecayRate = 1f / (decay * sampleRate);
        filter = new MoogFilter(sampleRate);
        currentFrequency = frequency;
        targetFrequency = frequency;
        LoadSequence();
        barLength = (60f / bpm) * 4f; // Length of one bar in seconds
    }

    void LoadSequence()
    {
        if (currentSequence != null)
        {
            sequence = new List<Step>(currentSequence.steps);
            if (sequence.Count > 0) ApplyStep(sequence[0]);
        }
    }

    public void ChangeSequence(VCOSequence newSequence)
    {
        currentSequence = newSequence;
        currentStep = 0;
        stepTimer = 0f;
        LoadSequence();
    }

    void Update()
    {
        if (sequence.Count > 0)
        {
            float baseStepLength = (60f / bpm) / 4f; // Base length for a 16th note
            stepTimer += Time.deltaTime;
            barTimer += Time.deltaTime;
            
            if (barTimer >= barLength)
            {
                barTimer -= barLength;
            }
            
            if (stepTimer >= baseStepLength)
            {
                stepTimer -= baseStepLength;
                currentStep = (currentStep + 1) % sequence.Count;
                ApplyStep(sequence[currentStep]);
            }

            // Handle sliding
            if (isSliding)
            {
                float t = (Time.time - slideStartTime) / slideTime;
                if (t >= 1f)
                {
                    frequency = targetFrequency;
                    currentFrequency = targetFrequency;
                    isSliding = false;
                }
                else
                {
                    currentFrequency = Mathf.Lerp(currentFrequency, targetFrequency, t);
                    frequency = currentFrequency;
                }
            }
        }

        envelopeDecayRate = 1f / (decay * sampleRate);
    }

    void ApplyStep(Step step)
    {
        if (step.duration > 0)
        {
            if (!step.slide)
            {
                currentFrequency = step.pitch;
                targetFrequency = step.pitch;
                frequency = step.pitch;
                phase = 0;
                isSliding = false;
            }
            else
            {
                targetFrequency = step.pitch;
                currentFrequency = frequency;
                slideStartTime = Time.time;
                isSliding = true;
            }
            
            if (step.accent) 
                envelopeValue = accentStrength;
            else 
                TriggerEnvelope();
        }
        else
        {
            // For zero duration, mute the sound
            frequency = 0f;
            currentFrequency = 0f;
            targetFrequency = 0f;
            envelopeValue = 0f;
        }

        // Zawsze wywołuj zdarzenie zmiany kroku, niezależnie od duration
        OnStepChanged?.Invoke(step.pitch, (int)currentStep);

        // Wywołaj ruch obiektu na podstawie wysokości nuty i numeru kroku
        NoteMovement noteMovement = GetComponent<NoteMovement>();
        if (noteMovement != null)
        {
            noteMovement.MoveToNote(step.pitch, currentStep);
        }
    }

    public void TriggerEnvelope()
    {
        envelopeValue = 1f;
        triggerEnvelope = true;
    }

    float SoftClip(float x)
    {
        return x * 0.5f * (1f + Mathf.Sign(x) * (1f - Mathf.Exp(-Mathf.Abs(x))));
    }

    float GenerateSample()
    {
        float sample = 0f;
        float envelope = envelopeValue;

        switch (waveform)
        {
            case Waveform.Sine:
                sample = Mathf.Sin((float)phase * 2f * Mathf.PI) * envelope;
                break;
            case Waveform.Square:
                sample = (Mathf.Sin((float)phase * 2f * Mathf.PI) > 0 ? 1f : -1f) * envelope;
                break;
            case Waveform.Saw:
                sample = ((float)(phase % 1.0) * 2f - 1f) * envelope;
                break;
            case Waveform.Triangle:
                float triPhase = (float)(phase % 1.0);
                sample = (Mathf.Abs(triPhase * 4f - 2f) - 1f) * envelope;
                break;
            case Waveform.Noise:
                sample = (float)(random.NextDouble() * 2.0 - 1.0) * envelope;
                break;
            case Waveform.Sine2:
                float sin2Phase = (float)phase * 2f * Mathf.PI;
                sample = (Mathf.Sin(sin2Phase) + Mathf.Sin(sin2Phase * 2f) * 0.5f) * 0.666f * envelope;
                break;
            case Waveform.SoftSquare:
                float softSqPhase = (float)phase * 2f * Mathf.PI;
                sample = Mathf.Tan(Mathf.Sin(softSqPhase) * 0.5f) * 0.5f * envelope;
                break;
            case Waveform.SoftSaw:
                float softSawPhase = (float)(phase % 1.0);
                sample = (Mathf.Sin(softSawPhase * Mathf.PI * 2f) + 
                        Mathf.Sin(softSawPhase * Mathf.PI * 4f) * 0.5f + 
                        Mathf.Sin(softSawPhase * Mathf.PI * 6f) * 0.25f) * 0.571f * envelope;
                break;
            case Waveform.Sine3:
                float sin3Phase = (float)phase * 2f * Mathf.PI;
                sample = (Mathf.Sin(sin3Phase) + 
                        Mathf.Sin(sin3Phase * 2f) * 0.5f + 
                        Mathf.Sin(sin3Phase * 3f) * 0.25f) * 0.571f * envelope;
                break;
            case Waveform.SoftPulse:
                float softPulsePhase = (float)phase * 2f * Mathf.PI;
                float pulseWidth = 0.5f; // Można dodać jako parametr
                float pulse = Mathf.Sin(softPulsePhase) > (pulseWidth * 2f - 1f) ? 1f : -1f;
                sample = Mathf.Lerp(pulse, Mathf.Sin(softPulsePhase), 0.7f) * envelope;
                break;
        }

        // Normalizacja do zakresu -1 do 1
        sample = Mathf.Clamp(sample, -1f, 1f);

        // Zastosuj wzmocnienie
        sample *= gain;

        return sample;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float currentFreq = isSliding ? currentFrequency : frequency;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = GenerateSample();

            phase += 2 * Math.PI * currentFreq / sampleRate;
            if (phase >= 2 * Math.PI)
                phase -= 2 * Math.PI;

            if (triggerEnvelope)
            {
                envelopeValue -= envelopeDecayRate;
                if (envelopeValue <= 0f)
                {
                    envelopeValue = 0f;
                    triggerEnvelope = false;
                }
            }

            float modulatedCutoff = baseCutoff + Mathf.Pow(envelopeValue, 2) * 8000f;
            filter.SetParams(modulatedCutoff, resonance);
            float filtered = filter.Process(sample);

            float finalSample = filtered * envelopeValue * volume;
            float clipped = Mathf.Clamp(SoftClip(finalSample), -0.95f, 0.95f);

            for (int ch = 0; ch < channels; ch++)
                data[i + ch] = clipped;
        }
    }

    public void SetCutoffFrequency(float cutoff)
    {
        baseCutoff = cutoff;
    }

    public int GetCurrentStep()
    {
        return currentStep;
    }

    public float GetBarTime()
    {
        return barTimer / barLength;
    }
}

public class MoogFilter
{
    private float sampleRate;
    private float cutoff;
    private float resonance;
    private float p, k, r;
    private float[] y = new float[4];
    private float[] oldx = new float[4];
    private float[] oldy = new float[4];

    public MoogFilter(float sampleRate)
    {
        this.sampleRate = sampleRate;
    }

    public void SetParams(float cutoffHz, float resonance)
    {
        cutoff = Mathf.Clamp(cutoffHz, 50f, sampleRate * 0.45f);
        this.resonance = Mathf.Clamp(resonance, 0f, 1f);

        float f = cutoff / sampleRate;
        p = f * (1.8f - 0.8f * f);
        k = 2f * Mathf.Sin(f * Mathf.PI * 0.5f) - 1f;
        r = resonance * (1.0f - 0.15f * p * p);
    }

    public float Process(float input)
    {
        input -= r * y[3];

        y[0] = p * (input + oldx[0]) - k * y[0];
        y[1] = p * (y[0] + oldx[1]) - k * y[1];
        y[2] = p * (y[1] + oldx[2]) - k * y[2];
        y[3] = p * (y[2] + oldx[3]) - k * y[3];

        for (int i = 0; i < 4; i++)
        {
            oldx[i] = i == 0 ? input : y[i - 1];
            oldy[i] = y[i];
        }

        return y[3];
    }
}