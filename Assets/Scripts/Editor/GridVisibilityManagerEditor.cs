using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridVisibilityManager))]
public class GridVisibilityManagerEditor : Editor
{
    private SerializedProperty visibilityLayersProp;
    private SerializedProperty showDiagonalsProp;
    private SerializedProperty fadeObjectsProp;
    private SerializedProperty fadeDurationProp;

    private void OnEnable()
    {
        visibilityLayersProp = serializedObject.FindProperty("visibilityLayers");
        showDiagonalsProp = serializedObject.FindProperty("showDiagonals");
        fadeObjectsProp = serializedObject.FindProperty("fadeObjects");
        fadeDurationProp = serializedObject.FindProperty("fadeDuration");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(visibilityLayersProp, true);
        EditorGUILayout.PropertyField(showDiagonalsProp);
        EditorGUILayout.PropertyField(fadeObjectsProp);
        EditorGUILayout.PropertyField(fadeDurationProp);

        serializedObject.ApplyModifiedProperties();
    }
} 