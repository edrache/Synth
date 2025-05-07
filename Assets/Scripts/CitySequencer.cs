using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

public class CitySequencer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private CityNoteContainer noteContainer;
    [SerializeField] private int trackIndex = 0;

    [Header("Sequencer Settings")]
    [SerializeField] private float timelineLength = 4f;
    [SerializeField] private float loopTime = 2f; // Nowy parametr określający kiedy timeline ma się zapętlić
    [SerializeField] private float beatFraction = 0.25f;
    [Tooltip("If true, sequence updates immediately when notes change. If false, updates at the end of timeline loop.")]
    [SerializeField] private bool updateImmediately = true;

    [Header("Position Mapping")]
    [SerializeField] private float worldScale = 1f; // How many world units per beat
    [SerializeField] private bool snapToGrid = false; // Wyłączone domyślnie

    private float lastTimelineTime = 0f;
    private bool sequenceNeedsUpdate = false;

    public float GetTimelineLength()
    {
        return timelineLength;
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
        
        if (noteContainer == null)
        {
            Debug.LogError("[CitySequencer] NoteContainer reference is missing! Please assign a CityNoteContainer in the inspector.");
            return;
        }

        // Set initial timeline length
        SetTimelineLength(timelineLength);
        
        // Subscribe to note container changes
        noteContainer.OnNotesChanged += HandleNotesChanged;
        
        // Update sequence and store initial time
        UpdateSequence();
        lastTimelineTime = (float)timeline.time;
    }

    private void OnDestroy()
    {
        // Unsubscribe from note container changes
        if (noteContainer != null)
        {
            noteContainer.OnNotesChanged -= HandleNotesChanged;
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
        
        if (noteContainer == null)
        {
            Debug.LogError("[CitySequencer] NoteContainer reference is missing! Please assign a CityNoteContainer in the inspector.");
            return;
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("[CitySequencer] Timeline asset is null!");
            return;
        }

        // Set timeline duration to the full length for note placement
        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
        timelineAsset.fixedDuration = timelineLength;

        // Validate timeline length and loop time
        if (timelineLength <= 0)
        {
            Debug.LogWarning("[CitySequencer] Timeline length must be greater than 0. Setting to default value of 8.");
            timelineLength = 8f;
        }

        if (loopTime <= 0)
        {
            Debug.LogWarning("[CitySequencer] Loop time must be greater than 0. Setting to default value of 2.");
            loopTime = 2f;
        }

        if (loopTime > timelineLength)
        {
            Debug.LogWarning("[CitySequencer] Loop time cannot be greater than timeline length. Setting loop time to timeline length.");
            loopTime = timelineLength;
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
        Debug.Log($"[CitySequencer] - Timeline length: {timelineLength}");
        Debug.Log($"[CitySequencer] - Last timeline time: {lastTimelineTime}");
        Debug.Log($"[CitySequencer] - Beat fraction: {beatFraction}");
        Debug.Log($"[CitySequencer] - World scale: {worldScale}");
        Debug.Log($"[CitySequencer] - Snap to grid: {snapToGrid}");
        
        if (timeline != null)
        {
            Debug.Log($"[CitySequencer] - Timeline object: {timeline.gameObject.name}");
            Debug.Log($"[CitySequencer] - Current time: {timeline.time}");
            
            var timelineAsset = timeline.playableAsset as TimelineAsset;
            if (timelineAsset != null)
            {
                Debug.Log($"[CitySequencer] - Timeline asset duration mode: {timelineAsset.durationMode}");
                Debug.Log($"[CitySequencer] - Timeline asset fixed duration: {timelineAsset.fixedDuration}");
            }
            else
            {
                Debug.Log("[CitySequencer] - Timeline asset is null!");
            }
        }
        else
        {
            Debug.Log("[CitySequencer] - Timeline is null!");
        }
        
        if (noteContainer != null)
        {
            var notes = noteContainer.GetAllNotes();
            Debug.Log($"[CitySequencer] - Note container: {noteContainer.gameObject.name}");
            Debug.Log($"[CitySequencer] - Total notes: {notes.Count}");
            
            foreach (var note in notes)
            {
                if (note != null)
                {
                    Debug.Log($"[CitySequencer] - Note: pitch={note.pitch}, position={note.position}, duration={note.duration}, velocity={note.velocity}");
                }
            }
        }
        else
        {
            Debug.Log("[CitySequencer] - Note container is null!");
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
            
            // Check if we need to update the sequence
            if (noteContainer != null)
            {
                UpdateSequence();
            }
            
            // Loop back to start
            timeline.time = 0;
            return;
        }
        
        lastTimelineTime = currentTime;
    }

    private float WorldToTimelinePosition(Vector3 worldPosition)
    {
        // Map world X position directly to timeline position
        // Each world unit represents one beat
        float timePosition = worldPosition.x * worldScale;

        if (snapToGrid)
        {
            // Snap to grid (beats)
            float beatSize = timelineLength * beatFraction;
            timePosition = Mathf.Round(timePosition / beatSize) * beatSize;
        }

        // Wrap the position around the timeline length
        return timePosition % timelineLength;
    }

    public void UpdateSequence()
    {
        if (timeline == null)
        {
            Debug.LogError("[CitySequencer] Cannot update sequence: timeline is null!");
            return;
        }
        
        if (noteContainer == null)
        {
            Debug.LogError("[CitySequencer] Cannot update sequence: noteContainer is null!");
            return;
        }

        // Update note positions based on their indices in the container
        var containerNotes = noteContainer.GetAllNotes();
        float spacing = timelineLength / Mathf.Max(1, containerNotes.Count);
        
        for (int i = 0; i < containerNotes.Count; i++)
        {
            if (containerNotes[i] != null && containerNotes[i].useAutoPosition)
            {
                containerNotes[i].position = i * spacing;
            }
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("[CitySequencer] Timeline asset is null!");
            return;
        }

        // Zapisz aktualny czas timeline
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

        // Add new clips only from the assigned noteContainer
        var allNotes = noteContainer.GetAllNotes();
        
        int totalClipsCreated = 0;
        foreach (var note in allNotes)
        {
            if (note == null)
            {
                Debug.LogError("[CitySequencer] Found null note in container!");
                continue;
            }

            // Get all positions for repeated notes
            Vector3[] positions = note.GetRepeatPositions();
            
            foreach (Vector3 pos in positions)
            {
                var clip = pianoRollTrack.CreateClip();
                if (clip == null || clip.asset == null)
                {
                    Debug.LogError("[CitySequencer] Failed to create clip or clip asset is null");
                    continue;
                }
                
                var samplerClip = clip.asset as SamplerPianoRollClip;
                if (samplerClip == null)
                {
                    Debug.LogError("[CitySequencer] Failed to cast clip asset to SamplerPianoRollClip");
                    continue;
                }

                float timePosition = WorldToTimelinePosition(pos);
                
                // Używamy pełnej długości timeline'a do rozmieszczania nut
                clip.start = timePosition;
                clip.duration = note.duration;
                samplerClip.midiNote = note.pitch;
                samplerClip.duration = note.duration;
                samplerClip.startTime = timePosition;
                samplerClip.velocity = note.velocity;
                clip.displayName = $"Note {note.pitch} at {timePosition:F2}s";
                totalClipsCreated++;
            }
        }

        timeline.RebuildGraph();
        
        // Przywróć czas timeline
        timeline.time = currentTime;
    }

    // Call this method whenever you want to mark the sequence for update
    public void MarkSequenceForUpdate()
    {
        Debug.Log("[CitySequencer] Sequence marked for update!");
        sequenceNeedsUpdate = true;
    }

    private void OnDrawGizmos()
    {
        // Draw grid if snapping is enabled
        if (snapToGrid)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
            float beatSize = timelineLength * beatFraction;
            int gridCount = Mathf.CeilToInt(timelineLength / beatSize);
            
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
        timelineLength = length;
        
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
        timelineAsset.fixedDuration = timelineLength;
        
        UpdateSequence();
    }
} 