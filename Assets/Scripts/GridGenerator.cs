using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GridElement
{
    public GameObject prefab;
    [Range(0, 100)]
    public int weight = 100;
}

public class GridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2 gridSize = new Vector2(800, 600); // Total size of the grid in pixels
    [SerializeField] private Vector2 cellSize = new Vector2(100, 100); // Size of each cell in pixels
    [SerializeField] private List<GridElement> gridElements = new List<GridElement>();

    private GameObject[,] grid;
    private Vector2Int gridDimensions;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        // Calculate grid dimensions based on total size and cell size
        gridDimensions = new Vector2Int(
            Mathf.FloorToInt(gridSize.x / cellSize.x),
            Mathf.FloorToInt(gridSize.y / cellSize.y)
        );

        // Initialize grid array
        grid = new GameObject[gridDimensions.x, gridDimensions.y];

        // Create grid cells
        for (int x = 0; x < gridDimensions.x; x++)
        {
            for (int y = 0; y < gridDimensions.y; y++)
            {
                CreateCell(x, y);
            }
        }
    }

    private void CreateCell(int x, int y)
    {
        // Calculate position in canvas space
        Vector2 position = new Vector2(
            x * cellSize.x - (gridSize.x / 2) + (cellSize.x / 2),
            y * cellSize.y - (gridSize.y / 2) + (cellSize.y / 2)
        );

        // Select random element based on weights
        GameObject selectedPrefab = GetRandomElement();
        if (selectedPrefab != null)
        {
            // Instantiate the element
            GameObject cell = Instantiate(selectedPrefab, transform);
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = cellSize;
            }

            // Store in grid array
            grid[x, y] = cell;
        }
    }

    private GameObject GetRandomElement()
    {
        if (gridElements == null || gridElements.Count == 0)
            return null;

        // Calculate total weight
        int totalWeight = 0;
        foreach (var element in gridElements)
        {
            totalWeight += element.weight;
        }

        // If no weights are set, return null
        if (totalWeight <= 0)
            return null;

        // Get random value
        int random = Random.Range(0, totalWeight);
        int currentWeight = 0;

        // Select element based on weight
        foreach (var element in gridElements)
        {
            currentWeight += element.weight;
            if (random < currentWeight)
            {
                return element.prefab;
            }
        }

        return gridElements[0].prefab; // Fallback to first element
    }

    // Get element at specific grid coordinates
    public GameObject GetElementAt(int x, int y)
    {
        if (x < 0 || x >= gridDimensions.x || y < 0 || y >= gridDimensions.y)
            return null;

        return grid[x, y];
    }

    // Get element at world position
    public GameObject GetElementAtPosition(Vector2 position)
    {
        Vector2 localPosition = transform.InverseTransformPoint(position);
        int x = Mathf.FloorToInt((localPosition.x + gridSize.x / 2) / cellSize.x);
        int y = Mathf.FloorToInt((localPosition.y + gridSize.y / 2) / cellSize.y);

        return GetElementAt(x, y);
    }

    // Get neighboring elements
    public GameObject[] GetNeighbors(int x, int y)
    {
        GameObject[] neighbors = new GameObject[4]; // Up, Right, Down, Left

        // Up
        neighbors[0] = GetElementAt(x, y + 1);
        // Right
        neighbors[1] = GetElementAt(x + 1, y);
        // Down
        neighbors[2] = GetElementAt(x, y - 1);
        // Left
        neighbors[3] = GetElementAt(x - 1, y);

        return neighbors;
    }

    // Get grid dimensions
    public Vector2Int GetGridDimensions()
    {
        return gridDimensions;
    }

    // Get cell size
    public Vector2 GetCellSize()
    {
        return cellSize;
    }
} 