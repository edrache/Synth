using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using TMPro;
using System.Collections.Generic;

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
    public bool enableTimelineControl = true;

    [Header("Synth Settings")]
    [Tooltip("Przypisz jeden z syntezatorów")]
    public ModularSynth modularSynth;
    public Sampler sampler;
    public DrumRackSampler drumRackSampler;

    private List<Button> whiteKeys = new List<Button>();
    private List<Button> blackKeys = new List<Button>();
    private Dictionary<Button, int> keyToMidiNote = new Dictionary<Button, int>();
    private Dictionary<int, Button> midiNoteToKey = new Dictionary<int, Button>();
    private HashSet<int> activeNotes = new HashSet<int>();

    private void Start()
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
                Debug.LogWarning("Nie znaleziono Timeline! Slider czasu będzie nieaktywny.");
                if (timelineSlider != null)
                {
                    timelineSlider.gameObject.SetActive(false);
                }
                enableTimelineControl = false;
            }
        }

        if (timelineSlider != null && enableTimelineControl)
        {
            timelineSlider.minValue = 0f;
            timelineSlider.maxValue = 1f;
            timelineSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        GenerateKeyboard();
    }

    private void Update()
    {
        if (timeline != null && timelineSlider != null && enableTimelineControl)
        {
            // Aktualizuj wartość slidera tylko jeśli nie jest aktualnie przeciągany
            if (!timelineSlider.gameObject.GetComponent<EventSystem>()?.currentSelectedGameObject == timelineSlider.gameObject)
            {
                float normalizedTime = (float)(timeline.time / timeline.duration);
                timelineSlider.value = normalizedTime;
            }
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (timeline != null && enableTimelineControl)
        {
            // Ustaw czas w timeline
            timeline.time = value * timeline.duration;
            
            // Jeśli timeline nie jest w trybie odtwarzania, zatrzymaj na wybranej pozycji
            if (timeline.state != PlayState.Playing)
            {
                timeline.Evaluate();
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

    private void OnKeyDown(int midiNote)
    {
        if (!activeNotes.Contains(midiNote))
        {
            activeNotes.Add(midiNote);
            if (modularSynth != null)
            {
                modularSynth.PlayNote(midiNote);
                Debug.Log($"ModularSynth gra nutę: {GetNoteName(midiNote)} (MIDI: {midiNote})");
            }
            else if (sampler != null)
            {
                sampler.PlayNote(midiNote);
                Debug.Log($"Sampler gra nutę: {GetNoteName(midiNote)} (MIDI: {midiNote})");
            }
            else if (drumRackSampler != null)
            {
                drumRackSampler.PlayNote((Note)(midiNote % 12));
                Debug.Log($"DrumRackSampler gra nutę: {GetNoteName(midiNote)} (MIDI: {midiNote})");
            }
        }
    }

    private void OnKeyUp(int midiNote)
    {
        if (activeNotes.Contains(midiNote))
        {
            activeNotes.Remove(midiNote);
            if (modularSynth != null)
            {
                modularSynth.StopNote(midiNote);
                Debug.Log($"ModularSynth zatrzymał nutę: {GetNoteName(midiNote)} (MIDI: {midiNote})");
            }
            else if (sampler != null)
            {
                sampler.StopNote(midiNote);
                Debug.Log($"Sampler zatrzymał nutę: {GetNoteName(midiNote)} (MIDI: {midiNote})");
            }
            else if (drumRackSampler != null)
            {
                drumRackSampler.StopNote((Note)(midiNote % 12));
                Debug.Log($"DrumRackSampler zatrzymał nutę: {GetNoteName(midiNote)} (MIDI: {midiNote})");
            }
        }
    }

    private void OnDestroy()
    {
        if (timelineSlider != null)
        {
            timelineSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
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
    }
} 