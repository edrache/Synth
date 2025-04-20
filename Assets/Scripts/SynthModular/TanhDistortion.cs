using UnityEngine;
using System;

public class TanhDistortion : IDistortion
{
    private float drive;

    public TanhDistortion(float drive)
    {
        this.drive = drive;
    }

    public float Apply(float sample)
    {
        return (float)Math.Tanh(sample * drive);
    }

    public DistortionSettings GetSettings()
    {
        return new DistortionSettings
        {
            Drive = this.drive
        };
    }

    public void SetSettings(DistortionSettings settings)
    {
        this.drive = settings.Drive;
    }
}