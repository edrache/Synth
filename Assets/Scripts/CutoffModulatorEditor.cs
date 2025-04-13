using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CutoffModulator))]
public class CutoffModulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CutoffModulator modulator = (CutoffModulator)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modulation Preview", EditorStyles.boldLabel);
        modulator.cutoffCurve = EditorGUILayout.CurveField("Cutoff Curve", modulator.cutoffCurve, Color.green, new Rect(0f, 0f, 1f, 1f));
        EditorGUILayout.HelpBox("The curve controls the cutoff frequency over the specified number of bars. Y axis values are normalized between 0 and 1.", MessageType.Info);
    }
} 