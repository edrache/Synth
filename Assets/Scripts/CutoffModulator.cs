using UnityEngine;
using UnityEngine.Events;

public class CutoffModulator : MonoBehaviour
{
    [System.Serializable]
    public class CutoffModulationEvent : UnityEvent<float> { }

    public AnimationCurve cutoffCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [Range(20f, 20000f)]
    public float minCutoff = 20f;
    [Range(20f, 20000f)]
    public float maxCutoff = 20000f;
    [Range(1, 4)]
    public int bars = 1;

    public CutoffModulationEvent onCutoffModulation;

    private float modulationTimer = 0f;
    private float modulationLength;
    private bool isModulating = false;
    private bool isWaitingForStep = false;
    private float targetBPM;
    private VCO vco;
    private int lastStep = -1;

    void Start()
    {
        vco = GetComponent<VCO>();
    }

    public void StartModulation(float bpm)
    {
        targetBPM = bpm;
        // Oblicz długość modulacji w sekundach: (60/bpm) * 4 * bars
        // gdzie 60/bpm to długość ćwierćnuty, 4 to liczba ćwierćnut w takcie
        modulationLength = (60f / bpm) * 4f * bars;
        
        if (vco != null)
        {
            int currentStep = vco.GetCurrentStep();
            if (currentStep != 0)
            {
                // Jeśli nie jesteśmy na pierwszym kroku, czekamy na następny takt
                isWaitingForStep = true;
                lastStep = currentStep;
                Debug.Log($"Waiting for step 0, current step: {currentStep}");
            }
            else
            {
                // Jeśli jesteśmy na pierwszym kroku, zaczynamy od razu
                StartModulationNow();
            }
        }
        else
        {
            // Jeśli nie mamy VCO, zaczynamy od razu
            StartModulationNow();
        }
    }

    private void StartModulationNow()
    {
        modulationTimer = 0f;
        isModulating = true;
        isWaitingForStep = false;
        Debug.Log($"Starting modulation at step 0, duration: {modulationLength:F2} seconds");
    }

    void Update()
    {
        if (isWaitingForStep && vco != null)
        {
            int currentStep = vco.GetCurrentStep();
            // Sprawdzamy czy wróciliśmy do pierwszego kroku
            if (currentStep == 0 && lastStep != 0)
            {
                StartModulationNow();
            }
            lastStep = currentStep;
        }

        if (isModulating)
        {
            modulationTimer += Time.deltaTime;
            
            if (modulationTimer >= modulationLength)
            {
                // At the end of modulation, keep the last value
                float finalValue = cutoffCurve.Evaluate(1f);
                float finalCutoff = Mathf.Lerp(minCutoff, maxCutoff, finalValue);
                onCutoffModulation.Invoke(finalCutoff);
                isModulating = false;
            }
            else
            {
                // During modulation, evaluate the curve
                float normalizedTime = modulationTimer / modulationLength;
                float curveValue = cutoffCurve.Evaluate(normalizedTime);
                float currentCutoff = Mathf.Lerp(minCutoff, maxCutoff, curveValue);
                onCutoffModulation.Invoke(currentCutoff);
            }
        }
    }
} 