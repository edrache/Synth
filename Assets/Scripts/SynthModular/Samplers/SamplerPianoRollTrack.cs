using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;

[TrackColor(0.5f, 0.5f, 0.5f)]
[TrackClipType(typeof(SamplerPianoRollClip))]
[TrackBindingType(typeof(Sampler))]
public class SamplerPianoRollTrack : TrackAsset, IPianoRollTrack
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
        var samplerClip = clip.asset as SamplerPianoRollClip;
        if (samplerClip != null)
        {
            samplerClip.duration = (float)clip.duration;
            samplerClip.startTime = (float)clip.start;
            clip.displayName = samplerClip.GetDisplayName();
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
        clip.displayName = "Sampler Note";
    }

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties(director, driver);
        
        foreach (var clip in GetClips())
        {
            var samplerClip = clip.asset as SamplerPianoRollClip;
            if (samplerClip != null)
            {
                samplerClip.duration = (float)clip.duration;
                samplerClip.startTime = (float)clip.start;
                clip.displayName = samplerClip.GetDisplayName();
            }
        }
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<SamplerPianoRollMixer>.Create(graph, inputCount);
        var behaviour = mixer.GetBehaviour();
        
        var sampler = FindSampler(go) as Sampler;
        if (sampler == null)
        {
            Debug.LogError($"Sampler not found on GameObject {go.name}. Make sure the Sampler component is attached to the GameObject and the track is bound to it in Timeline.");
            return mixer;
        }

        if (sampler.sample == null)
        {
            Debug.LogError($"No sample loaded in Sampler on GameObject {go.name}. Please load a sample first.");
            return mixer;
        }

        behaviour.sampler = sampler;
        return mixer;
    }

    private ISampler FindSampler(GameObject go)
    {
        if (string.IsNullOrEmpty(targetSamplerName))
        {
            var sampler = go.GetComponent<ISampler>();
            if (sampler == null)
            {
                Debug.LogError($"No ISampler component found on GameObject {go.name}");
            }
            return sampler;
        }

        var samplers = go.GetComponents<ISampler>();
        foreach (var sampler in samplers)
        {
            if (sampler.GetType().Name == targetSamplerName)
            {
                return sampler;
            }
        }

        Debug.LogError($"No sampler of type {targetSamplerName} found on GameObject {go.name}");
        return null;
    }
} 