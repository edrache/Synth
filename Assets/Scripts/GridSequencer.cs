using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GridSequencer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private Sampler sampler;
    [SerializeField] private TimelineBPMController bpmController;
    [SerializeField] private GameObject dropdownPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private TMP_Dropdown octaveDropdown;
    [SerializeField] private TMP_Dropdown noteLengthDropdown;

    [Header("Sequencer Settings")]
    [SerializeField] private float containerDuration = 4f;
    [SerializeField] private float beatFraction = 0.25f;
    [SerializeField] private bool updateImmediately = true;
    [SerializeField] private bool enableOctaveTransposition = true;
    [SerializeField] private bool enableVelocityControl = true;
    [SerializeField] private float velocityCurveLoopDuration = 0f;
    [SerializeField] private int defaultOctave = 4; // Middle octave (C4 = 60)
    [SerializeField] private int defaultNoteLength = 1; // Default to quarter note

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
    private List<TMP_Dropdown> dropdowns = new List<TMP_Dropdown>();
    private float lastTimelineTime = 0f;
    private bool sequenceNeedsUpdate = false;
    private float totalTimelineLength => containerDuration;
    private float loopTime => totalTimelineLength;

    private void Awake()
    {
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
        UpdateSequence();
        lastTimelineTime = (float)timeline.time;

        // Subscribe to timeline events for synchronization
        if (timeline != null)
        {
            timeline.played += OnTimelinePlayed;
            timeline.stopped += OnTimelineStopped;
        }
    }

    private void OnDestroy()
    {
        if (timeline != null)
        {
            timeline.played -= OnTimelinePlayed;
            timeline.stopped -= OnTimelineStopped;
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

    private void InitializeGrid()
    {
        // Clear existing dropdowns
        foreach (var dropdown in dropdowns)
        {
            if (dropdown != null)
            {
                Destroy(dropdown.gameObject);
            }
        }
        dropdowns.Clear();

        // Create new dropdowns
        for (int i = 0; i < GRID_SIZE; i++)
        {
            GameObject dropdownObj = Instantiate(dropdownPrefab, gridContainer);
            TMP_Dropdown dropdown = dropdownObj.GetComponent<TMP_Dropdown>();
            
            if (dropdown != null)
            {
                // Initialize dropdown options (0-7)
                dropdown.ClearOptions();
                List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                options.Add(new TMP_Dropdown.OptionData("0")); // No note
                for (int j = 1; j <= 7; j++)
                {
                    options.Add(new TMP_Dropdown.OptionData(j.ToString()));
                }
                dropdown.AddOptions(options);

                // Add listener
                int index = i; // Capture index for lambda
                dropdown.onValueChanged.AddListener((value) => OnDropdownValueChanged(index, value));
                
                dropdowns.Add(dropdown);
            }
        }
    }

    private void OnDropdownValueChanged(int index, int value)
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

    private void UpdateSequence()
    {
        if (timeline == null || sampler == null) return;

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null) return;

        // Save current timeline time
        double currentTime = timeline.time;

        // Get piano roll track
        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        if (pianoRollTracks.Count == 0)
        {
            Debug.LogError("[GridSequencer] No piano roll tracks found in timeline!");
            return;
        }

        var track = pianoRollTracks[0]; // Use first track
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
        int octaveOffset = (currentOctave - 4) * 12; // Convert octave to semitones (4 is middle octave)
        float noteDuration = GetNoteDuration(currentNoteLength);

        // Update sequence based on dropdown values
        for (int i = 0; i < dropdowns.Count; i++)
        {
            int value = dropdowns[i].value;
            if (value > 0) // Skip if no note (value = 0)
            {
                int noteIndex = value - 1; // Convert to 0-based index
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

        timeline.RebuildGraph();
        timeline.time = currentTime;
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
} 