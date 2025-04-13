using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class VCO : MonoBehaviour
{
    public enum Waveform { Sine, Square, Saw, Triangle, Noise, Sine2, SoftSquare, SoftSaw, Sine3, SoftPulse }
    public enum FilterType { LowPass, HighPass, BandPass }
    public enum LFOWaveform { Sine, Square, Triangle, Saw }

    [Header("Podstawowe parametry")]
    [Range(20f, 2000f)]
    public float frequency = 440f;
    public Waveform waveform = Waveform.Sine;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    public float gain = 1.0f;

    [Header("Filtr")]
    public FilterType filterType = FilterType.LowPass;
    [Range(20f, 5000f)]
    public float baseCutoff = 1000f;
    [Range(0f, 1f)]
    public float resonance = 0.5f;
    [Range(0.1f, 2f)]
    public float filterQ = 0.7f; // Q factor for bandpass filter

    [Header("Filter LFO")]
    public bool filterLFOEnabled = false;
    [Range(0.1f, 20f)]
    public float filterLFORate = 1f;
    [Range(0f, 1f)]
    public float filterLFODepth = 0.5f;
    public LFOWaveform filterLFOWaveform = LFOWaveform.Sine;

    [Header("Envelope")]
    [Range(0.001f, 2f)]
    public float attackTime = 0.01f; // Czas ataku
    [Range(0.001f, 2f)]
    public float decayTime = 0.1f; // Czas opadania
    [Range(0f, 1f)]
    public float sustainLevel = 0.7f; // Poziom podtrzymania
    [Range(0.001f, 2f)]
    public float releaseTime = 0.2f; // Czas wybrzmiewania
    [Range(0.001f, 1f)]
    public float slideTime = 0.1f; // Czas przejścia między nutami
    [Range(1f, 2f)]
    public float accentStrength = 1.2f;

    [Header("Sekwencer")]
    [Range(40f, 300f)]
    public float bpm = 120f;
    [Range(-2, 2)]
    public int globalOctaveShift = 0; // Globalne przesunięcie oktawy

    [Header("Chorus")]
    public bool chorusEnabled = true;
    [Range(0f, 0.5f)]
    public float chorusAmount = 0.5f; // Siła efektu
    [Range(0f, 1f)]
    public float chorusRate = 0.5f; // Szybkość modulacji
    [Range(0f, 0.01f)]
    public float chorusDepth = 0.01f; // Głębokość modulacji
    [Range(0f, 0.3f)]
    public float chorusFeedback = 0.2f; // Sprzężenie zwrotne
    [Range(0f, 1f)]
    public float chorusWidth = 0.5f; // Szerokość stereo

    [Header("Delay")]
    [Range(0f, 1f)]
    public float delayAmount = 0.5f; // Siła efektu
    [Range(0f, 1f)]
    public float delayFeedback = 0.3f; // Sprzężenie zwrotne
    public enum DelayTime { 
        Off,
        Quarter, // ćwierćnuta
        Eighth, // ósemka
        EighthDotted, // ósemka z kropką
        Sixteenth, // szesnastka
        Triplet // triola
    }
    public DelayTime delayTime = DelayTime.Eighth;
    [Range(0f, 1f)]
    public float delayWidth = 0.5f; // Szerokość stereo

    [Header("Filter Envelope")]
    [Range(0.001f, 2f)]
    public float filterAttack = 0.01f;
    [Range(0.001f, 2f)]
    public float filterDecay = 0.1f;
    [Range(0f, 1f)]
    public float filterSustain = 0.7f;
    [Range(0.001f, 2f)]
    public float filterRelease = 0.2f;

    [Header("Chorus Envelope")]
    [Range(0.001f, 2f)]
    public float chorusAttack = 0.01f;
    [Range(0.001f, 2f)]
    public float chorusDecay = 0.1f;
    [Range(0f, 1f)]
    public float chorusSustain = 0.7f;
    [Range(0.001f, 2f)]
    public float chorusRelease = 0.2f;

    [Header("Delay Envelope")]
    [Range(0.001f, 2f)]
    public float delayAttack = 0.01f;
    [Range(0.001f, 2f)]
    public float delayDecay = 0.1f;
    [Range(0f, 1f)]
    public float delaySustain = 0.7f;
    [Range(0.001f, 2f)]
    public float delayRelease = 0.2f;

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
    private float envelopeAttackRate;
    private float envelopeDecayRate;
    private float envelopeReleaseRate;
    private bool triggerEnvelope = false;
    private bool isReleasing = false;

    private MoogFilter filter;
    private System.Random random = new System.Random();

    private float currentFrequency;
    private float targetFrequency;
    private bool isSliding = false;
    private float slideStartTime;

    // Chorus variables
    private float[] chorusBuffer;
    private int chorusBufferLength;
    private int chorusWritePosition;
    private float chorusPhase;
    private const int MAX_CHORUS_DELAY_SAMPLES = 2048;

    // Delay variables
    private float[] delayBufferLeft;
    private float[] delayBufferRight;
    private int delayBufferLength;
    private int delayWritePosition;
    private float delayTimeInSamples;
    private const int MAX_DELAY_SAMPLES = 192000; // 4 seconds at 48kHz

    private float filterLFOPhase = 0f;
    private float currentCutoff;

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

        public void UpdatePitchFromNote(int globalOctaveShift)
        {
            if (useNote)
            {
                pitch = MusicUtils.GetFrequency(note, octave + globalOctaveShift);
            }
        }
    }

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        envelopeAttackRate = 1f / (attackTime * sampleRate);
        envelopeDecayRate = 1f / (decayTime * sampleRate);
        envelopeReleaseRate = 1f / (releaseTime * sampleRate);
        filter = new MoogFilter(sampleRate, filterType);
        currentFrequency = frequency;
        targetFrequency = frequency;
        LoadSequence();
        barLength = (60f / bpm) * 4f;

        // Initialize chorus
        chorusBufferLength = MAX_CHORUS_DELAY_SAMPLES;
        chorusBuffer = new float[chorusBufferLength];
        chorusWritePosition = 0;
        chorusPhase = 0f;

        // Initialize delay buffers
        delayBufferLeft = new float[MAX_DELAY_SAMPLES];
        delayBufferRight = new float[MAX_DELAY_SAMPLES];
        delayBufferLength = MAX_DELAY_SAMPLES;
        delayWritePosition = 0;
        UpdateDelayTime();
        UpdateEnvelopeRates();
        currentCutoff = baseCutoff;
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

    private void UpdateDelayTime()
    {
        float delayTimeInSeconds = 0f;
        float quarterNoteTime = 60f / bpm;

        switch (delayTime)
        {
            case DelayTime.Quarter:
                delayTimeInSeconds = quarterNoteTime;
                break;
            case DelayTime.Eighth:
                delayTimeInSeconds = quarterNoteTime * 0.5f;
                break;
            case DelayTime.EighthDotted:
                delayTimeInSeconds = quarterNoteTime * 0.75f;
                break;
            case DelayTime.Sixteenth:
                delayTimeInSeconds = quarterNoteTime * 0.25f;
                break;
            case DelayTime.Triplet:
                delayTimeInSeconds = quarterNoteTime / 3f;
                break;
        }

        delayTimeInSamples = delayTimeInSeconds * AudioSettings.outputSampleRate;
    }

    void Update()
    {
        if (sequence.Count > 0)
        {
            float baseStepLength = (60f / bpm) / 4f;
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

            // Update delay time when BPM changes
            UpdateDelayTime();
        }

        UpdateEnvelopeRates();

        // Update filter LFO
        if (filterLFOEnabled)
        {
            float lfoValue = 0f;
            switch (filterLFOWaveform)
            {
                case LFOWaveform.Sine:
                    lfoValue = Mathf.Sin(filterLFOPhase * 2f * Mathf.PI) * 0.5f + 0.5f;
                    break;
                case LFOWaveform.Square:
                    lfoValue = filterLFOPhase < 0.5f ? 1f : 0f;
                    break;
                case LFOWaveform.Triangle:
                    lfoValue = filterLFOPhase < 0.5f ? 
                        filterLFOPhase * 2f : 
                        1f - (filterLFOPhase - 0.5f) * 2f;
                    break;
                case LFOWaveform.Saw:
                    lfoValue = filterLFOPhase;
                    break;
            }

            // Calculate cutoff modulation
            float modulation = (lfoValue * 2f - 1f) * filterLFODepth;
            currentCutoff = baseCutoff * Mathf.Pow(2f, modulation * 4f); // 4 oktawy modulacji
            filterLFOPhase += filterLFORate / sampleRate;
            if (filterLFOPhase >= 1f) filterLFOPhase -= 1f;

            // Update filter cutoff
            filter.SetParams(currentCutoff, resonance, filterQ);
        }
        else
        {
            currentCutoff = baseCutoff;
            filter.SetParams(currentCutoff, resonance, filterQ);
        }
    }

    private void UpdateEnvelopeRates()
    {
        envelopeAttackRate = 1f / (attackTime * sampleRate);
        envelopeDecayRate = 1f / (decayTime * sampleRate);
        envelopeReleaseRate = 1f / (releaseTime * sampleRate);
    }

    void ApplyStep(Step step)
    {
        float adjustedPitch = step.pitch;
        if (step.useNote)
        {
            // Jeśli używamy nut, przeliczamy pitch z uwzględnieniem globalnego przesunięcia oktawy
            step.UpdatePitchFromNote(globalOctaveShift);
            adjustedPitch = step.pitch;
        }
        else
        {
            // Jeśli używamy częstotliwości, przesuwamy o oktawy (mnożymy/dzielimy przez 2 dla każdej oktawy)
            adjustedPitch *= Mathf.Pow(2, globalOctaveShift);
        }

        if (step.duration > 0)
        {
            if (!step.slide)
            {
                currentFrequency = adjustedPitch;
                targetFrequency = adjustedPitch;
                frequency = adjustedPitch;
                phase = 0;
                isSliding = false;
            }
            else
            {
                targetFrequency = adjustedPitch;
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
            ReleaseEnvelope();
        }

        // Zawsze wywołuj zdarzenie zmiany kroku, niezależnie od duration
        OnStepChanged?.Invoke(adjustedPitch, (int)currentStep);

        // Wywołaj ruch obiektu na podstawie wysokości nuty i numeru kroku
        NoteMovement noteMovement = GetComponent<NoteMovement>();
        if (noteMovement != null)
        {
            noteMovement.MoveToNote(adjustedPitch, currentStep);
        }
    }

    public void TriggerEnvelope()
    {
        triggerEnvelope = true;
        isReleasing = false;
    }

    public void ReleaseEnvelope()
    {
        isReleasing = true;
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
                float pulseWidth = 0.5f;
                float pulse = Mathf.Sin(softPulsePhase) > (pulseWidth * 2f - 1f) ? 1f : -1f;
                sample = Mathf.Lerp(pulse, Mathf.Sin(softPulsePhase), 0.7f) * envelope;
                break;
        }

        return sample;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float currentFreq = isSliding ? currentFrequency : frequency;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = GenerateSample();

            phase += currentFreq / sampleRate;
            if (phase >= 1.0)
                phase -= 1.0;

            if (triggerEnvelope)
            {
                envelopeValue += envelopeAttackRate;
                if (envelopeValue >= 1f)
                {
                    envelopeValue = 1f;
                    triggerEnvelope = false;
                }
            }
            else if (!isReleasing)
            {
                envelopeValue -= envelopeDecayRate;
                if (envelopeValue <= sustainLevel)
                {
                    envelopeValue = sustainLevel;
                }
            }
            else
            {
                envelopeValue -= envelopeReleaseRate;
                if (envelopeValue <= 0f)
                {
                    envelopeValue = 0f;
                }
            }

            float modulatedCutoff = currentCutoff + Mathf.Pow(envelopeValue, 2) * 8000f;
            filter.SetParams(modulatedCutoff, resonance, filterQ);
            float filtered = filter.Process(sample);

            // Apply chorus effect
            if (chorusEnabled && chorusAmount > 0)
            {
                float chorusLFO = Mathf.Sin(chorusPhase * 2f * Mathf.PI) * 0.5f + 0.5f;
                chorusPhase += chorusRate / sampleRate;
                if (chorusPhase >= 1f) chorusPhase -= 1f;

                int delayLength = (int)(chorusDepth * sampleRate + chorusLFO * chorusDepth * sampleRate);
                delayLength = Mathf.Clamp(delayLength, 1, chorusBufferLength - 1);

                int readPosition = chorusWritePosition - delayLength;
                if (readPosition < 0) readPosition += chorusBufferLength;

                float delaySample = chorusBuffer[readPosition];
                float wetSample = delaySample * chorusAmount;
                float drySample = filtered * (1f - chorusAmount);

                // Update chorus buffer
                chorusBuffer[chorusWritePosition] = filtered + delaySample * chorusFeedback;
                chorusWritePosition = (chorusWritePosition + 1) % chorusBufferLength;

                float mixedSample = drySample + wetSample;

                // Apply delay effect
                if (delayTime != DelayTime.Off && delayTimeInSamples > 0)
                {
                    int delayReadPosition = delayWritePosition - (int)delayTimeInSamples;
                    if (delayReadPosition < 0) delayReadPosition += delayBufferLength;

                    // Get delayed samples
                    float delayedLeft = delayBufferLeft[delayReadPosition];
                    float delayedRight = delayBufferRight[delayReadPosition];

                    // Write to delay buffer with feedback
                    delayBufferLeft[delayWritePosition] = mixedSample + delayedLeft * delayFeedback;
                    delayBufferRight[delayWritePosition] = mixedSample + delayedRight * delayFeedback;

                    // Mix delayed signal with width control
                    float leftMix = delayedLeft * (1f - delayWidth * 0.5f) + delayedRight * (delayWidth * 0.5f);
                    float rightMix = delayedRight * (1f - delayWidth * 0.5f) + delayedLeft * (delayWidth * 0.5f);

                    // Add delayed signal to output
                    data[i] += leftMix * delayAmount;
                    if (channels > 1)
                        data[i + 1] += rightMix * delayAmount;

                    // Increment and wrap delay write position
                    delayWritePosition = (delayWritePosition + 1) % delayBufferLength;
                }
                else
                {
                    // No delay, just output the mixed sample
                    float finalSample = mixedSample * volume;
                    float clipped = Mathf.Clamp(SoftClip(finalSample), -0.95f, 0.95f);

                    // Output stereo
                    for (int ch = 0; ch < channels; ch++)
                    {
                        float pan = ch == 0 ? 1f - chorusWidth * 0.5f : 0.5f + chorusWidth * 0.5f;
                        data[i + ch] = clipped * pan;
                    }
                }
            }
            else
            {
                // No chorus, just output the filtered sample
                float finalSample = filtered * volume;
                float clipped = Mathf.Clamp(SoftClip(finalSample), -0.95f, 0.95f);

                // Output stereo
                for (int ch = 0; ch < channels; ch++)
                {
                    float pan = ch == 0 ? 1f - chorusWidth * 0.5f : 0.5f + chorusWidth * 0.5f;
                    data[i + ch] = clipped * pan;
                }
            }
        }

        // Apply final volume and clipping
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Mathf.Clamp(data[i] * volume, -1f, 1f);
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

    public class MoogFilter
    {
        private float sampleRate;
        private float cutoff;
        private float resonance;
        private float p, k, r;
        private float[] y = new float[4];
        private float[] oldx = new float[4];
        private float[] oldy = new float[4];
        private FilterType type;
        private float q;

        public MoogFilter(float sampleRate, FilterType type = FilterType.LowPass)
        {
            this.sampleRate = sampleRate;
            this.type = type;
        }

        public void SetParams(float cutoffHz, float resonance, float q = 0.7f)
        {
            cutoff = Mathf.Clamp(cutoffHz, 50f, sampleRate * 0.45f);
            this.resonance = Mathf.Clamp(resonance, 0f, 1f);
            this.q = Mathf.Clamp(q, 0.1f, 2f);

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

            float output = 0f;
            switch (type)
            {
                case FilterType.LowPass:
                    output = y[3];
                    break;
                case FilterType.HighPass:
                    output = input - y[3];
                    break;
                case FilterType.BandPass:
                    float bandWidth = cutoff / q;
                    float centerFreq = cutoff;
                    float lowCutoff = centerFreq - bandWidth * 0.5f;
                    float highCutoff = centerFreq + bandWidth * 0.5f;
                    
                    // Create bandpass by subtracting lowpass from highpass
                    float lowPass = y[3];
                    float highPass = input - y[3];
                    output = highPass - lowPass;
                    break;
            }

            return output;
        }
    }
}