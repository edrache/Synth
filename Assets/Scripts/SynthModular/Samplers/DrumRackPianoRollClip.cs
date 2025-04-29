using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class DrumRackPianoRollClip : PlayableAsset, ITimelineClipAsset
{
    public Note note;
    public float duration;
    public float startTime;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DrumRackPianoRollBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.note = note;
        return playable;
    }

    public string GetDisplayName()
    {
        return note.ToString();
    }
} 