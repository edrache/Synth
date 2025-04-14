using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(VCOSequence))]
public class VCOSequenceEditor : Editor
{
    private int stepsCount = 16;
    private float minPitch = 100f;
    private float maxPitch = 1000f;
    private float accentProbability = 0.2f;
    private float slideProbability = 0.3f;

    // Duration probabilities
    private float noSoundProbability = 0.2f;
    private float sixteenthNoteProbability = 0f;
    private float eighthNoteProbability = 0f;
    private float dottedEighthNoteProbability = 0f;
    private float quarterNoteProbability = 0.8f;

    // Note selection for random generation
    private enum NoteSelectionMode { Frequencies, NoteRange, Scale }
    private NoteSelectionMode selectionMode = NoteSelectionMode.Frequencies;
    
    private MusicUtils.Note minNote = MusicUtils.Note.C;
    private MusicUtils.Note maxNote = MusicUtils.Note.B;
    private int minOctave = 3;
    private int maxOctave = 5;

    // Scale selection
    private MusicUtils.Note rootNote = MusicUtils.Note.C;
    private MusicUtils.ScaleType scaleType = MusicUtils.ScaleType.Major;
    private bool useScaleNotes = true;

    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VCOSequence sequence = (VCOSequence)target;

        // Get the VCO component to access globalOctaveShift
        VCO vco = FindObjectOfType<VCO>();
        int globalOctaveShift = vco != null ? vco.globalOctaveShift : 0;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sequence Editor", EditorStyles.boldLabel);

        // Scale selection
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scale Selection", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        sequence.rootNote = (MusicUtils.Note)EditorGUILayout.EnumPopup("Root Note", sequence.rootNote);
        sequence.scaleType = (MusicUtils.ScaleType)EditorGUILayout.EnumPopup("Scale", sequence.scaleType);
        EditorGUILayout.EndHorizontal();

        // Sequence length control
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        stepsCount = EditorGUILayout.IntField("Number of Steps", stepsCount);
        if (GUILayout.Button("Resize"))
        {
            ResizeSequence(sequence);
        }
        EditorGUILayout.EndHorizontal();

        // Steps editor
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < sequence.steps.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Step {i + 1}", EditorStyles.boldLabel);

            VCO.Step step = sequence.steps[i];
            
            // Note selection
            step.useNote = EditorGUILayout.Toggle("Use Musical Note", step.useNote);
            if (step.useNote)
            {
                EditorGUILayout.BeginHorizontal();
                step.note = (MusicUtils.Note)EditorGUILayout.EnumPopup("Note", step.note);
                step.octave = EditorGUILayout.IntField("Octave", step.octave);
                EditorGUILayout.EndHorizontal();
                step.UpdatePitchFromNote(globalOctaveShift);

                // Show warning if note is not in scale
                if (sequence.scaleType != MusicUtils.ScaleType.None && !sequence.IsNoteInScale(step.note))
                {
                    EditorGUILayout.HelpBox("Note is not in the selected scale!", MessageType.Warning);
                }
            }
            else
            {
                step.pitch = EditorGUILayout.FloatField("Pitch (Hz)", step.pitch);
            }

            step.slide = EditorGUILayout.Toggle("Slide", step.slide);
            step.accent = EditorGUILayout.Toggle("Accent", step.accent);
            if (step.accent)
            {
                step.accentStrength = EditorGUILayout.Slider("Accent Strength", step.accentStrength, 1f, 2f);
            }
            step.duration = EditorGUILayout.Slider("Duration", step.duration, 0f, 4f);
            EditorGUILayout.HelpBox("Duration: 0 = no sound, 1 = 16th note, 2 = 8th note, 3 = dotted 8th note, 4 = quarter note", MessageType.Info);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Random Sequence Generator", EditorStyles.boldLabel);

        // Duration probabilities
        EditorGUILayout.LabelField("Duration Probabilities", EditorStyles.boldLabel);
        noSoundProbability = EditorGUILayout.Slider("No Sound (0)", noSoundProbability, 0f, 1f);
        sixteenthNoteProbability = EditorGUILayout.Slider("16th Note (1)", sixteenthNoteProbability, 0f, 1f);
        eighthNoteProbability = EditorGUILayout.Slider("8th Note (2)", eighthNoteProbability, 0f, 1f);
        dottedEighthNoteProbability = EditorGUILayout.Slider("Dotted 8th (3)", dottedEighthNoteProbability, 0f, 1f);
        quarterNoteProbability = EditorGUILayout.Slider("Quarter Note (4)", quarterNoteProbability, 0f, 1f);

        // Normalize probabilities
        float totalProbability = noSoundProbability + sixteenthNoteProbability + eighthNoteProbability + 
                               dottedEighthNoteProbability + quarterNoteProbability;
        if (totalProbability > 0)
        {
            noSoundProbability /= totalProbability;
            sixteenthNoteProbability /= totalProbability;
            eighthNoteProbability /= totalProbability;
            dottedEighthNoteProbability /= totalProbability;
            quarterNoteProbability /= totalProbability;
        }

        // Random generation controls
        selectionMode = (NoteSelectionMode)EditorGUILayout.EnumPopup("Note Selection Mode", selectionMode);

