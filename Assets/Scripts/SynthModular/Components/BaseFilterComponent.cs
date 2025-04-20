using UnityEngine;

public abstract class BaseFilterComponent : MonoBehaviour
{
    protected float sampleRate;

    protected virtual void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    public abstract float ProcessSample(float input);
} 