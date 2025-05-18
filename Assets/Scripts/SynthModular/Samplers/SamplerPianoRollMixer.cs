using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

public class SamplerPianoRollMixer : PlayableBehaviour
{
    public Sampler sampler;
    private HashSet<int> currentlyPlayingNotes = new HashSet<int>();

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //Debug.Log($"Processing frame for mixer");
        
        if (sampler == null)
            return;

        int inputCount = playable.GetInputCount();
        HashSet<int> notesToStop = new HashSet<int>(currentlyPlayingNotes);

        for (int i = 0; i < inputCount; i++)
        {
            //Debug.Log($"Processing input {i}");
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<SamplerPianoRollBehaviour> inputPlayable = (ScriptPlayable<SamplerPianoRollBehaviour>)playable.GetInput(i);
            SamplerPianoRollBehaviour input = inputPlayable.GetBehaviour();

            if (inputWeight > 0)
            {
                //Debug.Log($"Input {i} has weight {inputWeight}");
                if (!currentlyPlayingNotes.Contains(input.midiNote))
                {
                    sampler.PlayNote(input.midiNote, input.velocity);
                    currentlyPlayingNotes.Add(input.midiNote);
                }
                notesToStop.Remove(input.midiNote);
            }
        }

        // Stop notes that are no longer playing
        foreach (var note in notesToStop)
        {
            sampler.StopNote(note);
            currentlyPlayingNotes.Remove(note);
        }
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        // Clean up when the playable is destroyed
        if (sampler != null && currentlyPlayingNotes != null)
        {
            foreach (var note in currentlyPlayingNotes)
            {
                if (sampler != null) // Double check in case sampler gets destroyed during iteration
                {
                    sampler.StopNote(note);
                }
            }
        }
        currentlyPlayingNotes?.Clear();
    }
} 