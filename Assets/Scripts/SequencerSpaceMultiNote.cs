using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

public class SequencerSpaceMultiNote : MonoBehaviour
{
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector timeline;
    [Tooltip("Track index in Timeline (starting from 0)")]
    [SerializeField] private int trackIndex = 0;

    [Header("Space Settings")]
    public float timelineLength = 4f;
    [SerializeField] private int baseMidiNote = 60;
    [SerializeField] private float semitonesPerUnit = 1f;
    [SerializeField] private LayerMask objectsLayer;

    [Header("Note Mapping Settings")]
    public MusicalScale scale = MusicalScale.Major;
    [SerializeField] private NoteName rootNote = NoteName.C;
    public int minOctave = 3;
    public int maxOctave = 5;

    [Header("Multi-Note Settings")]
    [Tooltip("How many notes to place per object on the beat")]
    public int notesPerBeat = 4;
    [Tooltip("Gap between notes (in seconds)")]
    public float gapBetweenNotes = 0.05f;
    [Tooltip("Fraction of timelineLength that is considered one beat (e.g. 0.25 for 1/4)")]
    public float beatFraction = 0.25f;

    private static readonly int[][] scaleIntervals = new int[][]
    {
        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, // Chromatic
        new int[] { 0, 2, 4, 5, 7, 9, 11 }, // Major
        new int[] { 0, 2, 3, 5, 7, 8, 10 }, // Minor
        new int[] { 0, 2, 4, 7, 9 }, // Pentatonic Major
        new int[] { 0, 3, 5, 7, 10 } // Pentatonic Minor
    };

    private List<GameObject> trackedObjects = new List<GameObject>();
    private int lastObjectCount = 0;
    private float lastTimelineTime = 0f;
    private bool sequenceNeedsUpdate = false;

    private void Start()
    {
        UpdateTrackedObjects();
        UpdateSequence();
        if (timeline != null)
            lastTimelineTime = (float)timeline.time;
    }

    private void Update()
    {
        UpdateTrackedObjects();
        float currentTime = timeline != null ? (float)timeline.time : 0f;
        // Check if Timeline has looped (reached the end and started over)
        if (currentTime < lastTimelineTime)
        {
            if (sequenceNeedsUpdate)
            {
                Debug.Log("[SequencerSpaceMultiNote] Timeline reached end, updating sequence.");
                UpdateSequence();
                sequenceNeedsUpdate = false;
            }
        }
        lastTimelineTime = currentTime;

        // If number of objects changed, mark for update at end of timeline
        if (trackedObjects.Count != lastObjectCount)
        {
            Debug.Log($"[SequencerSpaceMultiNote] Zmiana liczby obiektów: {lastObjectCount} -> {trackedObjects.Count}, update przy końcu timeline.");
            sequenceNeedsUpdate = true;
            lastObjectCount = trackedObjects.Count;
        }
    }

    private void UpdateTrackedObjects()
    {
        trackedObjects.Clear();
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        Vector3 boxCenter = boxCollider.transform.TransformPoint(boxCollider.center);
        Vector3 boxSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);

        Collider[] colliders = Physics.OverlapBox(
            boxCenter,
            boxSize * 0.5f,
            boxCollider.transform.rotation,
            objectsLayer
        );

