using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ModularSynth))]
public class PianoRollPlayer : MonoBehaviour
{
    public PianoRollData pianoRollData;

    private ModularSynth synth;
    private int currentStep = 0;
    private int totalSteps = 32;

    private List<PlayingNote> activeNotes = new();

    private class PlayingNote
    {
        public int step;
        public int length;
        public int voiceId;
    }

    private void Start()
    {
        synth = GetComponent<ModularSynth>();
        if (pianoRollData != null)
        {
            StartCoroutine(StepLoop());
        }
    }

    private IEnumerator StepLoop()
    {
        while (true)
        {
            float stepDuration = 60f / Mathf.Max(1, pianoRollData.bpm) / 4f;
            PlayStep(currentStep);
            currentStep = (currentStep + 1) % totalSteps;
            yield return new WaitForSeconds(stepDuration);
        }
    }

    private void PlayStep(int step)
    {
        List<PlayingNote> toRemove = new();

        foreach (var n in activeNotes)
        {
            if (n.step + n.length <= step)
            {
                synth.StopVoiceById(n.voiceId);
                toRemove.Add(n);
            }
        }

        foreach (var n in toRemove)
        {
            activeNotes.Remove(n);
        }

        foreach (var note in pianoRollData.notes)
        {
            if (note.step == step)
            {
                float freq = MidiToFreq(note.midi);
                int voiceId = synth.AddVoice(freq);
                activeNotes.Add(new PlayingNote
                {
                    step = note.step,
                    length = note.length,
                    voiceId = voiceId
                });
            }
        }
        PianoRollEditorWindow.SetPlaybackStep(step);
    }

    private float MidiToFreq(int midi)
    {
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }
}
