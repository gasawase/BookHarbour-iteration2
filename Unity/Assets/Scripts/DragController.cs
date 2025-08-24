using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    // place the object
    // when going past a certain point on the UI, turn it to 3D object.
    [SerializeField] 
    private InputAction mouseClick;

    [SerializeField] 
    private float mouseDragSpeed = 0.1f;
    private Vector2 velocity = Vector2.zero;
    
    [Header("UI bounds")]
    [SerializeField] RectTransform panelRect;   // <-- assign your Panel in Inspector
    [SerializeField] float edgeMargin = 8f;     // hysteresis so it doesn’t flicker at the border

    [Header("World drop")]
    [SerializeField] GameObject worldPrefab;
    [SerializeField] GameObject worldGhostPrefab;      // optional transluc. preview
    [SerializeField] LayerMask placementMask = ~0;
    [SerializeField] float rayDistance = 1000f;

    RectTransform rectTransform, parentRect;
    CanvasGroup canvasGroup;
    Vector2 grabOffset, vel;
    public float smoothTime = 0.05f;

    bool isOutside;                 // state while dragging
    GameObject ghost;               // world preview instance

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
        
        // initialize in/out state
        isOutside = IsInsidePanel(e);
        if (isOutside) BeginWorldPreview(e);
    }
    
    public void OnDrag(PointerEventData e)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, e.position, e.pressEventCamera, out var pPoint))
        {
            Vector2 target = pPoint + grabOffset;

            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                rectTransform.anchoredPosition, target, ref vel,
                smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }
        
        // transition detection
        bool outsideNow = IsInsidePanel(e);
        if (outsideNow != isOutside)
        {
            isOutside = outsideNow;
            if (isOutside) BeginWorldPreview(e);
            else EndWorldPreview();
        }

        // drive world preview while outside
        if (isOutside) UpdateWorldPreview(e);
    }
    
    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;

        if (isOutside && TryRaycastWorld(e, out var hit))
        {
            // finalize spawn
            //Quaternion rot = LookAlongSurface(hit.normal, e.pressEventCamera ?? Camera.main);
            //Instantiate(worldPrefab, hit.point, rot);
            if (this.GetComponent<BookScript>().BookPrefab)
            {
                Instantiate(this.GetComponent<BookScript>().BookPrefab, hit.point, Quaternion.identity);
                this.GetComponent<BookScript>().SetBookScaleFactor();
            }
            // TODO: later checks for other objects and instantiate those here; alternatively can create a function that goes through the different objects that can be spawned
        }

        EndWorldPreview();
        // if you want to return UI to its list when not spawned:
        // rectTransform.anchoredPosition = startAnchoredPos;
        // if (not dropped on a shelf) then put back into list; else remove from list and save to shelf
        // rectTransform.anchoredPosition = startAnchoredPos;

    }
    
    // ----- Panel containment -----

    bool IsInsidePanel(PointerEventData e)
    {
        Debug.Log("Is inside panel");
        var cam = e.pressEventCamera ?? Camera.main;
        if (!panelRect) return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect, e.position, cam, out var local);
        var r = panelRect.rect;
        // shrink a bit to avoid oscillating at the edge
        r.xMin += edgeMargin; r.yMin += edgeMargin;
        r.xMax -= edgeMargin; r.yMax -= edgeMargin;
        return !r.Contains(local);
    }

    // ----- World preview / drop -----

    void BeginWorldPreview(PointerEventData e)
    {
        Debug.Log("Begin World Preview");
        if (worldGhostPrefab && !ghost)
        {
            //worldGhostPrefab.transform.localScale = this.GetComponent<BookScript>().bookModel.BookScaleFactor;
            Debug.Log($"world ghost local scale: {worldGhostPrefab.transform.localScale}");
            ghost = Instantiate(worldGhostPrefab);
            Debug.Log($"ghost local scale: {ghost.transform.localScale}");
            //this.gameObject.transform.localScale = scaleFactorFactor;
        }
        // optional: fade/hide the UI icon while outside
        // canvasGroup.alpha = 0.6f;
    }

    void UpdateWorldPreview(PointerEventData e)
    {
        //Debug.Log("Update World Preview");
        if (!ghost) return;
        if (TryRaycastWorld(e, out var hit))
        {
            ghost.transform.SetPositionAndRotation(
                hit.point, LookAlongSurface(hit.normal, e.pressEventCamera ?? Camera.main));
        }
    }

    void EndWorldPreview()
    {
        if (ghost) Destroy(ghost);
        // canvasGroup.alpha = 1f;
    }

    bool TryRaycastWorld(PointerEventData e, out RaycastHit hit)
    {
        var cam = e.pressEventCamera ?? Camera.main;
        Ray ray = cam.ScreenPointToRay(e.position);
        return Physics.Raycast(ray, out hit, rayDistance, placementMask);
    }

    static Quaternion LookAlongSurface(Vector3 normal, Camera cam)
    {
        Vector3 fwd = Vector3.ProjectOnPlane(cam.transform.forward, normal).normalized;
        return fwd.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(fwd, normal) : Quaternion.identity;
    }

    public void Spawn3DModel()
    {
        
    }

    public void GenerateSnapPoints(GameObject draggingObject)
    {
        // generates spawn points on drag start
        // get all shelves' width // another option can get whatever shelf you're highlighting's width
        // get shelf
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
}
