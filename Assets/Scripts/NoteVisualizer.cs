using UnityEngine;

[RequireComponent(typeof(CityNote))]
public class NoteVisualizer : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f); // (X, Y, Z) min scale
    [SerializeField] private Vector3 maxScale = new Vector3(2.0f, 2.0f, 2.0f); // (X, Y, Z) max scale
    [SerializeField] private float minPitch = 0f;   // Najniższa wysokość dźwięku (np. 0)
    [SerializeField] private float maxPitch = 127f; // Najwyższa wysokość dźwięku (127 dla MIDI)
    [SerializeField] private bool useUniformScale = true; // Czy używać tej samej skali dla wszystkich osi

    [Header("Animation Settings")]
    [SerializeField] private float scaleChangeSpeed = 5f; // Szybkość zmiany skali
    [SerializeField] private bool smoothScaleChange = true; // Czy płynnie zmieniać skalę

    private CityNote note;
    private Vector3 targetScale;
    private Vector3 currentScale;

    private void Start()
    {
        note = GetComponent<CityNote>();
        if (note == null)
        {
            Debug.LogError("[NoteVisualizer] CityNote component not found!");
            enabled = false;
            return;
        }

        // Inicjalizacja skali
        currentScale = transform.localScale;
        UpdateTargetScale();
    }

    private void Update()
    {
        // Aktualizuj docelową skalę
        UpdateTargetScale();

        // Płynnie zmieniaj skalę
        if (smoothScaleChange)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleChangeSpeed);
        }
        else
        {
            transform.localScale = targetScale;
        }
    }

    private void UpdateTargetScale()
    {
        float pitch = note.pitch;
        
        // Normalizuj pitch do zakresu 0-1
        float normalizedPitch = Mathf.InverseLerp(minPitch, maxPitch, pitch);
        
        if (useUniformScale)
        {
            // Użyj średniej z min i max skali dla jednolitej skali
            float avgMinScale = (minScale.x + minScale.y + minScale.z) / 3f;
            float avgMaxScale = (maxScale.x + maxScale.y + maxScale.z) / 3f;
            float scale = Mathf.Lerp(avgMinScale, avgMaxScale, normalizedPitch);
            targetScale = new Vector3(scale, scale, scale);
        }
        else
        {
            // Oblicz skalę osobno dla każdej osi
            float scaleX = Mathf.Lerp(minScale.x, maxScale.x, normalizedPitch);
            float scaleY = Mathf.Lerp(minScale.y, maxScale.y, normalizedPitch);
            float scaleZ = Mathf.Lerp(minScale.z, maxScale.z, normalizedPitch);
            targetScale = new Vector3(scaleX, scaleY, scaleZ);
        }
    }

    // Metoda do ręcznej aktualizacji skali
    public void ForceUpdateScale()
    {
        UpdateTargetScale();
        transform.localScale = targetScale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Upewnij się, że wartości są poprawne
        minScale.x = Mathf.Max(0.01f, minScale.x);
        minScale.y = Mathf.Max(0.01f, minScale.y);
        minScale.z = Mathf.Max(0.01f, minScale.z);
        maxScale.x = Mathf.Max(minScale.x, maxScale.x);
        maxScale.y = Mathf.Max(minScale.y, maxScale.y);
        maxScale.z = Mathf.Max(minScale.z, maxScale.z);
        minPitch = Mathf.Clamp(minPitch, 0, 127);
        maxPitch = Mathf.Clamp(maxPitch, minPitch, 127);
        scaleChangeSpeed = Mathf.Max(0.1f, scaleChangeSpeed);
    }
#endif
} 