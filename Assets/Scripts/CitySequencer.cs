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
    [SerializeField] private float beatFraction = 0.25f;

    [Header("Position Mapping")]
    [SerializeField] private float worldScale = 1f; // How many world units per beat
    [SerializeField] private bool snapToGrid = false; // Wyłączone domyślnie

    private float lastTimelineTime = 0f;
    private bool sequenceNeedsUpdate = false;

    public float GetTimelineLength()
    {
        return timelineLength;
    }

    private void Start()
    {
        Debug.Log("[CitySequencer] Starting initialization...");
        ValidateConfiguration();
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
            Debug.LogError("[CitySequencer] NoteContainer reference is missing!");
            return;
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("[CitySequencer] Timeline asset is null!");
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

        Debug.Log("[CitySequencer] Configuration validated successfully");
        Debug.Log($"[CitySequencer] Timeline: {timeline.gameObject.name}");
        Debug.Log($"[CitySequencer] NoteContainer: {noteContainer.gameObject.name}");
        Debug.Log($"[CitySequencer] Track index: {trackIndex}");
        Debug.Log($"[CitySequencer] Timeline length: {timelineLength}");
        Debug.Log($"[CitySequencer] Beat fraction: {beatFraction}");
        Debug.Log($"[CitySequencer] World scale: {worldScale}");
        Debug.Log($"[CitySequencer] Snap to grid: {snapToGrid}");

        UpdateSequence();
        
        if (timeline != null)
        {
            lastTimelineTime = (float)timeline.time;
            Debug.Log($"[CitySequencer] Initial timeline time: {lastTimelineTime}");
        }
    }

    private void Update()
    {
        if (timeline == null)
        {
            Debug.LogError("[CitySequencer] Timeline is null in Update!");
            return;
        }

        float currentTime = (float)timeline.time;
        float nextFrameTime = currentTime + Time.deltaTime;
        
        // Check if Timeline will reach the end in the next frame
        if (nextFrameTime >= timelineLength && currentTime < timelineLength)
        {
            Debug.Log($"[CitySequencer] Timeline will reach end in next frame! Current: {currentTime}, Next: {nextFrameTime}, Length: {timelineLength}");
            if (sequenceNeedsUpdate)
            {
                Debug.Log("[CitySequencer] Updating sequence before timeline end...");
                UpdateSequence();
                sequenceNeedsUpdate = false;
            }
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

        return Mathf.Clamp(timePosition, 0f, timelineLength);
    }

    public void UpdateSequence()
    {
        Debug.Log("[CitySequencer] UpdateSequence called");
        
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

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("[CitySequencer] Timeline asset is null!");
            return;
        }

        // Zapisz aktualny czas timeline
        double currentTime = timeline.time;
        Debug.Log($"[CitySequencer] Current timeline time before update: {currentTime}");

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
        Debug.Log($"[CitySequencer] Removing {existingClips.Count} existing clips");
        foreach (var clip in existingClips)
        {
            if (clip != null)
            {
                pianoRollTrack.DeleteClip(clip);
            }
        }

        // Add new clips from CityNotes
        var allNotes = noteContainer.GetAllNotes();
        Debug.Log($"[CitySequencer] Processing {allNotes.Count} notes");
        
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
            Debug.Log($"[CitySequencer] Processing note with {positions.Length} repeat positions");
            
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
                Debug.Log($"[CitySequencer] Creating clip at time {timePosition} for note {note.pitch}");
                
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

        Debug.Log($"[CitySequencer] Created {totalClipsCreated} total clips");
        timeline.RebuildGraph();
        
        // Przywróć czas timeline
        timeline.time = currentTime;
        Debug.Log($"[CitySequencer] Timeline time restored to: {currentTime}");
        
        Debug.Log($"[CitySequencer] Sequence updated successfully!");
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
} 