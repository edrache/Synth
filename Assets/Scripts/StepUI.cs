using UnityEngine;
using TMPro;

public class StepUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField] private GameObject activeIndicator;

    private void Awake()
    {
        if (noteText == null)
        {
            Debug.LogError("StepUI: Brak przypisanego TextMeshProUGUI!");
        }

        if (activeIndicator == null)
        {
            Debug.LogError("StepUI: Brak przypisanego wskaźnika aktywnego kroku!");
        }
    }

    public void SetNoteText(string text)
    {
        if (noteText != null)
        {
            noteText.text = text;
        }
    }

    public void SetStepActive(bool isActive)
    {
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isActive);
        }
    }

    private void OnDisable()
    {
        // Upewnij się, że wskaźnik jest wyłączony gdy obiekt jest dezaktywowany
        SetStepActive(false);
    }
} 