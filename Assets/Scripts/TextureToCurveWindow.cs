using UnityEngine;
using UnityEditor;

public class TextureToCurveWindow : EditorWindow
{
    public Texture2D sourceTexture;
    public AnimationCurve generatedCurve = new AnimationCurve();
    public int resolution = 256;

    [MenuItem("Tools/Convert Texture to Animation Curve")]
    public static void OpenWindow()
    {
        GetWindow<TextureToCurveWindow>("Texture to Curve");
    }

    private void OnGUI()
    {
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);
        resolution = EditorGUILayout.IntSlider("Resolution", resolution, 16, 1024);

        if (GUILayout.Button("Generate Curve") && sourceTexture != null)
        {
            generatedCurve = GenerateCurveFromTexture(sourceTexture, resolution);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview:");
        EditorGUILayout.CurveField(generatedCurve);
    }

    private AnimationCurve GenerateCurveFromTexture(Texture2D tex, int resolution)
    {
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            int x = Mathf.RoundToInt(t * (tex.width - 1));
            Color color = tex.GetPixel(x, tex.height / 2); // środek obrazu
            float value = color.r; // zakładamy grayscale lub kanał R jako sygnał

            curve.AddKey(t, value);
        }

        return curve;
    }
}
