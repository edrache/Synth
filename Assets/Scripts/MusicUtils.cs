using UnityEngine;
using System.Collections.Generic;

public static class MusicUtils
{
    public enum Note { C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B }
    public enum ScaleType { None, Chromatic, Major, Minor, Pentatonic, Blues, Dorian, Phrygian, Lydian, Mixolydian, Locrian }
    public enum NoteLength { Whole = 1, Half = 2, Quarter = 4, Eighth = 8, Sixteenth = 16, ThirtySecond = 32 }

    private static readonly float[] noteFrequencies = new float[]
    {
        16.35f, 17.32f, 18.35f, 19.45f, 20.60f, 21.83f, 23.12f, 24.50f, 25.96f, 27.50f, 29.14f, 30.87f
    };

    private static readonly Dictionary<ScaleType, int[]> scalePatterns = new Dictionary<ScaleType, int[]>
    {
        { ScaleType.None, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
        { ScaleType.Chromatic, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
        { ScaleType.Major, new int[] { 0, 2, 4, 5, 7, 9, 11 } },
        { ScaleType.Minor, new int[] { 0, 2, 3, 5, 7, 8, 10 } },
        { ScaleType.Pentatonic, new int[] { 0, 2, 4, 7, 9 } },
        { ScaleType.Blues, new int[] { 0, 3, 5, 6, 7, 10 } },
        { ScaleType.Dorian, new int[] { 0, 2, 3, 5, 7, 9, 10 } },
        { ScaleType.Phrygian, new int[] { 0, 1, 3, 5, 7, 8, 10 } },
        { ScaleType.Lydian, new int[] { 0, 2, 4, 6, 7, 9, 11 } },
        { ScaleType.Mixolydian, new int[] { 0, 2, 4, 5, 7, 9, 10 } },
        { ScaleType.Locrian, new int[] { 0, 1, 3, 5, 6, 8, 10 } }
    };

    public static float GetFrequency(Note note, int octave)
    {
        int noteIndex = (int)note;
        return noteFrequencies[noteIndex] * Mathf.Pow(2, octave);
    }

    public static string GetNoteName(Note note)
    {
        return note.ToString().Replace("Sharp", "#");
    }

    public static Note[] GetScaleNotes(Note rootNote, ScaleType scaleType)
    {
        if (scaleType == ScaleType.None)
            return new Note[] { rootNote };
            
        int[] pattern = scalePatterns[scaleType];
        Note[] scaleNotes = new Note[pattern.Length];
        
        for (int i = 0; i < pattern.Length; i++)
        {
            int noteIndex = ((int)rootNote + pattern[i]) % 12;
            scaleNotes[i] = (Note)noteIndex;
        }
        
        return scaleNotes;
    }

    public static string GetScaleName(ScaleType scaleType)
    {
        return scaleType.ToString();
    }

    public static float GetNoteLengthInBeats(NoteLength length)
    {
        return 4f / (int)length; // 4 beats per whole note
    }

    public static string GetNoteLengthName(NoteLength length)
    {
        switch (length)
        {
            case NoteLength.Whole: return "Whole";
            case NoteLength.Half: return "Half";
            case NoteLength.Quarter: return "Quarter";
            case NoteLength.Eighth: return "Eighth";
            case NoteLength.Sixteenth: return "16th";
            case NoteLength.ThirtySecond: return "32nd";
            default: return length.ToString();
        }
    }
} 