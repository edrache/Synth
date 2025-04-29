using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TimelineBPMController))]
public class TimelineBPMControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var controller = target as TimelineBPMController;
        if (controller == null) return;

        EditorGUI.BeginChangeCheck();
        
        // BPM Section
        EditorGUILayout.LabelField("Tempo", EditorStyles.boldLabel);
        var newBPM = EditorGUILayout.Slider("BPM", controller.BPM, 20f, 300f);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "4.00 on timeline = 4 bars (16 beats)\n" +
            "Timeline speed is automatically adjusted based on BPM",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        // Scale Section
        EditorGUILayout.LabelField("Musical Scale", EditorStyles.boldLabel);
        var newScale = (TimelineBPMController.MusicalScale)EditorGUILayout.EnumPopup("Scale", controller.Scale);
        var newRootNote = EditorGUILayout.IntSlider("Root Note", controller.RootNote, 24, 96);

        string rootNoteName = GetNoteNameFromMidi(controller.RootNote);
        EditorGUILayout.HelpBox(
            $"Current root note: {rootNoteName}\n" +
            "24 = C0, 60 = C4 (Middle C), 96 = C8",
            MessageType.Info
        );
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(controller, "Change Timeline BPM Controller Settings");
            controller.BPM = newBPM;
            controller.Scale = newScale;
            controller.RootNote = newRootNote;
            EditorUtility.SetDirty(controller);
        }
    }

    private string GetNoteNameFromMidi(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{noteNames[noteIndex]}{octave}";
    }
} 