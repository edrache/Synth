using UnityEngine;
using TMPro;
using UnityEngine.Playables;

public class LoopCounter : MonoBehaviour
{
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

        // Subscribe to events
        gridNavigator.OnMovementAnimationCompleted += OnMovementCompleted;
        timeline.played += OnTimelinePlayed;
        timeline.paused += OnTimelinePaused;
        timeline.stopped += OnTimelineStopped;

        lastPosition = gridNavigator.GetCurrentPosition();
        UpdateLoopCountText();
    }

    private void OnDestroy()
    {
        if (gridNavigator != null)
        {
            gridNavigator.OnMovementAnimationCompleted -= OnMovementCompleted;
        }

        if (timeline != null)
        {
            timeline.played -= OnTimelinePlayed;
            timeline.paused -= OnTimelinePaused;
            timeline.stopped -= OnTimelineStopped;
        }
    }

    private void OnMovementCompleted(Vector2Int newPosition)
    {
        if (newPosition != lastPosition)
        {
            loopCount = 0;
            lastPosition = newPosition;
            UpdateLoopCountText();
        }
    }

    private void Update()
    {
        if (!isPlaying || timeline == null) return;

        float currentTime = (float)timeline.time;
        float nextFrameTime = currentTime + Time.deltaTime;
        float loopTime = (float)timeline.duration;

        // Check if timeline will reach the loop point in the next frame
        if (nextFrameTime >= loopTime)
        {
            Vector2Int currentPosition = gridNavigator.GetCurrentPosition();
            if (currentPosition == lastPosition)
            {
                loopCount++;
                UpdateLoopCountText();
            }
        }

        lastTimelineTime = currentTime;
    }

    private void OnTimelinePlayed(PlayableDirector director)
    {
        isPlaying = true;
        lastTimelineTime = (float)director.time;
    }

    private void OnTimelinePaused(PlayableDirector director)
    {
        isPlaying = false;
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        isPlaying = false;
        lastTimelineTime = 0f;
    }

    private void UpdateLoopCountText()
    {
        if (loopCountText != null)
        {
            loopCountText.text = $"{loopCount}";
        }
    }
} 