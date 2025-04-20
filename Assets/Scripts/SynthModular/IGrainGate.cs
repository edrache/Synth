using UnityEngine;

public interface IGrainGate
{
    float Apply(float input, float step);
    GrainGateSettings GetSettings();
    void SetSettings(GrainGateSettings settings);
}