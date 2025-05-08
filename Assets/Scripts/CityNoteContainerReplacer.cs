using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class CityNoteContainerReplacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CitySequencer sequencer;
    [SerializeField] private GameObject activeIndicator;

    private CityNoteContainer thisContainer;
    private bool isActive = false;

    private void Awake()
    {
        // Get the CityNoteContainer component from this object
        thisContainer = GetComponent<CityNoteContainer>();
        if (thisContainer == null)
        {
            Debug.LogError("[CityNoteContainerReplacer] No CityNoteContainer component found on this object!");
            return;
        }

        // Ensure we have a collider for click detection
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("[CityNoteContainerReplacer] No Collider component found! Adding a BoxCollider.");
            gameObject.AddComponent<BoxCollider>();
        }

        // Initialize active indicator state
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(false);
        }
    }

    private void Start()
    {
        if (sequencer == null)
        {
            Debug.LogError("[CityNoteContainerReplacer] Sequencer reference is missing!");
            return;
        }

        // Subscribe to sequencer's update events
        sequencer.OnSequenceUpdated += CheckIfActive;
    }

    private void OnDestroy()
    {
        if (sequencer != null)
        {
            sequencer.OnSequenceUpdated -= CheckIfActive;
        }
    }

    private void OnMouseDown()
    {
        ReplaceAllContainers();
    }

    private void ReplaceAllContainers()
    {
        if (sequencer == null)
        {
            Debug.LogError("[CityNoteContainerReplacer] Sequencer reference is missing!");
            return;
        }

        if (thisContainer == null)
        {
            Debug.LogError("[CityNoteContainerReplacer] This container reference is missing!");
            return;
        }

        // Get the current list of containers
        var containers = sequencer.GetNoteContainers();
        if (containers == null || containers.Count == 0)
        {
            Debug.LogWarning("[CityNoteContainerReplacer] No containers found in sequencer!");
            return;
        }

        Debug.Log($"[CityNoteContainerReplacer] Replacing {containers.Count} containers with {thisContainer.name}");

        // Create new list with this container repeated
        var newContainers = new List<CityNoteContainer>();
        for (int i = 0; i < containers.Count; i++)
        {
            newContainers.Add(thisContainer);
        }

        // Set the new list in sequencer without triggering updates
        sequencer.SetNoteContainersWithoutUpdate(newContainers);

        Debug.Log("[CityNoteContainerReplacer] All containers replaced successfully!");
    }

    private void CheckIfActive()
    {
        if (sequencer == null || thisContainer == null) return;

        var containers = sequencer.GetNoteContainers();
        if (containers == null) return;

        // Check if this container is used in the sequencer
        bool wasActive = isActive;
        isActive = containers.Count > 0 && containers.TrueForAll(c => c == thisContainer);

        // Update active indicator if state changed
        if (wasActive != isActive && activeIndicator != null)
        {
            activeIndicator.SetActive(isActive);
            Debug.Log($"[CityNoteContainerReplacer] Active state changed to: {isActive}");
        }
    }
} 