using UnityEngine.Timeline;

public interface IPianoRollTrack
{
    TimelineClip CreateClip();
    void DeleteClip(TimelineClip clip);
} 