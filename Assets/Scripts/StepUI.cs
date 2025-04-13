using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StepUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField] private GameObject activeIndicator;
    [SerializeField] private GameObject accentIndicator;
    [SerializeField] private GameObject slideIndicator;
    [SerializeField] private TextMeshProUGUI octaveText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private Button durationButton;
    [SerializeField] private Button accentButton;
    [SerializeField] private Button slideButton;
    private VCO.Step currentStep;

    private void Awake()
    {
        if (noteText == null)
            Debug.LogError("StepUI: Brak przypisanego TextMeshProUGUI dla nuty!");
        if (activeIndicator == null)
            Debug.LogError("StepUI: Brak przypisanego wskaźnika aktywnego kroku!");
        if (accentIndicator == null)
            Debug.LogError("StepUI: Brak przypisanego wskaźnika akcentu!");
        if (slideIndicator == null)
            Debug.LogError("StepUI: Brak przypisanego wskaźnika slide!");
        if (octaveText == null)
            Debug.LogError("StepUI: Brak przypisanego TextMeshProUGUI dla oktawy!");
        if (durationText == null)
            Debug.LogError("StepUI: Brak przypisanego TextMeshProUGUI dla długości nuty!");
        if (durationButton == null)
            Debug.LogError("StepUI: Brak przypisanego przycisku dla długości nuty!");
        else
            durationButton.onClick.AddListener(CycleDuration);
            
        if (accentButton == null)
            Debug.LogError("StepUI: Brak przypisanego przycisku dla akcentu!");
        else
            accentButton.onClick.AddListener(ToggleAccent);
            
        if (slideButton == null)
            Debug.LogError("StepUI: Brak przypisanego przycisku dla slide!");
        else
            slideButton.onClick.AddListener(ToggleSlide);
    }

    public void UpdateStepInfo(VCO.Step step)
    {
        currentStep = step;
        // Ustaw tekst nuty
        if (noteText != null)
        {
            string noteName = step.note.ToString().Replace("Sharp", "#");
            noteText.text = noteName;
        }

        // Ustaw wskaźnik akcentu
        if (accentIndicator != null)
        {
            accentIndicator.SetActive(step.accent);
        }

        // Ustaw wskaźnik slide
        if (slideIndicator != null)
        {
            slideIndicator.SetActive(step.slide);
        }

        // Ustaw tekst oktawy
        if (octaveText != null)
        {
            octaveText.text = step.octave.ToString();
        }

        // Ustaw tekst długości nuty
        if (durationText != null)
        {
            string durationSymbol = GetDurationSymbol(step.duration);
            durationText.text = durationSymbol;
        }
    }

    private void CycleDuration()
    {
        if (currentStep != null)
        {
            currentStep.duration = (currentStep.duration + 1f) % 5f;
            UpdateStepInfo(currentStep);
        }
    }

    private void ToggleAccent()
    {
        if (currentStep != null)
        {
            currentStep.accent = !currentStep.accent;
            UpdateStepInfo(currentStep);
        }
    }

    private void ToggleSlide()
    {
        if (currentStep != null)
        {
            currentStep.slide = !currentStep.slide;
            UpdateStepInfo(currentStep);
        }
    }

    private string GetDurationSymbol(float duration)
    {
        if (duration == 0f) return "×"; // wyciszona
        if (duration == 1f) return "♬"; // szesnastka
        if (duration == 2f) return "♪"; // ósemka
        if (duration == 3f) return "♪."; // ósemka z kropką
        if (duration == 4f) return "♩"; // ćwierćnuta
        return "-";
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