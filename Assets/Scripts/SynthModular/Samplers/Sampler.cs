using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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
    [Tooltip("Minimalny czas trwania nuty w sekundach")]
    public float minNoteDuration = 0.1f;

    [Header("Envelope Settings")]
    [Tooltip("Krzywa zanikania dźwięku po puszczeniu nuty (tylko gdy OneShot = false). Oś X: czas [s], oś Y: głośność (1 = velocity nuty, 0 = cisza)")]
    public AnimationCurve decayCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Tooltip("Jednostka czasu zanikania dźwięku")] public DecayTimeUnit decayTimeUnit = DecayTimeUnit.Seconds;
    [Tooltip("Czas trwania zanikania dźwięku (w sekundach lub beatach)")]
    public float decayTime = 1.0f;

    private AudioSource audioSource;
    private Dictionary<int, AudioSource> activeVoices;
    private List<int> activeNotes = new List<int>();
    private Dictionary<int, Coroutine> activeFadeOuts = new Dictionary<int, Coroutine>();
    private float sampleRate;
    private float rootFrequency;
    private int rootMidiNote;
    private bool isQuitting = false;
    private int currentLoop = 0;
    private float timelineLength = 10f; // Domyślna długość timeline
    private PlayableDirector timelineDirector;
    private Dictionary<int, float> noteEndTimes = new Dictionary<int, float>();
    private Coroutine checkNotesCoroutine;
    private const float MIN_NOTE_DURATION = 0.1f; // Minimum duration in seconds
    private const float DURATION_CHECK_INTERVAL = 0.1f; // How often to check note durations

    public static event Action<Sampler, int> OnAnyNotePlayed;

    public bool OneShot
    {
        get => oneShot;
        set => oneShot = value;
    }

    public void SetTimelineLength(float length)
    {
        timelineLength = length;
        // Debug.Log($"Timeline length set to {length} seconds");
    }

    private void Awake()
    {
        // Find the Timeline director
        timelineDirector = FindObjectOfType<PlayableDirector>();
        if (timelineDirector == null)
        {
            Debug.LogError("No PlayableDirector found in scene!");
        }
        else
        {
            timelineLength = (float)timelineDirector.duration;
            // Debug.Log($"Timeline length set to {timelineLength} seconds from PlayableDirector");
        }

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

        // Prevent destruction when loading new scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateRootNote();
        StartCoroutine(CleanupInactiveAudioSourcesRoutine());
        checkNotesCoroutine = StartCoroutine(CheckNotesDurationRoutine());
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

        // Stop any existing fade out for this note
        if (activeFadeOuts.TryGetValue(midiNote, out var existingFadeOut))
        {
            if (existingFadeOut != null)
            {
                StopCoroutine(existingFadeOut);
            }
            activeFadeOuts.Remove(midiNote);
        }

        // If there's an existing voice, stop it with fade out
        if (activeVoices.TryGetValue(midiNote, out var existingVoice))
        {
            if (existingVoice != null)
            {
                StopNote(midiNote);
            }
        }

        float targetFreq = MidiToFreq(midiNote);
        float pitch = targetFreq / rootFrequency;

        // Get adjusted time and loop number from Timeline
        var (adjustedTime, loopNumber) = GetAdjustedTimeAndLoop();

        // Calculate note end time
        float noteDuration = GetNoteDuration(midiNote);
        float currentTime = (float)timelineDirector.time;
        float endTime = currentTime + noteDuration;
        noteEndTimes[midiNote] = endTime;

        // Create a new child GameObject for this voice
        GameObject voiceObj = new GameObject($"Voice_{midiNote}_Loop{loopNumber}_{adjustedTime:F2}s_Dur{noteDuration:F2}s");
        voiceObj.transform.SetParent(transform);
        voiceObj.transform.localPosition = Vector3.zero;

        var voice = voiceObj.AddComponent<AudioSource>();
        voice.clip = sample;
        voice.volume = volume * velocity;
        voice.panStereo = pan;
        voice.pitch = pitch;
        voice.loop = false;
        voice.outputAudioMixerGroup = outputMixerGroup;
        
        // Debug.Log($"Playing note {midiNote} with velocity {velocity}, oneShot: {oneShot}, Loop: {loopNumber}, Time: {adjustedTime:F2}s, Duration: {noteDuration:F2}s, End time: {endTime:F2}s");
        voice.Play();

        activeVoices[midiNote] = voice;
        if (!activeNotes.Contains(midiNote))
        {
            activeNotes.Add(midiNote);
        }

        if (oneShot)
        {
            StartCoroutine(CleanupVoiceAfterPlayback(voice, midiNote));
        }

        OnAnyNotePlayed?.Invoke(this, midiNote);
    }

    private System.Collections.IEnumerator CleanupVoiceAfterPlayback(AudioSource voice, int midiNote)
    {
        if (voice == null || voice.clip == null)
        {
            CleanupVoice(midiNote);
            yield break;
        }

        float startTime = Time.time;
        float maxDuration = voice.clip.length * 1.1f; // 10% margin for safety

        while (voice != null && (voice.isPlaying || voice.time < voice.clip.length))
        {
            // Safety check - if voice is null or not playing, clean up
            if (voice == null || !voice.isPlaying)
            {
                CleanupVoice(midiNote);
                yield break;
            }

            // Timeout check - if playing longer than sample length, force stop
            if (Time.time - startTime > maxDuration)
            {
                Debug.LogWarning($"Voice timeout for note {midiNote}. Forcing cleanup.");
                CleanupVoice(midiNote);
                yield break;
            }

            yield return null;
        }

        CleanupVoice(midiNote);
    }

    public void StopNote(int midiNote)
    {
        if (activeVoices == null)
            return;

        // Debug.Log($"StopNote called for note {midiNote}, oneShot: {oneShot}");
        
        // Stop any existing fade out for this note
        if (activeFadeOuts.TryGetValue(midiNote, out var existingFadeOut))
        {
            if (existingFadeOut != null)
            {
                StopCoroutine(existingFadeOut);
                // Debug.Log($"Stopped existing fade out for note {midiNote}");
            }
            activeFadeOuts.Remove(midiNote);
        }
        
        if (!oneShot && activeVoices.TryGetValue(midiNote, out var voice))
        {
            if (voice == null)
            {
                Debug.LogWarning($"Voice is null for note {midiNote}");
                return;
            }
            var fadeOutCoroutine = StartCoroutine(FadeOutAndStop(voice, midiNote));
            activeFadeOuts[midiNote] = fadeOutCoroutine;
        }
        else
        {
            // If it's oneShot or voice not found, clean up immediately
            CleanupVoice(midiNote);
        }
    }

    private System.Collections.IEnumerator FadeOutAndStop(AudioSource voice, int midiNote)
    {
        if (voice == null)
        {
            Debug.LogWarning($"FadeOutAndStop: Voice is null for note {midiNote}");
            yield break;
        }

        // Debug.Log($"Starting fade out for note {midiNote}. Current volume: {voice.volume}, Decay curve: {decayCurve.length} points");
        
        float startVolume = voice.volume;
        float elapsed = 0f;
        float decayDuration = GetDecayTimeSeconds();
        
        // Debug.Log($"Fade out parameters - Start volume: {startVolume}, Decay duration: {decayDuration}s, Time unit: {decayTimeUnit}");

        // Safety check - if decay duration is too long, cap it
        if (decayDuration > 10f)
        {
            Debug.LogWarning($"Decay duration too long ({decayDuration}s), capping to 10s");
            decayDuration = 10f;
        }

        while (elapsed < decayDuration && voice != null)
        {
            // Check if the voice is still valid
            if (voice == null || !voice.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"Voice became invalid during fade out for note {midiNote}");
                yield break;
            }

            float t = decayDuration > 0f ? (elapsed / decayDuration) : 1f;
            float curveValue = decayCurve.Evaluate(t);
            float newVolume = startVolume * curveValue;
            voice.volume = newVolume;
            
            if (elapsed % 0.5f < Time.deltaTime) // Log every 0.5 seconds
            {
                // Debug.Log($"Fade out progress for note {midiNote}: {t:P0}, Volume: {newVolume:F2}, Curve value: {curveValue:F2}");
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Debug.Log($"Fade out completed for note {midiNote}. Final volume: {voice?.volume ?? 0f}");
        CleanupVoice(midiNote);
    }

    private void CleanupVoice(int midiNote)
    {
        // Debug.Log($"Cleaning up voice for note {midiNote}");
        
        // Remove any existing fade out
        if (activeFadeOuts.TryGetValue(midiNote, out var fadeOut))
        {
            if (fadeOut != null)
            {
                StopCoroutine(fadeOut);
            }
            activeFadeOuts.Remove(midiNote);
        }
        
        if (activeVoices.TryGetValue(midiNote, out var voice))
        {
            if (voice != null)
            {
                // Debug.Log($"Destroying voice GameObject for note {midiNote}");
                Destroy(voice.gameObject);
            }
            else
            {
                Debug.LogWarning($"Voice is null for note {midiNote} during cleanup");
            }
            activeVoices.Remove(midiNote);
        }
        else
        {
            Debug.LogWarning($"No voice found for note {midiNote} during cleanup");
        }
        
        if (activeNotes.Contains(midiNote))
        {
            activeNotes.Remove(midiNote);
            // Debug.Log($"Removed note {midiNote} from active notes list");
        }

        // Remove from note end times
        noteEndTimes.Remove(midiNote);
    }

    public void StopAllNotes()
    {
        // Debug.Log("StopAllNotes called");
        if (activeVoices == null)
        {
            Debug.LogWarning("activeVoices is null in StopAllNotes");
            return;
        }

        // Stop all fade out coroutines
        foreach (var fadeOut in activeFadeOuts.Values)
        {
            if (fadeOut != null)
            {
                StopCoroutine(fadeOut);
            }
        }
        activeFadeOuts.Clear();

        foreach (var midiNote in new List<int>(activeNotes))
        {
            // Debug.Log($"Stopping note {midiNote} in StopAllNotes");
            CleanupVoice(midiNote);
        }
        activeNotes.Clear();
        // Debug.Log("All notes stopped and cleared");
    }

    private float GetDecayTimeSeconds()
    {
        float bpm = 120f;
        var bpmController = FindObjectOfType<TimelineBPMController>();
        if (bpmController != null)
        {
            bpm = bpmController.BPM;
            // Debug.Log($"Using BPM from controller: {bpm}");
        }
        else
        {
            // Debug.Log("No BPM controller found, using default 120 BPM");
        }

        float decayTimeInSeconds;
        if (decayTimeUnit == DecayTimeUnit.Seconds)
        {
            decayTimeInSeconds = decayTime;
        }
        else // Beats
        {
            decayTimeInSeconds = 60f * decayTime / Mathf.Max(1f, bpm);
            // Debug.Log($"Converting {decayTime} beats to {decayTimeInSeconds:F2} seconds at {bpm} BPM");
        }

        // Add safety margin for BPM changes
        decayTimeInSeconds *= 1.1f;

        return decayTimeInSeconds;
    }

    public void SetOctave(int octave)
    {
        octaveOffset = Mathf.Clamp(octave, -2, 2);
    }

    public int GetOctave()
    {
        return octaveOffset;
    }

    private float MidiToFreq(int midi)
    {
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }

    private void OnDestroy()
    {
        if (checkNotesCoroutine != null)
        {
            StopCoroutine(checkNotesCoroutine);
        }
        // Only stop notes if the application is quitting
        if (!isQuitting)
        {
            // Debug.Log("Sampler being destroyed but application is not quitting. Preventing note cleanup.");
            return;
        }
        
        StopAllNotes();
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
        StopAllNotes();
    }

    private void OnValidate()
    {
        UpdateRootNote();
    }

    private System.Collections.IEnumerator CleanupInactiveAudioSourcesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            var sources = GetComponents<AudioSource>();
            foreach (var src in sources)
            {
                if (src != null && !src.isPlaying && src != audioSource)
                {
                    Destroy(src);
                }
            }
        }
    }

    public void SetLoopNumber(int loopNumber)
    {
        currentLoop = loopNumber;
    }

    private float GetNoteDuration(int midiNote)
    {
        if (timelineDirector == null || timelineDirector.playableAsset == null)
            return minNoteDuration;

        var timelineAsset = timelineDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null)
            return minNoteDuration;

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track is SamplerPianoRollTrack)
            {
                foreach (var clip in track.GetClips())
                {
                    var samplerClip = clip.asset as SamplerPianoRollClip;
                    if (samplerClip != null && samplerClip.midiNote == midiNote)
                    {
                        float duration = samplerClip.duration;
                        return Mathf.Max(duration, minNoteDuration);
                    }
                }
            }
        }
        return minNoteDuration;
    }

    private System.Collections.IEnumerator CheckNotesDurationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(DURATION_CHECK_INTERVAL);
            
            if (timelineDirector == null || timelineDirector.state != PlayState.Playing)
                continue;

            float currentTime = (float)timelineDirector.time;
            var notesToStop = new List<int>();

            foreach (var kvp in noteEndTimes)
            {
                if (currentTime >= kvp.Value)
                {
                    notesToStop.Add(kvp.Key);
                }
            }

            foreach (var midiNote in notesToStop)
            {
                if (activeVoices.ContainsKey(midiNote))
                {
                    // Debug.Log($"Note {midiNote} exceeded its duration at time {currentTime:F2}s (end time was {noteEndTimes[midiNote]:F2}s)");
                    StopNote(midiNote);
                    noteEndTimes.Remove(midiNote);
                }
            }
        }
    }

    private void CleanupNotesFromPreviousLoops(int currentLoop)
    {
        var objectsToRemove = new List<GameObject>();
        var midiNotesToRemove = new List<int>();

        foreach (Transform child in transform)
        {
            string name = child.gameObject.name;
            if (name.Contains("_Loop"))
            {
                int loopNumber = ExtractLoopNumber(name);
                if (loopNumber < currentLoop - 1)
                {
                    objectsToRemove.Add(child.gameObject);
                    // Debug.Log($"Destroying leftover voice object from old loop: {name}");
                }
            }
        }

        foreach (var obj in objectsToRemove)
        {
            // Remove from dictionaries/lists if possible
            int midiNote = ExtractMidiNote(obj.name);
            if (activeVoices.ContainsKey(midiNote) && activeVoices[midiNote] != null && activeVoices[midiNote].gameObject == obj)
            {
                activeVoices.Remove(midiNote);
            }
            if (activeNotes.Contains(midiNote))
            {
                activeNotes.Remove(midiNote);
            }
            if (noteEndTimes.ContainsKey(midiNote))
            {
                noteEndTimes.Remove(midiNote);
            }
            if (activeFadeOuts.ContainsKey(midiNote))
            {
                activeFadeOuts.Remove(midiNote);
            }
            Destroy(obj);
        }
    }

    private int ExtractLoopNumber(string voiceName)
    {
        try
        {
            int loopIndex = voiceName.IndexOf("_Loop");
            if (loopIndex != -1)
            {
                int startIndex = loopIndex + 5; // "_Loop".Length
                int endIndex = voiceName.IndexOf('_', startIndex);
                if (endIndex == -1) endIndex = voiceName.Length;
                
                string loopStr = voiceName.Substring(startIndex, endIndex - startIndex);
                return int.Parse(loopStr);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to extract loop number from voice name: {voiceName}. Error: {e.Message}");
        }
        return 0;
    }

    private int ExtractMidiNote(string voiceName)
    {
        try
        {
            int voiceIndex = voiceName.IndexOf("Voice_");
            if (voiceIndex != -1)
            {
                int startIndex = voiceIndex + 6; // "Voice_".Length
                int endIndex = voiceName.IndexOf('_', startIndex);
                if (endIndex == -1) endIndex = voiceName.Length;
                string midiStr = voiceName.Substring(startIndex, endIndex - startIndex);
                return int.Parse(midiStr);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to extract midi note from voice name: {voiceName}. Error: {e.Message}");
        }
        return -1;
    }

    private (float adjustedTime, int loopNumber) GetAdjustedTimeAndLoop()
    {
        if (timelineDirector == null)
        {
            Debug.LogWarning("No PlayableDirector found, using Time.time");
            return (Time.time, 0);
        }

        float currentTime = (float)timelineDirector.time;
        
        // Check if timeline just restarted
        if (currentTime < 0.1f && timelineDirector.state == PlayState.Playing)
        {
            currentLoop++;
            // Debug.Log($"Timeline restarted, incrementing loop number to {currentLoop}");
            CleanupNotesFromPreviousLoops(currentLoop);
        }
        
        int loopCount = Mathf.FloorToInt(currentTime / timelineLength);
        float adjustedTime = currentTime - (loopCount * timelineLength);
        return (adjustedTime, currentLoop);
    }
} 