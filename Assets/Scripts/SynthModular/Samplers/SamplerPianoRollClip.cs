using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class SamplerPianoRollClip : PlayableAsset, ITimelineClipAsset
{
    public int midiNote;
    public float duration;
    public float startTime;
    public float velocity = 1f;

    private static readonly string[] noteNames = new string[]
    {
        "Rest", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SamplerPianoRollBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.midiNote = midiNote;
        behaviour.velocity = velocity;
        return playable;
    }

    public string GetDisplayName()
    {
        if (midiNote == -1) return "Rest";
        
        int octave = (midiNote / 12) - 1;
        int noteIndex = midiNote % 12;
        return $"{noteNames[noteIndex + 1]}{octave}";
    }
} 