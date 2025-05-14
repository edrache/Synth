using UnityEngine;

[RequireComponent(typeof(CityNote))]
public class CityNoteRandomizer : MonoBehaviour
{
    [Header("Pitch Range")]
    [SerializeField] private int minPitch = 60; // Middle C
    [SerializeField] private int maxPitch = 72; // One octave up

    [Header("Velocity Range")]
    [SerializeField] private float minVelocity = 0.5f;
    [SerializeField] private float maxVelocity = 1.0f;

    [Header("Duration Range")]
    [SerializeField] private float minDuration = 0.1f;
    [SerializeField] private float maxDuration = 0.5f;

    [Header("Repeat Count Range")]
    [SerializeField] private int minRepeatCount = 0;
    [SerializeField] private int maxRepeatCount = 3;

    private CityNote cityNote;

    private void Awake()
    {
        cityNote = GetComponent<CityNote>();
        if (cityNote == null)
        {
            Debug.LogError("[CityNoteRandomizer] No CityNote component found!");
            return;
        }

        RandomizeValues();
    }

    public void RandomizeValues()
    {
        if (cityNote == null) return;

        // Randomize pitch
        int randomPitch = Random.Range(minPitch, maxPitch + 1);
        cityNote.pitch = randomPitch;

        // Randomize velocity
        float randomVelocity = Random.Range(minVelocity, maxVelocity);
        cityNote.velocity = randomVelocity;

        // Randomize duration
        float randomDuration = Random.Range(minDuration, maxDuration);
        cityNote.duration = randomDuration;

        // Randomize repeat count
        int randomRepeatCount = Random.Range(minRepeatCount, maxRepeatCount + 1);
        cityNote.repeatCount = randomRepeatCount;
    }

    // Context menu item to randomize values in editor
    [ContextMenu("Randomize Values")]
    private void RandomizeValuesInEditor()
    {
        if (cityNote == null)
        {
            cityNote = GetComponent<CityNote>();
        }
        RandomizeValues();
    }
} 