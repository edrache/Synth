using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioReverbFilter))]
public class DungeonSynth : MonoBehaviour {
    private float phase;
    private float frequency = 440f;
    private float sampleRate;
    private float amplitude = 0f;
    private float targetAmp = 0f;
    private float synthTime = 0f;
    private float prevFilteredSample = 0f;
    private float noiseTimer = 0f;
    private float currentNoiseValue = 0f;

    [Header("ADSR Envelope")]
    public float attack = 0.5f;
    public float release = 1.5f;

    [Header("Octave Settings")]
    public int octaveOffset = 0; // -2 to +2

    [Header("Noise Settings")]
    public bool addSoftNoise = false;
    public float noiseLevel = 0.05f;
    public float noiseGrainHz = 60f; // granularność szumu w Hz

    [Header("Low Pass Filter")]
    public bool useLowPassFilter = true;
    public float filterSmoothing = 0.05f; // 0 = no smoothing, 1 = max smoothing

    [Header("Reverb Settings")]
    public bool enableReverb = true;
    public AudioReverbPreset reverbPreset = AudioReverbPreset.Cave;
    [Range(0f, 1.1f)] public float reverbLevel = 0.8f; // wzmocnienie reverbu

    private Dictionary<KeyCode, int> keyToNote;

    void Start() {
        sampleRate = AudioSettings.outputSampleRate;

        keyToNote = new Dictionary<KeyCode, int> {
            { KeyCode.A, 60 }, { KeyCode.W, 61 }, { KeyCode.S, 62 }, { KeyCode.E, 63 }, { KeyCode.D, 64 },
            { KeyCode.F, 65 }, { KeyCode.T, 66 }, { KeyCode.G, 67 }, { KeyCode.Y, 68 }, { KeyCode.H, 69 },
            { KeyCode.U, 70 }, { KeyCode.J, 71 }, { KeyCode.K, 72 }
        };

        GetComponent<AudioSource>().Play();

        var reverb = GetComponent<AudioReverbFilter>();
        reverb.enabled = enableReverb;
        reverb.reverbPreset = reverbPreset;
        reverb.dryLevel = 0f;
        reverb.reverbLevel = Mathf.Lerp(-10000f, 0f, Mathf.Clamp01(reverbLevel));
    }

    void Update() {
        foreach (var kvp in keyToNote) {
            if (Input.GetKeyDown(kvp.Key)) {
                frequency = MidiToFreq(kvp.Value + (octaveOffset * 12));
                targetAmp = 0.4f;
            }
            if (Input.GetKeyUp(kvp.Key)) {
                targetAmp = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Z)) octaveOffset = Mathf.Max(octaveOffset - 1, -2);
        if (Input.GetKeyDown(KeyCode.X)) octaveOffset = Mathf.Min(octaveOffset + 1, 2);
    }

    void OnAudioFilterRead(float[] data, int channels) {
        float step = 1f / sampleRate;
        float detune2 = 0.01f;
        float detune3 = -0.01f;
        float noiseStep = 1f / noiseGrainHz;

        for (int i = 0; i < data.Length; i += channels) {
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

            if (addSoftNoise) {
                noiseTimer += step;
                if (noiseTimer >= noiseStep) {
                    noiseTimer -= noiseStep;
                    float seed = Mathf.Sin((synthTime + noiseTimer) * 1234.567f) * 43758.5453f;
                    currentNoiseValue = (seed - Mathf.Floor(seed)) * 2f - 1f;
                }
                sample += currentNoiseValue * noiseLevel * amplitude;
            }

            if (useLowPassFilter) {
                sample = Mathf.Lerp(prevFilteredSample, sample, 1f - filterSmoothing);
                prevFilteredSample = sample;
            }

            for (int c = 0; c < channels; c++)
                data[i + c] = sample;

            synthTime += step;
        }
    }

    float MidiToFreq(int midi) {
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }
}
