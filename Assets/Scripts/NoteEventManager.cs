using UnityEngine;
using System.Collections.Generic;

public class NoteEventManager : MonoBehaviour
{
    private List<INoteEventListener> listeners = new List<INoteEventListener>();

    public void RegisterListener(INoteEventListener listener)
    {
        // Debug.Log($"[NoteEventManager] Registering listener: {listener.GetType().Name}");
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
            // Debug.Log($"[NoteEventManager] Listener registered successfully. Total listeners: {listeners.Count}");
        }
        else
        {
            Debug.LogWarning($"[NoteEventManager] Listener {listener.GetType().Name} is already registered");
        }
    }

    public void UnregisterListener(INoteEventListener listener)
    {
        // Debug.Log($"[NoteEventManager] Unregistering listener: {listener.GetType().Name}");
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
            // Debug.Log($"[NoteEventManager] Listener unregistered successfully. Remaining listeners: {listeners.Count}");
        }
        else
        {
            Debug.LogWarning($"[NoteEventManager] Listener {listener.GetType().Name} was not registered");
        }
    }

    public void NotifyNoteStart(CityNote note, float velocity, TimelineType timelineType)
    {
        // Debug.Log($"[NoteEventManager] Notifying note start for note {note.pitch} with velocity {velocity} and timeline type {timelineType}");
        foreach (var listener in listeners)
        {
            // Debug.Log($"[NoteEventManager] Notifying listener: {listener.GetType().Name}");
            listener.OnNoteStart(note, velocity, timelineType);
        }
    }

    public void NotifyNoteEnd(CityNote note, TimelineType timelineType)
    {
        // Debug.Log($"[NoteEventManager] Notifying note end for note {note.pitch} with timeline type {timelineType}");
        foreach (var listener in listeners)
        {
            // Debug.Log($"[NoteEventManager] Notifying listener: {listener.GetType().Name}");
            listener.OnNoteEnd(note, timelineType);
        }
    }
} 