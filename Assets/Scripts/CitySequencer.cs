using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

public class CitySequencer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private List<CityNoteContainer> noteContainers = new List<CityNoteContainer>();
    [SerializeField] private int trackIndex = 0;

    [Header("Sequencer Settings")]
    [SerializeField] private float containerDuration = 4f; // Duration for each container
    [SerializeField] private float beatFraction = 0.25f;
    [Tooltip("If true, sequence updates immediately when notes change. If false, updates at the end of timeline loop.")]
    [SerializeField] private bool updateImmediately = true;
    [SerializeField] private bool shouldShiftNotes = false;
    [SerializeField] private bool enableOctaveTransposition = true;
    [Tooltip("Curve controlling octave transposition. X axis is timeline time, Y axis is octave change (0 = no change, 1 = +1 octave, -1 = -1 octave)")]
    [SerializeField] private AnimationCurve octaveTranspositionCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f), // Start at 0 octaves
        new Keyframe(4f, 1f, 0f, 0f), // At 4 seconds, +1 octave
        new Keyframe(8f, -1f, 0f, 0f), // At 8 seconds, -1 octave
        new Keyframe(12f, 0f, 0f, 0f)  // At 12 seconds, back to 0
    );
    private bool wasShiftRequested = false;

    [Header("Position Mapping")]
    [SerializeField] private float worldScale = 1f;
    [SerializeField] private bool snapToGrid = false;

    private float lastTimelineTime = 0f;
    private bool sequenceNeedsUpdate = false;
    private float totalTimelineLength => containerDuration * noteContainers.Count;
    private float loopTime => totalTimelineLength;

    // Calculate transposition based on the curve at a specific time
    private int GetTranspositionAtTime(float time)
    {
        if (!enableOctaveTransposition) return 0;
        
        // Evaluate the curve at the given time
        float octaveChange = octaveTranspositionCurve.Evaluate(time);
        
        // Convert octave change to semitones (1 octave = 12 semitones)
        int semitones = Mathf.RoundToInt(octaveChange * 12);
        
        Debug.Log($"[CitySequencer] Time {time:F2}s: octave change = {octaveChange:F2}, semitones = {semitones}");
        return semitones;
    }

    public float GetTimelineLength()
    {
        return totalTimelineLength;
    }

    public float GetLoopTime()
    {
        return loopTime;
    }

    private void Start()
    {
        if (timeline == null)
        {
            Debug.LogError("[CitySequencer] Timeline reference is missing!");
            return;
        }
        
        if (noteContainers.Count == 0)
        {
            Debug.LogError("[CitySequencer] No note containers assigned! Please assign at least one CityNoteContainer in the inspector.");
            return;
        }

        // Subscribe to all note container changes
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                container.OnNotesChanged += HandleNotesChanged;
            }
        }

        // Set initial timeline length
        SetTimelineLength(totalTimelineLength);
        
        // Update sequence and store initial time
        UpdateSequence();
        lastTimelineTime = (float)timeline.time;
    }

    private void OnDestroy()
    {
        // Unsubscribe from all note container changes
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                container.OnNotesChanged -= HandleNotesChanged;
            }
        }
    }

    public void HandleNotesChanged()
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

    private void ValidateConfiguration()
    {
        if (timeline == null)
        {
            Debug.LogError("[CitySequencer] Timeline reference is missing!");
            return;
        }
        
        if (noteContainers.Count == 0)
        {
            Debug.LogError("[CitySequencer] No note containers assigned!");
            return;
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("[CitySequencer] Timeline asset is null!");
            return;
        }

        // Set timeline duration to the total length for all containers
        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
        timelineAsset.fixedDuration = totalTimelineLength;

        // Validate timeline length
        if (totalTimelineLength <= 0)
        {
            Debug.LogWarning("[CitySequencer] Total timeline length must be greater than 0.");
            return;
        }

        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        if (pianoRollTracks.Count == 0)
        {
            Debug.LogError("[CitySequencer] No piano roll tracks found in timeline!");
            return;
        }

        if (trackIndex >= pianoRollTracks.Count)
        {
            Debug.LogError($"[CitySequencer] Track index {trackIndex} out of range! Available tracks: {pianoRollTracks.Count}");
            return;
        }

        UpdateSequence();
        
        if (timeline != null)
        {
            lastTimelineTime = (float)timeline.time;
        }
    }

    private void LogState()
    {
        Debug.Log("[CitySequencer] Current state:");
        Debug.Log($"[CitySequencer] - Total timeline length: {totalTimelineLength}");
        Debug.Log($"[CitySequencer] - Container duration: {containerDuration}");
        Debug.Log($"[CitySequencer] - Number of containers: {noteContainers.Count}");
        Debug.Log($"[CitySequencer] - Last timeline time: {lastTimelineTime}");
        Debug.Log($"[CitySequencer] - Beat fraction: {beatFraction}");
        Debug.Log($"[CitySequencer] - World scale: {worldScale}");
        Debug.Log($"[CitySequencer] - Snap to grid: {snapToGrid}");
        
        if (timeline != null)
        {
            Debug.Log($"[CitySequencer] - Timeline object: {timeline.gameObject.name}");
            Debug.Log($"[CitySequencer] - Current time: {timeline.time}");
        }
        
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                var notes = container.GetAllNotes();
                Debug.Log($"[CitySequencer] - Container: {container.gameObject.name}");
                Debug.Log($"[CitySequencer] - Total notes: {notes.Count}");
                
                foreach (var note in notes)
                {
                    if (note != null)
                    {
                        Debug.Log($"[CitySequencer] - Note: pitch={note.pitch}, position={note.position}, duration={note.duration}, velocity={note.velocity}");
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            LogState();
        }

        if (timeline == null)
        {
            Debug.LogError("[CitySequencer] Timeline is null in Update!");
            return;
        }

        float currentTime = (float)timeline.time;
        float nextFrameTime = currentTime + Time.deltaTime;
        
        // Check if Timeline will reach the loop point in the next frame
        if (nextFrameTime >= loopTime)
        {
            Debug.Log($"[CitySequencer] Timeline reached loop point, looping back to start. Current: {currentTime}, Next: {nextFrameTime}, Loop time: {loopTime}");
            
            // Shift notes forward one frame before the loop ends
            if (shouldShiftNotes && wasShiftRequested)
            {
                foreach (var container in noteContainers)
                {
                    if (container != null)
                    {
                        container.ShiftNotesForward();
                    }
                }
                shouldShiftNotes = false;
                wasShiftRequested = false;
            }
            
            // Check if we need to update the sequence
            UpdateSequence();
            
            // Loop back to start
            timeline.time = 0;
            return;
        }
        
        lastTimelineTime = currentTime;
    }

    private float WorldToTimelinePosition(Vector3 worldPosition, int containerIndex)
    {
        // Calculate base position for this container
        float containerBaseTime = containerIndex * containerDuration;
        
        // Map world X position to timeline position within container's time slot
        float timePosition = containerBaseTime + (worldPosition.x * worldScale);

        if (snapToGrid)
        {
            float beatSize = containerDuration * beatFraction;
            timePosition = Mathf.Round(timePosition / beatSize) * beatSize;
        }

        return timePosition;
    }

    public void UpdateSequence()
    {
        if (timeline == null)
        {
            Debug.LogError("[CitySequencer] Cannot update sequence: timeline is null!");
            return;
        }
        
        if (noteContainers.Count == 0)
        {
            Debug.LogError("[CitySequencer] No note containers assigned!");
            return;
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("[CitySequencer] Timeline asset is null!");
            return;
        }

        // Save current timeline time
        double currentTime = timeline.time;

        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        if (pianoRollTracks.Count == 0)
        {
            Debug.LogError("[CitySequencer] No piano roll tracks found in timeline!");
            return;
        }

        if (trackIndex >= pianoRollTracks.Count)
        {
            Debug.LogError($"[CitySequencer] Track index {trackIndex} out of range! Available tracks: {pianoRollTracks.Count}");
            return;
        }

        var track = pianoRollTracks[trackIndex];
        var pianoRollTrack = track as IPianoRollTrack;

        if (pianoRollTrack == null)
        {
            Debug.LogError("[CitySequencer] Failed to cast track to IPianoRollTrack!");
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

        // Add clips from all containers
        int totalClipsCreated = 0;
        for (int containerIndex = 0; containerIndex < noteContainers.Count; containerIndex++)
        {
            var container = noteContainers[containerIndex];
            if (container == null) continue;

            var allNotes = container.GetAllNotes();
            float spacing = containerDuration / Mathf.Max(1, allNotes.Count);
            
            // Update note positions based on their indices
            for (int i = 0; i < allNotes.Count; i++)
            {
                if (allNotes[i] != null && allNotes[i].useAutoPosition)
                {
                    allNotes[i].position = i * spacing;
                }
            }
            
            foreach (var note in allNotes)
            {
                if (note == null) continue;

                Vector3[] positions = note.GetRepeatPositions();
                
                foreach (Vector3 pos in positions)
                {
                    var clip = pianoRollTrack.CreateClip();
                    if (clip == null || clip.asset == null) continue;
                    
                    var samplerClip = clip.asset as SamplerPianoRollClip;
                    if (samplerClip == null) continue;

                    float timePosition = WorldToTimelinePosition(pos, containerIndex);
                    
                    // Get transposition from the curve at this time position
                    int transposition = GetTranspositionAtTime(timePosition);
                    
                    // Apply transposition to the note pitch
                    int transposedPitch = note.pitch + transposition;
                    
                    clip.start = timePosition;
                    clip.duration = note.duration;
                    samplerClip.midiNote = transposedPitch;
                    samplerClip.duration = note.duration;
                    samplerClip.startTime = timePosition;
                    samplerClip.velocity = note.velocity;
                    clip.displayName = $"Note {transposedPitch} (original: {note.pitch}, transposition: {transposition}) at {timePosition:F2}s";
                    totalClipsCreated++;
                }
            }
        }

        timeline.RebuildGraph();
        timeline.time = currentTime;
    }

    public void MarkSequenceForUpdate()
    {
        Debug.Log("[CitySequencer] Sequence marked for update!");
        sequenceNeedsUpdate = true;
    }

    private void OnDrawGizmos()
    {
        if (snapToGrid)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
            float beatSize = containerDuration * beatFraction;
            int gridCount = Mathf.CeilToInt(totalTimelineLength / beatSize);
            
            for (int i = 0; i <= gridCount; i++)
            {
                float x = i * worldScale;
                Gizmos.DrawLine(
                    new Vector3(x, 0, -5f),
                    new Vector3(x, 0, 5f)
                );
            }
        }
    }

    public void SetTimelineLength(float length)
    {
        if (timeline == null)
        {
            Debug.LogError("[CitySequencer] Cannot set timeline length: timeline is null!");
            return;
        }
        
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("[CitySequencer] Cannot set timeline length: timeline asset is null!");
            return;
        }
        
        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
        timelineAsset.fixedDuration = length;
        
        UpdateSequence();
    }

    public void SetShouldShiftNotes(bool value)
    {
        shouldShiftNotes = value;
        wasShiftRequested = true;
    }
} 