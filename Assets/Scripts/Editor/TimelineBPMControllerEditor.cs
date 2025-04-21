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
        
        var newBPM = EditorGUILayout.FloatField("BPM", controller.BPM);
        if (newBPM < 1) newBPM = 1;
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(controller, "Change BPM");
            controller.BPM = newBPM;
            EditorUtility.SetDirty(controller);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "4.00 on timeline = 4 bars (16 beats)\n" +
            "Timeline speed is automatically adjusted based on BPM",
            MessageType.Info
        );
    }
} 