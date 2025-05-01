using UnityEngine.Timeline;
using System.Collections.Generic;

public interface IPianoRollTrack
{
    string name { get; }
    TimelineClip CreateClip();
    void DeleteClip(TimelineClip clip);
    IEnumerable<TimelineClip> GetClips();
} 