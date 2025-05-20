using UnityEngine;
using TMPro;
using UnityEngine.Playables;
using System;

public class LoopCounter : MonoBehaviour
{
    // Event that will be called when loop count changes
    public event Action<int> OnLoopCountChanged;

    [Header("References")]
    [SerializeField] private PlayableDirector timeline; // Reference to the PlayableDirector
    [SerializeField] private GridNavigator gridNavigator; // Reference to the GridNavigator
    [SerializeField] private TextMeshProUGUI loopCountText; // Reference to the TextMeshPro component

    private int loopCount = 0; // Counter for the number of loops
    private Vector2Int lastPosition; // Last position of the GridNavigator
    private float lastTimelineTime = 0f;
    private bool isPlaying = false;

    private void Start()
    {
        if (timeline == null || gridNavigator == null || loopCountText == null)
        {
            Debug.LogError("LoopCounter: Missing references!");
            enabled = false;
            return;
        }

        Debug.Log("[LoopCounter] Starting initialization...");

        // Subscribe to events
        gridNavigator.OnMovementAnimationCompleted += OnPositionChanged;
        timeline.played += OnTimelinePlayed;
        timeline.paused += OnTimelinePaused;
        timeline.stopped += OnTimelineStopped;

        lastPosition = gridNavigator.GetCurrentPosition();
        Debug.Log($"[LoopCounter] Initial position set to: {lastPosition}");
        UpdateLoopCountText();
    }

    private void OnDestroy()
    {
        if (gridNavigator != null)
        {
            gridNavigator.OnMovementAnimationCompleted -= OnPositionChanged;
        }

        if (timeline != null)
        {
            timeline.played -= OnTimelinePlayed;
            timeline.paused -= OnTimelinePaused;
            timeline.stopped -= OnTimelineStopped;
        }
    }

    private void OnPositionChanged(Vector2Int newPosition)
    {
        Debug.Log($"[LoopCounter] Position changed from {lastPosition} to {newPosition}");
        if (newPosition != lastPosition)
        {
            Debug.Log($"[LoopCounter] Position different, resetting loop count from {loopCount} to 0");
            loopCount = 0;
            lastPosition = newPosition;
            UpdateLoopCountText();
        }
        else
        {
            Debug.Log("[LoopCounter] Position unchanged, keeping loop count");
        }
    }

    private void Update()
    {
        if (!isPlaying || timeline == null) return;

        float currentTime = (float)timeline.time;
        float loopTime = (float)timeline.duration;

        // Check if timeline has wrapped around (current time is less than last time)
        if (currentTime < lastTimelineTime)
        {
            Debug.Log($"[LoopCounter] Timeline wrapped around - Current: {currentTime:F2}, Last: {lastTimelineTime:F2}");
            Vector2Int currentPosition = gridNavigator.GetCurrentPosition();
            Debug.Log($"[LoopCounter] Current position: {currentPosition}, Last position: {lastPosition}");
            
            if (currentPosition == lastPosition)
            {
                loopCount++;
                Debug.Log($"[LoopCounter] Position unchanged at loop end, incrementing count to: {loopCount}");
                UpdateLoopCountText();
                OnLoopCountChanged?.Invoke(loopCount);
            }
            else
            {
                Debug.Log("[LoopCounter] Position changed at loop end, not incrementing count");
            }
        }

        // Debug when near loop end
        if (currentTime >= loopTime - 0.1f)
        {
            Debug.Log($"[LoopCounter] Timeline near end - Time: {currentTime:F2}/{loopTime:F2}");
        }

        lastTimelineTime = currentTime;
    }

    private void OnTimelinePlayed(PlayableDirector director)
    {
        Debug.Log("[LoopCounter] Timeline started playing");
        isPlaying = true;
        lastTimelineTime = (float)director.time;
    }

    private void OnTimelinePaused(PlayableDirector director)
    {
        Debug.Log("[LoopCounter] Timeline paused");
        isPlaying = false;
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        Debug.Log("[LoopCounter] Timeline stopped");
        isPlaying = false;
        lastTimelineTime = 0f;
    }

    private void UpdateLoopCountText()
    {
        if (loopCountText != null)
        {
            loopCountText.text = $"{loopCount}";
            Debug.Log($"[LoopCounter] Updated text to: {loopCount}");
        }
        else
        {
            Debug.LogWarning("[LoopCounter] Text component is null!");
        }
    }
} 