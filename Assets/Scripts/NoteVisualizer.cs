using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CityNote))]
public class NoteVisualizer : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private RectTransform visualPrefab;
    [SerializeField] private TextMeshProUGUI noteText;

    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;

    [Header("Position Settings")]
    [SerializeField] private float margin = 10f; // Margin from edges of the parent RectTransform
    [SerializeField] private float minZ = -1f; // Minimum Z position
    [SerializeField] private float maxZ = 1f; // Maximum Z position

    private CityNote cityNote;
    private RectTransform rectTransform;
    private RectTransform container;
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

        if (noteText == null)
        {
            Debug.LogError("[NoteVisualizer] No TextMeshProUGUI component assigned!");
            enabled = false;
            return;
        }

        // Find or create container
        container = transform.Find("Container") as RectTransform;
        if (container == null)
        {
            GameObject containerObj = new GameObject("Container");
            container = containerObj.AddComponent<RectTransform>();
            container.SetParent(transform, false);
            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            container.localScale = Vector3.one;
            container.anchorMin = Vector2.zero;
            container.anchorMax = Vector2.one;
            container.sizeDelta = Vector2.zero;
        }
    }

    private void Start()
    {
        UpdateVisuals();
        UpdateNoteText();
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
        foreach (Transform child in container)
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
        RectTransform instance = Instantiate(visualPrefab, container);
        
        // Set random scale (same for all axes)
        float randomScale = Random.Range(minScale, maxScale);
        instance.localScale = new Vector3(randomScale, randomScale, randomScale);

        // Calculate random position within margins
        float maxX = (rectTransform.rect.width / 2) - margin;
        float maxY = (rectTransform.rect.height / 2) - margin;
        
        float randomX = Random.Range(-maxX, maxX);
        float randomY = Random.Range(-maxY, maxY);
        float randomZ = Random.Range(minZ, maxZ);
        
        instance.anchoredPosition = new Vector2(randomX, randomY);
        instance.localPosition = new Vector3(instance.localPosition.x, instance.localPosition.y, randomZ);
    }

    private void UpdateNoteText()
    {
        // Get current note letter
        string currentNote = GetNoteLetter(cityNote.pitch);
        
        // Calculate the value
        float frequency = Mathf.Pow(2, (cityNote.pitch - 69) / 12f) * 440f; // Convert MIDI to frequency
        int value = Mathf.CeilToInt(frequency * cityNote.velocity * cityNote.duration * 100f);
        
        // Get the note letter for pitch + 3 and + 6
        string noteX3 = GetNoteLetter(cityNote.pitch + 3);
        string noteY6 = GetNoteLetter(cityNote.pitch + 6);
        
        // Set the text
        noteText.text = $"{currentNote}-{value}-{noteX3}.{noteY6}";
    }

    private string GetNoteLetter(int midiNote)
    {
        string[] noteNames = { "C", "C0", "D", "D0", "E", "F", "F0", "G", "G0", "A", "A0", "B" };
        return noteNames[midiNote % 12];
    }
} 