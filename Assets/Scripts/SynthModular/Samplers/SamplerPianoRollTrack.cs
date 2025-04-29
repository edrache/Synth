using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackColor(0.5f, 0.5f, 0.5f)]
[TrackClipType(typeof(SamplerPianoRollClip))]
[TrackBindingType(typeof(Sampler))]
public class SamplerPianoRollTrack : TrackAsset
{
    [SerializeField]
    private string targetSamplerName;

    public string TargetSamplerName
    {
        get => targetSamplerName;
        set => targetSamplerName = value;
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
            Debug.LogError($"Sampler not found on GameObject {go.name}. Make sure the Sampler component is attached to the GameObject.");
            return mixer;
        }

        if (sampler.sample == null)
        {
            Debug.LogError($"No sample loaded in Sampler on GameObject {go.name}. Please load a sample first.");
            return mixer;
        }

        behaviour.sampler = sampler;
        Debug.Log($"Sampler initialized for track: {name}, sample: {sampler.sample.name}");
        return mixer;
    }

    private ISampler FindSampler(GameObject go)
    {
        if (string.IsNullOrEmpty(targetSamplerName))
            return go.GetComponent<ISampler>();

        var samplers = go.GetComponents<ISampler>();
        foreach (var sampler in samplers)
        {
            if (sampler.GetType().Name == targetSamplerName)
                return sampler;
        }

        return null;
    }
} 