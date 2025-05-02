using UnityEngine;

[ExecuteAlways]
public class MaterialTilingFromSequencer : MonoBehaviour
{
    [Tooltip("Renderer z materiałem do modyfikacji (jeśli null, użyje MeshRenderer z tego obiektu)")]
    public Renderer targetRenderer;
    [Tooltip("Obiekt z komponentem SequencerSpace")]
    public SequencerSpace sequencerSpace;
    [Tooltip("Czy ustawiać tiling automatycznie w każdej klatce (w edytorze i w grze)?")]
    public bool liveUpdate = true;

    private void Start()
    {
        UpdateTiling();
    }

    private void Update()
    {
        if (liveUpdate)
            UpdateTiling();
    }

    [ContextMenu("Update Tiling Now")]
    public void UpdateTiling()
    {
        if (sequencerSpace == null)
        {
            Debug.LogError("Brak przypisanego SequencerSpace!");
            return;
        }
        Renderer rend = targetRenderer != null ? targetRenderer : GetComponent<MeshRenderer>();
        if (rend == null || rend.sharedMaterial == null)
        {
            Debug.LogError("Brak Renderer lub materiału!");
            return;
        }
        float tilingX = sequencerSpace.timelineLength;
        int[][] allIntervals = typeof(SequencerSpace).GetField("scaleIntervals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .GetValue(null) as int[][];
        int[] intervals = allIntervals[(int)sequencerSpace.scale];
        int notesPerOctave = intervals.Length;
        int totalNotes = (sequencerSpace.maxOctave - sequencerSpace.minOctave + 1) * notesPerOctave;
        float tilingY = totalNotes / 2f;
        Vector2 tiling = new Vector2(tilingX, tilingY);
        rend.sharedMaterial.mainTextureScale = tiling;
    }
} 