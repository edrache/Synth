using UnityEngine;
using UnityEngine.Audio;
using System;

[Serializable]
public class SoundState
{
    [Header("Curve States")]
    public AnimationCurve octaveTranspositionCurve;
    public AnimationCurve velocityCurve;
    
    [Header("State Info")]
    public string stateName;
    public string description;

    public SoundState()
    {
        // Initialize default curves
        octaveTranspositionCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(4f, 1f, 0f, 0f),
            new Keyframe(8f, -1f, 0f, 0f),
            new Keyframe(12f, 0f, 0f, 0f)
        );

        velocityCurve = new AnimationCurve(
            new Keyframe(0f, 0.5f, 0f, 0f),
            new Keyframe(4f, 1f, 0f, 0f),
            new Keyframe(8f, 0.2f, 0f, 0f),
            new Keyframe(12f, 0.5f, 0f, 0f)
        );
    }

    public SoundState Clone()
    {
        var clone = new SoundState
        {
            octaveTranspositionCurve = new AnimationCurve(octaveTranspositionCurve.keys),
            velocityCurve = new AnimationCurve(velocityCurve.keys),
            stateName = stateName,
            description = description
        };
        return clone;
    }
}

[Serializable]
public class MixerState
{
    [Header("Audio Mixer Parameters")]
    public AudioMixerSnapshot audioMixerSnapshot;
    
    [Header("State Info")]
    public string stateName;
    public string description;

    public MixerState Clone()
    {
        return new MixerState
        {
            audioMixerSnapshot = audioMixerSnapshot,
            stateName = stateName,
            description = description
        };
    }
} 