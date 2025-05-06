using UnityEngine;

public class CityNote : MonoBehaviour
{
    [Header("Note Properties")]
    [SerializeField] private int _pitch = 60; // MIDI note number (60 = middle C)
    [SerializeField] private float _velocity = 1.0f; // Note velocity (0-1)
    [SerializeField] private float _duration = 0.25f; // Note duration in seconds
    [SerializeField] private int _repeatCount = 0; // How many times to repeat this note
    [SerializeField] private float _position = 0f; // Position on the timeline (X axis)
    [SerializeField] private bool _useAutoPosition = true; // Whether to use automatic positioning

    private CityNoteContainer container;
    private int _index = -1; // Index in the container's list

    private void Start()
    {
        container = FindObjectOfType<CityNoteContainer>();
        if (container == null)
        {
            Debug.LogError("[CityNote] Could not find CityNoteContainer!");
        }
        else
        {
            container.AddNote(this);
        }
    }

    private void OnDestroy()
    {
        if (container != null)
        {
            container.RemoveNote(this);
        }
    }

    public void SetIndex(int index)
    {
        if (_index != index)
        {
            _index = index;
            if (_useAutoPosition)
            {
                UpdateAutoPosition();
                Debug.Log($"[CityNote] Index changed to {index}, updating position to {_position}");
            }
        }
    }

    private void UpdateAutoPosition()
    {
        if (container == null || _index < 0) return;

        var sequencer = FindObjectOfType<CitySequencer>();
        if (sequencer == null) return;

        var allNotes = container.GetAllNotes();
        if (allNotes.Count == 0) return;

        // Get timeline length from sequencer
        float timelineLength = sequencer.GetTimelineLength();
        
        // Calculate spacing based on number of notes
        float spacing = timelineLength / allNotes.Count;

        // Calculate position based on index
        float newPosition = _index * spacing;

        Debug.Log($"[CityNote] Calculating position for note {_index}:");
        Debug.Log($"[CityNote] - Timeline length: {timelineLength}");
        Debug.Log($"[CityNote] - Total notes: {allNotes.Count}");
        Debug.Log($"[CityNote] - Spacing: {spacing}");
        Debug.Log($"[CityNote] - Current position: {_position}");
        Debug.Log($"[CityNote] - New position: {newPosition}");

        if (_position != newPosition)
        {
            _position = newPosition;
            Debug.Log($"[CityNote] Position updated to {_position}");
            
            // Force container update to reflect position change
            if (container != null)
            {
                container.ForceUpdate();
            }
        }
    }

    public Vector3[] GetRepeatPositions()
    {
        if (_repeatCount <= 0)
        {
            return new Vector3[] { new Vector3(_position, 0, 0) };
        }

        Vector3[] positions = new Vector3[_repeatCount + 1];
        positions[0] = new Vector3(_position, 0, 0);

        // Get the next note's position to calculate spacing
        float nextNotePosition = GetNextNotePosition();
        float spacing = CalculateRepeatSpacing(nextNotePosition);

        for (int i = 1; i <= _repeatCount; i++)
        {
            positions[i] = new Vector3(_position + (spacing * i), 0, 0);
        }

        return positions;
    }

    private float GetNextNotePosition()
    {
        if (container == null) return _position + 1f; // Default spacing if no container

        var allNotes = container.GetAllNotes();
        float nextX = float.MaxValue;

        foreach (var note in allNotes)
        {
            if (note != this && note.position > _position && note.position < nextX)
            {
                nextX = note.position;
            }
        }

        if (nextX == float.MaxValue)
        {
            // If no next note found, use a default spacing
            return _position + 1f;
        }

        return nextX;
    }

    private float CalculateRepeatSpacing(float nextNotePosition)
    {
        float distanceToNextNote = nextNotePosition - _position;
        float totalDuration = _duration * (_repeatCount + 1); // Total duration including all repeats

        // Calculate spacing based on the distance to next note and total duration
        // We want the repeats to fit between this note and the next note
        float spacing = distanceToNextNote / (_repeatCount + 1);

        // Ensure the spacing is not too small (at least 0.1 world units)
        return Mathf.Max(spacing, 0.1f);
    }

    // Public properties for accessing private fields
    public int pitch
    {
        get => _pitch;
        set
        {
            if (_pitch != value)
            {
                _pitch = value;
                if (container != null)
                {
                    container.ForceUpdate();
                }
            }
        }
    }

    public float velocity
    {
        get => _velocity;
        set
        {
            if (_velocity != value)
            {
                _velocity = value;
                if (container != null)
                {
                    container.ForceUpdate();
                }
            }
        }
    }

    public float duration
    {
        get => _duration;
        set
        {
            if (_duration != value)
            {
                _duration = value;
                if (container != null)
                {
                    container.ForceUpdate();
                }
            }
        }
    }

    public int repeatCount
    {
        get => _repeatCount;
        set
        {
            if (_repeatCount != value)
            {
                _repeatCount = value;
                if (container != null)
                {
                    container.ForceUpdate();
                }
            }
        }
    }

    public float position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                if (container != null)
                {
                    container.ForceUpdate();
                }
            }
        }
    }

    public bool useAutoPosition
    {
        get => _useAutoPosition;
        set
        {
            if (_useAutoPosition != value)
            {
                _useAutoPosition = value;
                if (_useAutoPosition)
                {
                    UpdateAutoPosition();
                }
            }
        }
    }
} 