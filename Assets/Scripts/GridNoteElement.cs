using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class GridNoteElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float fadeOutDuration = 0.2f;
    private int noteValue;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(int value, string text)
    {
        noteValue = value;
        if (noteText != null)
        {
            noteText.text = text;
        }

        // Set random color based on note value
        if (backgroundImage != null)
        {
            float hue = (float)value / 7f; // 7 notes in scale
            backgroundImage.color = Color.HSVToRGB(hue, 0.7f, 0.9f);
        }
    }

    public int GetNoteValue()
    {
        return noteValue;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Move to canvas root for dragging
        transform.SetParent(canvas.transform);
        
        // Make semi-transparent while dragging
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Update position
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
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

        // If not dropped on valid target, return to original position
        if (!droppedOnValidTarget)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
        else
        {
            // Destroy this element as it's been placed
            Remove();
        }
    }

    public void Remove()
    {
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float startTime = Time.time;
        float startAlpha = canvasGroup.alpha;

        while (Time.time < startTime + fadeOutDuration)
        {
            float t = (Time.time - startTime) / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        Destroy(gameObject);
    }
} 