using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineBPMController : MonoBehaviour
{
    [SerializeField]
    private float bpm = 120f;

    private const float SECONDS_PER_MINUTE = 60f;

    public float BPM
    {
        get => bpm;
        set
        {
            bpm = value;
            UpdateAllTimelines();
        }
    }

    private void UpdateAllTimelines()
    {
        // Calculate the speed multiplier based on BPM
        // We want 1.00 on timeline to equal one beat
        // At BPM 120, one beat should take 0.5 seconds (60/120)
        float secondsPerBeat = SECONDS_PER_MINUTE / bpm;
        
        // The speed multiplier is the inverse of the time it should take
        // If it should take 0.5 seconds, we need to play at 2x speed
        float speedMultiplier = 1f / secondsPerBeat;

        // Update all PlayableDirectors in the scene
        var directors = FindObjectsOfType<PlayableDirector>();
        foreach (var director in directors)
        {
            if (director.playableGraph.IsValid())
            {
                var rootPlayable = director.playableGraph.GetRootPlayable(0);
                rootPlayable.SetSpeed(speedMultiplier);
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateAllTimelines();
        }
    }
} 