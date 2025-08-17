using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    // get the mouse object and find the center
    // attach the object to the mouse object
    // place the object
    [SerializeField] 
    private InputAction mouseClick;

    [SerializeField] 
    private float mouseDragSpeed = 0.1f;
    private Vector2 velocity = Vector2.zero;
    
    [SerializeField] 
    LayerMask placementMask = ~0;   // Which layers count as drop surfaces (defaults to everything)
    RectTransform rectTransform;
    RectTransform parentRect;
    CanvasGroup canvasGroup;

    Vector2 grabOffset;    // anchoredPosition - pointer (both in parent space)
    Vector2 vel;
    public float smoothTime = 0.05f;

    void Awake()
    {
        rectTransform = (RectTransform)transform;
        parentRect = (RectTransform)rectTransform.parent;  // <-- IMPORTANT
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }
    
    public void OnBeginDrag(PointerEventData e)
    {
        // Convert pointer to PARENT local space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, e.position, e.pressEventCamera, out var pPoint);

        // Store exact grab offset in the same space as anchoredPosition
        grabOffset = rectTransform.anchoredPosition - pPoint;

        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData e)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, e.position, e.pressEventCamera, out var pPoint))
        {
            Vector2 target = pPoint + grabOffset;

            // Direct set is fine; SmoothDamp if you want easing
            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                rectTransform.anchoredPosition, target, ref vel,
                smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }
    }
    
    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
}
