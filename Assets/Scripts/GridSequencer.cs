using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;

public class GridSequencer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private Sampler sampler;
    [SerializeField] private TimelineBPMController bpmController;
    [SerializeField] private GameObject dropTargetPrefab;
    [SerializeField] private GameObject noteElementPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private TMP_Dropdown octaveDropdown;
    [SerializeField] private TMP_Dropdown noteLengthDropdown;
    [SerializeField] private GameObject timelineIndicator;
    [SerializeField] private Canvas canvas;

    [Header("Sequencer Settings")]
    [SerializeField] private float containerDuration = 4f;
    [SerializeField] private float beatFraction = 0.25f;
    [SerializeField] private bool updateImmediately = true;
    [SerializeField] private bool enableOctaveTransposition = true;
    [SerializeField] private bool enableVelocityControl = true;
    [SerializeField] private float velocityCurveLoopDuration = 0f;
    [SerializeField] private int defaultOctave = 4;
    [SerializeField] private int defaultNoteLength = 1;

    [Header("Curve Visualization")]
    [SerializeField] private AnimationCurve octaveTranspositionCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(4f, 1f, 0f, 0f),
        new Keyframe(8f, -1f, 0f, 0f),
        new Keyframe(12f, 0f, 0f, 0f)
    );
    [SerializeField] private AnimationCurve velocityCurve = new AnimationCurve(
        new Keyframe(0f, 0.5f, 0f, 0f),
        new Keyframe(4f, 1f, 0f, 0f),
        new Keyframe(8f, 0.2f, 0f, 0f),
        new Keyframe(12f, 0.5f, 0f, 0f)
    );

    [Header("Synchronization")]
    [SerializeField] private bool isMasterSequencer = false;
    [SerializeField] private List<GridSequencer> slaveSequencers = new List<GridSequencer>();

    private const int GRID_SIZE = 16;
    private const int ROWS = 4;
    private const int COLS = 4;
    private List<GridDropTarget> dropTargets = new List<GridDropTarget>();
    private int[] gridValues = new int[GRID_SIZE];
    private float lastTimelineTime = 0f;
    private bool sequenceNeedsUpdate = false;
    private float totalTimelineLength => containerDuration;
    private float loopTime => totalTimelineLength;
    private int currentPlayingIndex = -1;
    private float currentSpeed = 1f;

    private void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[GridSequencer] Canvas reference is missing and could not be found in parent!");
                return;
            }
        }
        InitializeGrid();
    }

    private void Start()
    {
        if (timeline == null)
        {
            Debug.LogError("[GridSequencer] Timeline reference is missing!");
            return;
        }

        if (sampler == null)
        {
            Debug.LogError("[GridSequencer] Sampler reference is missing!");
            return;
        }

        if (bpmController == null)
        {
            Debug.LogError("[GridSequencer] BPM Controller reference is missing!");
            return;
        }

        if (octaveDropdown != null)
        {
            InitializeOctaveDropdown();
        }

        if (noteLengthDropdown != null)
        {
            InitializeNoteLengthDropdown();
        }

        SetTimelineLength(totalTimelineLength);
        
        // Subscribe to timeline events for synchronization
        if (timeline != null)
        {
            timeline.played += OnTimelinePlayed;
            timeline.stopped += OnTimelineStopped;
        }

        // Subscribe to BPM changes
        if (bpmController != null)
        {
            bpmController.OnBPMChanged += OnBPMChanged;
            // Get initial speed from BPM controller
            OnBPMChanged(bpmController.BPM);
        }

        // Initialize sequence
        UpdateSequence();
        lastTimelineTime = (float)timeline.time;
    }

    private System.Collections.IEnumerator InitializeSequence()
    {
        // Wait for timeline to be fully initialized
        yield return new WaitForEndOfFrame();
        
        // Update sequence
        UpdateSequence();
        lastTimelineTime = (float)timeline.time;

        // Set initial speed
        if (bpmController != null)
        {
            currentSpeed = 300f / bpmController.BPM;
            if (timeline != null && timeline.playableGraph.IsValid())
            {
                var rootPlayable = timeline.playableGraph.GetRootPlayable(0);
                if (rootPlayable.IsValid())
                {
                    rootPlayable.SetSpeed(currentSpeed);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (timeline != null)
        {
            timeline.played -= OnTimelinePlayed;
            timeline.stopped -= OnTimelineStopped;
        }

        // Unsubscribe from BPM changes
        if (bpmController != null)
        {
            bpmController.OnBPMChanged -= OnBPMChanged;
        }
    }

    private void OnTimelinePlayed(PlayableDirector director)
    {
        if (isMasterSequencer)
        {
            // Start all slave sequencers
            foreach (var slave in slaveSequencers)
            {
                if (slave != null && slave.timeline != null)
                {
                    slave.timeline.Play();
                }
            }
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        if (isMasterSequencer)
        {
            // Stop all slave sequencers
            foreach (var slave in slaveSequencers)
            {
                if (slave != null && slave.timeline != null)
                {
                    slave.timeline.Stop();
                }
            }
        }
    }

    private void OnBPMChanged(float newBPM)
    {
        if (timeline != null && timeline.playableGraph.IsValid())
        {
            var rootPlayable = timeline.playableGraph.GetRootPlayable(0);
            if (rootPlayable.IsValid())
            {
                float speedMultiplier = 60f / newBPM;
                rootPlayable.SetSpeed(speedMultiplier);
            }
        }
    }

    private void InitializeGrid()
    {
        // Clear existing drop targets
        foreach (var target in dropTargets)
        {
            if (target != null)
            {
                Destroy(target.gameObject);
            }
        }
        dropTargets.Clear();

        // Initialize grid values
        for (int i = 0; i < GRID_SIZE; i++)
        {
            gridValues[i] = 0;
        }

        // Create new drop targets
        for (int i = 0; i < GRID_SIZE; i++)
        {
            GameObject targetObj = Instantiate(dropTargetPrefab, gridContainer);
            GridDropTarget dropTarget = targetObj.GetComponent<GridDropTarget>();
            
            if (dropTarget != null)
            {
                dropTarget.Initialize(i);
                dropTargets.Add(dropTarget);
            }
        }
    }

    public void OnNoteDropped(int gridIndex, int noteValue)
    {
        Debug.Log($"[GridSequencer] OnNoteDropped called with gridIndex: {gridIndex}, noteValue: {noteValue}");
        
        if (gridIndex >= 0 && gridIndex < GRID_SIZE)
        {
            gridValues[gridIndex] = noteValue;
            
            // Remove existing note element if any
            if (dropTargets[gridIndex] != null)
            {
                // Find and destroy any existing note elements
                GridNoteElement[] existingNotes = dropTargets[gridIndex].GetComponentsInChildren<GridNoteElement>();
                foreach (var note in existingNotes)
                {
                    if (note != null)
                    {
                        Debug.Log($"[GridSequencer] Removing existing note at index {gridIndex}");
                        note.Remove();
                    }
                }
            }
            
            // Create visual representation of the note
            if (noteElementPrefab != null && dropTargets[gridIndex] != null)
            {
                Debug.Log($"[GridSequencer] Creating note element at index {gridIndex}");
                GameObject noteObj = Instantiate(noteElementPrefab, dropTargets[gridIndex].transform);
                GridNoteElement noteElement = noteObj.GetComponent<GridNoteElement>();
                if (noteElement != null)
                {
                    string[] noteNames = { "C", "D", "E", "F", "G", "A", "B" };
                    string noteName = noteNames[noteValue - 1];
                    noteElement.Initialize(noteValue, noteName);
                    Debug.Log($"[GridSequencer] Note element created and initialized with name: {noteName}");
                }
                else
                {
                    Debug.LogError("[GridSequencer] Failed to get GridNoteElement component from instantiated prefab");
                }
            }
            else
            {
                Debug.LogError($"[GridSequencer] Cannot create note element. noteElementPrefab: {(noteElementPrefab != null ? "set" : "null")}, dropTarget: {(dropTargets[gridIndex] != null ? "exists" : "null")}");
            }
            
            if (updateImmediately)
            {
                UpdateSequence();
            }
            else
            {
                MarkSequenceForUpdate();
            }
        }
        else
        {
            Debug.LogError($"[GridSequencer] Invalid grid index: {gridIndex}");
        }
    }

    private void InitializeOctaveDropdown()
    {
        octaveDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        
        // Add octaves from 0 to 8
        for (int i = 0; i <= 8; i++)
        {
            options.Add(new TMP_Dropdown.OptionData($"Octave {i}"));
        }
        
        octaveDropdown.AddOptions(options);
        octaveDropdown.value = defaultOctave;
        octaveDropdown.onValueChanged.AddListener(OnOctaveChanged);
    }

    private void OnOctaveChanged(int octave)
    {
        if (updateImmediately)
        {
            UpdateSequence();
        }
        else
        {
            MarkSequenceForUpdate();
        }
    }

    private void InitializeNoteLengthDropdown()
    {
        noteLengthDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        
        // Add note lengths: whole, half, quarter, eighth, sixteenth
        options.Add(new TMP_Dropdown.OptionData("Whole"));
        options.Add(new TMP_Dropdown.OptionData("Half"));
        options.Add(new TMP_Dropdown.OptionData("Quarter"));
        options.Add(new TMP_Dropdown.OptionData("Eighth"));
        options.Add(new TMP_Dropdown.OptionData("Sixteenth"));
        
        noteLengthDropdown.AddOptions(options);
        noteLengthDropdown.value = defaultNoteLength;
        noteLengthDropdown.onValueChanged.AddListener(OnNoteLengthChanged);
    }

    private void OnNoteLengthChanged(int lengthIndex)
    {
        if (updateImmediately)
        {
            UpdateSequence();
        }
        else
        {
            MarkSequenceForUpdate();
        }
    }

    private float GetNoteDuration(int lengthIndex)
    {
        // Convert note length index to duration in beats
        return lengthIndex switch
        {
            0 => 4f * beatFraction,  // Whole note
            1 => 2f * beatFraction,  // Half note
            2 => 1f * beatFraction,  // Quarter note
            3 => 0.5f * beatFraction, // Eighth note
            4 => 0.25f * beatFraction, // Sixteenth note
            _ => beatFraction // Default to quarter note
        };
    }

    private void Update()
    {
        if (timeline == null) return;

        float currentTime = (float)timeline.time;
        float nextFrameTime = currentTime + Time.deltaTime;

        // Update indicator position based on current time
        UpdateIndicatorPosition(currentTime);

        if (nextFrameTime >= loopTime)
        {
            UpdateSequence();
            
            if (isMasterSequencer)
            {
                // Reset all slave sequencers
                foreach (var slave in slaveSequencers)
                {
                    if (slave != null && slave.timeline != null)
                    {
                        slave.timeline.time = 0;
                    }
                }
            }
            
            timeline.time = 0;
            return;
        }

        lastTimelineTime = currentTime;
    }

    private void UpdateIndicatorPosition(float currentTime)
    {
        if (timelineIndicator == null || dropTargets.Count == 0) return;

        // Calculate time per row
        float timePerRow = containerDuration / ROWS;
        
        // Calculate current row and column based on time
        int row = Mathf.FloorToInt(currentTime / timePerRow);
        float timeInRow = currentTime % timePerRow;
        int col = Mathf.FloorToInt(timeInRow / (timePerRow / COLS));
        
        // Calculate index in the grid
        int newIndex = (row * COLS) + col;
        
        // Clamp index to valid range
        newIndex = Mathf.Clamp(newIndex, 0, dropTargets.Count - 1);
        
        // Only update if the index has changed
        if (newIndex != currentPlayingIndex)
        {
            currentPlayingIndex = newIndex;
            
            // Get the position of the current drop target
            if (currentPlayingIndex >= 0 && currentPlayingIndex < dropTargets.Count)
            {
                RectTransform dropTargetRect = dropTargets[currentPlayingIndex].GetComponent<RectTransform>();
                if (dropTargetRect != null)
                {
                    // Set indicator position to match the drop target
                    RectTransform indicatorRect = timelineIndicator.GetComponent<RectTransform>();
                    if (indicatorRect != null)
                    {
                        indicatorRect.position = dropTargetRect.position;
                    }
                }
            }
        }
    }

    private void UpdateSequence()
    {
        if (timeline == null || sampler == null) return;

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null) return;

        // Save current timeline time and speed
        double currentTime = timeline.time;
        float currentSpeed = 1f;
        if (timeline.playableGraph.IsValid())
        {
            var rootPlayable = timeline.playableGraph.GetRootPlayable(0);
            if (rootPlayable.IsValid())
            {
                currentSpeed = (float)rootPlayable.GetSpeed();
            }
        }

        // Get piano roll track
        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        if (pianoRollTracks.Count == 0)
        {
            Debug.LogError("[GridSequencer] No piano roll tracks found in timeline!");
            return;
        }

        var track = pianoRollTracks[0];
        var pianoRollTrack = track as IPianoRollTrack;

        if (pianoRollTrack == null)
        {
            Debug.LogError("[GridSequencer] Failed to cast track to IPianoRollTrack!");
            return;
        }

        // Remove existing clips
        var existingClips = track.GetClips().ToList();
        foreach (var clip in existingClips)
        {
            if (clip != null)
            {
                pianoRollTrack.DeleteClip(clip);
            }
        }

        // Get scale notes from BPM controller
        int[] scaleNotes = bpmController.GetScaleNotes();

        // Calculate time per row (1/4 of container duration)
        float timePerRow = containerDuration / ROWS;

        // Get current octave and note length
        int currentOctave = octaveDropdown != null ? octaveDropdown.value : defaultOctave;
        int currentNoteLength = noteLengthDropdown != null ? noteLengthDropdown.value : defaultNoteLength;
        int octaveOffset = (currentOctave - 4) * 12;
        float noteDuration = GetNoteDuration(currentNoteLength);

        // Update sequence based on grid values
        for (int i = 0; i < gridValues.Length; i++)
        {
            int value = gridValues[i];
            if (value > 0)
            {
                int noteIndex = value - 1;
                if (noteIndex < scaleNotes.Length)
                {
                    int note = scaleNotes[noteIndex];
                    
                    // Calculate row and column
                    int row = i / COLS;
                    int col = i % COLS;
                    
                    // Calculate time position based on row and column
                    float timePosition = (row * timePerRow) + (col * (timePerRow / COLS));
                    
                    // Get transposition and velocity from curves
                    int transposition = GetTranspositionAtTime(timePosition);
                    float velocity = GetVelocityAtTime(timePosition);
                    
                    // Apply transposition and octave offset
                    int transposedNote = note + transposition + octaveOffset;
                    
                    // Create clip in timeline
                    var clip = pianoRollTrack.CreateClip();
                    if (clip != null && clip.asset != null)
                    {
                        var samplerClip = clip.asset as SamplerPianoRollClip;
                        if (samplerClip != null)
                        {
                            clip.start = timePosition;
                            clip.duration = noteDuration;
                            samplerClip.midiNote = transposedNote;
                            samplerClip.duration = noteDuration;
                            samplerClip.startTime = timePosition;
                            samplerClip.velocity = velocity;
                            samplerClip.sourceObject = sampler.gameObject;
                            clip.displayName = $"Note {transposedNote} (original: {note}, transposition: {transposition}, octave: {currentOctave}, length: {currentNoteLength}, velocity: {velocity:F2}) at {timePosition:F2}s";
                        }
                    }
                }
            }
        }

        // Rebuild timeline and restore time and speed
        timeline.RebuildGraph();
        timeline.time = currentTime;

        if (timeline.playableGraph.IsValid())
        {
            var rootPlayable = timeline.playableGraph.GetRootPlayable(0);
            if (rootPlayable.IsValid())
            {
                rootPlayable.SetSpeed(currentSpeed);
            }
        }
    }

    private int GetTranspositionAtTime(float time)
    {
        if (!enableOctaveTransposition) return 0;
        float octaveChange = octaveTranspositionCurve.Evaluate(time);
        return Mathf.RoundToInt(octaveChange * 12);
    }

    private float GetVelocityAtTime(float time)
    {
        if (!enableVelocityControl) return 1f;
        float curveTime = time;
        if (velocityCurveLoopDuration > 0)
        {
            curveTime = time % velocityCurveLoopDuration;
        }
        return Mathf.Clamp01(velocityCurve.Evaluate(curveTime));
    }

    public void SetTimelineLength(float length)
    {
        if (timeline == null) return;
        
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null) return;
        
        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
        timelineAsset.fixedDuration = length;
        
        UpdateSequence();
    }

    public void MarkSequenceForUpdate()
    {
        sequenceNeedsUpdate = true;
    }

    // Public methods to control curves
    public void SetOctaveCurve(AnimationCurve curve)
    {
        if (curve == null) return;
        octaveTranspositionCurve = new AnimationCurve(curve.keys);
    }

    public void SetVelocityCurve(AnimationCurve curve)
    {
        if (curve == null) return;
        velocityCurve = new AnimationCurve(curve.keys);
    }

    public void AddSlaveSequencer(GridSequencer sequencer)
    {
        if (sequencer != null && !slaveSequencers.Contains(sequencer))
        {
            slaveSequencers.Add(sequencer);
        }
    }

    public void RemoveSlaveSequencer(GridSequencer sequencer)
    {
        if (sequencer != null)
        {
            slaveSequencers.Remove(sequencer);
        }
    }

    public void SetAsMaster(bool isMaster)
    {
        isMasterSequencer = isMaster;
    }

    // Method to create a new note element for dragging
    public void CreateNoteElement(int noteValue, string noteText, Vector2 position)
    {
        GameObject noteObj = Instantiate(noteElementPrefab, canvas.transform);
        GridNoteElement noteElement = noteObj.GetComponent<GridNoteElement>();
        
        if (noteElement != null)
        {
            noteElement.Initialize(noteValue, noteText);
            RectTransform rectTransform = noteObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = position;
            }
        }
    }

    private void CreateGrid()
    {
        // Clear existing grid
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        dropTargets.Clear();
        gridValues = new int[GRID_SIZE];

        // Create grid container
        GameObject gridContainer = new GameObject("GridContainer");
        gridContainer.transform.SetParent(transform, false);
        RectTransform gridRect = gridContainer.AddComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;

        // Calculate cell size
        float cellWidth = gridRect.rect.width / GRID_SIZE;
        float cellHeight = gridRect.rect.height;

        // Create drop targets
        for (int i = 0; i < GRID_SIZE; i++)
        {
            GameObject dropTargetObj = Instantiate(dropTargetPrefab, gridContainer.transform);
            RectTransform dropTargetRect = dropTargetObj.GetComponent<RectTransform>();
            
            // Set position and size
            dropTargetRect.anchorMin = new Vector2(i / (float)GRID_SIZE, 0);
            dropTargetRect.anchorMax = new Vector2((i + 1) / (float)GRID_SIZE, 1);
            dropTargetRect.offsetMin = Vector2.zero;
            dropTargetRect.offsetMax = Vector2.zero;

            // Initialize drop target
            GridDropTarget dropTarget = dropTargetObj.GetComponent<GridDropTarget>();
            if (dropTarget != null)
            {
                dropTarget.Initialize(i);
                dropTargets.Add(dropTarget);
            }
        }

        // Create note buttons container
        GameObject noteButtonsContainer = new GameObject("NoteButtonsContainer");
        noteButtonsContainer.transform.SetParent(transform, false);
        RectTransform noteButtonsRect = noteButtonsContainer.AddComponent<RectTransform>();
        noteButtonsRect.anchorMin = new Vector2(0, 1);
        noteButtonsRect.anchorMax = new Vector2(1, 1);
        noteButtonsRect.pivot = new Vector2(0.5f, 0);
        noteButtonsRect.sizeDelta = new Vector2(0, 50);
        noteButtonsRect.anchoredPosition = new Vector2(0, 0);

        // Create note buttons
        int[] scaleNotes = bpmController.GetScaleNotes();
        float buttonWidth = noteButtonsRect.rect.width / scaleNotes.Length;

        for (int i = 0; i < scaleNotes.Length; i++)
        {
            GameObject buttonObj = Instantiate(noteElementPrefab, noteButtonsContainer.transform);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            
            // Set position and size
            buttonRect.anchorMin = new Vector2(i / (float)scaleNotes.Length, 0);
            buttonRect.anchorMax = new Vector2((i + 1) / (float)scaleNotes.Length, 1);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            // Initialize note button
            GridNoteElement noteButton = buttonObj.GetComponent<GridNoteElement>();
            if (noteButton != null)
            {
                string noteName = GetNoteName(scaleNotes[i]);
                noteButton.Initialize(scaleNotes[i], noteName);
            }
        }
    }

    private string GetNoteName(int midiNote)
    {
        // Convert MIDI note number to note name
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{noteNames[noteIndex]}{octave}";
    }
} 