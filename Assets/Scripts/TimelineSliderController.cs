using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(Slider))]
public class TimelineSliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private bool updateSliderOnTimelineChange = true;
    [SerializeField] private bool updateTimelineOnSliderChange = true;
    [SerializeField] private bool updateOnlyOnClip = false;

    private Slider slider;
    private bool isSliderBeingDragged = false;
    private TimelineAsset timelineAsset;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogError("[TimelineSliderController] No Slider component found!");
            return;
        }

        // Set up slider
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        // Add listener for slider value changes
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnEnable()
    {
        if (timeline != null)
        {
            // Subscribe to timeline events
            timeline.played += OnTimelinePlayed;
            timeline.paused += OnTimelinePaused;
            timeline.stopped += OnTimelineStopped;
            
            // Get timeline asset
            timelineAsset = timeline.playableAsset as TimelineAsset;
        }
    }

    private void OnDisable()
    {
        if (timeline != null)
        {
            // Unsubscribe from timeline events
            timeline.played -= OnTimelinePlayed;
            timeline.paused -= OnTimelinePaused;
            timeline.stopped -= OnTimelineStopped;
        }
    }

    private void Update()
    {
        if (timeline == null || !updateSliderOnTimelineChange || isSliderBeingDragged)
            return;

        // Update slider value based on timeline progress
        float normalizedTime = (float)(timeline.time / timeline.duration);
        
        if (updateOnlyOnClip && timelineAsset != null)
        {
            // Check if there's any clip at current time
            bool hasClipAtTime = false;
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                foreach (var clip in track.GetClips())
                {
                    if (timeline.time >= clip.start && timeline.time <= clip.end)
                    {
                        hasClipAtTime = true;
                        break;
                    }
                }
                if (hasClipAtTime) break;
            }
            
            // Only update slider if there's a clip at current time
            if (hasClipAtTime)
            {
                slider.value = normalizedTime;
            }
        }
        else
        {
            slider.value = normalizedTime;
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (timeline == null || !updateTimelineOnSliderChange)
            return;

        // Update timeline time based on slider value
        double newTime = value * timeline.duration;
        timeline.time = newTime;
    }

    public void OnSliderDragStart()
    {
        isSliderBeingDragged = true;
    }

    public void OnSliderDragEnd()
    {
        isSliderBeingDragged = false;
    }

    private void OnTimelinePlayed(PlayableDirector director)
    {
        Debug.Log("[TimelineSliderController] Timeline started playing");
    }

    private void OnTimelinePaused(PlayableDirector director)
    {
        Debug.Log("[TimelineSliderController] Timeline paused");
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        Debug.Log("[TimelineSliderController] Timeline stopped");
        // Reset slider to start
        slider.value = 0f;
    }

    // Public method to set the timeline reference
    public void SetTimeline(PlayableDirector newTimeline)
    {
        if (timeline != null)
        {
            // Unsubscribe from old timeline events
            timeline.played -= OnTimelinePlayed;
            timeline.paused -= OnTimelinePaused;
            timeline.stopped -= OnTimelineStopped;
        }

        timeline = newTimeline;

        if (timeline != null)
        {
            // Subscribe to new timeline events
            timeline.played += OnTimelinePlayed;
            timeline.paused += OnTimelinePaused;
            timeline.stopped += OnTimelineStopped;
            
            // Get timeline asset
            timelineAsset = timeline.playableAsset as TimelineAsset;
        }
    }
} 