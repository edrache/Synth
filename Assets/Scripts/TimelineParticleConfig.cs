using UnityEngine;

[System.Serializable]
public class TimelineParticleConfig
{
    [SerializeField] private ParticleParameterConfig sizeConfig = new ParticleParameterConfig();
    [SerializeField] private ParticleParameterConfig rateConfig = new ParticleParameterConfig();

    public ParticleParameterConfig SizeConfig => sizeConfig;
    public ParticleParameterConfig RateConfig => rateConfig;

    public void ApplyToParticleSystem(ParticleSystem particleSystem, float velocity)
    {
        if (particleSystem == null) return;

        var main = particleSystem.main;
        var emission = particleSystem.emission;

        if (sizeConfig.Enabled)
        {
            var startSize = main.startSize;
            startSize.constant = sizeConfig.Evaluate(velocity);
            main.startSize = startSize;
        }

        if (rateConfig.Enabled)
        {
            var rateOverTime = emission.rateOverTime;
            rateOverTime.constant = rateConfig.Evaluate(velocity);
            emission.rateOverTime = rateOverTime;
        }
    }

    public void CopyFrom(TimelineParticleConfig other)
    {
        sizeConfig.CopyFrom(other.sizeConfig);
        rateConfig.CopyFrom(other.rateConfig);
    }
} 