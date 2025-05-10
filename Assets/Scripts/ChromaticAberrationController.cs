using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private Volume postProcessVolume;

    [Header("Effect Settings")]
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private float fadeOutDuration = 2f;

    private UnityEngine.Rendering.Universal.ChromaticAberration chromaticAberration;
    private float currentIntensity = 0f;
    private float fadeStartTime = 0f;
    private bool isFading = false;

    private void Start()
    {
        if (postProcessVolume == null)
        {
            //Debug.LogError("[ChromaticAberrationController] Post Process Volume reference is missing!");
            return;
        }

        // Get the Chromatic Aberration effect
        if (!postProcessVolume.profile.TryGet(out chromaticAberration))
        {
            //Debug.LogError("[ChromaticAberrationController] Chromatic Aberration effect not found in Post Process Volume!");
            return;
        }

        // Initialize effect
        chromaticAberration.intensity.value = 0f;
    }

    private void Update()
    {
        if (timeline == null || chromaticAberration == null) return;

        // Check if there's any clip at current time
        bool hasClipAtTime = false;
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset != null)
        {
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
        }

        // Handle effect intensity
        if (hasClipAtTime)
        {
            // Clip appeared, start fade out
            if (!isFading)
            {
                isFading = true;
                fadeStartTime = Time.time;
                currentIntensity = maxIntensity;
                chromaticAberration.intensity.value = maxIntensity;
                //Debug.Log($"[ChromaticAberrationController] Clip detected, starting fade from {maxIntensity}");
            }
        }
        else if (isFading)
        {
            // Calculate fade progress
            float fadeProgress = (Time.time - fadeStartTime) / fadeOutDuration;
            
            if (fadeProgress >= 1f)
            {
                // Fade complete
                isFading = false;
                currentIntensity = 0f;
                chromaticAberration.intensity.value = 0f;
                //Debug.Log("[ChromaticAberrationController] Fade complete");
            }
            else
            {
                // Update intensity
                currentIntensity = Mathf.Lerp(maxIntensity, 0f, fadeProgress);
                chromaticAberration.intensity.value = currentIntensity;
            }
        }
    }

    private void OnValidate()
    {
        // Validate settings
        maxIntensity = Mathf.Max(0f, maxIntensity);
        fadeOutDuration = Mathf.Max(0.1f, fadeOutDuration);
    }
} 