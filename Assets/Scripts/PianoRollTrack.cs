using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackColor(0.2f, 0.8f, 0.2f)]
[TrackClipType(typeof(PianoRollClip))]
public class PianoRollTrack : TrackAsset
{
    [SerializeField]
    private string targetSynthName;

    public string TargetSynthName
    {
        get => targetSynthName;
        set => targetSynthName = value;
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        clip.displayName = "Note";
        
        var pianoRollClip = clip.asset as PianoRollClip;
        if (pianoRollClip != null)
        {
            pianoRollClip.note = 60; // Middle C
            pianoRollClip.duration = (float)clip.duration;
            pianoRollClip.startTime = (float)clip.start;
        }
    }

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties(director, driver);
        
        foreach (var clip in GetClips())
        {
            var pianoRollClip = clip.asset as PianoRollClip;
            if (pianoRollClip != null)
            {
                pianoRollClip.duration = (float)clip.duration;
                pianoRollClip.startTime = (float)clip.start;
            }
        }
    }
} 