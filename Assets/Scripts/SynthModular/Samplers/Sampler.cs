using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Sampler : MonoBehaviour, ISampler
{
    [Header("Sample Settings")]
    public AudioClip sample;
    [Range(0, 9)] public int octave = 4;
    public Note rootNote = Note.C;
    [Range(-2, 2)] public int octaveOffset = 0;
    public bool oneShot = false;

    [Header("Playback Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float pan = 0f;

    private AudioSource audioSource;
    private Dictionary<int, AudioSource> activeVoices;
    private float sampleRate;
    private float rootFrequency;
    private int rootMidiNote;

    public bool OneShot
    {
        get => oneShot;
        set => oneShot = value;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        activeVoices = new Dictionary<int, AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;
        UpdateRootNote();
    }

    private void UpdateRootNote()
    {
        rootMidiNote = (int)rootNote + (octave + 1) * 12;
        rootFrequency = MidiToFreq(rootMidiNote);
    }

    public void LoadSample(AudioClip clip, int rootNote)
    {
        sample = clip;
        this.rootNote = (Note)(rootNote % 12);
        this.octave = (rootNote / 12) - 1;
        UpdateRootNote();
    }

    public void PlayNote(int midiNote)
    {
        if (sample == null) return;

        // Calculate pitch based on root note
        float targetFreq = MidiToFreq(midiNote);
        float pitch = targetFreq / rootFrequency;

        // Create new voice
        var voice = gameObject.AddComponent<AudioSource>();
        voice.clip = sample;
        voice.volume = volume;
        voice.panStereo = pan;
        voice.pitch = pitch;
        voice.loop = false;
        voice.Play();

        activeVoices[midiNote] = voice;

        // If oneShot is enabled, start coroutine to clean up after sample finishes
        if (oneShot)
        {
            StartCoroutine(CleanupVoiceAfterPlayback(voice, midiNote));
        }
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
        if (!oneShot && activeVoices.TryGetValue(midiNote, out var voice))
        {
            voice.Stop();
            Destroy(voice);
            activeVoices.Remove(midiNote);
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
        foreach (var voice in activeVoices.Values)
        {
            voice.Stop();
            Destroy(voice);
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