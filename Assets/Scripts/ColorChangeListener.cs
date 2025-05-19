using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ColorChangeListener : MonoBehaviour, INoteEventListener
{
    [Header("Color Change Settings")]
    [SerializeField] private Gradient colorGradient; // Gradient to be set in the inspector
    [SerializeField] private float changeDuration = 1f; // Duration of the color change
    [SerializeField] private Image imageComponent; // Reference to the Image component
    [SerializeField] private TimelineType timelineType; // Which timeline type should trigger color change

    private void Start()
    {
        if (imageComponent == null)
        {
            Debug.LogError("No Image component assigned to ColorChangeListener!");
        }

        // Register with NoteEventManager
        var eventManager = GetComponent<NoteEventManager>();
        if (eventManager == null)
        {
            eventManager = gameObject.AddComponent<NoteEventManager>();
        }
        eventManager.RegisterListener(this);
    }

    private void OnDestroy()
    {
        var eventManager = GetComponent<NoteEventManager>();
        if (eventManager != null)
        {
            eventManager.UnregisterListener(this);
        }
    }

    public void OnNoteStart(CityNote note, float velocity, TimelineType timelineType)
    {
        // Only react if this is the timeline type we're listening for
        if (timelineType != this.timelineType)
            return;

        if (imageComponent != null)
        {
            // Calculate the start color based on velocity (1 - velocity to invert the mapping)
            Color startColor = colorGradient.Evaluate(1f - velocity);
            // Set the initial color
            imageComponent.color = startColor;

            // Animate the color change to the end of the gradient
            imageComponent.DOColor(colorGradient.Evaluate(1f), changeDuration).SetEase(Ease.Linear);
        }
    }

    public void OnNoteEnd(CityNote note, TimelineType timelineType)
    {
        // No action needed on note end
    }
} 