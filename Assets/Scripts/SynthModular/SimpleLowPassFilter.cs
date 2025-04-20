public class SimpleLowPassFilter : ILowPassFilter
{
    private float smoothing;
    private float previous = 0f;

    public SimpleLowPassFilter(float smoothing)
    {
        this.smoothing = smoothing;
    }

    public float Apply(float sample)
    {
        float output = (1f - smoothing) * previous + smoothing * sample;
        previous = output;
        return output;
    }

    public FilterSettings GetSettings()
    {
        return new FilterSettings
        {
            Smoothing = this.smoothing
        };
    }

    public void SetSettings(FilterSettings settings)
    {
        this.smoothing = settings.Smoothing;
    }
}