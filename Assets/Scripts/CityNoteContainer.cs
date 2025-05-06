using UnityEngine;
using System.Collections.Generic;

public class CityNoteContainer : MonoBehaviour
{
    [Header("Update Settings")]
    [Tooltip("If true, sequence updates immediately when notes change. If false, updates at the end of timeline loop.")]
    [SerializeField] private bool updateImmediately = true;
    
    [Header("Note Positioning")]
    [Tooltip("If true, note positions will be automatically recalculated when their order changes.")]
    [SerializeField] private bool autoRecalculatePositions = true;

    [SerializeField]
    private List<CityNote> notes = new List<CityNote>();
    private CitySequencer sequencer;
    private Dictionary<CityNote, float> lastPositions = new Dictionary<CityNote, float>();
    private Dictionary<CityNote, int> lastRepeatCounts = new Dictionary<CityNote, int>();
    private Dictionary<CityNote, int> lastPitches = new Dictionary<CityNote, int>();
    private Dictionary<CityNote, float> lastVelocities = new Dictionary<CityNote, float>();
    private Dictionary<CityNote, float> lastDurations = new Dictionary<CityNote, float>();
    private bool forceUpdate = false;
    private int lastNoteCount = 0;

    private void Start()
    {
        ValidateSequencerConnection();
        Debug.Log($"[CityNoteContainer] Initial note count: {notes.Count}");
        lastNoteCount = notes.Count;
        
        // Initialize all note properties
        foreach (var note in notes)
        {
            lastPositions[note] = note.position;
            lastRepeatCounts[note] = note.repeatCount;
            lastPitches[note] = note.pitch;
            lastVelocities[note] = note.velocity;
            lastDurations[note] = note.duration;
        }

        // Set initial indices
        UpdateNoteIndices();
    }

    private void ValidateSequencerConnection()
    {
        if (sequencer == null)
        {
            sequencer = FindObjectOfType<CitySequencer>();
            if (sequencer == null)
            {
                Debug.LogError("[CityNoteContainer] Could not find CitySequencer in scene!");
            }
            else
            {
                Debug.Log("[CityNoteContainer] Successfully connected to CitySequencer");
            }
        }

        if (sequencer != null)
        {
            Debug.Log($"[CityNoteContainer] Sequencer found: {sequencer.gameObject.name}");
            Debug.Log($"[CityNoteContainer] Update immediately: {updateImmediately}");
        }
    }

    private void UpdateNoteIndices()
    {
        Debug.Log($"[CityNoteContainer] Updating indices for {notes.Count} notes");
        
        if (notes.Count == 0) return;

        // Get timeline length from sequencer
        float timelineLength = sequencer.GetTimelineLength();
        float spacing = timelineLength / notes.Count;
        
        Debug.Log($"[CityNoteContainer] Timeline length: {timelineLength}, Spacing: {spacing}");

        // Set first note to position 0
        notes[0].position = 0;
        notes[0].SetIndex(0);
        Debug.Log($"[CityNoteContainer] Setting index 0 for note at position 0");

        // Set positions for remaining notes
        for (int i = 1; i < notes.Count; i++)
        {
            float newPosition = notes[i-1].position + spacing;
            notes[i].position = newPosition;
            notes[i].SetIndex(i);
            Debug.Log($"[CityNoteContainer] Setting index {i} for note at position {newPosition}");
        }
    }

