using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class GridVisibilityManager : MonoBehaviour
{
    [Header("Visibility Settings")]
    [SerializeField] private List<VisibilityLayer> visibilityLayers = new List<VisibilityLayer>();
    [SerializeField] private bool showDiagonals = true;
    [SerializeField] private bool fadeObjects = true;
    [SerializeField] private float fadeDuration = 0.3f;

    private GridGenerator gridGenerator;
    private GridNavigator gridNavigator;
    private Dictionary<GameObject, CanvasGroup> objectCanvasGroups = new Dictionary<GameObject, CanvasGroup>();
    private Dictionary<GameObject, Tweener> activeTweens = new Dictionary<GameObject, Tweener>();
    private bool isInitialized = false;

    private void Awake()
    {
        Debug.Log("[GridVisibilityManager] Awake called");
        gridGenerator = GetComponent<GridGenerator>();
        if (gridGenerator == null)
        {
            Debug.LogError("[GridVisibilityManager] No GridGenerator found!");
            enabled = false;
            return;
        }

        // Initialize default layers if none are set
        if (visibilityLayers.Count == 0)
        {
            var layer1 = new VisibilityLayer();
            layer1.Radius = 1;
            layer1.Alpha = 1f;
            visibilityLayers.Add(layer1);

            var layer2 = new VisibilityLayer();
            layer2.Radius = 2;
            layer2.Alpha = 0.7f;
            visibilityLayers.Add(layer2);

            var layer3 = new VisibilityLayer();
            layer3.Radius = 3;
            layer3.Alpha = 0.3f;
            visibilityLayers.Add(layer3);
        }
    }

    private void Start()
    {
        Debug.Log("[GridVisibilityManager] Start called");
        StartCoroutine(InitializeWhenReady());
    }

    private System.Collections.IEnumerator InitializeWhenReady()
    {
        Debug.Log("[GridVisibilityManager] Starting initialization...");
        
        // Wait for grid to be ready
        while (gridGenerator.GetGridDimensions().x == 0 || gridGenerator.GetGridDimensions().y == 0)
        {
            Debug.Log("[GridVisibilityManager] Waiting for grid to be ready...");
            yield return new WaitForEndOfFrame();
        }

        // Wait for GridNavigator to be ready
        gridNavigator = FindObjectOfType<GridNavigator>();
        while (gridNavigator == null || !gridNavigator.IsInitialized())
        {
            Debug.Log("[GridVisibilityManager] Waiting for GridNavigator to be ready...");
            gridNavigator = FindObjectOfType<GridNavigator>();
            yield return new WaitForEndOfFrame();
        }

        if (gridNavigator == null)
        {
            Debug.LogError("[GridVisibilityManager] No GridNavigator found in scene!");
            enabled = false;
            yield break;
        }

        Debug.Log("[GridVisibilityManager] GridNavigator found and initialized");

        // Initialize canvas groups for all grid objects
        InitializeCanvasGroups();
        
        // Subscribe only to movement animation completion
        gridNavigator.OnMovementAnimationCompleted += UpdateVisibility;

        // Initial visibility update with current position
        Vector2Int currentPosition = gridNavigator.GetCurrentPosition();
        Debug.Log($"[GridVisibilityManager] Setting initial visibility for position: {currentPosition}");
        UpdateVisibility(currentPosition);
        
        isInitialized = true;
        Debug.Log("[GridVisibilityManager] Initialization complete");
    }

    private void InitializeCanvasGroups()
    {
        Debug.Log("[GridVisibilityManager] Initializing canvas groups");
        Vector2Int dimensions = gridGenerator.GetGridDimensions();
        int initializedCount = 0;

        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                GameObject cell = gridGenerator.GetElementAt(x, y);
                if (cell != null)
                {
                    CanvasGroup canvasGroup = cell.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = cell.AddComponent<CanvasGroup>();
                    }
                    objectCanvasGroups[cell] = canvasGroup;
                    initializedCount++;
                }
            }
        }

        Debug.Log($"[GridVisibilityManager] Initialized {initializedCount} canvas groups");
    }

    private void UpdateVisibility(Vector2Int position)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[GridVisibilityManager] UpdateVisibility called before initialization");
            return;
        }

        Vector2Int dimensions = gridGenerator.GetGridDimensions();
        int totalCount = 0;
        
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                GameObject cell = gridGenerator.GetElementAt(x, y);
                if (cell != null)
                {
                    totalCount++;
                    float alpha = GetAlphaForDistance(x, y, position);
                    SetObjectVisibility(cell, alpha);
                }
            }
        }

        Debug.Log($"[GridVisibilityManager] Updated visibility for {totalCount} objects");
    }

    private float GetAlphaForDistance(int x, int y, Vector2Int center)
    {
        int distanceX = Mathf.Abs(x - center.x);
        int distanceY = Mathf.Abs(y - center.y);
        int distance = showDiagonals ? 
            Mathf.Max(distanceX, distanceY) : 
            distanceX + distanceY;

        // Find the appropriate layer for this distance
        for (int i = visibilityLayers.Count - 1; i >= 0; i--)
        {
            if (distance <= visibilityLayers[i].Radius)
            {
                return visibilityLayers[i].Alpha;
            }
        }

        return 0f; // Beyond all layers
    }

    private void SetObjectVisibility(GameObject obj, float alpha)
    {
        if (!objectCanvasGroups.ContainsKey(obj))
        {
            Debug.LogWarning($"[GridVisibilityManager] No CanvasGroup found for object {obj.name}");
            return;
        }

        CanvasGroup canvasGroup = objectCanvasGroups[obj];
        
        if (fadeObjects)
        {
            // Kill existing tween if any
            if (activeTweens.ContainsKey(obj))
            {
                activeTweens[obj].Kill();
                activeTweens.Remove(obj);
            }

            Tweener tween = canvasGroup.DOFade(alpha, fadeDuration);
            activeTweens[obj] = tween;
        }
        else
        {
            canvasGroup.alpha = alpha;
        }
    }

    private void OnDestroy()
    {
        if (gridNavigator != null)
        {
            gridNavigator.OnMovementAnimationCompleted -= UpdateVisibility;
        }

        // Kill all active tweens
        foreach (var tween in activeTweens.Values)
        {
            if (tween != null && tween.IsActive())
            {
                tween.Kill();
            }
        }
        activeTweens.Clear();
    }
} 