using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PolyDungeonSynth : MonoBehaviour {
    class Voice {
        public float frequency;
        public float phase;
        public float amplitude = 0f;
        public float targetAmp = 0f;
        public bool active = true;

        private float attack;
        private float release;
        private float synthTime = 0f;
        private float noiseTimer = 0f;
        private float currentNoiseValue = 0f;

        private float detune2 = 0.01f;
        private float detune3 = -0.01f;

        private float noiseLevel;
        private float noiseGrainHz;
        private bool addNoise;

        private float filterSmoothing;
        private bool useFilter;
        private float prevFilteredSample = 0f;

        public Voice(float freq, float attack, float release, float noiseLevel, float noiseGrainHz, bool addNoise, bool useFilter, float filterSmoothing) {
            frequency = freq;
            phase = 0f;
            this.attack = attack;
            this.release = release;
            targetAmp = 0.4f;
            this.noiseLevel = noiseLevel;
            this.noiseGrainHz = noiseGrainHz;
            this.addNoise = addNoise;
            this.useFilter = useFilter;
            this.filterSmoothing = filterSmoothing;
        }

        public float NextSample(float step, float sampleRate) {
            if (targetAmp > amplitude)
                amplitude += step / attack;
            else
                amplitude -= step / release;
            amplitude = Mathf.Clamp01(amplitude);

            float lfo = Mathf.Sin(synthTime * 0.5f * 2f * Mathf.PI) * 0.2f + 0.8f;

            float sample =
                Mathf.Sin(phase) +
                Mathf.Sin(phase * (1f + detune2)) +
                Mathf.Sin(phase * (1f + detune3));
            sample /= 3f;
            sample *= amplitude * lfo * 0.5f;

            float increment = 2f * Mathf.PI * frequency / sampleRate;
            phase += increment;
            if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI;

            if (addNoise) {
                noiseTimer += step;
                float noiseStep = 1f / noiseGrainHz;
                if (noiseTimer >= noiseStep) {
                    noiseTimer -= noiseStep;
                    float seed = Mathf.Sin((synthTime + noiseTimer) * 1234.567f) * 43758.5453f;
                    currentNoiseValue = (seed - Mathf.Floor(seed)) * 2f - 1f;
                }
                sample += currentNoiseValue * noiseLevel * amplitude;
            }

            if (useFilter) {
                sample = Mathf.Lerp(prevFilteredSample, sample, 1f - filterSmoothing);
                prevFilteredSample = sample;
            }

            synthTime += step;

            if (amplitude <= 0.001f && targetAmp == 0f) active = false;

            return sample;
        }
    }

    private List<Voice> voices = new List<Voice>();
    private Dictionary<KeyCode, int> keyToNote;
    private float sampleRate;

    [Header("ADSR Envelope")]
    public float attack = 0.5f;
    public float release = 1.5f;

    [Header("Noise Settings")]
    public bool addSoftNoise = true;
    public float noiseLevel = 0.05f;
    public float noiseGrainHz = 60f;

    [Header("Low Pass Filter")]
    public bool useLowPassFilter = true;
    public float filterSmoothing = 0.05f;

    void Start() {
        sampleRate = AudioSettings.outputSampleRate;

        keyToNote = new Dictionary<KeyCode, int> {
            { KeyCode.A, 60 }, { KeyCode.W, 61 }, { KeyCode.S, 62 }, { KeyCode.E, 63 }, { KeyCode.D, 64 },
            { KeyCode.F, 65 }, { KeyCode.T, 66 }, { KeyCode.G, 67 }, { KeyCode.Y, 68 }, { KeyCode.H, 69 },
            { KeyCode.U, 70 }, { KeyCode.J, 71 }, { KeyCode.K, 72 }
        };

        var source = GetComponent<AudioSource>();
        source.clip = AudioClip.Create("Synth", 1, 1, 44100, true);
        source.loop = true;
        source.spatialBlend = 0f;
        source.Play();
    }

    void Update() {
        foreach (var kvp in keyToNote) {
            if (Input.GetKeyDown(kvp.Key)) {
                float freq = MidiToFreq(kvp.Value);
                voices.Add(new Voice(freq, attack, release, noiseLevel, noiseGrainHz, addSoftNoise, useLowPassFilter, filterSmoothing));
            }
            if (Input.GetKeyUp(kvp.Key)) {
                float freq = MidiToFreq(kvp.Value);
                foreach (var voice in voices) {
                    if (Mathf.Approximately(voice.frequency, freq)) {
                        voice.targetAmp = 0f;
                    }
                }
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels) {
        float step = 1f / sampleRate;

        for (int i = 0; i < data.Length; i += channels) {
            float sample = 0f;

            foreach (var voice in voices)
                sample += voice.NextSample(step, sampleRate);

            sample = Mathf.Clamp(sample, -1f, 1f);

            for (int c = 0; c < channels; c++)
                data[i + c] = sample;

            voices.RemoveAll(v => !v.active);
        }
    }

    float MidiToFreq(int midi) {
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }
}
