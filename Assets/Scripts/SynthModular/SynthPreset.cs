using System;

public class SynthPreset
{
    public float Frequency;
    public string OscillatorType;
    public string OscillatorTypeB;
    public float OscMix;
    public bool EnableGrainGate;
    public EnvelopeSettings EnvelopeSettings;
    public FilterSettings FilterSettings;
    public DistortionSettings DistortionSettings;
    public GrainGateSettings GrainGateSettings;
}

// Example settings classes
public class EnvelopeSettings
{
    public float Attack;
    public float Release;
    // Add properties for envelope settings
}

public class FilterSettings
{
    public float Smoothing;
    // Add other properties if needed
}

public class DistortionSettings
{
    public float Drive;
    // Add other properties if needed
}

public class GrainGateSettings
{
    public float BaseRate;
    public float BaseDuty;
    public float JitterAmount;
    // Add other properties if needed
} 