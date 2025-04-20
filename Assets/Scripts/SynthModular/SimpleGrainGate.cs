using UnityEngine;

public class SimpleGrainGate : IGrainGate
{
    private float baseRate;
    private float baseDuty;

    private float grainTimer;
    private float grainLength;
    private float dutyDuration;

    private bool isActive;

    private float jitterAmount;
    private System.Random rand;

    public SimpleGrainGate(float rateHz = 20f, float duty = 0.5f, float jitter = 0.05f)
    {
        baseRate = Mathf.Max(1f, rateHz);
        baseDuty = Mathf.Clamp01(duty);
        jitterAmount = Mathf.Clamp01(jitter);

        rand = new System.Random();
        ResetGrain();
    }

    void ResetGrain()
    {
        float jitteredRate = baseRate + (float)(rand.NextDouble() * 2 - 1) * baseRate * jitterAmount;
        grainLength = 1f / Mathf.Max(1f, jitteredRate);

        float jitteredDuty = baseDuty + (float)(rand.NextDouble() * 2 - 1) * jitterAmount;
        dutyDuration = Mathf.Clamp01(jitteredDuty) * grainLength;

        grainTimer = 0f;
        isActive = true;
    }

    public float Apply(float input, float step)
    {
        grainTimer += step;
        if (grainTimer >= grainLength)
            ResetGrain();

        return grainTimer < dutyDuration ? input : 0f;
    }

    public GrainGateSettings GetSettings()
    {
        return new GrainGateSettings
        {
            BaseRate = this.baseRate,
            BaseDuty = this.baseDuty,
            JitterAmount = this.jitterAmount
        };
    }

    public void SetSettings(GrainGateSettings settings)
    {
        this.baseRate = settings.BaseRate;
        this.baseDuty = settings.BaseDuty;
        this.jitterAmount = settings.JitterAmount;
        ResetGrain();
    }
}