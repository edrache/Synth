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

    [Header("Note Distribution")]
    [Tooltip("Time in seconds over which to distribute the notes.")]
    [SerializeField] private float noteDistributionTime = 4f;

    [SerializeField]
    private List<CityNote> notes = new List<CityNote>();
    private Dictionary<CityNote, float> lastPositions = new Dictionary<CityNote, float>();
    private Dictionary<CityNote, int> lastRepeatCounts = new Dictionary<CityNote, int>();
    private Dictionary<CityNote, int> lastPitches = new Dictionary<CityNote, int>();
    private Dictionary<CityNote, float> lastVelocities = new Dictionary<CityNote, float>();
    private Dictionary<CityNote, float> lastDurations = new Dictionary<CityNote, float>();
    private bool forceUpdate = false;
    private int lastNoteCount = 0;
    private CitySequencer sequencer;

    // Event that will be called when notes change
    public delegate void NotesChangedHandler();
    public event NotesChangedHandler OnNotesChanged;

    private void Start()
    {
        Debug.Log("[CityNoteContainer] Starting initialization...");
        Debug.Log($"[CityNoteContainer] Note distribution time: {noteDistributionTime}");
        Debug.Log($"[CityNoteContainer] Auto recalculate positions: {autoRecalculatePositions}");
        Debug.Log($"[CityNoteContainer] Update immediately: {updateImmediately}");
        
        if (notes == null)
        {
            Debug.LogError("[CityNoteContainer] Notes list is null! Initializing empty list.");
            notes = new List<CityNote>();
        }
        
        // Find sequencer reference
        sequencer = FindObjectOfType<CitySequencer>();
        if (sequencer != null)
        {
            Debug.Log($"[CityNoteContainer] Found sequencer: {sequencer.name}");
            Debug.Log($"[CityNoteContainer] Sequencer timeline length: {sequencer.GetTimelineLength()}");
        }
        else
        {
            Debug.LogWarning("[CityNoteContainer] No sequencer found in scene!");
        }
        
        // Initialize dictionaries
        lastPositions = new Dictionary<CityNote, float>();
        lastRepeatCounts = new Dictionary<CityNote, int>();
        lastPitches = new Dictionary<CityNote, int>();
        lastVelocities = new Dictionary<CityNote, float>();
        lastDurations = new Dictionary<CityNote, float>();
        
        Debug.Log("[CityNoteContainer] Initialization completed");
        
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

    private void UpdateNoteIndices()
    {
        Debug.Log($"[CityNoteContainer] Updating note indices. Total notes: {notes.Count}, Distribution time: {noteDistributionTime}");
        
        if (notes == null || notes.Count == 0)
        {
            Debug.Log("[CityNoteContainer] No notes to update indices for");
            return;
        }

        // Sort notes by their current positions
        notes.Sort((a, b) => a.position.CompareTo(b.position));

        // Update indices and positions
        for (int i = 0; i < notes.Count; i++)
        {
            var note = notes[i];
            if (note == null)
            {
                Debug.LogError($"[CityNoteContainer] Null note found at index {i}");
                continue;
            }

            note.SetIndex(i);
            
            if (autoRecalculatePositions)
            {
                float spacing = noteDistributionTime / notes.Count;
                float newPosition = i * spacing;
                
                Debug.Log($"[CityNoteContainer] Note {i}: Old position={note.position}, New position={newPosition}, Spacing={spacing}");
                
                if (note.position != newPosition)
                {
                    note.position = newPosition;
                }
            }
        }

        // Notify about changes
        if (OnNotesChanged != null)
        {
            Debug.Log("[CityNoteContainer] Notifying about notes changed");
            OnNotesChanged.Invoke();
        }
    }

    private void Update()
    {
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
            UpdateNoteIndices();
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

        // Notify listeners if anything changed
        if (positionsChanged || notesChanged || repeatCountsChanged || pitchesChanged || 
            velocitiesChanged || durationsChanged || forceUpdate)
        {
            Debug.Log($"[CityNoteContainer] Notes changed: positions={positionsChanged}, " +
                     $"notes={notesChanged}, repeats={repeatCountsChanged}, pitches={pitchesChanged}, " +
                     $"velocities={velocitiesChanged}, durations={durationsChanged}, force={forceUpdate}");
            OnNotesChanged?.Invoke();
            forceUpdate = false;
        }
    }

    public void AddNote(CityNote note)
    {
        Debug.Log($"[CityNoteContainer] Adding note: pitch={note.pitch}, position={note.position}, duration={note.duration}, velocity={note.velocity}");
        
        if (note == null)
        {
            Debug.LogError("[CityNoteContainer] Cannot add null note!");
            return;
        }

        if (notes == null)
        {
            Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        notes.Add(note);
        Debug.Log($"[CityNoteContainer] Note added. Total notes: {notes.Count}");
        
        UpdateNoteIndices();
        
        // Notify sequencer
        if (sequencer != null)
        {
            Debug.Log("[CityNoteContainer] Notifying sequencer about note addition");
            sequencer.UpdateSequence();
        }
        else
        {
            Debug.LogWarning("[CityNoteContainer] No sequencer connected to notify about note addition");
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
        
        UpdateNoteIndices();
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
        
        UpdateNoteIndices();
    }

    public void ForceUpdate()
    {
        Debug.Log("[CityNoteContainer] Force update called");
        
        if (notes == null)
        {
            Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        // Check for changes in note properties
        bool hasChanges = false;
        foreach (var note in notes)
        {
            if (note == null)
            {
                Debug.LogError("[CityNoteContainer] Null note found in container!");
                continue;
            }

            if (!lastPositions.ContainsKey(note) || 
                !lastRepeatCounts.ContainsKey(note) ||
                !lastPitches.ContainsKey(note) ||
                !lastVelocities.ContainsKey(note) ||
                !lastDurations.ContainsKey(note))
            {
                Debug.Log($"[CityNoteContainer] New note detected: pitch={note.pitch}, position={note.position}");
                hasChanges = true;
                break;
            }

            if (lastPositions[note] != note.position ||
                lastRepeatCounts[note] != note.repeatCount ||
                lastPitches[note] != note.pitch ||
                lastVelocities[note] != note.velocity ||
                lastDurations[note] != note.duration)
            {
                Debug.Log($"[CityNoteContainer] Note changes detected:");
                Debug.Log($"[CityNoteContainer] - Position: {lastPositions[note]} -> {note.position}");
                Debug.Log($"[CityNoteContainer] - Repeat count: {lastRepeatCounts[note]} -> {note.repeatCount}");
                Debug.Log($"[CityNoteContainer] - Pitch: {lastPitches[note]} -> {note.pitch}");
                Debug.Log($"[CityNoteContainer] - Velocity: {lastVelocities[note]} -> {note.velocity}");
                Debug.Log($"[CityNoteContainer] - Duration: {lastDurations[note]} -> {note.duration}");
                hasChanges = true;
                break;
            }
        }

        // Check if number of notes changed
        if (notes.Count != lastNoteCount)
        {
            Debug.Log($"[CityNoteContainer] Number of notes changed: {lastNoteCount} -> {notes.Count}");
            hasChanges = true;
        }

        if (hasChanges || forceUpdate)
        {
            Debug.Log("[CityNoteContainer] Changes detected, updating note indices and notifying listeners");
            
            // Update note indices
            UpdateNoteIndices();

            // Update last known values
            foreach (var note in notes)
            {
                if (note == null) continue;
                
                lastPositions[note] = note.position;
                lastRepeatCounts[note] = note.repeatCount;
                lastPitches[note] = note.pitch;
                lastVelocities[note] = note.velocity;
                lastDurations[note] = note.duration;
            }
            lastNoteCount = notes.Count;
            forceUpdate = false;

            // Notify listeners if immediate updates are enabled
            if (updateImmediately && OnNotesChanged != null)
            {
                Debug.Log("[CityNoteContainer] Immediate update enabled, notifying listeners");
                OnNotesChanged.Invoke();
            }
        }
        else
        {
            Debug.Log("[CityNoteContainer] No changes detected");
        }
    }

    public float GetNoteDistributionTime()
    {
        return noteDistributionTime;
    }

    public void SetNoteDistributionTime(float time)
    {
        Debug.Log($"[CityNoteContainer] Setting note distribution time to {time}");
        
        if (time <= 0)
        {
            Debug.LogError("[CityNoteContainer] Cannot set note distribution time to zero or negative value!");
            return;
        }
        
        noteDistributionTime = time;
        Debug.Log($"[CityNoteContainer] Note distribution time updated to {noteDistributionTime}");
        
        // Update note positions if auto-recalculate is enabled
        if (autoRecalculatePositions)
        {
            Debug.Log("[CityNoteContainer] Auto-recalculate enabled, updating note positions");
            UpdateNoteIndices();
        }
        else
        {
            Debug.Log("[CityNoteContainer] Auto-recalculate disabled, skipping position update");
        }
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

    public void SetSequencer(CitySequencer newSequencer)
    {
        Debug.Log($"[CityNoteContainer] Setting sequencer reference to: {(newSequencer != null ? newSequencer.name : "null")}");
        sequencer = newSequencer;
    }
} 