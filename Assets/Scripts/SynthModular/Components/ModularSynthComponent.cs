using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ModularSynthComponent : MonoBehaviour
{
    [Header("Voice Settings")]
    [SerializeField] private int maxVoices = 8;
    [SerializeField] private float masterVolume = 1f;

    [Header("Components")]
    [SerializeField] private BaseOscillatorComponent[] oscillators;
    [SerializeField] private BaseFilterComponent[] filters;
    [SerializeField] private BaseEnvelopeComponent[] envelopes;

    private AudioSource audioSource;
    private float sampleRate;
    private List<Voice> activeVoices = new();
    private float[] voiceBuffer;

    private class Voice
    {
        public int id;
        public float frequency;
        public float[] phases;
        public bool isActive;
        public BaseEnvelopeComponent envelope;
        public float envelopeValue;

        public Voice(int id, float frequency, int oscillatorCount)
        {
            this.id = id;
            this.frequency = frequency;
            this.phases = new float[oscillatorCount];
            this.isActive = true;
            this.envelopeValue = 0f;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;

        // Initialize audio source
        audioSource.clip = AudioClip.Create("ModularSynth", 1, 1, (int)sampleRate, true);
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.Play();

        // Initialize voice buffer
        voiceBuffer = new float[maxVoices];

        // Validate components
        if (oscillators == null || oscillators.Length == 0)
        {
            Debug.LogError("No oscillators assigned to ModularSynthComponent!");
            return;
        }
        if (filters == null)
        {
            filters = new BaseFilterComponent[0];
        }
        if (envelopes == null || envelopes.Length == 0)
        {
            Debug.LogWarning("No envelopes assigned to ModularSynthComponent!");
            envelopes = new BaseEnvelopeComponent[0];
        }
    }

    public int NoteOn(float frequency)
    {
        // Find inactive voice or oldest voice
        Voice voice = null;
        if (activeVoices.Count < maxVoices)
        {
            voice = new Voice(activeVoices.Count, frequency, oscillators.Length);
            if (envelopes.Length > 0)
            {
                voice.envelope = envelopes[0];
                voice.envelope.NoteOn();
            }
            activeVoices.Add(voice);
        }
        else
        {
            // Replace oldest voice
            voice = activeVoices[0];
            voice.frequency = frequency;
            for (int i = 0; i < voice.phases.Length; i++)
            {
                voice.phases[i] = 0f;
            }
            voice.isActive = true;
            if (voice.envelope != null)
            {
                voice.envelope.NoteOn();
            }
        }

        return voice.id;
    }

    public void NoteOff(int voiceId)
    {
        foreach (var voice in activeVoices)
        {
            if (voice.id == voiceId && voice.isActive)
            {
                if (voice.envelope != null)
                {
                    voice.envelope.NoteOff();
                }
                else
                {
                    voice.isActive = false;
                }
                break;
            }
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (oscillators == null || oscillators.Length == 0)
            return;

        float step = 1f / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = 0f;

            // Process each voice
            for (int v = 0; v < activeVoices.Count; v++)
            {
                var voice = activeVoices[v];
                if (!voice.isActive && voice.envelope == null) continue;

                // Get oscillator samples
                float oscSample = 0f;
                for (int o = 0; o < oscillators.Length; o++)
                {
                    if (oscillators[o] != null)
                    {
                        float increment = 2f * Mathf.PI * voice.frequency / sampleRate;
                        voice.phases[o] += increment;
                        if (voice.phases[o] > 2f * Mathf.PI) voice.phases[o] -= 2f * Mathf.PI;
                        
                        oscSample += oscillators[o].GetSample(voice.phases[o]) * oscillators[o].GetAmplitude();
                    }
                }
                oscSample /= oscillators.Length;

                // Apply filters
                float filtered = oscSample;
                foreach (var filter in filters)
                {
                    if (filter != null)
                    {
                        filtered = filter.ProcessSample(filtered);
                    }
                }

                // Apply envelope
                float amplitude = 1f;
                if (voice.envelope != null)
                {
                    amplitude = voice.envelope.GetAmplitude(step);
                    voice.envelopeValue = amplitude;
                    
                    // Jeśli koperta zakończyła się (amplitude <= 0), oznacz głos jako nieaktywny
                    if (amplitude <= 0.001f && !voice.isActive)
                    {
                        voice.isActive = false;
                    }
                }

                voiceBuffer[v] = filtered * amplitude;
                sample += voiceBuffer[v];
            }

            // Mix voices and apply master volume
            sample = Mathf.Clamp(sample * masterVolume, -1f, 1f);

            // Write to all channels
            for (int c = 0; c < channels; c++)
            {
                data[i + c] = sample;
            }
        }

        // Remove finished voices
        activeVoices.RemoveAll(v => !v.isActive && (v.envelope == null || v.envelopeValue <= 0.001f));
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }
} 