using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class NoteButtonUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField] private int noteValue;
    [SerializeField] private string noteName;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Camera mainCamera;
    private Vector3 originalWorldPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCamera = Camera.main;
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(int value, string name)
    {
        noteValue = value;
        noteName = name;
        if (noteText != null)
        {
            noteText.text = name;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalWorldPosition = transform.position;
        
        // Move to canvas root for dragging
        transform.SetParent(canvas.transform);
        
        // Make semi-transparent while dragging
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // Convert screen position to world position
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, canvas.planeDistance));
            transform.position = worldPosition;
        }
        else
        {
            // Update position for Screen Space
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if we're over a valid drop target
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool droppedOnValidTarget = false;
        foreach (var result in results)
        {
            var dropTarget = result.gameObject.GetComponent<GridDropTarget>();
            if (dropTarget != null)
            {
                dropTarget.OnNoteDropped(noteValue);
                droppedOnValidTarget = true;
                break;
            }
        }

        // Always return to original position
        transform.SetParent(originalParent);
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.position = originalWorldPosition;
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
} 