        switch (selectionMode)
        {
            case NoteSelectionMode.Frequencies:
                minPitch = EditorGUILayout.FloatField("Min Pitch (Hz)", minPitch);
                maxPitch = EditorGUILayout.FloatField("Max Pitch (Hz)", maxPitch);
                break;

            case NoteSelectionMode.NoteRange:
                EditorGUILayout.BeginHorizontal();
                minNote = (MusicUtils.Note)EditorGUILayout.EnumPopup("Min Note", minNote);
                minOctave = EditorGUILayout.IntField("Min Octave", minOctave);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                maxNote = (MusicUtils.Note)EditorGUILayout.EnumPopup("Max Note", maxNote);
                maxOctave = EditorGUILayout.IntField("Max Octave", maxOctave);
                EditorGUILayout.EndHorizontal();

                minPitch = MusicUtils.GetFrequency(minNote, minOctave);
                maxPitch = MusicUtils.GetFrequency(maxNote, maxOctave);
                break;

            case NoteSelectionMode.Scale:
                EditorGUILayout.BeginHorizontal();
                rootNote = (MusicUtils.Note)EditorGUILayout.EnumPopup("Root Note", rootNote);
                scaleType = (MusicUtils.ScaleType)EditorGUILayout.EnumPopup("Scale", scaleType);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                minOctave = EditorGUILayout.IntField("Min Octave", minOctave);
                maxOctave = EditorGUILayout.IntField("Max Octave", maxOctave);
                EditorGUILayout.EndHorizontal();

                useScaleNotes = EditorGUILayout.Toggle("Use Only Scale Notes", useScaleNotes);
                break;
        }

        accentProbability = EditorGUILayout.Slider("Accent Probability", accentProbability, 0f, 1f);
        slideProbability = EditorGUILayout.Slider("Slide Probability", slideProbability, 0f, 1f);

        if (GUILayout.Button("Generate Random Sequence"))
        {
            GenerateRandomSequence(sequence);
        }
    }

    private void ResizeSequence(VCOSequence sequence)
    {
        while (sequence.steps.Count < stepsCount)
        {
            sequence.steps.Add(new VCO.Step());
        }
        while (sequence.steps.Count > stepsCount)
        {
            sequence.steps.RemoveAt(sequence.steps.Count - 1);
        }
        EditorUtility.SetDirty(sequence);
        AssetDatabase.SaveAssets();
    }

    private void GenerateRandomSequence(VCOSequence sequence)
    {
        sequence.steps.Clear();

        MusicUtils.Note[] scaleNotes = null;
        if (selectionMode == NoteSelectionMode.Scale)
        {
            scaleNotes = MusicUtils.GetScaleNotes(rootNote, scaleType);
        }

        // Get the VCO component to access globalOctaveShift
        VCO vco = FindObjectOfType<VCO>();
        int globalOctaveShift = vco != null ? vco.globalOctaveShift : 0;

        for (int i = 0; i < stepsCount; i++)
        {
            VCO.Step step = new VCO.Step();
            
            if (selectionMode != NoteSelectionMode.Frequencies)
            {
                step.useNote = true;
                int randomOctave = Random.Range(minOctave, maxOctave + 1);
                MusicUtils.Note randomNote;

                if (selectionMode == NoteSelectionMode.Scale && useScaleNotes)
                {
                    randomNote = scaleNotes[Random.Range(0, scaleNotes.Length)];
                }
                else if (selectionMode == NoteSelectionMode.Scale)
                {
                    randomNote = (MusicUtils.Note)Random.Range(0, 12);
                }
                else
                {
                    if (randomOctave == minOctave)
                    {
                        randomNote = (MusicUtils.Note)Random.Range((int)minNote, 12);
                    }
                    else if (randomOctave == maxOctave)
                    {
                        randomNote = (MusicUtils.Note)Random.Range(0, (int)maxNote + 1);
                    }
                    else
                    {
                        randomNote = (MusicUtils.Note)Random.Range(0, 12);
                    }
                }

                step.note = randomNote;
                step.octave = randomOctave;
                step.UpdatePitchFromNote(globalOctaveShift);
            }
            else
            {
                step.useNote = false;
                step.pitch = Random.Range(minPitch, maxPitch);
            }

            step.accent = Random.value < accentProbability;
            if (step.accent)
            {
                step.accentStrength = Random.Range(1.2f, 2f);
            }
            step.slide = Random.value < slideProbability;

            // Set duration based on probabilities
            float randomValue = Random.value;
            if (randomValue < noSoundProbability)
                step.duration = 0f;
            else if (randomValue < noSoundProbability + sixteenthNoteProbability)
                step.duration = 1f;
            else if (randomValue < noSoundProbability + sixteenthNoteProbability + eighthNoteProbability)
                step.duration = 2f;
            else if (randomValue < noSoundProbability + sixteenthNoteProbability + eighthNoteProbability + dottedEighthNoteProbability)
                step.duration = 3f;
            else
                step.duration = 4f;
            
            sequence.steps.Add(step);
        }

        EditorUtility.SetDirty(sequence);
        AssetDatabase.SaveAssets();
    }

    private void GenerateScaleSequence(VCOSequence sequence)
    {
        sequence.steps.Clear();

        MusicUtils.Note[] scaleNotes = MusicUtils.GetScaleNotes(rootNote, scaleType);
        
        // Get the VCO component to access globalOctaveShift
        VCO vco = FindObjectOfType<VCO>();
        int globalOctaveShift = vco != null ? vco.globalOctaveShift : 0;

        // Generate two octaves of the scale
        for (int octave = minOctave; octave <= minOctave + 1; octave++)
        {
            for (int i = 0; i < scaleNotes.Length; i++)
            {
                VCO.Step step = new VCO.Step
                {
                    useNote = true,
                    note = scaleNotes[i],
                    octave = octave,
                    slide = false,
                    accent = (i == 0) // Accent the root note
                };
                step.UpdatePitchFromNote(globalOctaveShift);
                sequence.steps.Add(step);
            }
        }

        EditorUtility.SetDirty(sequence);
        AssetDatabase.SaveAssets();
    }
} 