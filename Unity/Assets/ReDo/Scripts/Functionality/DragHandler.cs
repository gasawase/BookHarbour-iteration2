using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    GameObject localThisObject;
    GameObject localThisObjectParent;
    int localThisObjectSiblingIndex;
    UIBookData bookData;
    public UIHandler uiHandler;
    public RectTransform rectTransform;
    public Canvas canvas;
    public CanvasGroup canvasGroup;
    public Vector3 originalPosition;

    void Awake()
    {
        localThisObject = this.gameObject;
        rectTransform = localThisObject.GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();
        //canvasGroup = GetComponentInParent<CanvasGroup>();
        bookData = localThisObject.GetComponent<PrefabParentScript>().UIGameObject.GetComponent<UIBookData>();
        uiHandler = GameObject.FindGameObjectsWithTag("GameManager").First().GetComponent<UIHandler>(); // TODO causing an error; i think it's being instantiated too early
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log(bookData.bookTitle + " Begin Drag");
        localThisObjectParent = rectTransform.parent.gameObject;
        localThisObjectSiblingIndex = rectTransform.GetSiblingIndex();
        originalPosition = rectTransform.position;

        // actually starting to drag but need to change some settings first
        rectTransform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out worldPos);

        rectTransform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log(bookData.bookTitle + " End Drag");
        // check if the 3D object is in the bookshelf collider
        // and if true, drop it there and do the placing functionality
        // if not true, return to its original position
        ResetBookPosition(originalPosition, localThisObjectParent, localThisObjectSiblingIndex);

    }

    // detect if a click occurs
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log(bookData.bookTitle + " Game Object Right Clicked");
        }

        if (eventData.dragging)
        {
            Debug.Log("Dragging " + bookData.bookTitle);
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log(bookData.bookTitle + " Game Object Left Clicked");
        }
    }

    void ResetBookPosition(Vector3 originalPosition, GameObject originalParent, int siblingIndex)
    {
        rectTransform.position = originalPosition;
        rectTransform.SetParent(originalParent.transform);
        rectTransform.SetSiblingIndex(siblingIndex);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}