using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using System;

public enum DecayTimeUnit
{
    Seconds,
    Beats
}

[RequireComponent(typeof(AudioSource))]
public class Sampler : MonoBehaviour, ISampler
{
    [Header("Sample Settings")]
    public AudioClip sample;
    [Range(0, 9)] public int octave = 4;
    public Note rootNote = Note.C;
    [Range(-2, 2)] public int octaveOffset = 0;
    public bool oneShot = false;
    [Tooltip("If true, notes will be automatically adjusted to fit the current musical scale")]
    public bool useScaleAdjustment = false;

    [Header("Playback Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float pan = 0f;
    [Tooltip("AudioMixer do którego będą dodawane głosy")]
    public AudioMixerGroup outputMixerGroup;

    [Header("Envelope Settings")]
    [Tooltip("Krzywa zanikania dźwięku po puszczeniu nuty (tylko gdy OneShot = false). Oś X: czas [s], oś Y: głośność (1 = velocity nuty, 0 = cisza)")]
    public AnimationCurve decayCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Tooltip("Jednostka czasu zanikania dźwięku")] public DecayTimeUnit decayTimeUnit = DecayTimeUnit.Seconds;
    [Tooltip("Czas trwania zanikania dźwięku (w sekundach lub beatach)")]
    public float decayTime = 1.0f;

    private AudioSource audioSource;
    private Dictionary<int, AudioSource> activeVoices;
    private float sampleRate;
    private float rootFrequency;
    private int rootMidiNote;

    public static event Action<Sampler, int> OnAnyNotePlayed;

    public bool OneShot
    {
        get => oneShot;
        set => oneShot = value;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on Sampler GameObject!");
            enabled = false;
            return;
        }

        activeVoices = new Dictionary<int, AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;
        UpdateRootNote();

        if (sample == null)
        {
            Debug.LogWarning("No sample loaded in Sampler. Please load a sample before playing notes.");
        }
    }

    private void UpdateRootNote()
    {
        rootMidiNote = (int)rootNote + (octave + 1) * 12;
        rootFrequency = MidiToFreq(rootMidiNote);
    }

    public void LoadSample(AudioClip clip, int rootNote)
    {
        if (clip == null)
        {
            Debug.LogError("Cannot load null sample!");
            return;
        }

        sample = clip;
        this.rootNote = (Note)(rootNote % 12);
        this.octave = (rootNote / 12) - 1;
        UpdateRootNote();
    }

    public void PlayNote(int midiNote)
    {
        PlayNote(midiNote, 0.8f);
    }

    public void PlayNote(int midiNote, float velocity)
    {
        if (sample == null)
        {
            Debug.LogWarning("Cannot play note: no sample loaded!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioSource not initialized!");
            return;
        }

        // If scale adjustment is enabled, modify the note to fit the current scale
        if (useScaleAdjustment)
        {
            var bpmController = FindObjectOfType<TimelineBPMController>();
            if (bpmController != null)
            {
                int[] scaleNotes = bpmController.GetScaleNotes();
                if (scaleNotes != null && scaleNotes.Length > 0)
                {
                    // Get the octave and note number (0-11) of the current note
                    int currentOctave = midiNote / 12;
                    int currentNoteInOctave = midiNote % 12;
                    
                    // Find the closest note in the scale within the same octave
                    int closestNoteInOctave = scaleNotes[0] % 12;
                    int minDistance = Mathf.Abs(currentNoteInOctave - closestNoteInOctave);
                    
                    foreach (int scaleNote in scaleNotes)
                    {
                        int noteInOctave = scaleNote % 12;
                        int distance = Mathf.Abs(currentNoteInOctave - noteInOctave);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestNoteInOctave = noteInOctave;
                        }
                    }
                    
                    // Combine the original octave with the closest note in scale
                    midiNote = (currentOctave * 12) + closestNoteInOctave;
                }
            }
        }

        float targetFreq = MidiToFreq(midiNote);
        float pitch = targetFreq / rootFrequency;

        var voice = gameObject.AddComponent<AudioSource>();
        voice.clip = sample;
        voice.volume = volume * velocity;
        voice.panStereo = pan;
        voice.pitch = pitch;
        voice.loop = false;
        voice.outputAudioMixerGroup = outputMixerGroup;
        voice.Play();

        activeVoices[midiNote] = voice;

        if (oneShot)
        {
            StartCoroutine(CleanupVoiceAfterPlayback(voice, midiNote));
        }

        OnAnyNotePlayed?.Invoke(this, midiNote);
    }

    private System.Collections.IEnumerator CleanupVoiceAfterPlayback(AudioSource voice, int midiNote)
    {
        yield return new WaitForSeconds(sample.length / voice.pitch);
        if (voice != null)
        {
            Destroy(voice);
            activeVoices.Remove(midiNote);
        }
    }

    public void StopNote(int midiNote)
    {
        if (activeVoices == null)
            return;

        if (!oneShot && activeVoices.TryGetValue(midiNote, out var voice))
        {
            if (voice == null)
                return;
            StartCoroutine(FadeOutAndStop(voice, midiNote));
            activeVoices.Remove(midiNote);
        }
    }

    private float GetDecayTimeSeconds()
    {
        float bpm = 120f;
        var bpmController = FindObjectOfType<TimelineBPMController>();
        if (bpmController != null)
            bpm = bpmController.BPM;
        if (decayTimeUnit == DecayTimeUnit.Seconds)
            return decayTime;
        else // Beats
            return 60f * decayTime / Mathf.Max(1f, bpm);
    }

    private System.Collections.IEnumerator FadeOutAndStop(AudioSource voice, int midiNote)
    {
        if (voice == null)
            yield break;

        float startVolume = voice.volume;
        float elapsed = 0f;
        float decayDuration = GetDecayTimeSeconds();
        while (elapsed < decayDuration && voice != null)
        {
            float t = decayDuration > 0f ? (elapsed / decayDuration) : 1f;
            float curveValue = decayCurve.Evaluate(t);
            voice.volume = startVolume * curveValue;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (voice != null)
        {
            voice.volume = 0f;
            voice.Stop();
            Destroy(voice);
        }
    }

    public void SetOctave(int octave)
    {
        octaveOffset = Mathf.Clamp(octave, -2, 2);
    }

    public int GetOctave()
    {
        return octaveOffset;
    }

    public void StopAllNotes()
    {
        if (activeVoices == null)
            return;

        foreach (var voice in activeVoices.Values)
        {
            if (voice != null)
            {
                voice.Stop();
                Destroy(voice);
            }
        }
        activeVoices.Clear();
    }

    private float MidiToFreq(int midi)
    {
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }

    private void OnDestroy()
    {
        StopAllNotes();
    }

    private void OnValidate()
    {
        UpdateRootNote();
    }
} 