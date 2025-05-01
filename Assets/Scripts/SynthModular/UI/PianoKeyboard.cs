using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class PianoKeyboard : MonoBehaviour
{
    [Header("Key Settings")]
    public float whiteKeyWidth = 50f;
    public float whiteKeyHeight = 200f;
    public float blackKeyWidth = 30f;
    public float blackKeyHeight = 120f;
    public Color whiteKeyColor = Color.white;
    public Color blackKeyColor = Color.black;
    public Color whiteKeyPressedColor = Color.gray;
    public Color blackKeyPressedColor = Color.gray;
    public Color whiteKeyTextColor = Color.black;
    public Color blackKeyTextColor = Color.white;

    [Header("References")]
    public GameObject keyPrefab;
    public Transform keysContainer;
    public PlayableDirector timeline;
    public Slider timelineSlider;
    public TMP_Dropdown trackDropdown;
    public bool enableTimelineControl = true;

    [Header("Note Settings")]
    public float noteDuration = 1f;
    public bool addNotesToTimeline = false;
    [Tooltip("Jeśli włączone, długość nuty będzie zależna od czasu przytrzymania klawisza")]
    public bool dynamicNoteDuration = false;
    [Tooltip("Jeśli włączone, nakładające się klipy zostaną automatycznie usunięte")]
    public bool removeOverlappingClips = false;
    [Range(0f, 1f)]
    [Tooltip("Głośność dodawanych nut (0 = cisza, 1 = maksymalna głośność)")]
    public float noteVelocity = 0.8f;

    [Header("Synth Settings")]
    [Tooltip("Przypisz jeden z syntezatorów")]
    public ModularSynth modularSynth;
    public Sampler sampler;
    public DrumRackSampler drumRackSampler;

    [Header("Sequence Settings")]
    public Button sequenceButton;
    public Button restButton;  // Przycisk do wstawiania przerw
    public Color sequenceActiveColor = Color.red;
    public Color sequenceInactiveColor = Color.white;
    [Tooltip("Jeśli włączone, wszystkie nuty w sekwencji będą miały taką samą długość")]
    public bool equalNoteDuration = false;

    private List<Button> whiteKeys = new List<Button>();
    private List<Button> blackKeys = new List<Button>();
    private Dictionary<Button, int> keyToMidiNote = new Dictionary<Button, int>();
    private Dictionary<int, Button> midiNoteToKey = new Dictionary<int, Button>();
    private HashSet<int> activeNotes = new HashSet<int>();
    private List<TrackAsset> pianoRollTracks = new List<TrackAsset>();
    private int selectedTrackIndex = 0;
    private Dictionary<int, float> noteStartTimes = new Dictionary<int, float>();
    private Dictionary<int, bool> noteAddedToTimeline = new Dictionary<int, bool>();
    private bool isRecordingSequence = false;
    private List<SequenceNote> recordedNotes = new List<SequenceNote>();
    private float sequenceStartTime;
    private bool sequencePendingAdd = false;
    private float lastTimelineTime = 0f;

    private struct SequenceNote
    {
        public int midiNote;
        public float timeOffset;
        public float duration;
        public bool isRest;  // Czy to jest przerwa

        public SequenceNote(int note, float offset, float dur, bool rest = false)
        {
            midiNote = note;
            timeOffset = offset;
            duration = dur;
            isRest = rest;
        }
    }

    private void Start()
    {
        FindReferences();
        InitializeUI();
        GenerateKeyboard();
        InitializeSequencer();
    }

    private void FindReferences()
    {
        // Try to find a synth if none assigned
        if (modularSynth == null && sampler == null && drumRackSampler == null)
        {
            modularSynth = FindObjectOfType<ModularSynth>();
            if (modularSynth == null)
            {
                sampler = FindObjectOfType<Sampler>();
                if (sampler == null)
                {
                    drumRackSampler = FindObjectOfType<DrumRackSampler>();
                }
            }
        }

        if (modularSynth == null && sampler == null && drumRackSampler == null)
        {
            Debug.LogError("Nie znaleziono żadnego syntezatora! Przypisz jeden w inspektorze.");
            return;
        }

        if (timeline == null)
        {
            timeline = FindObjectOfType<PlayableDirector>();
            if (timeline == null)
            {
                Debug.LogWarning("Nie znaleziono Timeline! Funkcje timeline będą nieaktywne.");
                if (timelineSlider != null) timelineSlider.gameObject.SetActive(false);
                if (trackDropdown != null) trackDropdown.gameObject.SetActive(false);
                enableTimelineControl = false;
                addNotesToTimeline = false;
            }
        }
    }

    private void InitializeUI()
    {
        if (timelineSlider != null && enableTimelineControl)
        {
            timelineSlider.minValue = 0f;
            timelineSlider.maxValue = 1f;
            timelineSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        if (trackDropdown != null && timeline != null)
        {
            PopulateTrackDropdown();
            trackDropdown.onValueChanged.AddListener(OnTrackSelected);
        }
    }

    private void PopulateTrackDropdown()
    {
        if (trackDropdown == null || timeline == null) return;

        trackDropdown.ClearOptions();
        pianoRollTracks.Clear();
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("Timeline asset is not a TimelineAsset!");
            return;
        }

        Debug.Log("Dostępne ścieżki w timeline:");
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            Debug.Log($"- Ścieżka: {track.name}, Typ: {track.GetType().Name}, Jest IPianoRollTrack: {track is IPianoRollTrack}");
        }

        int trackNumber = 1;
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track is IPianoRollTrack)
            {
                pianoRollTracks.Add(track);
                trackDropdown.options.Add(new TMP_Dropdown.OptionData($"Ścieżka {trackNumber}"));
                Debug.Log($"Dodano ścieżkę PianoRoll: {track.name} jako Ścieżka {trackNumber}");
                trackNumber++;
            }
        }

        if (trackDropdown.options.Count > 0)
        {
            trackDropdown.value = 0;
            selectedTrackIndex = 0;
            Debug.Log($"Znaleziono ścieżek PianoRoll: {pianoRollTracks.Count}");
        }
        else
        {
            Debug.LogWarning("Nie znaleziono ścieżek PianoRoll w timeline!");
            addNotesToTimeline = false;
        }
    }

    private void OnTrackSelected(int index)
    {
        if (index >= 0 && index < pianoRollTracks.Count)
        {
            selectedTrackIndex = index;
            Debug.Log($"Wybrano ścieżkę: {pianoRollTracks[selectedTrackIndex].name}");
        }
    }

    private void InitializeSequencer()
    {
        if (sequenceButton != null)
        {
            sequenceButton.onClick.AddListener(ToggleSequenceRecording);
            var buttonImage = sequenceButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = sequenceInactiveColor;
            }
        }

        if (restButton != null)
        {
            restButton.onClick.AddListener(AddRestToSequence);
            restButton.interactable = false; // Domyślnie nieaktywny
        }
    }

    private void ToggleSequenceRecording()
    {
        isRecordingSequence = !isRecordingSequence;

        if (isRecordingSequence)
        {
            // Wyczyść poprzednią sekwencję
            recordedNotes.Clear();
            
            // Jeśli poprzednia sekwencja czeka na dodanie, dodaj ją teraz
            if (sequencePendingAdd)
            {
                AddSequenceToTimeline();
            }

            // Wyczyść ścieżkę przed rozpoczęciem nowego nagrywania
            ClearTrackClips();

            sequenceStartTime = Time.time;
            Debug.Log("Rozpoczęto nagrywanie nowej sekwencji");

            // Aktywuj przycisk przerwy podczas nagrywania
            if (restButton != null)
            {
                restButton.interactable = true;
            }

            if (sequenceButton != null)
            {
                var buttonImage = sequenceButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = sequenceActiveColor;
                }
            }
        }
        else
        {
            if (recordedNotes.Count > 0)
            {
                PrepareSequenceForTimeline();
            }

            // Dezaktywuj przycisk przerwy po zakończeniu nagrywania
            if (restButton != null)
            {
                restButton.interactable = false;
            }

            if (sequenceButton != null)
            {
                var buttonImage = sequenceButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = sequenceInactiveColor;
                }
            }
        }
    }

    private void ClearTrackClips()
    {
        if (timeline == null || selectedTrackIndex >= pianoRollTracks.Count) return;

        var track = pianoRollTracks[selectedTrackIndex] as IPianoRollTrack;
        if (track == null) return;

        // Zapamiętaj stan Timeline
        bool wasPlaying = timeline.state == PlayState.Playing;
        float currentTime = (float)timeline.time;
        double playbackSpeed = timeline.playableGraph.GetRootPlayable(0).GetSpeed();

        // Zatrzymaj Timeline na czas czyszczenia
        if (wasPlaying)
        {
            timeline.Pause();
        }

        // Usuń wszystkie klipy
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset != null)
        {
            var clips = track.GetClips().ToList();
            foreach (var clip in clips)
            {
                track.DeleteClip(clip);
            }
        }

        // Przywróć stan Timeline
        timeline.time = currentTime;
        timeline.playableGraph.GetRootPlayable(0).SetSpeed(playbackSpeed);
        
        if (wasPlaying)
        {
            timeline.Play();
        }
        else
        {
            timeline.Evaluate();
        }

        Debug.Log($"Wyczyszczono ścieżkę {track.name} przed rozpoczęciem nowego nagrywania");
    }

    private void AddRestToSequence()
    {
        if (!isRecordingSequence) return;

        float currentTime = Time.time;
        float timeOffset = currentTime - sequenceStartTime;
        
        // Dodaj przerwę z domyślną długością
        recordedNotes.Add(new SequenceNote(0, timeOffset, 0.1f, true));
        Debug.Log("Dodano przerwę do sekwencji");
    }

    private void PrepareSequenceForTimeline()
    {
        if (timeline == null || recordedNotes.Count == 0) return;

        // Oznacz sekwencję jako gotową do dodania
        sequencePendingAdd = true;
        Debug.Log($"Sekwencja gotowa do dodania: {recordedNotes.Count} nut");
    }

    private void Update()
    {
        // Aktualizacja slidera bez wpływania na Timeline
        if (timeline != null && timelineSlider != null && !timelineSlider.gameObject.GetComponent<EventSystem>()?.currentSelectedGameObject == timelineSlider.gameObject)
        {
            float normalizedTime = (float)(timeline.time / timeline.duration);
            timelineSlider.SetValueWithoutNotify(normalizedTime);

            // Sprawdź czy Timeline zakończył odtwarzanie
            if (timeline.time < lastTimelineTime && sequencePendingAdd)
            {
                // Zapamiętaj aktualną prędkość przed dodaniem sekwencji
                double currentSpeed = timeline.playableGraph.GetRootPlayable(0).GetSpeed();
                AddSequenceToTimeline();
                // Przywróć prędkość po dodaniu sekwencji
                timeline.playableGraph.GetRootPlayable(0).SetSpeed(currentSpeed);
            }
            lastTimelineTime = (float)timeline.time;
        }
    }

    private void OnSliderValueChanged(float value)
    {
        // Usuwamy kontrolę nad Timeline z PianoKeyboard
        Debug.Log($"Slider zmieniony na wartość: {value}");
    }

    private void AddNoteToTimeline(int midiNote, float startTime, float duration)
    {
        if (timeline == null || selectedTrackIndex >= pianoRollTracks.Count) return;

        var track = pianoRollTracks[selectedTrackIndex] as IPianoRollTrack;
        if (track == null) return;

        // Clear any existing clips at this time if removeOverlappingClips is enabled
        if (removeOverlappingClips)
        {
            var timelineAsset = timeline.playableAsset as TimelineAsset;
            if (timelineAsset != null)
            {
                var overlappingClips = timelineAsset.GetOutputTracks()
                    .Where(t => t == pianoRollTracks[selectedTrackIndex])
                    .SelectMany(t => t.GetClips())
                    .Where(existingClip => 
                        (existingClip.start <= startTime && existingClip.end > startTime) || 
                        (existingClip.start < startTime + duration && existingClip.end >= startTime + duration) ||
                        (existingClip.start >= startTime && existingClip.end <= startTime + duration))
                    .ToList();

                foreach (var overlappingClip in overlappingClips)
                {
                    track.DeleteClip(overlappingClip);
                }
            }
        }

        var clip = track.CreateClip();
        if (clip != null)
        {
            clip.start = startTime;
            clip.duration = duration;

            var samplerClip = clip.asset as SamplerPianoRollClip;
            if (samplerClip != null)
            {
                samplerClip.midiNote = midiNote;
                samplerClip.duration = duration;
                samplerClip.startTime = startTime;
                samplerClip.velocity = noteVelocity;
                clip.displayName = samplerClip.GetDisplayName();
                Debug.Log($"Dodano nutę MIDI {midiNote} do SamplerPianoRollClip");
            }
            else
            {
                Debug.LogError("Nie można przekonwertować klipu na SamplerPianoRollClip");
            }

            timeline.RebuildGraph();
            Debug.Log($"Added note {midiNote} at time {startTime} with duration {duration}");
        }
    }

    private void AddSequenceToTimeline()
    {
        if (!sequencePendingAdd || recordedNotes.Count == 0 || timeline == null || 
            selectedTrackIndex >= pianoRollTracks.Count) return;

        var track = pianoRollTracks[selectedTrackIndex] as IPianoRollTrack;
        if (track == null) return;

        // Zapamiętaj stan Timeline przed modyfikacjami
        bool wasPlaying = timeline.state == PlayState.Playing;
        float currentTime = (float)timeline.time;
        double playbackSpeed = timeline.playableGraph.GetRootPlayable(0).GetSpeed();

        // Zatrzymaj Timeline na czas modyfikacji
        if (wasPlaying)
        {
            timeline.Pause();
        }

        // Usuń wszystkie istniejące klipy na ścieżce
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset != null)
        {
            var clips = track.GetClips().ToList();
            foreach (var clip in clips)
            {
                track.DeleteClip(clip);
            }
        }

        float timelineDuration = (float)timeline.duration;
        float firstNoteTime = recordedNotes.Min(n => n.timeOffset);
        float lastNoteEnd = recordedNotes.Max(n => n.timeOffset + n.duration);
        float sequenceDuration = lastNoteEnd - firstNoteTime;
        float timeScale = timelineDuration / sequenceDuration;

        if (equalNoteDuration)
        {
            float equalDuration = timelineDuration / recordedNotes.Count;
            var sortedNotes = recordedNotes.OrderBy(n => n.timeOffset).ToList();
            
            for (int i = 0; i < sortedNotes.Count; i++)
            {
                float scaledTime = i * equalDuration;
                if (!sortedNotes[i].isRest)  // Dodaj nutę tylko jeśli to nie jest przerwa
                {
                    AddNoteToTimeline(sortedNotes[i].midiNote, scaledTime, equalDuration);
                }
            }
        }
        else
        {
            foreach (var note in recordedNotes)
            {
                if (!note.isRest)  // Dodaj nutę tylko jeśli to nie jest przerwa
                {
                    float normalizedStartTime = note.timeOffset - firstNoteTime;
                    float scaledTime = normalizedStartTime * timeScale;
                    float scaledDuration = note.duration * timeScale;
                    AddNoteToTimeline(note.midiNote, scaledTime, scaledDuration);
                }
            }
        }

        // Przywróć stan Timeline
        timeline.time = currentTime;
        timeline.playableGraph.GetRootPlayable(0).SetSpeed(playbackSpeed);
        
        if (wasPlaying)
        {
            timeline.Play();
        }
        else
        {
            timeline.Evaluate();
        }

        Debug.Log($"Dodano nową sekwencję: {recordedNotes.Count(n => !n.isRest)} nut i {recordedNotes.Count(n => n.isRest)} przerw do timeline");
        sequencePendingAdd = false;
        recordedNotes.Clear();
    }

    private void OnKeyDown(int midiNote)
    {
        if (!activeNotes.Contains(midiNote))
        {
            activeNotes.Add(midiNote);
            
            // Zapisz czas rozpoczęcia nuty dla trybu sekwencji
            if (isRecordingSequence)
            {
                noteStartTimes[midiNote] = Time.time;
            }
            else if (dynamicNoteDuration)
            {
                noteStartTimes[midiNote] = (float)timeline.time;
                noteAddedToTimeline[midiNote] = false;
            }
            
            // Odtwórz dźwięk na syntezatorze
            if (modularSynth != null)
            {
                modularSynth.PlayNote(midiNote);
            }
            else if (sampler != null)
            {
                sampler.PlayNote(midiNote);
            }
            else if (drumRackSampler != null)
            {
                drumRackSampler.PlayNote((Note)(midiNote % 12));
            }

            // Dodaj nutę do timeline tylko jeśli nie nagrywamy sekwencji i nie używamy dynamicznej długości
            if (!isRecordingSequence && !dynamicNoteDuration)
            {
                AddNoteToTimeline(midiNote, (float)timeline.time, noteDuration);
            }
        }
    }

    private void OnKeyUp(int midiNote)
    {
        if (activeNotes.Contains(midiNote))
        {
            float noteStartTime = noteStartTimes[midiNote];
            float currentTime = isRecordingSequence ? Time.time : (float)timeline.time;
            float duration = currentTime - noteStartTime;
            duration = Mathf.Max(duration, 0.1f);

            // Dodaj nutę do sekwencji jeśli nagrywamy
            if (isRecordingSequence)
            {
                float timeOffset = noteStartTime - sequenceStartTime;
                recordedNotes.Add(new SequenceNote(midiNote, timeOffset, duration));
            }
            // W przeciwnym razie dodaj do timeline jeśli używamy dynamicznej długości
            else if (dynamicNoteDuration && !noteAddedToTimeline[midiNote])
            {
                AddNoteToTimeline(midiNote, noteStartTime, duration);
                noteAddedToTimeline[midiNote] = true;
            }

            activeNotes.Remove(midiNote);
            if (modularSynth != null)
            {
                modularSynth.StopNote(midiNote);
            }
            else if (sampler != null)
            {
                sampler.StopNote(midiNote);
            }
            else if (drumRackSampler != null)
            {
                drumRackSampler.StopNote((Note)(midiNote % 12));
            }

            // Wyczyść dane o czasie nuty
            if (dynamicNoteDuration)
            {
                noteStartTimes.Remove(midiNote);
                noteAddedToTimeline.Remove(midiNote);
            }
        }
    }

    private void GenerateKeyboard()
    {
        // Clear existing keys
        foreach (Transform child in keysContainer)
            Destroy(child.gameObject);

        whiteKeys.Clear();
        blackKeys.Clear();
        keyToMidiNote.Clear();
        midiNoteToKey.Clear();
        activeNotes.Clear();
        noteStartTimes.Clear();
        noteAddedToTimeline.Clear();

        // Generate 3 octaves starting from C3 (MIDI note 48)
        int startNote = 48;
        int endNote = 72;

        // Create keys in the correct order
        float currentX = 0f;
        for (int midiNote = startNote; midiNote <= endNote; midiNote++)
        {
            string noteName = GetNoteName(midiNote);
            bool isBlackKey = IsBlackKey(midiNote);

            // Create key
            GameObject keyObj = Instantiate(keyPrefab, keysContainer);
            Button keyButton = keyObj.GetComponent<Button>();
            TMP_Text keyText = keyObj.GetComponentInChildren<TMP_Text>();

            // Add event trigger component for handling pointer events
            EventTrigger trigger = keyObj.AddComponent<EventTrigger>();

            // Configure key appearance
            RectTransform rectTransform = keyObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(
                isBlackKey ? blackKeyWidth : whiteKeyWidth,
                isBlackKey ? blackKeyHeight : whiteKeyHeight
            );

            // Set key colors
            ColorBlock colors = keyButton.colors;
            colors.normalColor = isBlackKey ? blackKeyColor : whiteKeyColor;
            colors.pressedColor = isBlackKey ? blackKeyPressedColor : whiteKeyPressedColor;
            keyButton.colors = colors;

            // Set key text
            keyText.text = noteName;
            keyText.color = isBlackKey ? blackKeyTextColor : whiteKeyTextColor;

            // Position the key
            if (isBlackKey)
            {
                // Position black key relative to the last white key
                float blackKeyX = currentX - whiteKeyWidth + (whiteKeyWidth / 2) - (blackKeyWidth / 2);
                rectTransform.anchoredPosition = new Vector2(blackKeyX, 0);
                blackKeys.Add(keyButton);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(currentX, 0);
                currentX += whiteKeyWidth;
                whiteKeys.Add(keyButton);
            }

            keyToMidiNote[keyButton] = midiNote;
            midiNoteToKey[midiNote] = keyButton;

            // Add pointer down event
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            int note = midiNote; // Capture in closure
            pointerDown.callback.AddListener((data) => OnKeyDown(note));
            trigger.triggers.Add(pointerDown);

            // Add pointer up event
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => OnKeyUp(note));
            trigger.triggers.Add(pointerUp);

            // Add pointer exit event (in case pointer leaves while holding)
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => OnKeyUp(note));
            trigger.triggers.Add(pointerExit);
        }
    }

    private string GetNoteName(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{noteNames[noteIndex]}{octave}";
    }

    private bool IsBlackKey(int midiNote)
    {
        int noteIndex = midiNote % 12;
        return noteIndex == 1 || noteIndex == 3 || noteIndex == 6 || noteIndex == 8 || noteIndex == 10;
    }

    private void OnDestroy()
    {
        if (timelineSlider != null)
        {
            timelineSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        if (sequenceButton != null)
        {
            sequenceButton.onClick.RemoveListener(ToggleSequenceRecording);
        }

        if (restButton != null)
        {
            restButton.onClick.RemoveListener(AddRestToSequence);
        }

        // Stop all active notes
        foreach (int midiNote in activeNotes)
        {
            if (modularSynth != null)
                modularSynth.StopNote(midiNote);
            else if (sampler != null)
                sampler.StopNote(midiNote);
            else if (drumRackSampler != null)
                drumRackSampler.StopNote((Note)(midiNote % 12));
        }
        activeNotes.Clear();
        noteStartTimes.Clear();
        noteAddedToTimeline.Clear();
        recordedNotes.Clear();
    }
} 