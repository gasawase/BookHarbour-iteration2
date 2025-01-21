using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BookHarbour
{
    public class DragController : GeneralFunctionality
    {
        [SerializeField] private Canvas canvas; // Reference to your canvas
        [SerializeField] private GameObject sidePanel;
        [SerializeField] private GameObject pointer;
        private GameObject selectedObject; // currently dragged object; 
        private GameObject selectedObjectOriginalParent;
        private GameObject spawned3DObject = null;
        private GameObject object3DPrefab;
        private GameObject objectSpawned = null;
        GameObject draggedObject = null;
        private Vector2 originalPosition;
        private Vector2 dragOffset; // Offset between pointer and object's center
        private Camera mainCamera;
        private RectTransform canvasRect;
        private Rect panelRect;
        private Bookshelf bookshelf;
        private List<Vector3> objectSnapPoints;

        private bool isDragging = false;
        private bool isUIObject;

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
            bookshelf = GetCurrentBookshelf();
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
            //Debug.Log($"SelectedObject: {selectedObject.name} and isDragging: {isDragging}");

            if (isDragging && selectedObject != null)
            {
                MoveObj();
            }
        }

        /// <summary>
        /// Returns whatever bookshelf is being shown and is visible; needs to be refined because it just
        /// shows what is shown on screen
        /// </summary>
        /// <returns></returns>
        public Bookshelf GetCurrentBookshelf()
        {
            BookshelfManager bookshelfManagerInScene = FindAnyObjectByType<BookshelfManager>();
            Bookshelf bookshelfInScene = bookshelfManagerInScene.bookshelf;
            return bookshelfInScene;
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
        private void MoveObj()
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
            //Debug.Log(selectedObject.name);

            if (spawned3DObject != null)
            {
                Debug.Log($"Spawned object at {spawned3DObject.gameObject.name}");
                Vector3 objTransform = new Vector3(newPosition.x, (newPosition.y - 0.5f), -0.6f);
                spawned3DObject.transform.position = objTransform;

            }
            pointer.transform.position = worldPoint;
            //Debug.Log($"{selectedObject.name} is dragged");

            if (isUIObject)
            {
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

        private void DragPerformed(InputAction.CallbackContext context) // assigns what is being dragged
        {
            if (isDragging) return; // Prevent re-detection during a drag

            // Detect object under pointer
            Vector2 pointerPosition = pointerLocationAction.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
            RaycastHit hit;

            //GameObject draggedObject;
            if (draggedObject != null)
            {
                Debug.Log($"Dragged object is: {draggedObject.gameObject.name}");

            }
            
            var raycastResults = new List<RaycastResult>();
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = pointerPosition
            };
            
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (var objHit in raycastResults)
            {
                Debug.Log($"Object hit: {objHit.gameObject.name}");
            }
            if (Physics.Raycast(ray, out hit))
            {
                draggedObject = GetParentDraggable(hit.collider.gameObject); // checks if it has a parent with the tag Draggable; if it has no parent, returns the object
                if (draggedObject.CompareTag("Draggable")) // double-checking to make sure that the object is draggable; catches if there is no parent
                {
                    selectedObject = draggedObject;
                    Debug.Log($"{selectedObject.name} is dragged; compare tag draggable");
                    var drag3DResult = Drag3DObject(selectedObject, pointerPosition);
                    //selectedObject = drag3DResult;
                }
                else
                {
                    Debug.Log("Whatever has been hit by a raycast can't be dragged");
                }
                isDragging = true;
            }
            // checking to see if the object was 2D
            else if (raycastResults.Count > 0)
            {
                // checking to see if the object was 3D
                //draggedObject = raycastResults[0].gameObject;
                //Debug.Log($"{draggedObject.name} is dragged");
                //draggedObject = GetParentDraggable(raycastResults[0].gameObject); // checks if it has a parent with the tag Draggable; if it has no parent, returns the object
                var dragUIResult = DragUIObject(raycastResults[0].gameObject, pointerPosition);
                spawned3DObject = dragUIResult.Item1;
                selectedObject = dragUIResult.Item2;
                draggedObject = selectedObject;
                isDragging = true;
            }
            else
            {
                Debug.Log("Error: Nothing to drag");
                isDragging = false;
                return;
            }
            
            // Calculate the offset
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                pointerPosition,
                canvas.worldCamera,
                out Vector3 worldPoint
            );
            dragOffset = new Vector2 ((worldPoint.x - draggedObject.transform.position.x),(worldPoint.y - draggedObject.transform.position.y));
        }

        private GameObject GetParentDraggable(GameObject rootTransform)
        {
            if (rootTransform.transform.parent == null)
            {
                return rootTransform;
            }
            // Traverse up the hierarchy to find the root prefab
            while (rootTransform.transform.parent != null && rootTransform.transform.parent.CompareTag("Draggable"))
            {
                rootTransform = rootTransform.transform.parent.gameObject;
            }
            return rootTransform;
        }

        private (GameObject, GameObject) DragUIObject(GameObject draggedObject, Vector2 pointerPosition)
        {
            isUIObject = true;
            draggedObject = GetParentDraggable(draggedObject);
            if (draggedObject.CompareTag("Draggable")) // double-checking to make sure that the object is draggable
            {
                selectedObject = draggedObject;
            }
            Debug.Log(draggedObject.name);

            var object3DPrefab = draggedObject.GetComponent<UIBookScript>().objPrefab;
            objectSpawned = Instantiate(object3DPrefab);
            for (int i = 0; i < bookshelf.arrayOfShelves.Length; i++)
            {
                objectSnapPoints = GenerateSnapPoints(objectSpawned, bookshelf, bookshelf.bookPadding);
            }

            isDragging = true;

            return (objectSpawned, draggedObject);
        }

        private GameObject Drag3DObject(GameObject draggedObject, Vector2 pointerPosition)
        {
            isUIObject = false;
            draggedObject = GetParentDraggable(draggedObject.gameObject);
            GameObject thisObject = draggedObject;
            if (draggedObject.CompareTag("Draggable")) // double-checking to make sure that the object is draggable
            {
                thisObject = draggedObject;
            }
            isDragging = true;
            return thisObject;
        }
        private void DragCancelled(InputAction.CallbackContext context)
        {
            // if it is not in a valid location, destroy it
            if (isDragging)
            {
                // perform snapping logic here if needed
                if (objectSpawned != null)
                {
                    Vector3 currPos = objectSpawned.transform.position;
                    Vector3 closestSnap = FindClosestSnapPoint(objectSnapPoints, currPos);
                    objectSpawned.transform.position = closestSnap;
                }
                isDragging = false; // flip the boolean
                //Debug.Log(selectedObject.transform.position);
                selectedObject = null; // remove the object from underneath the mouse
                draggedObject = null;
            }
            spawned3DObject = null;

        }
        private bool IsValidDropArea(Vector3 position)
        {
            // Replace this with your logic for detecting valid drop zones
            return true;
        }

        private void ResetPosition(GameObject selectedObject)
        {
            // Re-parent to the original parent
            //selectedObject.SetActive(true);
            if (selectedObjectOriginalParent != null)
            {
                selectedObject.transform.SetParent(selectedObjectOriginalParent.transform, true);
            }
            selectedObject.transform.position = originalPosition;
        }
    }
}

