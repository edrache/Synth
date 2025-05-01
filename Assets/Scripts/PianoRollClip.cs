using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class PianoRollClip : PlayableAsset, ITimelineClipAsset
{
    public int note = 60; // Middle C
    public float duration;
    public float startTime;
    [Range(0f, 1f)]
    [Tooltip("Głośność nuty (0 = cisza, 1 = maksymalna głośność)")]
    public float velocity = 0.8f;

    private static readonly string[] noteNames = new string[]
    {
        "Rest", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PianoRollBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.note = note;
        behaviour.velocity = velocity;
        return playable;
    }

    public string GetDisplayName()
    {
        if (note == -1) return "Rest";
        
        int octave = (note / 12) - 1;
        int noteIndex = note % 12;
        return $"{noteNames[noteIndex + 1]}{octave} (v:{velocity:F2})";
    }
} 