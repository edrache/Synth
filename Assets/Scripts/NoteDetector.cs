using UnityEngine;
using System;

[Serializable]
public class NeighborContainerMapping
{
    public bool isEnabled = true;
    public CityNoteContainer targetContainer;
}

public class NoteDetector : MonoBehaviour
{
    [Header("Container References")]
    [SerializeField] private CityNoteContainer containerA;
    [SerializeField] private CityNoteContainer containerB;

    [Header("Neighbor Mappings")]
    [SerializeField] private NeighborContainerMapping upNeighbor = new NeighborContainerMapping();
    [SerializeField] private NeighborContainerMapping upRightNeighbor = new NeighborContainerMapping();
    [SerializeField] private NeighborContainerMapping rightNeighbor = new NeighborContainerMapping();
    [SerializeField] private NeighborContainerMapping downRightNeighbor = new NeighborContainerMapping();
    [SerializeField] private NeighborContainerMapping downNeighbor = new NeighborContainerMapping();
    [SerializeField] private NeighborContainerMapping downLeftNeighbor = new NeighborContainerMapping();
    [SerializeField] private NeighborContainerMapping leftNeighbor = new NeighborContainerMapping();
    [SerializeField] private NeighborContainerMapping upLeftNeighbor = new NeighborContainerMapping();

    private GridNavigator gridNavigator;

    private void Awake()
    {
        gridNavigator = GetComponent<GridNavigator>();
        if (gridNavigator == null)
        {
            Debug.LogError("[NoteDetector] No GridNavigator found on the same GameObject!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Subscribe to GridNavigator events
        gridNavigator.OnCellEntered += HandleCellEntered;
        
        // Initial check of neighbors
        CheckNeighbors();
    }

    private void OnDestroy()
    {
        if (gridNavigator != null)
        {
            gridNavigator.OnCellEntered -= HandleCellEntered;
        }
    }

    private void HandleCellEntered(Vector2Int position)
    {
        CheckNeighbors();
    }

    public void CheckNeighbors()
    {
        // Clear both containers first
        if (containerA != null) containerA.ClearNotes();
        if (containerB != null) containerB.ClearNotes();

        // Get all neighbors
        GameObject[] neighbors = gridNavigator.GetAllNeighbors();
        
        // Process each neighbor
        ProcessNeighbor(neighbors[0], upNeighbor);        // Up
        ProcessNeighbor(neighbors[4], upRightNeighbor);   // Up-Right
        ProcessNeighbor(neighbors[1], rightNeighbor);     // Right
        ProcessNeighbor(neighbors[5], downRightNeighbor); // Down-Right
        ProcessNeighbor(neighbors[2], downNeighbor);      // Down
        ProcessNeighbor(neighbors[6], downLeftNeighbor);  // Down-Left
        ProcessNeighbor(neighbors[3], leftNeighbor);      // Left
        ProcessNeighbor(neighbors[7], upLeftNeighbor);    // Up-Left
    }

    private void ProcessNeighbor(GameObject neighbor, NeighborContainerMapping mapping)
    {
        if (!mapping.isEnabled || mapping.targetContainer == null || neighbor == null)
            return;

        CityNote note = neighbor.GetComponent<CityNote>();
        if (note != null)
        {
            note.SetContainer(mapping.targetContainer);
        }
    }
} 