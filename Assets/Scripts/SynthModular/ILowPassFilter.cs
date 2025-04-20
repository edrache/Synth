public interface ILowPassFilter
{
    float Apply(float sample);
    FilterSettings GetSettings();
    void SetSettings(FilterSettings settings);
}