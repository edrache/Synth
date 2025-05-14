using UnityEngine;
using Rewired;
using System;
using DG.Tweening;

public class GridNavigator : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private GridGenerator gridGenerator;

    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private Vector2Int startPosition = Vector2Int.zero;

    [Header("Rewired Settings")]
    [SerializeField] private int playerId = 0;

    // Events
    public event Action<Vector2Int> OnPositionChanged;
    public event Action<Vector2Int> OnCellEntered;
    public event Action<Vector2Int> OnBoundaryReached;

    // Private variables
    private Player rewiredPlayer;
    private Vector2Int currentGridPosition;
    private bool isMoving = false;
    private Tweener currentTween;
    private bool isInitialized = false;

    private void Awake()
    {
        rewiredPlayer = ReInput.players.GetPlayer(playerId);
        if (gridGenerator == null)
        {
            gridGenerator = FindObjectOfType<GridGenerator>();
            if (gridGenerator == null)
            {
                Debug.LogError("[GridNavigator] No GridGenerator found in scene!");
                enabled = false;
                return;
            }
        }
    }

    private void Start()
    {
        // Wait for grid to be ready
        StartCoroutine(InitializeWhenGridReady());
    }

    private System.Collections.IEnumerator InitializeWhenGridReady()
    {
        // Wait until grid has valid dimensions
        while (gridGenerator.GetGridDimensions().x == 0 || gridGenerator.GetGridDimensions().y == 0)
        {
            yield return new WaitForEndOfFrame();
        }

        SetPosition(startPosition);
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || isMoving) return;

        // Check for movement input
        if (rewiredPlayer.GetButtonDown("MoveUp"))
        {
            TryMove(Vector2Int.up);
        }
        else if (rewiredPlayer.GetButtonDown("MoveDown"))
        {
            TryMove(Vector2Int.down);
        }
        else if (rewiredPlayer.GetButtonDown("MoveRight"))
        {
            TryMove(Vector2Int.right);
        }
        else if (rewiredPlayer.GetButtonDown("MoveLeft"))
        {
            TryMove(Vector2Int.left);
        }
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = currentGridPosition + direction;
        Vector2Int gridDimensions = gridGenerator.GetGridDimensions();

        // Check if new position is within grid bounds
        if (newPosition.x < 0 || newPosition.x >= gridDimensions.x ||
            newPosition.y < 0 || newPosition.y >= gridDimensions.y)
        {
            OnBoundaryReached?.Invoke(newPosition);
            return;
        }

        // Calculate world position starting from bottom-left corner
        Vector2 cellSize = gridGenerator.GetCellSize();
        Vector2 targetPosition = new Vector2(
            newPosition.x * cellSize.x + (cellSize.x / 2),
            newPosition.y * cellSize.y + (cellSize.y / 2)
        );

        // Move to new position
        isMoving = true;
        currentTween = transform.DOLocalMove(targetPosition, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() => {
                isMoving = false;
                currentGridPosition = newPosition;
                OnPositionChanged?.Invoke(currentGridPosition);
                OnCellEntered?.Invoke(currentGridPosition);
            });
    }

    public void SetPosition(Vector2Int newPosition)
    {
        Vector2Int gridDimensions = gridGenerator.GetGridDimensions();
        
        // Clamp position to grid bounds
        newPosition.x = Mathf.Clamp(newPosition.x, 0, gridDimensions.x - 1);
        newPosition.y = Mathf.Clamp(newPosition.y, 0, gridDimensions.y - 1);

        // Calculate world position starting from bottom-left corner
        Vector2 cellSize = gridGenerator.GetCellSize();
        Vector2 targetPosition = new Vector2(
            newPosition.x * cellSize.x + (cellSize.x / 2),
            newPosition.y * cellSize.y + (cellSize.y / 2)
        );

        // Set position immediately
        transform.localPosition = targetPosition;
        currentGridPosition = newPosition;
        OnPositionChanged?.Invoke(currentGridPosition);
        OnCellEntered?.Invoke(currentGridPosition);
    }

    // Get current grid position
    public Vector2Int GetCurrentPosition()
    {
        return currentGridPosition;
    }

    // Get neighbor at specific direction
    public GameObject GetNeighbor(Vector2Int direction)
    {
        Vector2Int neighborPosition = currentGridPosition + direction;
        return gridGenerator.GetElementAt(neighborPosition.x, neighborPosition.y);
    }

    // Get all neighbors (including diagonals)
    public GameObject[] GetAllNeighbors()
    {
        GameObject[] neighbors = new GameObject[8];
        
        // Cardinal directions
        neighbors[0] = GetNeighbor(Vector2Int.up);      // Up
        neighbors[1] = GetNeighbor(Vector2Int.right);   // Right
        neighbors[2] = GetNeighbor(Vector2Int.down);    // Down
        neighbors[3] = GetNeighbor(Vector2Int.left);    // Left
        
        // Diagonal directions
        neighbors[4] = GetNeighbor(new Vector2Int(1, 1));   // Up-Right
        neighbors[5] = GetNeighbor(new Vector2Int(1, -1));  // Down-Right
        neighbors[6] = GetNeighbor(new Vector2Int(-1, -1)); // Down-Left
        neighbors[7] = GetNeighbor(new Vector2Int(-1, 1));  // Up-Left

        return neighbors;
    }

    // Get current cell
    public GameObject GetCurrentCell()
    {
        return gridGenerator.GetElementAt(currentGridPosition.x, currentGridPosition.y);
    }

    private void OnDestroy()
    {
        // Kill any active tweens
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
    }
} 