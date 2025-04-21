using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PianoRollBehaviour : PlayableBehaviour
{
    public int note;
    private ModularSynth synth;
    private int voiceId = -1;

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
            float frequency = 440f * Mathf.Pow(2f, (note - 69) / 12f);
            voiceId = synth.AddVoice(frequency);
            Debug.Log($"Playing note {note} (frequency: {frequency}Hz) on synth: {synth.name}");
        }
        else
        {
            Debug.LogError("No ModularSynth found! Please assign a ModularSynth name in the PianoRollTrack inspector.");
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (synth != null && voiceId != -1)
        {
            synth.StopVoiceById(voiceId);
            voiceId = -1;
            Debug.Log($"Stopped note on synth: {synth.name}");
        }
    }
} 