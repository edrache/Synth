using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class SoundStateTransition
{
    [Header("Transition Settings")]
    public string transitionName;
    public float transitionDuration = 1f;
    public Ease transitionEase = Ease.InOutQuad;
    
    [Header("State Change Flags")]
    [Tooltip("If true, sound state (curves) will be changed during this transition")]
    public bool changeSoundState = true;
    [Tooltip("If true, mixer state will be changed during this transition")]
    public bool changeMixerState = true;
    
    [Header("States")]
    [Tooltip("Target sound state (curves) for this transition")]
    public SoundState toSoundState;
    [Tooltip("Target mixer state for this transition")]
    public MixerState toMixerState;

    [Header("Loop Settings")]
    public int targetLoopNumber;
    public bool isCyclic = true;

    public SoundStateTransition Clone()
    {
        return new SoundStateTransition
        {
            transitionName = transitionName,
            transitionDuration = transitionDuration,
            transitionEase = transitionEase,
            changeSoundState = changeSoundState,
            changeMixerState = changeMixerState,
            toSoundState = toSoundState?.Clone(),
            toMixerState = toMixerState?.Clone(),
            targetLoopNumber = targetLoopNumber,
            isCyclic = isCyclic
        };
    }
} 