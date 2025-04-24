using UnityEngine;

public class CustomWaveOscillator : IOscillator
{
    private const int WAVETABLE_SIZE = 1024;
    private float[] waveTable;
    private AnimationCurve waveCurve;

    public CustomWaveOscillator(AnimationCurve curve = null)
    {
        waveTable = new float[WAVETABLE_SIZE];
        waveCurve = curve ?? new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 1f),
            new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, -1f),
            new Keyframe(1f, 0f)
        );
        UpdateWaveTable();
    }

    public void SetCurve(AnimationCurve newCurve)
    {
        waveCurve = newCurve;
        UpdateWaveTable();
    }

    private void UpdateWaveTable()
    {
        for (int i = 0; i < WAVETABLE_SIZE; i++)
        {
            float t = i / (float)WAVETABLE_SIZE;
            waveTable[i] = waveCurve.Evaluate(t);
        }
    }

    public float GetSample(float phase)
    {
        // Normalize phase to 0-1 range
        float normalizedPhase = (phase % (2f * Mathf.PI)) / (2f * Mathf.PI);
        
        // Get the two nearest samples
        float position = normalizedPhase * WAVETABLE_SIZE;
        int index1 = Mathf.FloorToInt(position) % WAVETABLE_SIZE;
        int index2 = (index1 + 1) % WAVETABLE_SIZE;
        
        // Linear interpolation between samples
        float t = position - Mathf.Floor(position);
        return Mathf.Lerp(waveTable[index1], waveTable[index2], t);
    }

    public void Reset()
    {
        // Custom oscillator doesn't need to reset any state
    }
} 