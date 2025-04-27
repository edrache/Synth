using UnityEngine;

public class TextureEffect : MonoBehaviour
{
    [Header("Texture Settings")]
    [Range(0f, 1f)] public float amount = 0.5f;
    [Range(0.1f, 100f)] public float rate = 10f;
    [Range(0f, 1f)] public float depth = 0.5f;
    [Range(0f, 1f)] public float smoothness = 0.5f;

    private float phase;
    private ModularSynth synth;
    private bool isActive = false;
    private float sampleRate;
    private float rateStep;

    private void Awake()
    {
        synth = GetComponent<ModularSynth>();
        if (synth == null)
        {
            Debug.LogError("TextureEffect requires ModularSynth component!");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        isActive = true;
        sampleRate = AudioSettings.outputSampleRate;
        rateStep = rate / sampleRate;
    }

    private void OnDisable()
    {
        isActive = false;
    }

    public float ProcessSample(float sample)
    {
        if (!isActive || amount <= 0f) return sample;

        // Generate texture signal
        phase += rateStep;
        if (phase >= 1f) phase -= 1f;
        
        // Use smooth noise for texture
        float texture = Mathf.PerlinNoise(phase * 100f, 0f) * 2f - 1f;
        texture = Mathf.Lerp(texture, Mathf.Sin(phase * Mathf.PI * 2f), smoothness);
        
        // Apply texture
        float modulated = sample * (1f + texture * depth * amount);
        return Mathf.Lerp(sample, modulated, amount);
    }

    private void OnValidate()
    {
        if (synth == null)
            synth = GetComponent<ModularSynth>();
        
        if (Application.isPlaying && isActive)
        {
            sampleRate = AudioSettings.outputSampleRate;
            rateStep = rate / sampleRate;
        }
    }
} 