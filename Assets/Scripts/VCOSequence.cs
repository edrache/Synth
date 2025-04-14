using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New VCO Sequence", menuName = "Synth/VCO Sequence")]
public class VCOSequence : ScriptableObject
{
    public string sequenceName = "New Sequence";
    public float bpm = 120f;
    public List<VCO.Step> steps = new List<VCO.Step>();
    
    // Scale selection
    public MusicUtils.Note rootNote = MusicUtils.Note.C;
    public MusicUtils.ScaleType scaleType = MusicUtils.ScaleType.None;
    
    // Method to check if a note is in the selected scale
    public bool IsNoteInScale(MusicUtils.Note note)
    {
        if (scaleType == MusicUtils.ScaleType.None)
            return true;
            
        MusicUtils.Note[] scaleNotes = MusicUtils.GetScaleNotes(rootNote, scaleType);
        foreach (MusicUtils.Note scaleNote in scaleNotes)
        {
            if (scaleNote == note)
                return true;
        }
        return false;
    }
    
    // Method to get the next note in scale
    public MusicUtils.Note GetNextNoteInScale(MusicUtils.Note currentNote)
    {
        if (scaleType == MusicUtils.ScaleType.None)
            return currentNote;
            
        MusicUtils.Note[] scaleNotes = MusicUtils.GetScaleNotes(rootNote, scaleType);
        for (int i = 0; i < scaleNotes.Length; i++)
        {
            if (scaleNotes[i] == currentNote)
            {
                return scaleNotes[(i + 1) % scaleNotes.Length];
            }
        }
        return currentNote;
    }
    
    // Method to get the previous note in scale
    public MusicUtils.Note GetPreviousNoteInScale(MusicUtils.Note currentNote)
    {
        if (scaleType == MusicUtils.ScaleType.None)
            return currentNote;
            
        MusicUtils.Note[] scaleNotes = MusicUtils.GetScaleNotes(rootNote, scaleType);
        for (int i = 0; i < scaleNotes.Length; i++)
        {
            if (scaleNotes[i] == currentNote)
            {
                return scaleNotes[(i - 1 + scaleNotes.Length) % scaleNotes.Length];
            }
        }
        return currentNote;
    }
} 