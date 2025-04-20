using UnityEngine;

public class LowPassFilterComponent : BaseFilterComponent
{
    [Header("Filter Settings")]
    [SerializeField] private float cutoffFrequency = 1000f;
    [Range(0f, 1f)] [SerializeField] private float resonance = 0.5f;

    private float prevSample = 0f;
    private float prevPrevSample = 0f;
    private float cutoff = 0f;
    private float feedback = 0f;

    protected override void Start()
    {
        base.Start();
        UpdateFilterParameters();
    }

    private void OnValidate()
    {
        UpdateFilterParameters();
    }

    private void UpdateFilterParameters()
    {
        // Ograniczenie częstotliwości do zakresu słyszalnego
        cutoffFrequency = Mathf.Clamp(cutoffFrequency, 20f, 20000f);

        // Obliczenie znormalizowanej częstotliwości (0-1)
        float fc = cutoffFrequency / sampleRate;
        
        // Obliczenie współczynnika cutoff (wzór na filtr biquad)
        float w0 = 2f * Mathf.PI * fc;
        float cosw0 = Mathf.Cos(w0);
        float alpha = Mathf.Sin(w0) / (2f * 0.707f);

        // Obliczenie współczynników filtra
        cutoff = (1f - cosw0) / 2f;
        feedback = resonance * (1.25f - 0.25f * cutoff);
    }

    public override float ProcessSample(float input)
    {
        // Implementacja filtra z poprawionymi współczynnikami
        float output = input * cutoff + 
                      prevSample * (1f - cutoff) + 
                      feedback * (prevSample - prevPrevSample);
        
        // Ograniczenie wyjścia
        output = Mathf.Clamp(output, -1f, 1f);
        
        // Aktualizacja poprzednich próbek
        prevPrevSample = prevSample;
        prevSample = output;
        
        return output;
    }

    public void SetCutoffFrequency(float frequency)
    {
        cutoffFrequency = frequency;
        UpdateFilterParameters();
    }

    public void SetResonance(float value)
    {
        resonance = Mathf.Clamp01(value);
        UpdateFilterParameters();
    }
} 