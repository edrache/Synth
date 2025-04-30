using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DrumRackSampler : MonoBehaviour, IDrumRackSampler
{
    [Header("Drum Rack Settings")]
    public bool oneShot = true;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float pan = 0f;

    [Header("Samples")]
    [SerializeField] private AudioClip cSample;
    [SerializeField] private AudioClip cSharpSample;
    [SerializeField] private AudioClip dSample;
    [SerializeField] private AudioClip dSharpSample;
    [SerializeField] private AudioClip eSample;
    [SerializeField] private AudioClip fSample;
    [SerializeField] private AudioClip fSharpSample;
    [SerializeField] private AudioClip gSample;
    [SerializeField] private AudioClip gSharpSample;
    [SerializeField] private AudioClip aSample;
    [SerializeField] private AudioClip aSharpSample;
    [SerializeField] private AudioClip bSample;

    private Dictionary<Note, AudioClip> samples;
    private Dictionary<Note, AudioSource> activeVoices;

    public bool OneShot
    {
        get => oneShot;
        set => oneShot = value;
    }

    private void Awake()
    {
        samples = new Dictionary<Note, AudioClip>();
        activeVoices = new Dictionary<Note, AudioSource>();
        InitializeSamples();
    }

    private void InitializeSamples()
    {
        samples[Note.C] = cSample;
        samples[Note.CSharp] = cSharpSample;
        samples[Note.D] = dSample;
        samples[Note.DSharp] = dSharpSample;
        samples[Note.E] = eSample;
        samples[Note.F] = fSample;
        samples[Note.FSharp] = fSharpSample;
        samples[Note.G] = gSample;
        samples[Note.GSharp] = gSharpSample;
        samples[Note.A] = aSample;
        samples[Note.ASharp] = aSharpSample;
        samples[Note.B] = bSample;
    }

    public void SetSample(Note note, AudioClip clip)
    {
        samples[note] = clip;
        UpdateSampleField(note, clip);
    }

    private void UpdateSampleField(Note note, AudioClip clip)
    {
        switch (note)
        {
            case Note.C: cSample = clip; break;
            case Note.CSharp: cSharpSample = clip; break;
            case Note.D: dSample = clip; break;
            case Note.DSharp: dSharpSample = clip; break;
            case Note.E: eSample = clip; break;
            case Note.F: fSample = clip; break;
            case Note.FSharp: fSharpSample = clip; break;
            case Note.G: gSample = clip; break;
            case Note.GSharp: gSharpSample = clip; break;
            case Note.A: aSample = clip; break;
            case Note.ASharp: aSharpSample = clip; break;
            case Note.B: bSample = clip; break;
        }
    }

    public void PlayNote(int midiNote)
    {
        Note note = (Note)(midiNote % 12);
        PlayNote(note);
    }

    public void PlayNote(Note note)
    {
        if (!samples.TryGetValue(note, out var clip) || clip == null) return;

        // Create new voice
        var voice = gameObject.AddComponent<AudioSource>();
        voice.clip = clip;
        voice.volume = volume;
        voice.panStereo = pan;
        voice.loop = false;
        voice.Play();

        activeVoices[note] = voice;

        // If oneShot is enabled, start coroutine to clean up after sample finishes
        if (oneShot)
        {
            StartCoroutine(CleanupVoiceAfterPlayback(voice, note));
        }
    }

    private System.Collections.IEnumerator CleanupVoiceAfterPlayback(AudioSource voice, Note note)
    {
        yield return new WaitForSeconds(voice.clip.length);
        if (voice != null)
        {
            Destroy(voice);
            activeVoices.Remove(note);
        }
    }

    public void StopNote(int midiNote)
    {
        Note note = (Note)(midiNote % 12);
        StopNote(note);
    }

    public void StopNote(Note note)
    {
        if (!oneShot && activeVoices.TryGetValue(note, out var voice))
        {
            voice.Stop();
            Destroy(voice);
            activeVoices.Remove(note);
        }
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

    private void OnDestroy()
    {
        StopAllNotes();
    }
} 