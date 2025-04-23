using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineBPMController : MonoBehaviour
{
    [Header("Tempo")]
    [Tooltip("Beats per minute")]
    [Range(20, 300)]
    [SerializeField]
    private float currentBPM = 120f;

    [Header("Musical Scale")]
    [Tooltip("Select the musical scale")]
    [SerializeField]
    private MusicalScale currentScale = MusicalScale.CMajor;
    
    [Tooltip("MIDI note number for the root note (60 = Middle C)")]
    [Range(24, 96)]
    [SerializeField]
    private int currentRootNote = 60;

    private const float SECONDS_PER_MINUTE = 60f;

    #region Public Properties
    public float BPM
    {
        get => currentBPM;
        set
        {
            currentBPM = value;
            UpdateAllTimelines();
        }
    }

    public MusicalScale Scale
    {
        get => currentScale;
        set => currentScale = value;
    }

    public int RootNote
    {
        get => currentRootNote;
        set => currentRootNote = value;
    }
    #endregion

    public enum MusicalScale
    {
        [InspectorName("C Major")] CMajor,
        [InspectorName("G Major")] GMajor,
        [InspectorName("D Major")] DMajor,
        [InspectorName("A Major")] AMajor,
        [InspectorName("E Major")] EMajor,
        [InspectorName("B Major")] BMajor,
        [InspectorName("F# Major")] FSharpMajor,
        [InspectorName("F Major")] FMajor,
        [InspectorName("Bb Major")] BFlatMajor,
        [InspectorName("Eb Major")] EFlatMajor,
        [InspectorName("Ab Major")] AFlatMajor,
        [InspectorName("Db Major")] DFlatMajor,
        [InspectorName("Gb Major")] GFlatMajor,
        [InspectorName("A Minor")] AMinor,
        [InspectorName("E Minor")] EMinor,
        [InspectorName("B Minor")] BMinor,
        [InspectorName("F# Minor")] FSharpMinor,
        [InspectorName("C# Minor")] CSharpMinor,
        [InspectorName("G# Minor")] GSharpMinor,
        [InspectorName("D# Minor")] DSharpMinor,
        [InspectorName("A# Minor")] ASharpMinor,
        [InspectorName("D Minor")] DMinor,
        [InspectorName("G Minor")] GMinor,
        [InspectorName("C Minor")] CMinor,
        [InspectorName("F Minor")] FMinor,
        [InspectorName("Bb Minor")] BFlatMinor,
        [InspectorName("Eb Minor")] EFlatMinor,
        [InspectorName("Ab Minor")] AFlatMinor
    }

    private void UpdateAllTimelines()
    {
        float secondsPerBeat = SECONDS_PER_MINUTE / currentBPM;
        float speedMultiplier = 1f / secondsPerBeat;

        var directors = FindObjectsOfType<PlayableDirector>();
        foreach (var director in directors)
        {
            if (director.playableGraph.IsValid())
            {
                var rootPlayable = director.playableGraph.GetRootPlayable(0);
                rootPlayable.SetSpeed(speedMultiplier);
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateAllTimelines();
        }
    }

    public int[] GetScaleNotes()
    {
        int[] intervals = GetScaleIntervals();
        int[] notes = new int[intervals.Length];
        
        for (int i = 0; i < intervals.Length; i++)
        {
            notes[i] = currentRootNote + intervals[i];
        }
        
        return notes;
    }

    private int[] GetScaleIntervals()
    {
        return currentScale switch
        {
            // Major scales
            MusicalScale.CMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.GMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.DMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.AMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.EMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.BMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.FSharpMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.FMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.BFlatMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.EFlatMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.AFlatMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.DFlatMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            MusicalScale.GFlatMajor => new[] { 0, 2, 4, 5, 7, 9, 11 },
            
            // Minor scales
            MusicalScale.AMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.EMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.BMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.FSharpMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.CSharpMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.GSharpMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.DSharpMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.ASharpMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.DMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.GMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.CMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.FMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.BFlatMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.EFlatMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            MusicalScale.AFlatMinor => new[] { 0, 2, 3, 5, 7, 8, 10 },
            _ => new[] { 0, 2, 4, 5, 7, 9, 11 } // Default to C Major
        };
    }
} 