using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CityNote))]
public class NoteVisualizer : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private RectTransform visualPrefab;

    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;

    [Header("Position Settings")]
    [SerializeField] private float margin = 10f; // Margin from edges of the parent RectTransform

    private CityNote cityNote;
    private RectTransform rectTransform;
    private int lastPitch = -1;

    private void Awake()
    {
        cityNote = GetComponent<CityNote>();
        rectTransform = GetComponent<RectTransform>();
        
        if (cityNote == null)
        {
            Debug.LogError("[NoteVisualizer] No CityNote component found!");
            enabled = false;
            return;
        }

        if (rectTransform == null)
        {
            Debug.LogError("[NoteVisualizer] No RectTransform component found!");
            enabled = false;
            return;
        }

        if (visualPrefab == null)
        {
            Debug.LogError("[NoteVisualizer] No visual prefab assigned!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void Update()
    {
        if (cityNote.pitch != lastPitch)
        {
            UpdateVisuals();
            lastPitch = cityNote.pitch;
        }
    }

    private void UpdateVisuals()
    {
        // Clear existing visuals
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Get note number in octave (1-12)
        int noteInOctave = (cityNote.pitch % 12) + 1;

        // Create new visuals
        for (int i = 0; i < noteInOctave; i++)
        {
            CreateVisualInstance();
        }
    }

    private void CreateVisualInstance()
    {
        // Instantiate the prefab
        RectTransform instance = Instantiate(visualPrefab, transform);
        
        // Set random scale (same for all axes)
        float randomScale = Random.Range(minScale, maxScale);
        instance.localScale = new Vector3(randomScale, randomScale, randomScale);

        // Calculate random position within margins
        float maxX = (rectTransform.rect.width / 2) - margin;
        float maxY = (rectTransform.rect.height / 2) - margin;
        
        float randomX = Random.Range(-maxX, maxX);
        float randomY = Random.Range(-maxY, maxY);
        
        instance.anchoredPosition = new Vector2(randomX, randomY);
    }
} 