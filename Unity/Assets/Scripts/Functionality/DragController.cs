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
        private GameObject selectedObject; // currently dragged object; TODO: needs to be the emptyobject above it but it is detecting only the button part so tagging is weird
        private Vector2 originalPosition;
        private Vector2 dragOffset; // Offset between pointer and object's center


        private bool isDragging;

        // Input Actions
        public UserInputActions userInputActions;
        private InputAction dragAction;
        private InputAction pointerLocationAction;

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
        }

        private void Update()
        {
            if (isDragging && selectedObject != null)
            {
                Debug.Log($"Currently dragging: {selectedObject.name}");
                // Update the dragged object position based on pointer location
                Vector2 pointerPosition = pointerLocationAction.ReadValue<Vector2>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    pointerPosition,
                    canvas.worldCamera,
                    out Vector2 localPoint
                );
                // Convert the 2D localPoint and offset to a Vector3 for localPosition
                Vector3 newPosition = new Vector3(localPoint.x - dragOffset.x, localPoint.y - dragOffset.y, selectedObject.transform.localPosition.z);
                selectedObject.transform.localPosition = newPosition;            
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
                if (draggedObject.CompareTag("Draggable"))
                {
                    //Debug.Log($"Dragging: {draggedObject.name}");
                    selectedObject = draggedObject;
                    originalPosition = selectedObject.transform.localPosition;
                    
                    // Calculate the offset
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        pointerPosition,
                        canvas.worldCamera,
                        out Vector2 localPoint
                    );
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
                if (!IsValidDropArea(selectedObject.transform.position))
                {
                    selectedObject.transform.localPosition = originalPosition;
                }
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

