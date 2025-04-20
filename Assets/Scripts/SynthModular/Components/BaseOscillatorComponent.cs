using UnityEngine;

public class BaseOscillatorComponent : MonoBehaviour
{
    [Header("Oscillator Settings")]
    [SerializeField] protected float frequency = 440f;
    [SerializeField] protected float amplitude = 1f;
    [SerializeField] protected float phaseOffset = 0f;
    
    protected float phase = 0f;
    protected float sampleRate;

    protected virtual void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    public virtual float GetSample(float phase)
    {
        return 0f;
    }

    public virtual float NextSample()
    {
        float increment = 2f * Mathf.PI * frequency / sampleRate;
        phase += increment;
        if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI;
        
        return GetSample(phase + phaseOffset) * amplitude;
    }

    public void SetFrequency(float newFrequency)
    {
        frequency = newFrequency;
    }

    public void SetAmplitude(float newAmplitude)
    {
        amplitude = Mathf.Clamp01(newAmplitude);
    }

    public float GetAmplitude()
    {
        return amplitude;
    }

    public void ResetPhase()
    {
        phase = 0f;
    }
} 