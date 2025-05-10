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

    // Property to handle notes list updates
    private List<CityNote> Notes
    {
        get { return notes; }
        set
        {
            if (notes != value)
            {
                notes = value;
                OnNotesChanged?.Invoke();
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }
    }

    // Event that will be called when notes change
    public delegate void NotesChangedHandler();
    public event NotesChangedHandler OnNotesChanged;

    private void Start()
    {
        //Debug.Log("[CityNoteContainer] Starting initialization...");
        //Debug.Log($"[CityNoteContainer] Note distribution time: {noteDistributionTime}");
        //Debug.Log($"[CityNoteContainer] Auto recalculate positions: {autoRecalculatePositions}");
        //Debug.Log($"[CityNoteContainer] Update immediately: {updateImmediately}");
        
        if (notes == null)
        {
            //Debug.LogError("[CityNoteContainer] Notes list is null! Initializing empty list.");
            notes = new List<CityNote>();
        }
        
        // Initialize dictionaries
        lastPositions = new Dictionary<CityNote, float>();
        lastRepeatCounts = new Dictionary<CityNote, int>();
        lastPitches = new Dictionary<CityNote, int>();
        lastVelocities = new Dictionary<CityNote, float>();
        lastDurations = new Dictionary<CityNote, float>();
        
        //Debug.Log("[CityNoteContainer] Initialization completed");
        
        //Debug.Log($"[CityNoteContainer] Initial note count: {notes.Count}");
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

        // Only update indices if auto-recalculate is enabled
        if (autoRecalculatePositions)
        {
            UpdateNoteIndices();
        }
        else
        {
            // Just set indices without changing order
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i] != null)
                {
                    notes[i].SetIndex(i);
                }
            }
        }
    }

    private void UpdateNoteIndices(bool sortByPosition = true)
    {
        if (notes == null)
        {
            //Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        if (sortByPosition)
        {
            // Sort notes by their current positions
            notes.Sort((a, b) => a.position.CompareTo(b.position));
        }
        
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
            //Debug.Log($"[CityNoteContainer] Note count changed from {lastNoteCount} to {notes.Count}");
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
                    //Debug.Log($"[CityNoteContainer] Note position changed from {lastPos} to {note.position}");
                    positionsChanged = true;
                    lastPositions[note] = note.position;
                }
            }
            else
            {
                //Debug.Log($"[CityNoteContainer] New note position tracked: {note.position}");
                lastPositions[note] = note.position;
                positionsChanged = true;
            }

            // Check repeat count changes
            if (lastRepeatCounts.TryGetValue(note, out int lastRepeatCount))
            {
                if (lastRepeatCount != note.repeatCount)
                {
                    //Debug.Log($"[CityNoteContainer] Note repeat count changed from {lastRepeatCount} to {note.repeatCount}");
                    repeatCountsChanged = true;
                    lastRepeatCounts[note] = note.repeatCount;
                }
            }
            else
            {
                //Debug.Log($"[CityNoteContainer] New note repeat count tracked: {note.repeatCount}");
                lastRepeatCounts[note] = note.repeatCount;
                repeatCountsChanged = true;
            }

            // Check pitch changes
            if (lastPitches.TryGetValue(note, out int lastPitch))
            {
                if (lastPitch != note.pitch)
                {
                    //Debug.Log($"[CityNoteContainer] Note pitch changed from {lastPitch} to {note.pitch}");
                    pitchesChanged = true;
                    lastPitches[note] = note.pitch;
                }
            }
            else
            {
                //Debug.Log($"[CityNoteContainer] New note pitch tracked: {note.pitch}");
                lastPitches[note] = note.pitch;
                pitchesChanged = true;
            }

            // Check velocity changes
            if (lastVelocities.TryGetValue(note, out float lastVelocity))
            {
                if (lastVelocity != note.velocity)
                {
                    //Debug.Log($"[CityNoteContainer] Note velocity changed from {lastVelocity} to {note.velocity}");
                    velocitiesChanged = true;
                    lastVelocities[note] = note.velocity;
                }
            }
            else
            {
                //Debug.Log($"[CityNoteContainer] New note velocity tracked: {note.velocity}");
                lastVelocities[note] = note.velocity;
                velocitiesChanged = true;
            }

            // Check duration changes
            if (lastDurations.TryGetValue(note, out float lastDuration))
            {
                if (lastDuration != note.duration)
                {
                    //Debug.Log($"[CityNoteContainer] Note duration changed from {lastDuration} to {note.duration}");
                    durationsChanged = true;
                    lastDurations[note] = note.duration;
                }
            }
            else
            {
                //Debug.Log($"[CityNoteContainer] New note duration tracked: {note.duration}");
                lastDurations[note] = note.duration;
                durationsChanged = true;
            }
        }

        // Notify listeners if anything changed
        if (positionsChanged || notesChanged || repeatCountsChanged || pitchesChanged || 
            velocitiesChanged || durationsChanged || forceUpdate)
        {
            //Debug.Log($"[CityNoteContainer] Notes changed: positions={positionsChanged}, " +
            //         $"notes={notesChanged}, repeats={repeatCountsChanged}, pitches={pitchesChanged}, " +
            //         $"velocities={velocitiesChanged}, durations={durationsChanged}, force={forceUpdate}");
            OnNotesChanged?.Invoke();
            forceUpdate = false;
        }
    }

    public void AddNote(CityNote note)
    {
        if (note == null)
        {
            //Debug.LogError("[CityNoteContainer] Cannot add null note!");
            return;
        }

        if (notes == null)
        {
            //Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        notes.Add(note);
        UpdateNoteIndices();
    }

    public void RemoveNote(CityNote note)
    {
        if (note == null)
        {
            //Debug.LogError("[CityNoteContainer] Cannot remove null note!");
            return;
        }

        if (notes == null)
        {
            //Debug.LogError("[CityNoteContainer] Notes list is null!");
            return;
        }

        notes.Remove(note);
        UpdateNoteIndices();
    }

    public List<CityNote> GetAllNotes()
    {
        // Return a new list with the same order as the original notes list
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
        //Debug.Log($"[CityNoteContainer] Cleared {count} notes");
        
        UpdateNoteIndices();
    }

    public float GetNoteDistributionTime()
    {
        return noteDistributionTime;
    }

    public void SetNoteDistributionTime(float time)
    {
        //Debug.Log($"[CityNoteContainer] Setting note distribution time to {time}");
        
        if (time <= 0)
        {
            //Debug.LogError("[CityNoteContainer] Cannot set note distribution time to zero or negative value!");
            return;
        }
        
        noteDistributionTime = time;
        //Debug.Log($"[CityNoteContainer] Note distribution time updated to {noteDistributionTime}");
        
        // Update note positions if auto-recalculate is enabled
        if (autoRecalculatePositions)
        {
            //Debug.Log("[CityNoteContainer] Auto-recalculate enabled, updating note positions");
            UpdateNoteIndices();
        }
        else
        {
            //Debug.Log("[CityNoteContainer] Auto-recalculate disabled, skipping position update");
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
            //Debug.Log($"[CityNoteContainer] Moved note up from index {index} to {index - 1}");
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
            //Debug.Log($"[CityNoteContainer] Moved note down from index {index} to {index + 1}");
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

    [ContextMenu("Sort Notes By Transform Position")]
    public void SortNotesByTransformPosition()
    {
        if (notes == null || notes.Count == 0)
        {
            //Debug.LogWarning("[CityNoteContainer] No notes to sort!");
            return;
        }

        //Debug.Log("[CityNoteContainer] Starting sort by transform position");
        //Debug.Log("[CityNoteContainer] Initial order:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                Vector3 pos = notes[i].transform.position;
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name} - Z={Mathf.Round(pos.z * 1000f) / 1000f}, X={Mathf.Round(pos.x * 1000f) / 1000f}");
            }
        }
        
        // Sort notes by Z (descending) and then by X (ascending)
        notes.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            
            // Round Z values to handle floating point precision
            float aZ = Mathf.Round(a.transform.position.z * 1000f) / 1000f;
            float bZ = Mathf.Round(b.transform.position.z * 1000f) / 1000f;
            
            // First compare Z (descending - highest Z first)
            int zComparison = bZ.CompareTo(aZ);
            if (zComparison != 0)
            {
                //Debug.Log($"[CityNoteContainer] Comparing Z: {a.gameObject.name}({aZ}) vs {b.gameObject.name}({bZ}) = {zComparison}");
                return zComparison;
            }
            
            // If Z is the same, compare X (ascending - lowest X first)
            float aX = Mathf.Round(a.transform.position.x * 1000f) / 1000f;
            float bX = Mathf.Round(b.transform.position.x * 1000f) / 1000f;
            int xComparison = aX.CompareTo(bX);
            //Debug.Log($"[CityNoteContainer] Z equal, comparing X: {a.gameObject.name}({aX}) vs {b.gameObject.name}({bX}) = {xComparison}");
            return xComparison;
        });

        // Update indices after sorting
        UpdateNoteIndices();
        
        // Log the new order
        //Debug.Log("[CityNoteContainer] Final order after sorting:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                Vector3 pos = notes[i].transform.position;
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name} - Z={Mathf.Round(pos.z * 1000f) / 1000f}, X={Mathf.Round(pos.x * 1000f) / 1000f}");
            }
        }

        // Notify listeners about the change
        OnNotesChanged?.Invoke();
    }

    [ContextMenu("Shift Notes Forward")]
    public void ShiftNotesForward()
    {
        if (notes == null || notes.Count <= 1)
        {
            //Debug.LogWarning("[CityNoteContainer] Not enough notes to shift!");
            return;
        }

        //Debug.Log("[CityNoteContainer] Starting shift forward");
        //Debug.Log("[CityNoteContainer] Initial order:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name}");
            }
        }
        
        // Store the last note
        var lastNote = notes[notes.Count - 1];
        //Debug.Log($"[CityNoteContainer] Stored last note: {lastNote.gameObject.name}");
        
        // Create a new list to store the shifted order
        var newNotes = new List<CityNote>();
        newNotes.Add(lastNote); // Add the last note first
        
        // Add all other notes
        for (int i = 0; i < notes.Count - 1; i++)
        {
            newNotes.Add(notes[i]);
        }
        
        // Update the notes list through the property
        Notes = newNotes;
        
        //Debug.Log("[CityNoteContainer] Final order after shift:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name}");
            }
        }

        // Update indices without sorting by position
        UpdateNoteIndices(false);
    }

    [ContextMenu("Shift First Note Forward By 2")]
    public void ShiftFirstNoteForwardBy2()
    {
        if (notes == null || notes.Count <= 2)
        {
            //Debug.LogWarning("[CityNoteContainer] Not enough notes to shift first note by 2 positions!");
            return;
        }

        //Debug.Log("[CityNoteContainer] Starting shift first note forward by 2");
        //Debug.Log("[CityNoteContainer] Initial order:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name}");
            }
        }
        
        // Store the first note
        var firstNote = notes[0];
        //Debug.Log($"[CityNoteContainer] Stored first note: {firstNote.gameObject.name}");
        
        // Create a new list to store the shifted order
        var newNotes = new List<CityNote>();
        
        // Add all notes except the first one
        for (int i = 1; i < notes.Count; i++)
        {
            newNotes.Add(notes[i]);
        }
        
        // Insert the first note at position 2
        newNotes.Insert(2, firstNote);
        
        // Update the notes list through the property
        Notes = newNotes;
        
        //Debug.Log("[CityNoteContainer] Final order after shift:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name}");
            }
        }

        // Update indices without sorting by position
        UpdateNoteIndices(false);
    }

    [ContextMenu("Sort Notes By 3D Position")]
    public void SortNotesBy3DPosition()
    {
        if (notes == null || notes.Count == 0)
        {
            //Debug.LogWarning("[CityNoteContainer] No notes to sort!");
            return;
        }

        //Debug.Log("[CityNoteContainer] Starting sort by 3D position");
        //Debug.Log("[CityNoteContainer] Initial order:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                Vector3 pos = notes[i].transform.position;
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name} - Z={Mathf.Round(pos.z * 1000f) / 1000f}, X={Mathf.Round(pos.x * 1000f) / 1000f}");
            }
        }

        // Sort notes by Z (descending) and then by X (ascending)
        notes.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            
            // Round Z values to handle floating point precision
            float aZ = Mathf.Round(a.transform.position.z * 1000f) / 1000f;
            float bZ = Mathf.Round(b.transform.position.z * 1000f) / 1000f;
            
            // First compare Z (descending - highest Z first)
            int zComparison = bZ.CompareTo(aZ);
            if (zComparison != 0)
            {
                //Debug.Log($"[CityNoteContainer] Comparing Z: {a.gameObject.name}({aZ}) vs {b.gameObject.name}({bZ}) = {zComparison}");
                return zComparison;
            }
            
            // If Z is the same, compare X (ascending - lowest X first)
            float aX = Mathf.Round(a.transform.position.x * 1000f) / 1000f;
            float bX = Mathf.Round(b.transform.position.x * 1000f) / 1000f;
            int xComparison = aX.CompareTo(bX);
            //Debug.Log($"[CityNoteContainer] Z equal, comparing X: {a.gameObject.name}({aX}) vs {b.gameObject.name}({bX}) = {xComparison}");
            return xComparison;
        });

        // Update the notes list through the property
        Notes = new List<CityNote>(notes);
        
        //Debug.Log("[CityNoteContainer] Final order after sorting:");
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] != null)
            {
                Vector3 pos = notes[i].transform.position;
                //Debug.Log($"[CityNoteContainer] Note {i}: {notes[i].gameObject.name} - Z={Mathf.Round(pos.z * 1000f) / 1000f}, X={Mathf.Round(pos.x * 1000f) / 1000f}");
            }
        }

        // Update indices without sorting by position
        UpdateNoteIndices(false);
    }
} 