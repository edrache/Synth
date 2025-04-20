using System;

public static class OscillatorFactory
{
    public static IOscillator Create(string oscillatorType)
    {
        switch (oscillatorType)
        {
            case "SineOscillator":
                return new SineOscillator();
            case "SquareOscillator":
                return new SquareOscillator();
            case "SawtoothOscillator":
                return new SawtoothOscillator();
            // Add more cases for other oscillator types
            default:
                throw new ArgumentException($"Unknown oscillator type: {oscillatorType}");
        }
    }
} 