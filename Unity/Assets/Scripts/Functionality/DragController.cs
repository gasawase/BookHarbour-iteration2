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
        private GameObject selectedObject; // currently dragged object; TODO: needs to be the emptyobject above it but it is detecting only the button part so tagging is weird
        private GameObject selectedObjectOriginalParent;
        private Vector2 originalPosition;
        private Vector2 dragOffset; // Offset between pointer and object's center
        private Camera mainCamera;
        private RectTransform canvasRect;
        private RectTransform panelRect;

        private bool isDragging;

        // Input Actions
        public UserInputActions userInputActions;
        private InputAction dragAction;
        private InputAction pointerLocationAction;
        
        private bool IsPointerWithinCanvas(Vector2 localPoint)
        {
            // Get the Canvas's local rect
            Rect panelBounds = panelRect.rect;

            // Check if the pointer is within the rect bounds
            return panelBounds.Contains(localPoint);
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
            panelRect = sidePanel.transform as RectTransform;

        }

        private void Update()
        {
            if (isDragging && selectedObject != null)
            {
                // Update the dragged object position
                Vector2 pointerPosition = pointerLocationAction.ReadValue<Vector2>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    pointerPosition,
                    canvas.worldCamera,
                    out Vector2 localPoint
                );

                // Update object position with drag offset
                Vector3 newPosition = new Vector3(localPoint.x - dragOffset.x, localPoint.y - dragOffset.y, selectedObject.transform.localPosition.z);
                selectedObject.transform.localPosition = newPosition;

                // Check if pointer is outside the Canvas bounds
                if (!IsPointerWithinCanvas(localPoint))
                {
                    Debug.Log("Pointer is outside the Canvas!");
                    // Handle logic for crossing the canvas edge
                }
                else
                {
                    Debug.Log("Pointer is inside the Canvas!");
                }
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
                    Debug.Log("Parent: " + rootTransform.name);
                }
                // if the root has the Draggable tag, it is the correct prefab parent and thus is set as the correct object
                draggedObject = rootTransform;
                if (draggedObject.CompareTag("Draggable"))
                {
                    //Debug.Log($"Dragging: {draggedObject.name}");
                    selectedObject = draggedObject;
                    originalPosition = selectedObject.transform.localPosition;
                    selectedObjectOriginalParent = selectedObject.transform.parent.gameObject;
                    
                    // Re-parent to the root canvas for unrestricted movement
                    draggedObject.transform.SetParent(canvas.transform, true);
                    
                    // Calculate the offset
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        pointerPosition,
                        canvas.worldCamera,
                        out Vector2 localPoint
                    );
                    Debug.Log($"Local Point: {localPoint.x}, {localPoint.y}");

                    dragOffset = new Vector2 ((localPoint.x - draggedObject.transform.localPosition.x),(localPoint.y - draggedObject.transform.localPosition.y));
                    
                    isDragging = true;
                }
            }
        }

        private void DragCancelled(InputAction.CallbackContext context)
        {
            //Debug.Log("drag cancelled");

            if (isDragging)
            {
                // perform snapping logic here if needed
                isDragging = false; // flip the boolean
                
                // Optional: Reset position if dropped outside valid area
                // if (!IsValidDropArea(selectedObject.transform.position))
                // {
                //     selectedObject.transform.localPosition = originalPosition;
                // }
                // Re-parent to the original parent
                selectedObject.transform.SetParent(selectedObjectOriginalParent.transform, true);
                selectedObject.transform.localPosition = originalPosition;

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

