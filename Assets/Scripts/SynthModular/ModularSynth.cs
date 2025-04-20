using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ModularSynth : MonoBehaviour
{
    private List<ModularVoice> voices = new();
    private float sampleRate;
    private int nextVoiceId = 1;

    [Header("Synth Settings")]
    public float attack = 0.5f;
    public float release = 1.5f;
    public float filterSmooth = 0.05f;
    public float drive = 2f;

    [Header("Oscillator Blend")]
    public OscillatorType oscA = OscillatorType.Sine;
    public OscillatorType oscB = OscillatorType.Square;
    [Range(0f, 1f)] public float oscMix = 0.5f;

    [Header("Grain Gate")]
    public bool enableGrainGate = false;
    public float grainRate = 20f;
    [Range(0.01f, 1f)] public float grainDuty = 0.5f;
    [Range(0f, 1f)] public float grainJitter = 0.1f;

    [Header("Octave Control")]
    public int octaveOffset = 0;

    public enum OscillatorType
    {
        Sine,
        Square,
        Saw,
        Triangle,
        Noise,
        HalfSine,
        Pulse,
        SubSine
    }

    private Dictionary<KeyCode, int> keyToNote;

    void Start()
    {
        if (!Application.isPlaying) return;

        sampleRate = AudioSettings.outputSampleRate;

        keyToNote = new()
        {
            { KeyCode.A, 60 }, { KeyCode.S, 62 }, { KeyCode.D, 64 },
            { KeyCode.F, 65 }, { KeyCode.G, 67 }, { KeyCode.H, 69 }, { KeyCode.J, 71 }
        };

        var source = GetComponent<AudioSource>();
        source.clip = AudioClip.Create("ModularSynth", 1, 1, 44100, true);
        source.loop = true;
        source.spatialBlend = 0f;
        source.Play();
    }

    void Update()
    {
        if (!Application.isPlaying || keyToNote == null)
            return;

        foreach (var kvp in keyToNote)
        {
            int midi = kvp.Value + octaveOffset * 12;

            if (Input.GetKeyDown(kvp.Key))
                AddVoice(MidiToFreq(midi));

            if (Input.GetKeyUp(kvp.Key))
                StopVoice(MidiToFreq(midi));
        }

        if (Input.GetKeyDown(KeyCode.Z))
            octaveOffset = Mathf.Max(octaveOffset - 1, -1);

        if (Input.GetKeyDown(KeyCode.X))
            octaveOffset = Mathf.Min(octaveOffset + 1, 1);
    }

    public int AddVoice(float freq)
    {
        IOscillator osc = new DualOscillator(
            MakeOscillator(oscA),
            MakeOscillator(oscB),
            oscMix
        );

        IGrainGate gate = enableGrainGate ? new SimpleGrainGate(grainRate, grainDuty, grainJitter) : null;

        int voiceId = nextVoiceId++;
        voices.Add(new ModularVoice(
            voiceId,
            freq,
            sampleRate,
            osc,
            new Envelope(attack, release),
            new SimpleLowPassFilter(filterSmooth),
            new TanhDistortion(drive),
            gate
        ));

        return voiceId;
    }

    public void StopVoice(float frequency)
    {
        foreach (var v in voices)
        {
            if (v != null && Mathf.Approximately(v.frequency, frequency))
                v.NoteOff();
        }
    }

    public void StopVoiceById(int id)
    {
        foreach (var v in voices)
        {
            if (v != null && v.id == id)
            {
                v.NoteOff();
                break;
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float step = 1f / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = 0f;

            for (int j = 0; j < voices.Count; j++)
            {
                if (voices[j] != null)
                    sample += voices[j].NextSample(step);
            }

            sample = Mathf.Clamp(sample, -1f, 1f);

            for (int c = 0; c < channels; c++)
                data[i + c] = sample;
        }

        voices.RemoveAll(v => v == null || !v.IsActive);
    }

    IOscillator MakeOscillator(OscillatorType type)
    {
        return type switch
        {
            OscillatorType.Sine => new SineOscillator(),
            OscillatorType.Square => new SquareOscillator(),
            OscillatorType.Saw => new SawOscillator(),
            OscillatorType.Triangle => new TriangleOscillator(),
            OscillatorType.Noise => new NoiseOscillator(),
            OscillatorType.HalfSine => new HalfSineOscillator(),
            OscillatorType.Pulse => new PulseOscillator(0.25f),
            OscillatorType.SubSine => new SubOscillator(new SineOscillator()),
            _ => new SineOscillator()
        };
    }

    float MidiToFreq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);
}
