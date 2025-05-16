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
    [SerializeField] private bool enableVelocityControl = true;
    [Tooltip("Duration of the velocity curve loop. Set to 0 to use full timeline length.")]
    [SerializeField] private float velocityCurveLoopDuration = 0f;
    [Tooltip("Curve controlling octave transposition. X axis is timeline time, Y axis is octave change (0 = no change, 1 = +1 octave, -1 = -1 octave)")]
    [SerializeField] private AnimationCurve octaveTranspositionCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f), // Start at 0 octaves
        new Keyframe(4f, 1f, 0f, 0f), // At 4 seconds, +1 octave
        new Keyframe(8f, -1f, 0f, 0f), // At 8 seconds, -1 octave
        new Keyframe(12f, 0f, 0f, 0f)  // At 12 seconds, back to 0
    );
    [Tooltip("Curve controlling note velocity. X axis is timeline time, Y axis is velocity (0-1)")]
    [SerializeField] private AnimationCurve velocityCurve = new AnimationCurve(
        new Keyframe(0f, 0.5f, 0f, 0f),  // Start at 0.5 velocity
        new Keyframe(4f, 1f, 0f, 0f),    // At 4 seconds, full velocity
        new Keyframe(8f, 0.2f, 0f, 0f),  // At 8 seconds, low velocity
        new Keyframe(12f, 0.5f, 0f, 0f)  // At 12 seconds, back to 0.5
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
        
        //Debug.Log($"[CitySequencer] Time {time:F2}s: octave change = {octaveChange:F2}, semitones = {semitones}");
        return semitones;
    }

    // Calculate velocity based on the curve at a specific time
    private float GetVelocityAtTime(float time)
    {
        if (!enableVelocityControl) return 1f;
        
        float curveTime = time;
        
        // If loop duration is set, use modulo to loop the time
        if (velocityCurveLoopDuration > 0)
        {
            curveTime = time % velocityCurveLoopDuration;
            //Debug.Log($"[CitySequencer] Time {time:F2}s: using looped time {curveTime:F2}s (loop duration: {velocityCurveLoopDuration:F2}s)");
        }
        
        // Evaluate the curve at the given time
        float velocity = velocityCurve.Evaluate(curveTime);
        
        // Clamp velocity between 0 and 1
        velocity = Mathf.Clamp01(velocity);
        
        //Debug.Log($"[CitySequencer] Time {time:F2}s: velocity = {velocity:F2}");
        return velocity;
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
            //Debug.LogError("[CitySequencer] Timeline reference is missing!");
            return;
        }
        
        if (noteContainers.Count == 0)
        {
            //Debug.LogError("[CitySequencer] No note containers assigned! Please assign at least one CityNoteContainer in the inspector.");
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
            //Debug.LogError("[CitySequencer] Timeline reference is missing!");
            return;
        }
        
        if (noteContainers.Count == 0)
        {
            //Debug.LogError("[CitySequencer] No note containers assigned!");
            return;
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            //Debug.LogError("[CitySequencer] Timeline asset is null!");
            return;
        }

        // Set timeline duration to the total length for all containers
        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
        timelineAsset.fixedDuration = totalTimelineLength;

        // Validate timeline length
        if (totalTimelineLength <= 0)
        {
            //Debug.LogWarning("[CitySequencer] Total timeline length must be greater than 0.");
            return;
        }

        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        if (pianoRollTracks.Count == 0)
        {
            //Debug.LogError("[CitySequencer] No piano roll tracks found in timeline!");
            return;
        }

        if (trackIndex >= pianoRollTracks.Count)
        {
            //Debug.LogError($"[CitySequencer] Track index {trackIndex} out of range! Available tracks: {pianoRollTracks.Count}");
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
        //Debug.Log("[CitySequencer] Current state:");
        //Debug.Log($"[CitySequencer] - Total timeline length: {totalTimelineLength}");
        //Debug.Log($"[CitySequencer] - Container duration: {containerDuration}");
        //Debug.Log($"[CitySequencer] - Number of containers: {noteContainers.Count}");
        //Debug.Log($"[CitySequencer] - Last timeline time: {lastTimelineTime}");
        //Debug.Log($"[CitySequencer] - Beat fraction: {beatFraction}");
        //Debug.Log($"[CitySequencer] - World scale: {worldScale}");
        //Debug.Log($"[CitySequencer] - Snap to grid: {snapToGrid}");
        
        if (timeline != null)
        {
            //Debug.Log($"[CitySequencer] - Timeline object: {timeline.gameObject.name}");
            //Debug.Log($"[CitySequencer] - Current time: {timeline.time}");
        }
        
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                var notes = container.GetAllNotes();
                //Debug.Log($"[CitySequencer] - Container: {container.gameObject.name}");
                //Debug.Log($"[CitySequencer] - Total notes: {notes.Count}");
                
                foreach (var note in notes)
                {
                    if (note != null)
                    {
                        //Debug.Log($"[CitySequencer] - Note: pitch={note.pitch}, position={note.position}, duration={note.duration}, velocity={note.velocity}");
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
            //Debug.LogError("[CitySequencer] Timeline is null in Update!");
            return;
        }

        float currentTime = (float)timeline.time;
        float nextFrameTime = currentTime + Time.deltaTime;
        
        // Check if Timeline will reach the loop point in the next frame
        if (nextFrameTime >= loopTime)
        {
            //Debug.Log($"[CitySequencer] Timeline reached loop point, looping back to start. Current: {currentTime}, Next: {nextFrameTime}, Loop time: {loopTime}");
            
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
            //Debug.LogError("[CitySequencer] Cannot update sequence: timeline is null!");
            return;
        }
        
        if (noteContainers.Count == 0)
        {
            //Debug.LogError("[CitySequencer] No note containers assigned!");
            return;
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            //Debug.LogError("[CitySequencer] Timeline asset is null!");
            return;
        }

        // Save current timeline time
        double currentTime = timeline.time;

        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        if (pianoRollTracks.Count == 0)
        {
            //Debug.LogError("[CitySequencer] No piano roll tracks found in timeline!");
            return;
        }

        if (trackIndex >= pianoRollTracks.Count)
        {
            //Debug.LogError($"[CitySequencer] Track index {trackIndex} out of range! Available tracks: {pianoRollTracks.Count}");
            return;
        }

        var track = pianoRollTracks[trackIndex];
        var pianoRollTrack = track as IPianoRollTrack;

        if (pianoRollTrack == null)
        {
            //Debug.LogError("[CitySequencer] Failed to cast track to IPianoRollTrack!");
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
            float containerSpacing = containerDuration / Mathf.Max(1, allNotes.Count);
            
            // Update note positions based on their indices
            for (int i = 0; i < allNotes.Count; i++)
            {
                if (allNotes[i] != null && allNotes[i].useAutoPosition)
                {
                    allNotes[i].position = i * containerSpacing;
                }
            }
            
            foreach (var note in allNotes)
            {
                if (note == null) continue;

                // Get modified note values from container
                var (modifiedPitch, modifiedDuration, modifiedRepeatCount) = container.GetModifiedNoteValues(note);
                
                // Create positions array based on modified repeat count
                Vector3[] positions;
                if (modifiedRepeatCount <= 0)
                {
                    positions = new Vector3[] { new Vector3(note.position, 0, 0) };
                }
                else
                {
                    positions = new Vector3[modifiedRepeatCount + 1];
                    positions[0] = new Vector3(note.position, 0, 0);

                    // Get the next note's position to calculate spacing
                    float nextNotePosition = GetNextNotePosition(note, allNotes);
                    float repeatSpacing = CalculateRepeatSpacing(note.position, nextNotePosition, modifiedRepeatCount, modifiedDuration);

                    for (int i = 1; i <= modifiedRepeatCount; i++)
                    {
                        positions[i] = new Vector3(note.position + (repeatSpacing * i), 0, 0);
                    }
                }
                
                foreach (Vector3 pos in positions)
                {
                    var clip = pianoRollTrack.CreateClip();
                    if (clip == null || clip.asset == null) continue;
                    
                    var samplerClip = clip.asset as SamplerPianoRollClip;
                    if (samplerClip == null) continue;

                    float timePosition = WorldToTimelinePosition(pos, containerIndex);
                    
                    // Get transposition from the curve at this time position
                    int transposition = GetTranspositionAtTime(timePosition);
                    
                    // Get velocity from the curve at this time position
                    float velocity = GetVelocityAtTime(timePosition);
                    
                    // Apply transposition to the modified pitch
                    int transposedPitch = modifiedPitch + transposition;
                    
                    clip.start = timePosition;
                    clip.duration = modifiedDuration;
                    samplerClip.midiNote = transposedPitch;
                    samplerClip.duration = modifiedDuration;
                    samplerClip.startTime = timePosition;
                    samplerClip.velocity = velocity;
                    clip.displayName = $"Note {transposedPitch} (original: {note.pitch}, transposition: {transposition}, velocity: {velocity:F2}) at {timePosition:F2}s";
                    totalClipsCreated++;
                }
            }
        }

        timeline.RebuildGraph();
        timeline.time = currentTime;
    }

    private float GetNextNotePosition(CityNote currentNote, List<CityNote> allNotes)
    {
        float nextX = float.MaxValue;
        foreach (var note in allNotes)
        {
            if (note != currentNote && note.position > currentNote.position && note.position < nextX)
            {
                nextX = note.position;
            }
        }
        return nextX == float.MaxValue ? currentNote.position + 1f : nextX;
    }

    private float CalculateRepeatSpacing(float currentPosition, float nextNotePosition, int repeatCount, float duration)
    {
        float distanceToNextNote = nextNotePosition - currentPosition;
        float totalDuration = duration * (repeatCount + 1);
        float spacing = distanceToNextNote / (repeatCount + 1);
        return Mathf.Max(spacing, 0.1f);
    }

    public void MarkSequenceForUpdate()
    {
        //Debug.Log("[CitySequencer] Sequence marked for update!");
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
            //Debug.LogError("[CitySequencer] Cannot set timeline length: timeline is null!");
            return;
        }
        
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            //Debug.LogError("[CitySequencer] Cannot set timeline length: timeline asset is null!");
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

    public List<CityNoteContainer> GetNoteContainers()
    {
        return noteContainers;
    }

    public void SetNoteContainers(List<CityNoteContainer> newContainers)
    {
        if (newContainers == null)
        {
            //Debug.LogError("[CitySequencer] Cannot set null containers list!");
            return;
        }

        // Unsubscribe from old containers
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                container.OnNotesChanged -= HandleNotesChanged;
            }
        }

        // Set new containers
        noteContainers = newContainers;

        // Subscribe to new containers
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                container.OnNotesChanged += HandleNotesChanged;
            }
        }

        // Update sequence with new containers
        UpdateSequence();
    }

    public void SetNoteContainersWithoutUpdate(List<CityNoteContainer> newContainers)
    {
        if (newContainers == null)
        {
            //Debug.LogError("[CitySequencer] Cannot set null containers list!");
            return;
        }

        // Unsubscribe from old containers
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                container.OnNotesChanged -= HandleNotesChanged;
            }
        }

        // Set new containers
        noteContainers = newContainers;

        // Subscribe to new containers
        foreach (var container in noteContainers)
        {
            if (container != null)
            {
                container.OnNotesChanged += HandleNotesChanged;
            }
        }

        // Notify listeners about the update
        OnSequenceUpdated?.Invoke();
    }

    // Event that will be called when sequence is updated
    public delegate void SequenceUpdatedHandler();
    public event SequenceUpdatedHandler OnSequenceUpdated;
} 