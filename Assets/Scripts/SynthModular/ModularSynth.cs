using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // Add LINQ support
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ModularSynth : MonoBehaviour
{
    private List<ModularVoice> voices = new();
    private float sampleRate;
    private int nextVoiceId = 1;
    private TimelineBPMController bpmController;

    [Header("Arpeggiator")]
    public bool enableArpeggiator = false;
    [Range(1, 32)]
    [Tooltip("Division of the beat (4 = sixteenth notes, 2 = eighth notes, 1 = quarter notes)")]
    public int arpDivision = 4;
    
    [Tooltip("How the arpeggiator plays notes")]
    public ArpeggiatorMode arpMode = ArpeggiatorMode.SingleNote;
    
    [Range(1, 4)]
    [Tooltip("How many octaves the arpeggiator can span. Example: if you press C4 and range is 2, ScaleUp will go up to C6")]
    public int arpOctaveRange = 1;

    private ArpeggiatorMode previousArpMode;
    private float arpTimer = 0f;
    private List<float> arpNotes = new();
    private int currentArpIndex = 0;
    private float baseFrequency; // Frequency of the currently held note
    private List<int> availableScaleNotes = new(); // Store all available notes for the sequence
    private int originalMidiNote; // Store the original note pressed

    [Header("Synth Settings")]
    public float attack = 0.5f;
    public float release = 1.5f;
    public float filterSmooth = 0.05f;
    public float drive = 2f;

    [Header("Oscillator Blend")]
    public OscillatorType oscA = OscillatorType.Sine;
    public OscillatorType oscB = OscillatorType.Square;
    [Range(0f, 1f)] public float oscMix = 0.5f;

    [Header("Morphing Oscillator Settings")]
    [Range(0.01f, 1f)] public float morphSpeed = 0.1f;
    [Range(0f, 1f)] public float morphPhase = 0f;
    public MorphingOscillator.OscillatorSlot[] morphingOscillators = new MorphingOscillator.OscillatorSlot[4];

    [Header("Grain Gate")]
    public bool enableGrainGate = false;
    public float grainRate = 20f;
    [Range(0.01f, 1f)] public float grainDuty = 0.5f;
    [Range(0f, 1f)] public float grainJitter = 0.1f;

    [Header("Octave Control")]
    public int octaveOffset = 0;

    [Header("Custom Wave Oscillator")]
    public AnimationCurve customWaveCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.25f, 1f),
        new Keyframe(0.5f, 0f),
        new Keyframe(0.75f, -1f),
        new Keyframe(1f, 0f)
    );

    [Header("Effects")]
    public bool enableTextureEffect = false;
    private TextureEffect textureEffect;

    public enum OscillatorType
    {
        Sine,
        Square,
        Saw,
        Triangle,
        Noise,
        HalfSine,
        Pulse,
        SubSine,
        Morphing,
        Custom
    }

    public enum ArpeggiatorMode
    {
        [InspectorName("Single Note (Repeat)")]
        SingleNote,      // Just repeat the same note
        [InspectorName("Scale Up")]
        ScaleUp,        // Go up the scale from the pressed note
        [InspectorName("Scale Down")]
        ScaleDown,      // Go down the scale from the pressed note
        [InspectorName("Up (Multiple Notes)")]
        Up,             // Original up mode
        [InspectorName("Down (Multiple Notes)")]
        Down,           // Original down mode
        [InspectorName("Up-Down (Multiple Notes)")]
        UpDown,         // Original up-down mode
        [InspectorName("Random (Multiple Notes)")]
        Random          // Original random mode
    }

    private Dictionary<KeyCode, int> keyToNote;

    void Start()
    {
        if (!Application.isPlaying) return;

        sampleRate = AudioSettings.outputSampleRate;
        bpmController = FindObjectOfType<TimelineBPMController>();
        textureEffect = GetComponent<TextureEffect>();

        keyToNote = new()
        {
            { KeyCode.A, 60 }, // C
            { KeyCode.W, 61 }, // C#
            { KeyCode.S, 62 }, // D
            { KeyCode.E, 63 }, // D#
            { KeyCode.D, 64 }, // E
            { KeyCode.F, 65 }, // F
            { KeyCode.T, 66 }, // F#
            { KeyCode.G, 67 }, // G
            { KeyCode.Y, 68 }, // G#
            { KeyCode.H, 69 }, // A
            { KeyCode.U, 70 }, // A#
            { KeyCode.J, 71 }, // B
            { KeyCode.K, 72 }  // C (octave up)
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

        // Reset index when mode changes
        if (previousArpMode != arpMode)
        {
            currentArpIndex = 0;
            availableScaleNotes.Clear(); // Clear the scale notes when mode changes
            previousArpMode = arpMode;
            Debug.Log($"Arpeggiator mode changed to: {arpMode}");
        }

        bool anyKeyHeld = false;
        bool anyTimelineNoteActive = false;

        // Check for timeline notes first
        if (enableArpeggiator && arpNotes.Count > 0)
        {
            anyTimelineNoteActive = true;
        }

        // Then check keyboard keys
        foreach (var kvp in keyToNote)
        {
            int midi = kvp.Value + octaveOffset * 12;

            if (Input.GetKeyDown(kvp.Key))
            {
                float freq = MidiToFreq(midi);
                Debug.Log($"Key pressed: {kvp.Key}, MIDI={midi}, Frequency={freq}Hz");
                
                if (enableArpeggiator)
                {
                    arpNotes.Clear();
                    availableScaleNotes.Clear(); // Clear the scale notes when new key is pressed
                    arpNotes.Add(freq);
                    currentArpIndex = -1; // Start at -1 so first increment puts us at 0
                    arpTimer = 0f;
                    Debug.Log($"Added note: {freq}Hz (MIDI: {midi})");
                }
                else
                {
                    AddVoice(freq);
                }
            }

            if (Input.GetKey(kvp.Key))
            {
                anyKeyHeld = true;
            }

            if (Input.GetKeyUp(kvp.Key))
            {
                float freq = MidiToFreq(midi);
                Debug.Log($"Key released: {kvp.Key}, MIDI={midi}, Frequency={freq}Hz");
                
                if (enableArpeggiator)
                {
                    arpNotes.Clear();
                    availableScaleNotes.Clear();
                    StopAllVoices();
                    Debug.Log("Cleared all arpeggiator notes");
                }
                else
                {
                    StopVoice(freq);
                }
            }
        }

        // Only stop arpeggiator if there are no active notes from any source
        if (!anyKeyHeld && !anyTimelineNoteActive && enableArpeggiator)
        {
            if (arpNotes.Count > 0)
            {
                arpNotes.Clear();
                availableScaleNotes.Clear();
                StopAllVoices();
                Debug.Log("No active notes - stopped arpeggiator");
            }
        }

        // Handle arpeggiator
        if (enableArpeggiator && arpNotes.Count > 0 && bpmController != null)
        {
            float beatDuration = 60f / bpmController.BPM;
            float arpInterval = beatDuration / arpDivision;
            
            arpTimer += Time.deltaTime;
            if (arpTimer >= arpInterval)
            {
                arpTimer = 0f;
                PlayNextArpNote();
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
            octaveOffset = Mathf.Max(octaveOffset - 1, -1);

        if (Input.GetKeyDown(KeyCode.X))
            octaveOffset = Mathf.Min(octaveOffset + 1, 1);
    }

    private void OnDisable()
    {
        // Clean up when component is disabled
        arpNotes.Clear();
        StopAllVoices();
    }

    private void StopAllVoices()
    {
        foreach (var voice in voices.ToArray()) // Create a copy of the list to avoid modification during iteration
        {
            if (voice != null)
            {
                voice.NoteOff();
            }
        }
        voices.RemoveAll(v => v == null || !v.IsActive);
    }

    private void PlayNextArpNote()
    {
        if (arpNotes.Count == 0) return;

        StopAllVoices();

        switch (arpMode)
        {
            case ArpeggiatorMode.SingleNote:
                // Just keep playing the same note
                AddVoice(arpNotes[0]);
                break;

            case ArpeggiatorMode.ScaleUp:
                if (bpmController != null)
                {
                    // If we haven't generated the scale notes yet or we're starting a new sequence
                    if (availableScaleNotes.Count == 0)
                    {
                        // Get the current MIDI note number
                        float freq = arpNotes[0];
                        originalMidiNote = Mathf.RoundToInt(12 * Mathf.Log(freq / 440.0f, 2) + 69);
                        
                        // Get the scale notes
                        int[] scaleNotes = bpmController.GetScaleNotes();
                        
                        // Find all possible notes in our range
                        availableScaleNotes.Add(originalMidiNote);
                        
                        // Add all scale notes within our octave range
                        foreach (int scaleNote in scaleNotes)
                        {
                            for (int octave = 0; octave < arpOctaveRange; octave++)
                            {
                                int noteInRange = scaleNote + (12 * octave);
                                if (noteInRange > originalMidiNote && noteInRange <= originalMidiNote + (12 * arpOctaveRange))
                                {
                                    availableScaleNotes.Add(noteInRange);
                                }
                            }
                        }
                        
                        // Sort the notes
                        availableScaleNotes.Sort();
                        currentArpIndex = -1; // Start at -1 so first increment puts us at 0

                        string availableNoteNames = string.Join(", ", availableScaleNotes.Select(n => GetNoteNameFromMidi(n)).ToArray());
                        Debug.Log($"Generated scale sequence: {availableNoteNames}");
                    }

                    // Get the next note in sequence
                    currentArpIndex = (currentArpIndex + 1) % availableScaleNotes.Count;
                    int nextNote = availableScaleNotes[currentArpIndex];
                    
                    // Play the note
                    float nextFreq = MidiToFreq(nextNote);
                    AddVoice(nextFreq);
                    
                    Debug.Log($"Playing note {currentArpIndex + 1} of {availableScaleNotes.Count}: {GetNoteNameFromMidi(nextNote)}");
                }
                break;

            case ArpeggiatorMode.ScaleDown:
                if (bpmController != null)
                {
                    if (availableScaleNotes.Count == 0)
                    {
                        float freq = arpNotes[0];
                        originalMidiNote = Mathf.RoundToInt(12 * Mathf.Log(freq / 440.0f, 2) + 69);
                        int[] scaleNotes = bpmController.GetScaleNotes();
                        
                        Debug.Log($"Starting ScaleDown from note {GetNoteNameFromMidi(originalMidiNote)}");
                        Debug.Log($"Scale notes: {string.Join(", ", scaleNotes.Select(n => GetNoteNameFromMidi(n)))}");
                        
                        // Start with the original note
                        availableScaleNotes.Add(originalMidiNote);
                        
                        // Find the position of our note in the scale
                        int noteIndexInScale = -1;
                        int originalNoteInOctave = originalMidiNote % 12;
                        for (int i = 0; i < scaleNotes.Length; i++)
                        {
                            if (scaleNotes[i] % 12 == originalNoteInOctave)
                            {
                                noteIndexInScale = i;
                                break;
                            }
                        }
                        
                        if (noteIndexInScale == -1)
                        {
                            Debug.LogError($"Note {GetNoteNameFromMidi(originalMidiNote)} not found in scale!");
                            break;
                        }
                        
                        // Generate all notes below the original note
                        for (int octave = 0; octave < arpOctaveRange; octave++)
                        {
                            int currentOctave = (originalMidiNote / 12) - octave;
                            
                            // Start from the note below our original note in the scale
                            for (int i = noteIndexInScale - 1; i >= 0; i--)
                            {
                                int note = scaleNotes[i] % 12; // Get note without octave
                                int fullNote = currentOctave * 12 + note;
                                
                                if (fullNote < originalMidiNote && fullNote >= originalMidiNote - (12 * arpOctaveRange))
                                {
                                    if (!availableScaleNotes.Contains(fullNote))
                                    {
                                        availableScaleNotes.Add(fullNote);
                                        Debug.Log($"Added note: {GetNoteNameFromMidi(fullNote)}");
                                    }
                                }
                            }
                            
                            // For the next octave, start from the highest note in the scale
                            for (int i = scaleNotes.Length - 1; i >= 0; i--)
                            {
                                int note = scaleNotes[i] % 12; // Get note without octave
                                int fullNote = (currentOctave - 1) * 12 + note;
                                
                                if (fullNote < originalMidiNote && fullNote >= originalMidiNote - (12 * arpOctaveRange))
                                {
                                    if (!availableScaleNotes.Contains(fullNote))
                                    {
                                        availableScaleNotes.Add(fullNote);
                                        Debug.Log($"Added note: {GetNoteNameFromMidi(fullNote)}");
                                    }
                                }
                            }
                        }
                        
                        availableScaleNotes.Sort((a, b) => b.CompareTo(a));
                        currentArpIndex = -1;
                        
                        string availableNoteNames = string.Join(", ", availableScaleNotes.Select(n => GetNoteNameFromMidi(n)).ToArray());
                        Debug.Log($"Final scale sequence (down): {availableNoteNames}");
                    }

                    currentArpIndex = (currentArpIndex + 1) % availableScaleNotes.Count;
                    int nextNote = availableScaleNotes[currentArpIndex];
                    float nextFreq = MidiToFreq(nextNote);
                    AddVoice(nextFreq);
                    
                    Debug.Log($"Playing note {currentArpIndex + 1} of {availableScaleNotes.Count}: {GetNoteNameFromMidi(nextNote)}");
                }
                break;

            case ArpeggiatorMode.Up:
                currentArpIndex = (currentArpIndex + 1) % arpNotes.Count;
                AddVoice(arpNotes[currentArpIndex]);
                break;

            case ArpeggiatorMode.Down:
                currentArpIndex = (currentArpIndex - 1 + arpNotes.Count) % arpNotes.Count;
                AddVoice(arpNotes[currentArpIndex]);
                break;

            case ArpeggiatorMode.UpDown:
                if (arpNotes.Count == 1)
                {
                    AddVoice(arpNotes[0]);
                }
                else
                {
                    if (currentArpIndex == arpNotes.Count - 1)
                        currentArpIndex = arpNotes.Count - 2;
                    else if (currentArpIndex == 0)
                        currentArpIndex = 1;
                    else
                        currentArpIndex = (currentArpIndex + 1) % arpNotes.Count;
                    AddVoice(arpNotes[currentArpIndex]);
                }
                break;

            case ArpeggiatorMode.Random:
                currentArpIndex = UnityEngine.Random.Range(0, arpNotes.Count);
                AddVoice(arpNotes[currentArpIndex]);
                break;
        }
    }

    private string GetNoteNameFromMidi(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{noteNames[noteIndex]}{octave}";
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

            // Apply texture effect if enabled
            if (enableTextureEffect && textureEffect != null)
            {
                sample = textureEffect.ProcessSample(sample);
            }

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
            OscillatorType.Morphing => new MorphingOscillator(morphSpeed, morphingOscillators),
            OscillatorType.Custom => new CustomWaveOscillator(customWaveCurve),
            _ => new SineOscillator()
        };
    }

    float MidiToFreq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

    public SynthPreset SavePreset()
    {
        SynthPreset preset = new SynthPreset
        {
            Frequency = 440f, // Example frequency, adjust as needed
            OscillatorType = oscA.ToString(),
            OscillatorTypeB = oscB.ToString(),
            OscMix = this.oscMix,
            EnableGrainGate = this.enableGrainGate,
            EnvelopeSettings = new EnvelopeSettings { Attack = this.attack, Release = this.release },
            FilterSettings = new FilterSettings { Smoothing = this.filterSmooth },
            DistortionSettings = new DistortionSettings { Drive = this.drive },
            GrainGateSettings = new GrainGateSettings { BaseRate = this.grainRate, BaseDuty = this.grainDuty, JitterAmount = this.grainJitter }
        };

        Debug.Log("Preset being saved with the following settings:");
        Debug.Log($"Frequency: {preset.Frequency}");
        Debug.Log($"OscillatorType: {preset.OscillatorType}, OscillatorTypeB: {preset.OscillatorTypeB}, OscMix: {preset.OscMix}");
        Debug.Log($"EnableGrainGate: {preset.EnableGrainGate}");
        Debug.Log($"Envelope Attack: {preset.EnvelopeSettings.Attack}, Release: {preset.EnvelopeSettings.Release}");
        Debug.Log($"Filter Smoothing: {preset.FilterSettings.Smoothing}");
        Debug.Log($"Distortion Drive: {preset.DistortionSettings.Drive}");
        Debug.Log($"GrainGate BaseRate: {preset.GrainGateSettings.BaseRate}, BaseDuty: {preset.GrainGateSettings.BaseDuty}, JitterAmount: {preset.GrainGateSettings.JitterAmount}");

        return preset;
    }

    public void LoadPreset(SynthPreset preset)
    {
        this.attack = preset.EnvelopeSettings.Attack;
        this.release = preset.EnvelopeSettings.Release;
        this.filterSmooth = preset.FilterSettings.Smoothing;
        this.drive = preset.DistortionSettings.Drive;
        this.grainRate = preset.GrainGateSettings.BaseRate;
        this.grainDuty = preset.GrainGateSettings.BaseDuty;
        this.grainJitter = preset.GrainGateSettings.JitterAmount;
        this.oscA = (OscillatorType)Enum.Parse(typeof(OscillatorType), preset.OscillatorType);
        this.oscB = (OscillatorType)Enum.Parse(typeof(OscillatorType), preset.OscillatorTypeB);
        this.oscMix = preset.OscMix;
        this.enableGrainGate = preset.EnableGrainGate;
    }

    // Metody do obsługi nut z timeline
    public void PlayTimelineNote(int midiNote, float duration)
    {
        if (enableArpeggiator)
        {
            float freq = MidiToFreq(midiNote);
            arpNotes.Clear();
            availableScaleNotes.Clear();
            arpNotes.Add(freq);
            currentArpIndex = -1;
            arpTimer = 0f;
            Debug.Log($"Timeline note added to arpeggiator: {freq}Hz (MIDI: {midiNote})");
        }
        else
        {
            float freq = MidiToFreq(midiNote);
            int voiceId = AddVoice(freq);
            Debug.Log($"Playing timeline note {GetNoteNameFromMidi(midiNote)} for {duration}s");
            
            // Start coroutine to stop the note after duration
            StartCoroutine(StopNoteAfterDuration(voiceId, duration));
        }
    }

    private System.Collections.IEnumerator StopNoteAfterDuration(int voiceId, float duration)
    {
        yield return new WaitForSeconds(duration);
        StopVoiceById(voiceId);
        Debug.Log($"Stopped timeline note after {duration}s");
    }

    public void StopTimelineNote(int midiNote)
    {
        if (enableArpeggiator)
        {
            arpNotes.Clear();
            availableScaleNotes.Clear();
            StopAllVoices();
            Debug.Log("Stopped timeline arpeggiator note");
        }
        else
        {
            float freq = MidiToFreq(midiNote);
            StopVoice(freq);
        }
    }

    public void PlayNote(int midiNote)
    {
        float freq = MidiToFreq(midiNote);
        AddVoice(freq);
    }

    public void StopNote(int midiNote)
    {
        float freq = MidiToFreq(midiNote);
        StopVoice(freq);
    }
}
