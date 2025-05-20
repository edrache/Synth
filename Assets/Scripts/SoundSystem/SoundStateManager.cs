using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using DG.Tweening;

public class SoundStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CitySequencer citySequencer;
    [SerializeField] private LoopCounter loopCounter;
    [SerializeField] private AudioMixer audioMixer;

    [Header("State Sets")]
    [SerializeField] private List<SoundStateSet> stateSets = new List<SoundStateSet>();
    [SerializeField] private int currentStateSetIndex = 0;

    private SoundState currentSoundState;
    private MixerState currentMixerState;
    private SoundState nextSoundState;
    private MixerState nextMixerState;
    private Sequence currentTransition;
    private Tween currentMixerTransition;

    private void Start()
    {
        if (loopCounter != null)
        {
            // Subscribe to loop counter changes
            loopCounter.OnLoopCountChanged += HandleLoopCountChanged;
        }
        else
        {
            Debug.LogError("[SoundStateManager] LoopCounter reference is missing!");
        }

        // Initialize with first state
        if (stateSets.Count > 0 && stateSets[currentStateSetIndex].transitions.Count > 0)
        {
            var firstTransition = stateSets[currentStateSetIndex].transitions[0];
            currentSoundState = firstTransition.toSoundState;
            currentMixerState = firstTransition.toMixerState;
            ApplyState(currentSoundState, currentMixerState, 
                      firstTransition.changeSoundState, 
                      firstTransition.changeMixerState);
        }
    }

    private void OnDestroy()
    {
        if (loopCounter != null)
        {
            loopCounter.OnLoopCountChanged -= HandleLoopCountChanged;
        }

        // Kill any active transitions
        if (currentTransition != null)
        {
            currentTransition.Kill();
        }
        if (currentMixerTransition != null)
        {
            currentMixerTransition.Kill();
        }
    }

    private void HandleLoopCountChanged(int newLoopCount)
    {
        if (stateSets.Count == 0 || currentStateSetIndex >= stateSets.Count)
            return;

        var currentSet = stateSets[currentStateSetIndex];
        var transition = currentSet.GetTransitionForLoop(newLoopCount);
        
        if (transition != null)
        {
            StartTransition(transition);
        }
    }

    private void StartTransition(SoundStateTransition transition)
    {
        // Kill any active transitions
        if (currentTransition != null)
        {
            currentTransition.Kill();
        }
        if (currentMixerTransition != null)
        {
            currentMixerTransition.Kill();
        }

        // Set next states only if they should be changed
        if (transition.changeSoundState)
        {
            nextSoundState = transition.toSoundState;
        }
        if (transition.changeMixerState)
        {
            nextMixerState = transition.toMixerState;
        }

        // Create new transition sequence
        currentTransition = DOTween.Sequence();

        // Transition curves only if sound state should change
        if (transition.changeSoundState)
        {
            TransitionCurves(transition);
        }

        // Transition audio mixer only if mixer state should change
        if (transition.changeMixerState && 
            currentMixerState?.audioMixerSnapshot != null && 
            transition.toMixerState?.audioMixerSnapshot != null)
        {
            // Kill any existing mixer transition
            if (currentMixerTransition != null)
            {
                currentMixerTransition.Kill();
            }

            // Start new mixer transition
            transition.toMixerState.audioMixerSnapshot.TransitionTo(transition.transitionDuration);
            
            // Create a tween to track the mixer transition
            currentMixerTransition = DOTween.To(
                () => 0f,
                x => { }, // Empty update function as the transition is handled by Unity
                transition.transitionDuration,
                transition.transitionDuration
            ).SetEase(transition.transitionEase)
             .OnComplete(() => {
                 currentMixerTransition = null;
             });
        }

        string stateChanges = "";
        if (transition.changeSoundState) stateChanges += $"Sound: {currentSoundState.stateName} -> {nextSoundState.stateName} ";
        if (transition.changeMixerState) stateChanges += $"Mixer: {currentMixerState.stateName} -> {nextMixerState.stateName}";
        
        Debug.Log($"[SoundStateManager] Starting transition: {transition.transitionName} ({stateChanges}) Duration: {transition.transitionDuration}s");
    }

    private void TransitionCurves(SoundStateTransition transition)
    {
        // TODO: Implement curve transitions using DOTween
        // This will be implemented in the next step
    }

    private void ApplyState(SoundState soundState, MixerState mixerState, bool applySound = true, bool applyMixer = true)
    {
        // Apply curves to CitySequencer if sound state should be applied
        if (applySound && soundState != null && citySequencer != null)
        {
            citySequencer.SetOctaveCurve(soundState.octaveTranspositionCurve);
            citySequencer.SetVelocityCurve(soundState.velocityCurve);
        }

        // Apply audio mixer snapshot if mixer state should be applied
        if (applyMixer && mixerState?.audioMixerSnapshot != null)
        {
            // Kill any existing mixer transition
            if (currentMixerTransition != null)
            {
                currentMixerTransition.Kill();
                currentMixerTransition = null;
            }

            mixerState.audioMixerSnapshot.TransitionTo(0f);
        }
    }

    // Public methods for testing
    public void SetStateSet(int index)
    {
        if (index >= 0 && index < stateSets.Count)
        {
            currentStateSetIndex = index;
            // Reset to first state of new set
            if (stateSets[currentStateSetIndex].transitions.Count > 0)
            {
                var firstTransition = stateSets[currentStateSetIndex].transitions[0];
                currentSoundState = firstTransition.toSoundState;
                currentMixerState = firstTransition.toMixerState;
                ApplyState(currentSoundState, currentMixerState,
                          firstTransition.changeSoundState,
                          firstTransition.changeMixerState);
            }
        }
    }

    public void TriggerTransition(int transitionIndex)
    {
        if (stateSets.Count == 0 || currentStateSetIndex >= stateSets.Count)
            return;

        var currentSet = stateSets[currentStateSetIndex];
        if (transitionIndex >= 0 && transitionIndex < currentSet.transitions.Count)
        {
            StartTransition(currentSet.transitions[transitionIndex]);
        }
    }
}

[System.Serializable]
public class SoundStateSet
{
    public string setName;
    public List<SoundStateTransition> transitions = new List<SoundStateTransition>();

    public SoundStateTransition GetTransitionForLoop(int loopNumber)
    {
        if (transitions.Count == 0) return null;

        // Find transition that matches the loop number
        var transition = transitions.Find(t => t.targetLoopNumber == loopNumber);
        
        // If no exact match and transitions are cyclic, wrap around
        if (transition == null && transitions[0].isCyclic)
        {
            int wrappedLoop = loopNumber % transitions.Count;
            transition = transitions[wrappedLoop];
        }

        return transition;
    }
} 