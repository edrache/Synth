using UnityEngine;
using UnityEngine.Events;

public class CityNoteProgrammer : MonoBehaviour
{
    [Header("Container Reference")]
    [SerializeField] private CityNoteContainer targetContainer;

    [Header("Note Parameters")]
    [Tooltip("Index of the note in the current scale (0-6)")]
    [Range(0, 6)]
    [SerializeField] private int scaleNoteIndex = 0;
    [Tooltip("Octave number (0-8)")]
    [Range(0, 8)]
    [SerializeField] private int octave = 4;
    [SerializeField] private float velocity = 1.0f;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private int repeatCount = 0;
    [SerializeField] private float position = 0f;
    [SerializeField] private bool useAutoPosition = true;

    [Header("Auto Position Settings")]
    [SerializeField] private bool autoIncrementPosition = false;
    [SerializeField] private float positionIncrement = 0.25f;

    [Header("Events")]
    public UnityEvent<CityNote> OnNoteCreated;
    [Tooltip("Event that can be used to set the Scale Note Index")]
    public UnityEvent<int> OnScaleNoteIndexChanged;

    private TimelineBPMController bpmController;

    private void Awake()
    {
        bpmController = FindObjectOfType<TimelineBPMController>();
        if (bpmController == null)
        {
            Debug.LogError("[CityNoteProgrammer] TimelineBPMController not found in scene!");
        }
    }

    private int GetMidiNoteFromScale()
    {
        if (bpmController == null) return 60; // Default to middle C if no controller found

        int[] scaleNotes = bpmController.GetScaleNotes();
        if (scaleNotes == null || scaleNotes.Length == 0) return 60;

        // Get the base note from the scale
        int baseNote = scaleNotes[scaleNoteIndex % scaleNotes.Length];
        
        // Calculate the octave offset
        int octaveOffset = (octave - 4) * 12; // 4 is the middle octave
        
        return baseNote + octaveOffset;
    }

    public void ProgramNote()
    {
        if (targetContainer == null)
        {
            Debug.LogError("[CityNoteProgrammer] Target container is not assigned!");
            return;
        }

        // If ScaleNoteIndex is 0, only increment position and return
        if (scaleNoteIndex == 0)
        {
            position += positionIncrement;
            return;
        }

        int midiNote = GetMidiNoteFromScale();

        // Create a new GameObject for the note
        GameObject noteObject = new GameObject($"Note_{midiNote}");
        // Set parent to the container's GameObject
        noteObject.transform.SetParent(targetContainer.transform);
        noteObject.transform.localPosition = Vector3.zero;

        // Add CityNote component
        CityNote note = noteObject.AddComponent<CityNote>();
        
        // Set note parameters
        note.pitch = midiNote;
        note.velocity = velocity;
        note.duration = duration;
        note.repeatCount = repeatCount;
        note.position = position;
        note.useAutoPosition = useAutoPosition;

        // Add note to container
        targetContainer.AddNote(note);

        // Increment position for next note if auto-increment is enabled
        if (autoIncrementPosition)
        {
            position += positionIncrement;
        }

        // Invoke event
        OnNoteCreated?.Invoke(note);
    }

    // Public methods to set parameters
    public void SetScaleNoteIndex(int newIndex)
    {
        scaleNoteIndex = Mathf.Clamp(newIndex, 0, 6);
        OnScaleNoteIndexChanged?.Invoke(scaleNoteIndex);
    }
    public void SetOctave(int newOctave) => octave = Mathf.Clamp(newOctave, 0, 8);
    public void SetVelocity(float newVelocity) => velocity = newVelocity;
    public void SetDuration(float newDuration) => duration = newDuration;
    public void SetRepeatCount(int newRepeatCount) => repeatCount = newRepeatCount;
    public void SetPosition(float newPosition) => position = newPosition;
    public void SetUseAutoPosition(bool newUseAutoPosition) => useAutoPosition = newUseAutoPosition;
    public void SetTargetContainer(CityNoteContainer newContainer) => targetContainer = newContainer;
    
    // Auto position methods
    public void SetAutoIncrementPosition(bool enable) => autoIncrementPosition = enable;
    public void SetPositionIncrement(float increment) => positionIncrement = increment;
    public void ResetPosition() => position = 0f;

    // Get current position
    public float GetCurrentPosition() => position;
} 