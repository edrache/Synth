using UnityEngine;
using System.Collections.Generic;

public class SequenceUI : MonoBehaviour
{
    [SerializeField] private VCO vcoSource;
    [SerializeField] private GameObject stepUIPrefab;
    [SerializeField] private Transform stepsContainer;

    private List<StepUI> stepUIElements = new List<StepUI>();
    private int currentStepIndex = -1;

    private void Start()
    {
        if (vcoSource == null)
        {
            vcoSource = FindFirstObjectByType<VCO>();
            if (vcoSource == null)
            {
                Debug.LogError("SequenceUI: Nie znaleziono źródła VCO w scenie!");
                return;
            }
        }

        if (stepUIPrefab == null)
        {
            Debug.LogError("SequenceUI: Brak przypisanego prefabu StepUI!");
            return;
        }

        if (stepsContainer == null)
        {
            Debug.LogError("SequenceUI: Brak przypisanego kontenera dla kroków!");
            return;
        }

        // Subskrybuj zdarzenia z VCO
        vcoSource.OnStepChanged += HandleStepChanged;

        // Utwórz UI dla wszystkich kroków
        CreateStepUIElements();
    }

    private void OnDestroy()
    {
        if (vcoSource != null)
        {
            vcoSource.OnStepChanged -= HandleStepChanged;
        }
    }

    private void CreateStepUIElements()
    {
        // Wyczyść istniejące elementy
        foreach (Transform child in stepsContainer)
        {
            Destroy(child.gameObject);
        }
        stepUIElements.Clear();

        // Utwórz nowe elementy UI
        if (vcoSource.currentSequence != null)
        {
            for (int i = 0; i < vcoSource.currentSequence.steps.Count; i++)
            {
                GameObject stepUIObject = Instantiate(stepUIPrefab, stepsContainer);
                StepUI stepUI = stepUIObject.GetComponent<StepUI>();
                
                if (stepUI != null)
                {
                    stepUIElements.Add(stepUI);
                    // Ustaw tekst dla kroku
                    UpdateStepUIText(i);
                }
                else
                {
                    Debug.LogError($"SequenceUI: Prefab StepUI nie zawiera komponentu StepUI! (krok {i})");
                }
            }
        }
        else
        {
            Debug.LogError("SequenceUI: Brak przypisanej sekwencji w VCO!");
        }
    }

    private void HandleStepChanged(float pitch, int stepNumber)
    {
        // Wyłącz wskaźnik poprzedniego kroku
        if (currentStepIndex >= 0 && currentStepIndex < stepUIElements.Count)
        {
            stepUIElements[currentStepIndex].SetStepActive(false);
        }

        // Włącz wskaźnik nowego kroku
        if (stepNumber >= 0 && stepNumber < stepUIElements.Count)
        {
            stepUIElements[stepNumber].SetStepActive(true);
            currentStepIndex = stepNumber;
        }

        // Aktualizuj tekst dla wszystkich kroków
        for (int i = 0; i < stepUIElements.Count; i++)
        {
            UpdateStepUIText(i);
        }
    }

    private void UpdateStepUIText(int stepIndex)
    {
        if (stepIndex >= 0 && stepIndex < stepUIElements.Count && 
            vcoSource.currentSequence != null && 
            stepIndex < vcoSource.currentSequence.steps.Count)
        {
            VCO.Step step = vcoSource.currentSequence.steps[stepIndex];
            stepUIElements[stepIndex].UpdateStepInfo(step);
        }
    }
} 