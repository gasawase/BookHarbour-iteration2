using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BookHarbour
{
    public class DragController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas; // Reference to your canvas
        [SerializeField] private GameObject sidePanel;
        [SerializeField] private GameObject pointer;
        private GameObject selectedObject; // currently dragged object; 
        private GameObject selectedObjectOriginalParent;
        private GameObject spawned3DObject;
        private GameObject object3DPrefab;
        private Vector2 originalPosition;
        private Vector2 dragOffset; // Offset between pointer and object's center
        private Camera mainCamera;
        private RectTransform canvasRect;
        private Rect panelRect;

        private bool isDragging;

        // Input Actions
        public UserInputActions userInputActions;
        private InputAction dragAction;
        private InputAction pointerLocationAction;
        
        private bool IsPointerWithinPanel(Vector2 worldPoint)
        {
            
            // Get the Panel's local rect
            //Rect panelBounds = panelRect.rect;
            Rect panelBounds = panelRect;
            // Check if the pointer is within the rect bounds
            return panelBounds.Contains(worldPoint);
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Awake()
        {
            // DragController controller = FindAnyObjectByType<DragController>();
            // if (controller != null)
            // {
            //     Destroy(gameObject);
            // }

            if (userInputActions == null)
            {
                userInputActions = new UserInputActions();
                userInputActions.Enable();
            }

            dragAction = userInputActions.User.ClickPress;
            pointerLocationAction = userInputActions.User.PointerPosition;
            canvasRect = canvas.transform as RectTransform;
            //panelRect = sidePanel.transform as RectTransform;
            panelRect = GetPanelWorldRect(sidePanel.transform as RectTransform);
        }

        private void Update()
        {
            if (isDragging && selectedObject != null)
            {
                MoveUIObj();
            }
        }

        public Rect GetPanelWorldRect(RectTransform rectTransform)
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            // Convert world corners into a Rect in world space
            Vector3 bottomLeft = worldCorners[0];
            Vector3 topRight = worldCorners[2];
            Rect worldRect = new Rect(
                bottomLeft.x,
                bottomLeft.y,
                topRight.x - bottomLeft.x,
                topRight.y - bottomLeft.y
            );
            return worldRect;
        }
        private void MoveUIObj()
        {
            // Update the dragged object position
            Vector2 pointerPosition = pointerLocationAction.ReadValue<Vector2>();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasRect,
                pointerPosition,
                mainCamera,
                out Vector3 worldPoint
            );

            // Update object position with drag offset
            Vector3 newPosition = new Vector3(worldPoint.x - dragOffset.x, worldPoint.y - dragOffset.y, selectedObject.transform.position.z);
            selectedObject.transform.position = newPosition;
            if (spawned3DObject != null)
            {
                Vector3 objTransform = new Vector3(newPosition.x, (newPosition.y - 0.5f), -0.6f);
                spawned3DObject.transform.position = objTransform;

            }
            pointer.transform.position = worldPoint;

            // Check if pointer is outside the Panel bounds
            if (!IsPointerWithinPanel(worldPoint))
            {
                // Handle logic for crossing the panel edge
                selectedObject.SetActive(false);
            }
            else
            {
                selectedObject.SetActive(true);
                // do something when inside the panel
            }
        }

        private void OnEnable()
        {
            dragAction.performed += DragPerformed;
            dragAction.canceled += DragCancelled;
        }

        private void OnDisable()
        {
            dragAction.performed -= DragPerformed;
            dragAction.canceled -= DragCancelled;
        }

        private void DragPerformed(InputAction.CallbackContext context)
        {
            //Debug.Log("drag performed");
            if (isDragging) return; // Prevent re-detection during a drag
            

            // Detect object under pointer
            Vector2 pointerPosition = pointerLocationAction.ReadValue<Vector2>();
            var raycastResults = new List<RaycastResult>();
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = pointerPosition
            };
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            if (raycastResults.Count > 0)
            {
                
                GameObject draggedObject = raycastResults[0].gameObject;
                
                // Traverse up the hierarchy to find the root prefab
                GameObject rootTransform = draggedObject.gameObject;
                while (rootTransform.transform.parent != null && rootTransform.transform.parent.CompareTag("Draggable"))
                {
                    rootTransform = rootTransform.transform.parent.gameObject;
                }
                // if the root has the Draggable tag, it is the correct prefab parent and thus is set as the correct object
                draggedObject = rootTransform;
                if (draggedObject.CompareTag("Draggable"))
                {
                    selectedObject = draggedObject;
                    originalPosition = selectedObject.transform.position;
                    selectedObjectOriginalParent = selectedObject.transform.parent.gameObject;
                    
                    // Re-parent to the root canvas for unrestricted movement
                    draggedObject.transform.SetParent(canvas.transform, true);
                    
                    //draggedObject.transform.parent
                    
                    // Calculate the offset
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        canvas.transform as RectTransform,
                        pointerPosition,
                        canvas.worldCamera,
                        out Vector3 worldPoint
                    );
                    dragOffset = new Vector2 ((worldPoint.x - draggedObject.transform.position.x),(worldPoint.y - draggedObject.transform.position.y));
                    
                    isDragging = true;
                }
                object3DPrefab = draggedObject.GetComponent<UIBookScript>().modelPrefab;
                spawned3DObject = Instantiate(object3DPrefab);
                pointer.SetActive(true);
            }
        }

        private void DragCancelled(InputAction.CallbackContext context)
        {
            //Debug.Log("drag cancelled");
            // if it is not in a valid location, destroy it
            if (isDragging)
            {
                // perform snapping logic here if needed
                isDragging = false; // flip the boolean
                
                // Re-parent to the original parent
                selectedObject.SetActive(true);
                selectedObject.transform.SetParent(selectedObjectOriginalParent.transform, true);
                selectedObject.transform.position = originalPosition;
                Debug.Log(selectedObject.transform.position);
                
                pointer.SetActive(false);
                selectedObject = null; // remove the object from underneath the mouse
            }
        }

        private bool IsValidDropArea(Vector3 position)
        {
            // Replace this with your logic for detecting valid drop zones
            return true;
        }
    }
}

