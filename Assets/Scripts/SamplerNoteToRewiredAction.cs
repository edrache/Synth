using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

// This script listens for notes played by a Sampler and triggers a UnityEvent based on the note (ignoring octave).
public class SamplerNoteToRewiredAction : MonoBehaviour
{
    public Sampler sampler; // Assign the Sampler in the inspector

    [Serializable]
    public class NoteActionMapping
    {
        public Note note;         // Note enum (C, D, E, etc.)
        public UnityEvent onNoteAction; // Event to invoke
    }

    public List<NoteActionMapping> noteActionMappings = new List<NoteActionMapping>();

    private void OnEnable()
    {
        Sampler.OnAnyNotePlayed += OnNotePlayed;
    }

    private void OnDisable()
    {
        Sampler.OnAnyNotePlayed -= OnNotePlayed;
    }

    private void OnNotePlayed(Sampler s, int midiNote)
    {
        if (s != sampler) return; // Only react to the selected sampler

        int noteInOctave = midiNote % 12;

        foreach (var mapping in noteActionMappings)
        {
            if ((int)mapping.note == noteInOctave)
            {
                mapping.onNoteAction?.Invoke();
                break;
            }
        }
    }
} 