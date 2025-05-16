using UnityEngine;
using System;
using System.Collections;

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

    [Header("Current Cell Settings")]
    [SerializeField] private bool processCurrentCell = true;
    [SerializeField] private CityNoteContainer currentCellContainer;

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
    private bool isInitialized = false;
    private bool hasProcessedInitialCell = false;

    private void Awake()
    {
        gridNavigator = GetComponent<GridNavigator>();
        if (gridNavigator == null)
        {
            Debug.LogError("[NoteDetector] No GridNavigator found on the same GameObject!");
            enabled = false;
            return;
        }
        Debug.Log("[NoteDetector] Awake: GridNavigator found");
    }

    private void Start()
    {
        Debug.Log("[NoteDetector] Start: Beginning initialization");
        
        // Subscribe to GridNavigator events
        gridNavigator.OnCellEntered += HandleCellEntered;
        Debug.Log("[NoteDetector] Start: Subscribed to OnCellEntered event");
        
        // Initial check of neighbors with delay to allow CityNotes to initialize
        StartCoroutine(InitialCheckWithDelay());
    }

    private IEnumerator InitialCheckWithDelay()
    {
        Debug.Log("[NoteDetector] InitialCheckWithDelay: Starting delay");
        // Wait for one frame to allow CityNotes to initialize
        yield return null;
        Debug.Log("[NoteDetector] InitialCheckWithDelay: Delay completed, checking neighbors");
        
        if (!hasProcessedInitialCell)
        {
            CheckNeighbors();
            hasProcessedInitialCell = true;
        }
        
        isInitialized = true;
        Debug.Log("[NoteDetector] InitialCheckWithDelay: Initialization completed");
    }

    private void OnDestroy()
    {
        if (gridNavigator != null)
        {
            gridNavigator.OnCellEntered -= HandleCellEntered;
            Debug.Log("[NoteDetector] OnDestroy: Unsubscribed from OnCellEntered event");
        }
    }

    private void HandleCellEntered(Vector2Int position)
    {
        Debug.Log($"[NoteDetector] HandleCellEntered: Entered cell at position {position}");
        
        // Only process cell changes after initial setup
        if (isInitialized)
        {
            CheckNeighbors();
        }
    }

    public void CheckNeighbors()
    {
        Debug.Log("[NoteDetector] CheckNeighbors: Starting check");
        
        // Clear both containers first
        if (containerA != null) 
        {
            containerA.ClearNotes();
            Debug.Log("[NoteDetector] CheckNeighbors: Cleared container A");
        }
        if (containerB != null) 
        {
            containerB.ClearNotes();
            Debug.Log("[NoteDetector] CheckNeighbors: Cleared container B");
        }

        // Process current cell if enabled
        if (processCurrentCell && currentCellContainer != null)
        {
            Debug.Log("[NoteDetector] CheckNeighbors: Processing current cell");
            ProcessCurrentCell();
        }
        else
        {
            Debug.Log($"[NoteDetector] CheckNeighbors: Skipping current cell - processCurrentCell: {processCurrentCell}, currentCellContainer: {(currentCellContainer != null ? "not null" : "null")}");
        }

        // Get all neighbors
        GameObject[] neighbors = gridNavigator.GetAllNeighbors();
        Debug.Log($"[NoteDetector] CheckNeighbors: Got {neighbors.Length} neighbors");
        
        // Process each neighbor
        ProcessNeighbor(neighbors[0], upNeighbor);        // Up
        ProcessNeighbor(neighbors[4], upRightNeighbor);   // Up-Right
        ProcessNeighbor(neighbors[1], rightNeighbor);     // Right
        ProcessNeighbor(neighbors[5], downRightNeighbor); // Down-Right
        ProcessNeighbor(neighbors[2], downNeighbor);      // Down
        ProcessNeighbor(neighbors[6], downLeftNeighbor);  // Down-Left
        ProcessNeighbor(neighbors[3], leftNeighbor);      // Left
        ProcessNeighbor(neighbors[7], upLeftNeighbor);    // Up-Left
        
        Debug.Log("[NoteDetector] CheckNeighbors: Check completed");
    }

    private void ProcessCurrentCell()
    {
        Debug.Log("[NoteDetector] ProcessCurrentCell: Starting");
        
        // Get the current cell's object
        GameObject currentCell = gridNavigator.GetCurrentCell();
        Debug.Log($"[NoteDetector] ProcessCurrentCell: Current cell object is {(currentCell != null ? "not null" : "null")}");
        
        if (currentCell != null)
        {
            CityNote note = currentCell.GetComponent<CityNote>();
            Debug.Log($"[NoteDetector] ProcessCurrentCell: CityNote component is {(note != null ? "not null" : "null")}");
            
            if (note != null)
            {
                note.SetContainer(currentCellContainer);
                Debug.Log("[NoteDetector] ProcessCurrentCell: Set container for current cell note");
            }
        }
        
        Debug.Log("[NoteDetector] ProcessCurrentCell: Completed");
    }

    private void ProcessNeighbor(GameObject neighbor, NeighborContainerMapping mapping)
    {
        if (!mapping.isEnabled || mapping.targetContainer == null || neighbor == null)
        {
            Debug.Log($"[NoteDetector] ProcessNeighbor: Skipping neighbor - enabled: {mapping.isEnabled}, container: {(mapping.targetContainer != null ? "not null" : "null")}, neighbor: {(neighbor != null ? "not null" : "null")}");
            return;
        }

        CityNote note = neighbor.GetComponent<CityNote>();
        Debug.Log($"[NoteDetector] ProcessNeighbor: CityNote component is {(note != null ? "not null" : "null")}");
        
        if (note != null)
        {
            note.SetContainer(mapping.targetContainer);
            Debug.Log("[NoteDetector] ProcessNeighbor: Set container for neighbor note");
        }
    }
} 