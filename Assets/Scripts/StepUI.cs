using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StepUI : MonoBehaviour
{
    [Header("VCO Reference")]
    [SerializeField] private VCO vcoSource;

    [Header("Step References")]
    [SerializeField] private Button durationButton;
    [SerializeField] private Button accentButton;
    [SerializeField] private Button slideButton;
    [SerializeField] private Image currentStepIndicator;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI noteText;

    [Header("Note Selection")]
    [SerializeField] private Button noteButton;
    [SerializeField] private GameObject noteSelectionPanel;
    [SerializeField] private List<Button> noteButtons;
    [SerializeField] private List<string> noteNames = new List<string> { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    private int stepIndex;
    private int currentNote = -1; // -1 means no note
    private int currentDuration = 1;
    private bool hasAccent = false;
    private bool hasSlide = false;
    private bool isCurrentStep = false;

    private void Awake()
    {
        if (vcoSource == null)
        {
            vcoSource = FindFirstObjectByType<VCO>();
            if (vcoSource == null)
            {
                Debug.LogError("StepUI: Nie znaleziono źródła VCO w scenie!");
            }
        }

        ValidateReferences();
        SetupButtonListeners();
        SetupNoteButtons();
    }

    private void ValidateReferences()
    {
        if (durationButton == null)
            Debug.LogError("StepUI: Brak przypisanego przycisku długości!");
        if (accentButton == null)
            Debug.LogError("StepUI: Brak przypisanego przycisku akcentu!");
        if (slideButton == null)
            Debug.LogError("StepUI: Brak przypisanego przycisku slide!");
        if (currentStepIndicator == null)
            Debug.LogError("StepUI: Brak przypisanego wskaźnika aktualnego kroku!");
        if (durationText == null)
            Debug.LogError("StepUI: Brak przypisanego tekstu długości!");
        if (noteText == null)
            Debug.LogError("StepUI: Brak przypisanego tekstu nuty!");
        if (noteButton == null)
            Debug.LogError("StepUI: Brak przypisanego przycisku nuty!");
        if (noteSelectionPanel == null)
            Debug.LogError("StepUI: Brak przypisanego panelu wyboru nuty!");
    }

    private void SetupButtonListeners()
    {
        if (durationButton != null)
            durationButton.onClick.AddListener(CycleDuration);
        if (accentButton != null)
            accentButton.onClick.AddListener(ToggleAccent);
        if (slideButton != null)
            slideButton.onClick.AddListener(ToggleSlide);
        if (noteButton != null)
            noteButton.onClick.AddListener(ToggleNoteSelection);
    }

    private void SetupNoteButtons()
    {
        if (noteButtons == null || noteButtons.Count == 0)
        {
            Debug.LogError("StepUI: Brak przypisanych przycisków nut!");
            return;
        }

        for (int i = 0; i < noteButtons.Count; i++)
        {
            int noteIndex = i; // Capture the index for the lambda
            noteButtons[i].onClick.AddListener(() => SelectNote(noteIndex));
            
            // Set button text
            TextMeshProUGUI buttonText = noteButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = noteNames[i];
            }
        }
    }

    private void ToggleNoteSelection()
    {
        if (noteSelectionPanel != null)
        {
            noteSelectionPanel.SetActive(!noteSelectionPanel.activeSelf);
        }
    }

    private void SelectNote(int noteIndex)
    {
        currentNote = noteIndex;
        UpdateStepInfo();
        ToggleNoteSelection(); // Hide the panel after selection

        // Update the sequence in VCO
        if (vcoSource != null && vcoSource.currentSequence != null && stepIndex < vcoSource.currentSequence.steps.Count)
        {
            VCO.Step step = vcoSource.currentSequence.steps[stepIndex];
            step.useNote = true;
            step.note = (MusicUtils.Note)noteIndex;
            step.UpdatePitchFromNote(vcoSource.globalOctaveShift);
        }
    }

    private void CycleDuration()
    {
        currentDuration = (currentDuration + 1) % 5; // Cycle through 0-4
        UpdateStepInfo();

        // Update the sequence in VCO
        if (vcoSource != null && vcoSource.currentSequence != null && stepIndex < vcoSource.currentSequence.steps.Count)
        {
            VCO.Step step = vcoSource.currentSequence.steps[stepIndex];
            step.duration = currentDuration;
        }
    }

    private void ToggleAccent()
    {
        hasAccent = !hasAccent;
        UpdateStepInfo();

        // Update the sequence in VCO
        if (vcoSource != null && vcoSource.currentSequence != null && stepIndex < vcoSource.currentSequence.steps.Count)
        {
            VCO.Step step = vcoSource.currentSequence.steps[stepIndex];
            step.accent = hasAccent;
        }
    }

    private void ToggleSlide()
    {
        hasSlide = !hasSlide;
        UpdateStepInfo();

        // Update the sequence in VCO
        if (vcoSource != null && vcoSource.currentSequence != null && stepIndex < vcoSource.currentSequence.steps.Count)
        {
            VCO.Step step = vcoSource.currentSequence.steps[stepIndex];
            step.slide = hasSlide;
        }
    }

    public void SetStepIndex(int index)
    {
        stepIndex = index;
        UpdateStepInfo();
    }

    public void SetNote(int note)
    {
        currentNote = note;
        UpdateStepInfo();
    }

    public void SetDuration(int duration)
    {
        currentDuration = duration;
        UpdateStepInfo();
    }

    public void SetAccent(bool accent)
    {
        hasAccent = accent;
        UpdateStepInfo();
    }

    public void SetSlide(bool slide)
    {
        hasSlide = slide;
        UpdateStepInfo();
    }

    public void SetIsCurrentStep(bool isCurrent)
    {
        isCurrentStep = isCurrent;
        UpdateStepInfo();
    }

    public void UpdateStepInfo()
    {
        // Update duration text
        if (durationText != null)
        {
            durationText.text = currentDuration.ToString();
        }

        // Update note text
        if (noteText != null)
        {
            noteText.text = currentNote >= 0 ? noteNames[currentNote] : "-";
        }

        // Update accent button color
        if (accentButton != null)
        {
            ColorBlock colors = accentButton.colors;
            colors.normalColor = hasAccent ? Color.yellow : Color.white;
            accentButton.colors = colors;
        }

        // Update slide button color
        if (slideButton != null)
        {
            ColorBlock colors = slideButton.colors;
            colors.normalColor = hasSlide ? Color.blue : Color.white;
            slideButton.colors = colors;
        }

        // Update current step indicator
        if (currentStepIndicator != null)
        {
            currentStepIndicator.enabled = isCurrentStep;
        }
    }

    public int GetStepIndex() => stepIndex;
    public int GetNote() => currentNote;
    public int GetDuration() => currentDuration;
    public bool HasAccent() => hasAccent;
    public bool HasSlide() => hasSlide;
} 