        foreach (var collider in colliders)
        {
            if (collider.attachedRigidbody != null)
            {
                trackedObjects.Add(collider.gameObject);
            }
        }
    }

    public void UpdateSequence()
    {
        if (timeline == null) return;

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null) return;

        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        if (trackIndex >= pianoRollTracks.Count)
        {
            Debug.LogError($"Track index {trackIndex} out of range!");
            return;
        }

        var track = pianoRollTracks[trackIndex];
        var pianoRollTrack = track as IPianoRollTrack;

        // Remove existing clips
        foreach (var clip in track.GetClips().ToList())
            pianoRollTrack.DeleteClip(clip);

        // Find all objects in the sequencer space
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        Vector3 boxCenter = boxCollider.transform.TransformPoint(boxCollider.center);
        Vector3 boxSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);

        Collider[] colliders = Physics.OverlapBox(
            boxCenter,
            boxSize * 0.5f,
            boxCollider.transform.rotation,
            objectsLayer
        );

        foreach (var collider in colliders)
        {
            if (collider.attachedRigidbody == null) continue;
            GameObject obj = collider.gameObject;
            Vector3 pos = obj.transform.position - boxCenter;
            float width = boxCollider.size.x;
            float timePosition = (pos.x / width) * timelineLength + (timelineLength / 2f);
            int midiNote = GetNearestScaleNote(baseMidiNote, pos.z * semitonesPerUnit);

            // Snap start time to the nearest lower 0.5 (including negatives)
            float snappedTime = Mathf.Floor(timePosition / 0.5f) * 0.5f;

            // Calculate total available time for notes (on this beat)
            float beatLength = timelineLength * beatFraction;
            float totalGaps = (notesPerBeat) * gapBetweenNotes;
            float noteLength = (beatLength - totalGaps) / notesPerBeat;

            float currentTime = snappedTime;
            for (int i = 0; i < notesPerBeat; i++)
            {
                var clip = pianoRollTrack.CreateClip();
                if (clip == null || clip.asset == null) continue;
                var samplerClip = clip.asset as SamplerPianoRollClip;
                if (samplerClip == null) continue;

                clip.start = currentTime;
                clip.duration = noteLength;
                samplerClip.midiNote = midiNote;
                samplerClip.duration = noteLength;
                samplerClip.startTime = currentTime;
                samplerClip.velocity = 0.8f;
                clip.displayName = $"Note {midiNote} at {currentTime:F2}s";

                currentTime += noteLength + gapBetweenNotes;
            }
        }

        timeline.RebuildGraph();
    }

    private int GetNearestScaleNote(int midiBase, float z)
    {
        int[] intervals = scaleIntervals[(int)scale];
        int notesPerOctave = intervals.Length;
        int totalNotes = (maxOctave - minOctave + 1) * notesPerOctave;
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return midiBase;
        float zSize = boxCollider.size.z;
        float zMin = -zSize / 2f;
        float zMax = zSize / 2f;
        float zNorm = Mathf.Clamp((z - zMin) / (zMax - zMin), 0f, 0.9999f);
        int noteIndex = Mathf.FloorToInt(zNorm * totalNotes);
        noteIndex = Mathf.Clamp(noteIndex, 0, totalNotes - 1);
        int octave = minOctave + (noteIndex / notesPerOctave);
        int interval = intervals[noteIndex % notesPerOctave];
        int root = (int)rootNote;
        return (octave + 1) * 12 + root + interval;
    }

    private void OnDrawGizmos()
    {
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        Gizmos.color = Color.cyan;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        Gizmos.matrix = oldMatrix;

        // Draw time markers
        Gizmos.color = Color.yellow;
        for (int i = 0; i <= 4; i++)
        {
            float x = (i / 4f) * boxCollider.size.x - (boxCollider.size.x / 2f);
            Vector3 basePos = boxCollider.transform.TransformPoint(boxCollider.center);
            Gizmos.DrawLine(
                basePos + new Vector3(x, 0, -boxCollider.size.z / 2f),
                basePos + new Vector3(x, 0, boxCollider.size.z / 2f)
            );
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                basePos + new Vector3(x, 0.5f, 0),
                $"{(i * timelineLength / 4f):F2}s"
            );
#endif
        }

        // Draw note markers
        Gizmos.color = Color.green;
        int[] intervals = scaleIntervals[(int)scale];
        int notesPerOctave = intervals.Length;
        int totalNotes = (maxOctave - minOctave + 1) * notesPerOctave;
        float zStep = boxCollider.size.z / (float)totalNotes;
        Vector3 basePosZ = boxCollider.transform.TransformPoint(boxCollider.center);
        for (int i = 0; i < totalNotes; i++)
        {
            int octave = minOctave + (i / notesPerOctave);
            int interval = intervals[i % notesPerOctave];
            int root = (int)rootNote;
            int midiNote = (octave + 1) * 12 + root + interval;
            float z = -boxCollider.size.z / 2f + i * zStep;
            Gizmos.DrawLine(
                basePosZ + new Vector3(-boxCollider.size.x / 2f, 0, z),
                basePosZ + new Vector3(boxCollider.size.x / 2f, 0, z)
            );
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                basePosZ + new Vector3(-boxCollider.size.x / 2f - 0.5f, 0.5f, z),
                GetNoteName(midiNote)
            );
#endif
        }
    }

    private string GetNoteName(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{noteNames[noteIndex]}{octave}";
    }
} 