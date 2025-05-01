using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;

[TrackColor(0.8f, 0.2f, 0.2f)]
[TrackClipType(typeof(DrumRackPianoRollClip))]
public class DrumRackPianoRollTrack : TrackAsset, IPianoRollTrack
{
    [SerializeField]
    private string targetSamplerName;

    public string TargetSamplerName
    {
        get => targetSamplerName;
        set => targetSamplerName = value;
    }

    public TimelineClip CreateClip()
    {
        var clip = base.CreateDefaultClip();
        var drumRackClip = clip.asset as DrumRackPianoRollClip;
        if (drumRackClip != null)
        {
            drumRackClip.note = Note.C;
            drumRackClip.duration = (float)clip.duration;
            drumRackClip.startTime = (float)clip.start;
            clip.displayName = drumRackClip.GetDisplayName();
        }
        return clip;
    }

    public void DeleteClip(TimelineClip clip)
    {
        if (clip != null)
        {
            var timelineAsset = clip.parentTrack.timelineAsset;
            if (timelineAsset != null)
            {
                timelineAsset.DeleteClip(clip);
            }
        }
    }

    public IEnumerable<TimelineClip> GetClips()
    {
        return base.GetClips();
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        
        var drumRackClip = clip.asset as DrumRackPianoRollClip;
        if (drumRackClip != null)
        {
            drumRackClip.note = Note.C;
            drumRackClip.duration = (float)clip.duration;
            drumRackClip.startTime = (float)clip.start;
            clip.displayName = drumRackClip.GetDisplayName();
        }
    }

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties(director, driver);
        
        foreach (var clip in GetClips())
        {
            var drumRackClip = clip.asset as DrumRackPianoRollClip;
            if (drumRackClip != null)
            {
                drumRackClip.duration = (float)clip.duration;
                drumRackClip.startTime = (float)clip.start;
                clip.displayName = drumRackClip.GetDisplayName();
            }
        }
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<DrumRackPianoRollMixer>.Create(graph, inputCount);
        var behaviour = mixer.GetBehaviour();
        behaviour.sampler = FindSampler(go);
        return mixer;
    }

    private IDrumRackSampler FindSampler(GameObject go)
    {
        if (string.IsNullOrEmpty(targetSamplerName))
            return go.GetComponent<IDrumRackSampler>();

        var samplers = go.GetComponents<IDrumRackSampler>();
        foreach (var sampler in samplers)
        {
            if (sampler.GetType().Name == targetSamplerName)
                return sampler;
        }

        return null;
    }
} 