using UnityEngine;

public class MorphingOscillator : IOscillator
{
    private readonly IOscillator[] oscillators;
    private readonly float morphSpeed;
    private readonly bool[] enabledOscillators;
    private float phase;
    private float morphPhase;

    [System.Serializable]
    public class OscillatorSlot
    {
        public bool enabled = true;
        public OscillatorType type = OscillatorType.Sine;
    }

    public enum OscillatorType
    {
        Sine,
        Square,
        Saw,
        Triangle,
        Noise,
        HalfSine,
        Pulse
    }

    public OscillatorSlot[] oscillatorSlots = new OscillatorSlot[4];

    public MorphingOscillator(float morphSpeed = 0.1f, OscillatorSlot[] slots = null)
    {
        this.morphSpeed = morphSpeed;
        
        if (slots == null || slots.Length == 0)
        {
            oscillatorSlots = new OscillatorSlot[4];
            for (int i = 0; i < 4; i++)
            {
                oscillatorSlots[i] = new OscillatorSlot();
            }
        }
        else
        {
            oscillatorSlots = slots;
        }

        oscillators = new IOscillator[oscillatorSlots.Length];
        UpdateOscillators();
    }

    private void UpdateOscillators()
    {
        for (int i = 0; i < oscillatorSlots.Length; i++)
        {
            oscillators[i] = CreateOscillator(oscillatorSlots[i].type);
        }
    }

    private IOscillator CreateOscillator(OscillatorType type)
    {
        return type switch
        {
            OscillatorType.Sine => new SineOscillator(),
            OscillatorType.Square => new SquareOscillator(),
            OscillatorType.Saw => new SawOscillator(),
            OscillatorType.Triangle => new TriangleOscillator(),
            OscillatorType.Noise => new NoiseOscillator(),
            OscillatorType.HalfSine => new HalfSineOscillator(),
            OscillatorType.Pulse => new PulseOscillator(0.5f),
            _ => new SineOscillator()
        };
    }

    public float GetSample(float phase)
    {
        // Update morph phase based on the input phase
        morphPhase += morphSpeed * 0.0001f;
        if (morphPhase >= 1f) morphPhase -= 1f;

        float total = 0f;
        int activeCount = 0;

        for (int i = 0; i < oscillators.Length; i++)
        {
            if (oscillatorSlots[i].enabled)
            {
                float weight = Mathf.Cos(2f * Mathf.PI * (morphPhase + i * 0.25f));
                weight = (weight + 1f) * 0.5f; // Normalize to 0-1
                total += oscillators[i].GetSample(phase) * weight;
                activeCount++;
            }
        }

        return activeCount > 0 ? total / activeCount : 0f;
    }

    public void Reset()
    {
        phase = 0f;
        morphPhase = 0f;
        foreach (var osc in oscillators)
        {
            osc.Reset();
        }
    }
} 