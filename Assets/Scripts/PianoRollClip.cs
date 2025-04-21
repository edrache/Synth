using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class PianoRollClip : PlayableAsset, ITimelineClipAsset
{
    public int note;
    public new float duration;
    public float startTime;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PianoRollBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.note = note;
        return playable;
    }
} 