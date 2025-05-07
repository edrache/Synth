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
            if (note != null)
            {
                lastPositions[note] = note.position;
                lastRepeatCounts[note] = note.repeatCount;
                lastPitches[note] = note.pitch;
                lastVelocities[note] = note.velocity;
                lastDurations[note] = note.duration;
            }
        }

        // Set initial indices
        UpdateNoteIndices();
    }

    private void UpdateNoteIndices()
    {
        if (notes == null)
        {
            Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        // Sort notes by their current positions
        notes.Sort((a, b) => a.position.CompareTo(b.position));
        
        // Calculate spacing for auto-positioned notes
        float spacing = noteDistributionTime / Mathf.Max(1, notes.Count);
        
        for (int i = 0; i < notes.Count; i++)
        {
            var note = notes[i];
            if (note != null)
            {
                note.SetIndex(i);
                
                // Update position only if auto-positioning is enabled
                if (note.useAutoPosition)
                {
                    float newPosition = i * spacing;
                    note.position = newPosition;
                }
            }
        }
        
        OnNotesChanged?.Invoke();
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
        UpdateNoteIndices();
    }

    public void RemoveNote(CityNote note)
    {
        if (note == null)
        {
            Debug.LogError("[CityNoteContainer] Cannot remove null note!");
            return;
        }

        if (notes == null)
        {
            Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        notes.Remove(note);
        UpdateNoteIndices();
    }

    public List<CityNote> GetAllNotes()
    {
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

    public void ForceUpdate()
    {
        if (notes == null)
        {
            Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        bool hasChanges = false;
        int previousCount = notes.Count;
        
        // Check for new notes
        foreach (var note in notes)
        {
            if (note != null && !lastPositions.ContainsKey(note))
            {
                hasChanges = true;
                break;
            }
        }
        
        // Check for removed notes
        if (notes.Count != previousCount)
        {
            hasChanges = true;
        }
        
        if (hasChanges)
        {
            UpdateNoteIndices();
        }
    }
} 