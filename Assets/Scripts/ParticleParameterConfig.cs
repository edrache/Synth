using UnityEngine;

[System.Serializable]
public class ParticleParameterConfig
{
    [SerializeField] private bool enabled = true;
    [SerializeField] private AnimationCurve curve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private bool useRandomCurve = false;
    [SerializeField] private AnimationCurve randomCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    public bool Enabled => enabled;
    public AnimationCurve Curve => curve;
    public float MinValue => minValue;
    public float MaxValue => maxValue;
    public bool UseRandomCurve => useRandomCurve;
    public AnimationCurve RandomCurve => randomCurve;

    public float Evaluate(float time, float randomValue = 0.5f)
    {
        if (!enabled) return 0f;

        float baseValue = curve.Evaluate(time);
        float value = Mathf.Lerp(minValue, maxValue, baseValue);

        if (useRandomCurve)
        {
            float randomFactor = randomCurve.Evaluate(randomValue);
            value *= randomFactor;
        }

        return value;
    }

    public void CopyFrom(ParticleParameterConfig source)
    {
        if (source == null) return;

        enabled = source.enabled;
        minValue = source.minValue;
        maxValue = source.maxValue;
        useRandomCurve = source.useRandomCurve;

        // Deep copy of curves
        curve = new AnimationCurve(source.curve.keys);
        randomCurve = new AnimationCurve(source.randomCurve.keys);
    }
} 