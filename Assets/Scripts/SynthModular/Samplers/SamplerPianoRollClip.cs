using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class SamplerPianoRollClip : PlayableAsset, ITimelineClipAsset
{
    public int midiNote;
    public float duration;
    public float startTime;
    [Range(0f, 1f)]
    [Tooltip("Głośność nuty (0 = cisza, 1 = maksymalna głośność)")]
    public float velocity = 1f;
    [Tooltip("Reference to the GameObject that contains the CityNote component")]
    public GameObject sourceObject;
    [Tooltip("Type of timeline that this note belongs to")]
    public TimelineType timelineType;

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
        behaviour.duration = duration;
        behaviour.startTime = startTime;
        behaviour.velocity = velocity;
        behaviour.sourceObject = sourceObject;
        behaviour.timelineType = timelineType;
        return playable;
    }

    public string GetDisplayName()
    {
        if (midiNote == -1) return "Rest";
        
        int octave = (midiNote / 12) - 1;
        int noteIndex = midiNote % 12;
        return $"{noteNames[noteIndex + 1]}{octave} (v:{velocity:F2})";
    }
} 