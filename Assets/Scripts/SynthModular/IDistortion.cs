public interface IDistortion
{
    float Apply(float sample);
    DistortionSettings GetSettings();
    void SetSettings(DistortionSettings settings);
}