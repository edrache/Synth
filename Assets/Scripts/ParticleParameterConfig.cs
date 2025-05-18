using UnityEngine;

[System.Serializable]
public class ParticleParameterConfig
{
    [SerializeField] private bool enabled = true;
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private bool useRandomRange = false;
    [SerializeField] private AnimationCurve curve2 = AnimationCurve.Linear(0, 0, 1, 1);

    public bool Enabled => enabled;
    public bool UseRandomRange => useRandomRange;

    public float Evaluate(float velocity)
    {
        if (!enabled) return 0f;
        
        if (useRandomRange)
        {
            float min = curve.Evaluate(velocity);
            float max = curve2.Evaluate(velocity);
            return Random.Range(min, max);
        }
        
        return curve.Evaluate(velocity);
    }

    public void CopyFrom(ParticleParameterConfig other)
    {
        enabled = other.enabled;
        curve = new AnimationCurve(other.curve.keys);
        useRandomRange = other.useRandomRange;
        curve2 = new AnimationCurve(other.curve2.keys);
    }
} 