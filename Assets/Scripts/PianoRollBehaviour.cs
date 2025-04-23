using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PianoRollBehaviour : PlayableBehaviour
{
    public int note;
    private ModularSynth synth;
    private int voiceId = -1;
    private float duration;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (note == -1) return;

        if (synth == null)
        {
            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            if (director != null)
            {
                var timelineAsset = director.playableAsset as TimelineAsset;
                if (timelineAsset != null)
                {
                    foreach (var track in timelineAsset.GetOutputTracks())
                    {
                        if (track is PianoRollTrack pianoRollTrack)
                        {
                            var synthName = pianoRollTrack.TargetSynthName;
                            if (!string.IsNullOrEmpty(synthName))
                            {
                                var synths = Object.FindObjectsOfType<ModularSynth>();
                                foreach (var s in synths)
                                {
                                    if (s.name == synthName)
                                    {
                                        synth = s;
                                        Debug.Log($"Found ModularSynth by name: {synth.name}");
                                        break;
                                    }
                                }
                            }
                            if (synth != null) break;
                        }
                    }
                }
            }
        }

        if (synth != null)
        {
            duration = (float)playable.GetDuration();
            synth.PlayTimelineNote(note, duration);
            Debug.Log($"Playing timeline note {note} with duration {duration}s on synth: {synth.name}");
        }
        else
        {
            Debug.LogError("No ModularSynth found! Please assign a ModularSynth name in the PianoRollTrack inspector.");
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (synth != null)
        {
            synth.StopTimelineNote(note);
            Debug.Log($"Stopped timeline note {note} on synth: {synth.name}");
        }
    }
} 