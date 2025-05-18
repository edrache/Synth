using UnityEngine;

public interface INoteEventListener
{
    void OnNoteStart(CityNote note, float velocity, TimelineType timelineType);
    void OnNoteEnd(CityNote note, TimelineType timelineType);
} 