    private void Update()
    {
        // Validate connection every frame
        if (sequencer == null)
        {
            ValidateSequencerConnection();
            return;
        }

        bool positionsChanged = false;
        bool notesChanged = false;
        bool repeatCountsChanged = false;
        bool pitchesChanged = false;
        bool velocitiesChanged = false;
        bool durationsChanged = false;

        // Check if note count changed
        if (notes.Count != lastNoteCount)
        {
            Debug.Log($"[CityNoteContainer] Note count changed from {lastNoteCount} to {notes.Count}");
            notesChanged = true;
            lastNoteCount = notes.Count;
            UpdateNoteIndices(); // Update indices when note count changes
        }

        // Check for all note property changes
        foreach (var note in notes)
        {
            // Check position changes
            if (lastPositions.TryGetValue(note, out float lastPos))
            {
                if (lastPos != note.position)
                {
                    Debug.Log($"[CityNoteContainer] Note position changed from {lastPos} to {note.position}");
                    positionsChanged = true;
                    lastPositions[note] = note.position;
                }
            }
            else
            {
                Debug.Log($"[CityNoteContainer] New note position tracked: {note.position}");
                lastPositions[note] = note.position;
                positionsChanged = true;
            }

            // Check repeat count changes
            if (lastRepeatCounts.TryGetValue(note, out int lastRepeatCount))
            {
                if (lastRepeatCount != note.repeatCount)
                {
                    Debug.Log($"[CityNoteContainer] Note repeat count changed from {lastRepeatCount} to {note.repeatCount}");
                    repeatCountsChanged = true;
                    lastRepeatCounts[note] = note.repeatCount;
                }
            }
            else
            {
                Debug.Log($"[CityNoteContainer] New note repeat count tracked: {note.repeatCount}");
                lastRepeatCounts[note] = note.repeatCount;
                repeatCountsChanged = true;
            }

            // Check pitch changes
            if (lastPitches.TryGetValue(note, out int lastPitch))
            {
                if (lastPitch != note.pitch)
                {
                    Debug.Log($"[CityNoteContainer] Note pitch changed from {lastPitch} to {note.pitch}");
                    pitchesChanged = true;
                    lastPitches[note] = note.pitch;
                }
            }
            else
            {
                Debug.Log($"[CityNoteContainer] New note pitch tracked: {note.pitch}");
                lastPitches[note] = note.pitch;
                pitchesChanged = true;
            }

            // Check velocity changes
            if (lastVelocities.TryGetValue(note, out float lastVelocity))
            {
                if (lastVelocity != note.velocity)
                {
                    Debug.Log($"[CityNoteContainer] Note velocity changed from {lastVelocity} to {note.velocity}");
                    velocitiesChanged = true;
                    lastVelocities[note] = note.velocity;
                }
            }
            else
            {
                Debug.Log($"[CityNoteContainer] New note velocity tracked: {note.velocity}");
                lastVelocities[note] = note.velocity;
                velocitiesChanged = true;
            }

            // Check duration changes
            if (lastDurations.TryGetValue(note, out float lastDuration))
            {
                if (lastDuration != note.duration)
                {
                    Debug.Log($"[CityNoteContainer] Note duration changed from {lastDuration} to {note.duration}");
                    durationsChanged = true;
                    lastDurations[note] = note.duration;
                }
            }
            else
            {
                Debug.Log($"[CityNoteContainer] New note duration tracked: {note.duration}");
                lastDurations[note] = note.duration;
                durationsChanged = true;
            }
        }

        // Update sequence if anything changed
        if ((positionsChanged || notesChanged || repeatCountsChanged || pitchesChanged || 
             velocitiesChanged || durationsChanged || forceUpdate) && sequencer != null)
        {
            Debug.Log($"[CityNoteContainer] Updating sequence due to: positions={positionsChanged}, " +
                     $"notes={notesChanged}, repeats={repeatCountsChanged}, pitches={pitchesChanged}, " +
                     $"velocities={velocitiesChanged}, durations={durationsChanged}, force={forceUpdate}");
            if (updateImmediately)
            {
                Debug.Log("[CityNoteContainer] Updating sequence immediately");
                sequencer.UpdateSequence();
            }
            else
            {
                Debug.Log("[CityNoteContainer] Marking sequence for update at end of loop");
                sequencer.MarkSequenceForUpdate();
            }
            forceUpdate = false;
        }

        // Log current state every second
        if (Time.frameCount % 60 == 0) // Assuming 60 FPS
        {
            Debug.Log($"[CityNoteContainer] Current note count: {notes.Count}");
            foreach (var note in notes)
            {
                Vector3[] positions = note.GetRepeatPositions();
                Debug.Log($"[CityNoteContainer] Note at position: {note.position}, pitch: {note.pitch}, " +
                         $"repeat count: {note.repeatCount}, velocity: {note.velocity}, duration: {note.duration}, " +
                         $"total positions: {positions.Length}");
                foreach (var pos in positions)
                {
                    Debug.Log($"[CityNoteContainer] - Repeat position: {pos}");
                }
            }
        }
    }

    public void AddNote(CityNote note)
    {
        if (note == null)
        {
            Debug.LogError("[CityNoteContainer] Attempted to add null note!");
            return;
        }

        // Upewnij się, że mamy sequencer przed dodaniem nuty
        if (sequencer == null)
        {
            ValidateSequencerConnection();
            if (sequencer == null)
            {
                Debug.LogError("[CityNoteContainer] Cannot add note - sequencer is still null!");
                return;
            }
        }

        // Calculate initial position based on current number of notes
        float timelineLength = sequencer.GetTimelineLength();
        float spacing = timelineLength / (notes.Count + 1);
        float initialPosition = notes.Count * spacing;
        note.position = initialPosition;

        Debug.Log($"[CityNoteContainer] Adding note at calculated position {initialPosition}");
        notes.Add(note);
        lastPositions[note] = note.position;
        lastRepeatCounts[note] = note.repeatCount;
        lastPitches[note] = note.pitch;
        lastVelocities[note] = note.velocity;
        lastDurations[note] = note.duration;
        
        // Update indices after adding a note
        UpdateNoteIndices();
        
        Debug.Log($"[CityNoteContainer] After adding note, positions are:");
        for (int i = 0; i < notes.Count; i++)
        {
            Debug.Log($"[CityNoteContainer] Note {i}: position = {notes[i].position}");
        }
        
        if (sequencer != null)
        {
            Debug.Log("[CityNoteContainer] Calling sequence update");
            if (updateImmediately)
            {
                Debug.Log("[CityNoteContainer] Updating sequence immediately");
                sequencer.UpdateSequence();
            }
            else
            {
                Debug.Log("[CityNoteContainer] Marking sequence for update at end of loop");
                sequencer.MarkSequenceForUpdate();
            }
        }
        else
        {
            Debug.LogWarning("[CityNoteContainer] Cannot update sequence - sequencer is null!");
        }
    }

