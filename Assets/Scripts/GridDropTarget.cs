using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class GridDropTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private float highlightDuration = 0.2f;
    
    private GridSequencer sequencer;
    private int gridIndex;
    private Canvas canvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        sequencer = GetComponentInParent<GridSequencer>();
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    public void Initialize(int index)
    {
        gridIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    public void OnNoteDropped(int noteValue)
    {
        if (sequencer != null)
        {
            sequencer.OnNoteDropped(gridIndex, noteValue);
            if (highlightObject != null)
            {
                StartCoroutine(ShowHighlightEffect());
            }
        }
    }

    private System.Collections.IEnumerator ShowHighlightEffect()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(true);
            yield return new WaitForSeconds(highlightDuration);
            highlightObject.SetActive(false);
        }
    }

    public Vector3 GetWorldPosition()
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            return transform.position;
        }
        else
        {
            // Convert anchored position to world position for Screen Space
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return (corners[0] + corners[2]) * 0.5f;
        }
    }
} 