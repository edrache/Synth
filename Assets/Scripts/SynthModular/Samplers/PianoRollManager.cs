using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;

public static class PianoRollManager
{
    /// <summary>
    /// Lists all available tracks in the timeline
    /// </summary>
    public static void ListAllTracks(PlayableDirector timeline)
    {
        if (timeline == null)
        {
            Debug.LogError("Timeline is null!");
            return;
        }

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("Timeline asset is not a TimelineAsset!");
            return;
        }

        Debug.Log("Available tracks:");
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            Debug.Log($"- {track.name} (Type: {track.GetType().Name})");
        }
    }

    /// <summary>
    /// Adds a note to the specified PianoRoll track at the given time and duration
    /// </summary>
    /// <param name="timeline">The PlayableDirector containing the timeline</param>
    /// <param name="trackName">Name of the PianoRoll track</param>
    /// <param name="midiNote">MIDI note number to add</param>
    /// <param name="startTime">Start time in seconds</param>
    /// <param name="duration">Duration in seconds</param>
    /// <returns>The created TimelineClip</returns>
    public static TimelineClip AddNote(PlayableDirector timeline, string trackName, int midiNote, float startTime, float duration)
    {
        if (timeline == null)
        {
            Debug.LogError("Timeline is null!");
            return null;
        }

        // Find the track
        var track = FindPianoRollTrack(timeline, trackName);
        if (track == null)
        {
            Debug.LogError($"Track '{trackName}' not found!");
            ListAllTracks(timeline);
            return null;
        }

        // Store current time
        double currentTime = timeline.time;

        // Create new clip
        var clip = track.CreateClip();
        if (clip == null)
        {
            Debug.LogError("Failed to create clip!");
            return null;
        }

        clip.start = startTime;
        clip.duration = duration;

        // Configure the clip based on its type
        if (track is SamplerPianoRollTrack)
        {
            var samplerClip = clip.asset as SamplerPianoRollClip;
            if (samplerClip != null)
            {
                samplerClip.midiNote = midiNote;
                samplerClip.duration = duration;
                samplerClip.startTime = startTime;
                clip.displayName = samplerClip.GetDisplayName();
            }
        }
        else if (track is DrumRackPianoRollTrack)
        {
            var drumRackClip = clip.asset as DrumRackPianoRollClip;
            if (drumRackClip != null)
            {
                drumRackClip.note = (Note)(midiNote % 12);
                drumRackClip.duration = duration;
                drumRackClip.startTime = startTime;
                clip.displayName = drumRackClip.GetDisplayName();
            }
        }
        else if (track is PianoRollTrack)
        {
            var pianoRollClip = clip.asset as PianoRollClip;
            if (pianoRollClip != null)
            {
                pianoRollClip.note = midiNote;
                pianoRollClip.duration = duration;
                pianoRollClip.startTime = startTime;
                clip.displayName = pianoRollClip.GetDisplayName();
            }
        }

        // Refresh the timeline
        timeline.RebuildGraph();

        // Restore the time
        timeline.time = currentTime;

        return clip;
    }

    /// <summary>
    /// Removes a note from the specified PianoRoll track
    /// </summary>
    /// <param name="timeline">The PlayableDirector containing the timeline</param>
    /// <param name="trackName">Name of the PianoRoll track</param>
    /// <param name="clip">The TimelineClip to remove</param>
    public static void RemoveNote(PlayableDirector timeline, string trackName, TimelineClip clip)
    {
        if (timeline == null || clip == null)
        {
            Debug.LogError("Timeline or clip is null!");
            return;
        }

        var track = FindPianoRollTrack(timeline, trackName);
        if (track == null)
        {
            Debug.LogError($"Track '{trackName}' not found!");
            ListAllTracks(timeline);
            return;
        }

        track.DeleteClip(clip);
        timeline.RebuildGraph();
    }

    /// <summary>
    /// Finds a PianoRoll track by name
    /// </summary>
    private static IPianoRollTrack FindPianoRollTrack(PlayableDirector director, string trackName)
    {
        var timelineAsset = director.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("Timeline asset is not a TimelineAsset!");
            return null;
        }

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track.name == trackName)
            {
                if (track is SamplerPianoRollTrack samplerTrack)
                    return (IPianoRollTrack)samplerTrack;
                if (track is DrumRackPianoRollTrack drumRackTrack)
                    return (IPianoRollTrack)drumRackTrack;
                if (track is PianoRollTrack pianoRollTrack)
                    return (IPianoRollTrack)pianoRollTrack;
            }
        }

        return null;
    }
} 