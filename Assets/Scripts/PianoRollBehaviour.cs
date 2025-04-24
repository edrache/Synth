using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PianoRollBehaviour : PlayableBehaviour
{
    public int note;
    private ModularSynth synth;
    private float duration;
    private TimelineBPMController bpmController;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (note == -1) return;

        if (synth == null || bpmController == null)
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

            // Find BPM controller
            bpmController = Object.FindObjectOfType<TimelineBPMController>();
            if (bpmController == null)
            {
                Debug.LogError("No TimelineBPMController found in scene!");
                return;
            }
        }

        if (synth != null)
        {
            duration = (float)playable.GetDuration();
            
            // Get current scale notes
            int[] scaleNotes = bpmController.GetScaleNotes();
            
            // Find the closest note in scale
            int correctedNote = note;
            if (!IsNoteInScale(note, scaleNotes))
            {
                correctedNote = FindNextHigherNoteInScale(note, scaleNotes);
                Debug.Log($"Note {GetNoteName(note)} corrected to {GetNoteName(correctedNote)} to match current scale");
            }
            
            synth.PlayTimelineNote(correctedNote, duration);
            Debug.Log($"Playing timeline note {GetNoteName(correctedNote)} with duration {duration}s on synth: {synth.name}");
        }
        else
        {
            Debug.LogError("No ModularSynth found! Please assign a ModularSynth name in the PianoRollTrack inspector.");
        }
    }

    private bool IsNoteInScale(int note, int[] scaleNotes)
    {
        int noteInOctave = note % 12;
        foreach (int scaleNote in scaleNotes)
        {
            if (scaleNote % 12 == noteInOctave)
                return true;
        }
        return false;
    }

    private int FindNextHigherNoteInScale(int note, int[] scaleNotes)
    {
        int noteInOctave = note % 12;
        int octave = note / 12;
        
        // Find the next higher note in the current octave
        for (int i = noteInOctave + 1; i < 12; i++)
        {
            foreach (int scaleNote in scaleNotes)
            {
                if (scaleNote % 12 == i)
                    return octave * 12 + i;
            }
        }
        
        // If not found in current octave, try the next octave
        for (int i = 0; i <= noteInOctave; i++)
        {
            foreach (int scaleNote in scaleNotes)
            {
                if (scaleNote % 12 == i)
                    return (octave + 1) * 12 + i;
            }
        }
        
        // Fallback - shouldn't happen as scale should have at least one note
        return note;
    }

    private string GetNoteName(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{noteNames[noteIndex]}{octave}";
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (synth != null)
        {
            synth.StopTimelineNote(note);
            Debug.Log($"Stopped timeline note {GetNoteName(note)} on synth: {synth.name}");
        }
    }
} 