    public void RemoveNote(CityNote note)
    {
        if (note == null)
        {
            Debug.LogError("[CityNoteContainer] Attempted to remove null note!");
            return;
        }

        bool removed = notes.Remove(note);
        lastPositions.Remove(note);
        lastRepeatCounts.Remove(note);
        lastPitches.Remove(note);
        lastVelocities.Remove(note);
        lastDurations.Remove(note);
        Debug.Log($"[CityNoteContainer] Removed note {(removed ? "successfully" : "failed to remove")}, total notes: {notes.Count}");
        
        // Update indices after removing a note
        UpdateNoteIndices();
        
        if (sequencer != null)
        {
            Debug.Log("[CityNoteContainer] Calling sequence update");
            if (updateImmediately)
            {
                Debug.Log("[CityNoteContainer] Updating sequence immediately");
                sequencer.UpdateSequence();
            }
            else
            {
                Debug.Log("[CityNoteContainer] Marking sequence for update at end of loop");
                sequencer.MarkSequenceForUpdate();
            }
        }
        else
        {
            Debug.LogWarning("[CityNoteContainer] Cannot update sequence - sequencer is null!");
        }
    }

    public List<CityNote> GetAllNotes()
    {
        Debug.Log($"[CityNoteContainer] Getting all notes, count: {notes.Count}");
        return new List<CityNote>(notes);
    }

    public void ClearNotes()
    {
        int count = notes.Count;
        notes.Clear();
        lastPositions.Clear();
        lastRepeatCounts.Clear();
        lastPitches.Clear();
        lastVelocities.Clear();
        lastDurations.Clear();
        Debug.Log($"[CityNoteContainer] Cleared {count} notes");
        
        if (sequencer != null)
        {
            Debug.Log("[CityNoteContainer] Calling sequence update");
            if (updateImmediately)
            {
                Debug.Log("[CityNoteContainer] Updating sequence immediately");
                sequencer.UpdateSequence();
            }
            else
            {
                Debug.Log("[CityNoteContainer] Marking sequence for update at end of loop");
                sequencer.MarkSequenceForUpdate();
            }
        }
        else
        {
            Debug.LogWarning("[CityNoteContainer] Cannot update sequence - sequencer is null!");
        }
    }

    // Call this method when you want to force an update of the sequence
    public void ForceUpdate()
    {
        Debug.Log("[CityNoteContainer] Force update requested");
        forceUpdate = true;
    }

    public void MoveNoteUp(CityNote note)
    {
        int index = notes.IndexOf(note);
        if (index > 0)
        {
            notes.RemoveAt(index);
            notes.Insert(index - 1, note);
            UpdateNoteIndices();
            if (autoRecalculatePositions)
            {
                ForceUpdate();
            }
            Debug.Log($"[CityNoteContainer] Moved note up from index {index} to {index - 1}");
        }
    }

    public void MoveNoteDown(CityNote note)
    {
        int index = notes.IndexOf(note);
        if (index >= 0 && index < notes.Count - 1)
        {
            notes.RemoveAt(index);
            notes.Insert(index + 1, note);
            UpdateNoteIndices();
            if (autoRecalculatePositions)
            {
                ForceUpdate();
            }
            Debug.Log($"[CityNoteContainer] Moved note down from index {index} to {index + 1}");
        }
    }

    public void MoveNoteToIndex(CityNote note, int newIndex)
    {
        int currentIndex = notes.IndexOf(note);
        if (currentIndex >= 0 && newIndex >= 0 && newIndex < notes.Count)
        {
            notes.RemoveAt(currentIndex);
            notes.Insert(newIndex, note);
            UpdateNoteIndices();
            if (autoRecalculatePositions)
            {
                ForceUpdate();
            }
            Debug.Log($"[CityNoteContainer] Moved note from index {currentIndex} to {newIndex}");
        }
    }

    [ContextMenu("Recalculate Note Positions")]
    public void RecalculateNotePositions()
    {
        Debug.Log("[CityNoteContainer] Recalculating note positions");
        UpdateNoteIndices();
        ForceUpdate();
    }

    [ContextMenu("Sort Notes By Position")]
    public void SortNotesByPosition()
    {
        Debug.Log("[CityNoteContainer] Sorting notes by position");
        notes.Sort((a, b) =>
        {
            // Najpierw porównaj Z (malejąco - najwyższe Z pierwsze)
            int zComparison = b.transform.position.z.CompareTo(a.transform.position.z);
            if (zComparison != 0)
                return zComparison;
            
            // Jeśli Z jest takie samo, porównaj X (rosnąco - najniższe X pierwsze)
            return a.transform.position.x.CompareTo(b.transform.position.x);
        });

        // Po posortowaniu zaktualizuj indeksy i pozycje
        UpdateNoteIndices();
        ForceUpdate();
        
        Debug.Log("[CityNoteContainer] Notes sorted. New order:");
        for (int i = 0; i < notes.Count; i++)
        {
            Debug.Log($"[CityNoteContainer] Note {i}: Z={notes[i].transform.position.z}, X={notes[i].transform.position.x}");
        }
    }
} 