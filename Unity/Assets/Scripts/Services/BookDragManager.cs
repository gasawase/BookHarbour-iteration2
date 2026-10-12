using Assets.Scripts.Models;
using Assets.Scripts.Services.Interfaces;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Owns book-drag state and logic for the whole scene. Intended to be
/// attached as a sibling component on the same GameObject as
/// PersistenceManager, so it rides along with that object's
/// DontDestroyOnLoad call rather than managing its own persistence.
///
/// BookDragSource components on individual book buttons forward pointer
/// events here rather than holding any drag logic themselves.
/// </summary>
namespace Assets.Scripts.Services
{
    public class BookDragManager : MonoBehaviour
    {
        public static BookDragManager Instance { get; private set; }

        [Header("Panel / masking")]
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private Camera uiCamera; // Leave null for Screen Space - Overlay

        [Header("3D projection")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float dragPlaneDepthOffset;

        private DragPlaneProjector projector;
        private BookshelfModel bookshelfInFocus;
        private Plane dragPlane;
        private bool hasDragPlane;
        private UIBookDragSource bookUIObject;
        private GameObject draggedObject;
        private bool isOffPanel;
        private bool insidePanel;
        private List<BookshelfModel> BookshelvesInScene => PersistanceManager.Instance.shelvesInScene;

        private Vector3 startingPos;

        [SerializeField] public string currentlyDraggedObjectUID;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            if (PersistanceManager.Instance == null)
            {
                Debug.LogError($"{name}: PersistanceManager not initialized. Check execution order.");
                return;
            }
            Instance = this;
            if (!worldCamera.GetComponent<PhysicsRaycaster>())
            {
                worldCamera.AddComponent<PhysicsRaycaster>();
            }
            projector = new DragPlaneProjector(worldCamera);
        }
        public void BeginDrag(UIBookDragSource source, Vector2 screenPoint)
        {
            bookUIObject = source;
            UIBookData locUIBookData = bookUIObject.GetComponent<UIBookData>();
            isOffPanel = false;
            currentlyDraggedObjectUID = locUIBookData.bookId;
            Debug.Log($"Currently dragged object: {currentlyDraggedObjectUID}");
            draggedObject = SpawningService.Instance.SpawnBook(currentlyDraggedObjectUID);
            startingPos = draggedObject.transform.position;
            TryBuildDragPlane();
            Debug.Log($"DragPlane normal: {dragPlane.normal} | BookshelfInFocus.Transform.Forward {bookshelfInFocus.transform.forward}");
            Debug.Log($"dragPlaneDepthOffset: {dragPlaneDepthOffset}");
            UpdateBookPosition(screenPoint);

        }

        public void BeginDrag3DObject(Object3DDragSource source, Vector2 screenPoint)
        {
            bookUIObject = null;
            draggedObject = source.gameObject;
            startingPos = draggedObject.transform.position;
            isOffPanel = false;
            TryBuildDragPlane();

            UpdateBookPosition(screenPoint);
        }

        public void UpdateDrag(Vector2 screenPoint)
        {
            if (draggedObject == null)
            {
                return;
            }

            UpdateBookPosition(screenPoint);
            // get collider and the shelfid of the collider 
            Collider locCollider = null;
            if (draggedObject.TryGetComponent<ShelfOverlapDetector>(out ShelfOverlapDetector shelfOverlapDetector))
            {
                locCollider = shelfOverlapDetector.shelfCollider;
            }

            insidePanel = RectTransformUtility.RectangleContainsScreenPoint(panelRectTransform, screenPoint, uiCamera);

            if (!insidePanel && !isOffPanel)
            {
                isOffPanel = true;
                SetActiveSourceVisible(false);
                // currently have a problem where if you've already dragged the ui book off and thus it's destroyed, dragging the 3D book back in won't do anything because there's no UI book object to make visible
            }
            else if (insidePanel && isOffPanel)
            {
                isOffPanel = false;
                SetActiveSourceVisible(true);
            }
        }

        // DRAG/DROP STATE RULES
        //
        // 1a) PANEL → PANEL (same place):
        //     - Destroy/despawn 3D object.
        //     - Do NOT change DB/location.
        //     - UI Book remains as-is (UI should look like nothing happened).
        //
        // 1b) PANEL → SHELF:
        //     - Destroy/despawn UI Book.
        //     - Remove book from PersistenceManager's "available UI books" dictionary.
        //     - Update book's DB/location to the shelf.
        //     - 3D shelf object becomes the book's representation/data holder (at minimum bookId).
        //
        // 1c) PANEL → INVALID/OTHER:
        //     - Same behavior as 1a.
        //     - Destroy/despawn 3D object.
        //     - Do NOT change DB/location.
        //     - UI Book remains as-is.
        //
        // 2a) SHELF → SHELF (different/new shelf location):
        //     - Update DB/location to the new shelf location.
        //     - Otherwise, nothing changes.
        //
        // 2b) SHELF → PANEL:
        //     - Destroy/despawn 3D shelf object.
        //     - Clear/null the book's shelf location in the DB.
        //     - Re-create/show the UI Book in the panel.
        //     - Add the book back to PersistenceManager's "available UI books" dictionary.
        //     - UI Book may not return to its exact original position because books are no longer sorted.
        //
        // 2c) SHELF → INVALID/OTHER:
        //     - Same behavior as 2b.
        //     - Destroy/despawn 3D shelf object.
        //     - Clear/null the shelf location.
        //     - Re-create/show the UI Book.
        //     - Add it back to the "available UI books" dictionary.

        public void EndDrag(Vector2 screenPoint)
        {
            if (draggedObject == null) { return; }

            ShelfLayoutService layoutService = PersistanceManager.Instance.shelfLayoutService;
            bool cameFromPanel = bookUIObject != null;
            draggedObject.TryGetComponent<IShelfPlaceable>(out IShelfPlaceable placeable);
            string draggedBookID = placeable?.GetObjectUID();

            // which of the shelves on the focused bookshelf is under the cursor and where on the selected shelf does the book fit
            ShelfModel targetShelf = bookshelfInFocus != null ? bookshelfInFocus.FindShelfUnder(draggedObject.transform.position) : null;
            float? snapOffset = null;
            if (targetShelf != null && placeable != null)
            {
                float desiredLeft = targetShelf.layoutInfo.ToU(draggedObject.transform.position) - placeable.GetPlacementWidth() / 2;
                snapOffset = layoutService.FindSnapOffset(targetShelf.shelfId, placeable, desiredLeft);
            }

            if (snapOffset.HasValue) // 1b PANEL → SHELF | 2a SHELF → SHELF
            {
                if (cameFromPanel)
                {
                    Destroy(bookUIObject.gameObject);
                    PersistanceManager.Instance.RemoveBookEntryFromUIBookDataDict(draggedBookID);
                }
                layoutService.PlaceItem(targetShelf.shelfId, placeable, snapOffset.Value);
            }
            else if (cameFromPanel) // 1a PANEL → PANEL | 1c PANEL → INVALID | panel → full shelf
            {
                SetActiveSourceVisible(true);
                Destroy(draggedObject); // TODO: will change when you move to pooling objects
            }
            else if (targetShelf != null) // SHELF → FULL SHELF; PUT BACK
            {
                draggedObject.transform.position = startingPos;
            }
            else // 2b SHELF → PANEL | 2c SHELF → INVALID
            {
                ReturnBookFromShelfToPanel(draggedBookID);
                Destroy(draggedObject); // TODO: will change when you move to pooling objects
            }
            ResetDragState();
        }

        private void ReturnBookFromShelfToPanel(string bookID)
        {
            PersistanceManager.Instance.shelfLayoutService.RemoveItemFromShelves(bookID);
            try
            {
                PersistanceManager.Instance.sqliteService.DeleteBookFromLocation(bookID);
                PersistanceManager.Instance.RestoreBookToPanel(bookID);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex); // TODO: rollback — the book is off the shelf but may not be back in the panel
            }
        }

        private void ResetDragState()
        {
            bookUIObject = null;
            draggedObject = null;
            hasDragPlane = false;
        }

        private void UpdateBookPosition(Vector2 screenPoint)
        {
            if (draggedObject == null || !hasDragPlane)
            {
                return;
            }
            if (projector.TryProject(screenPoint, dragPlane, out Vector3 worldPoint))
            {
                draggedObject.transform.position = worldPoint;
            }
            //    if (draggedObject.TryGetComponent<ShelfOverlapDetector>(out ShelfOverlapDetector draggedObjectShelfOverlapDetector) 
            //        && draggedObject.TryGetComponent<IShelfPlaceable>(out IShelfPlaceable draggedObjectShelfPlaceableInterface) 
            //        && !draggedObjectShelfOverlapDetector.currentShelfID.IsNullOrWhiteSpace())
            //    {
            //        //PersistanceManager.Instance.shelfLayoutService.PreviewInsertion(draggedObjectShelfOverlapDetector.currentShelfID, draggedObjectShelfPlaceableInterface, draggedObject.transform.position);
            //    }
        }

        public void SetFocusedBookshelf(BookshelfModel bookshelf)
        {
            if (bookshelf == null || draggedObject != null)
            {
                return; // we're dragging something but not focusing on a shelf
            }
            bookshelfInFocus = bookshelf;
        }

        public void ClearFocusedBookshelf()
        {
            if (draggedObject != null) { return; } // prevents clearing the bookshelf if we're trying to drag it onto this bookshelf
            BookshelfModel bookshelf = bookshelfInFocus ?? FindShelfNearestScreenCenter();
            PersistanceManager.Instance.ClearBookshelf(bookshelf);
        }

        private bool TryBuildDragPlane()
        {
            if (bookshelfInFocus == null)
            {
                List<BookshelfModel> shelves = BookshelvesInScene;

                if (shelves == null || shelves.Count == 0)
                {
                    hasDragPlane = false;
                    return false;
                }
                bookshelfInFocus = shelves[0];
            }

            dragPlane = projector.BuildPlane(bookshelfInFocus.transform, dragPlaneDepthOffset);
            //Plane dragPlane = projector.BuildPlane(shelfReference, dragPlaneDepthOffset);
            //if (projector.TryProject(screenPoint, dragPlane, out Vector3 worldPoint))
            //{
            //    draggedObject.transform.position = worldPoint;
            //}
            //dragPlane = projector.BuildPlane(bookshelfInFocus.transform, dragPlaneDepthOffset);
            hasDragPlane = true;
            return true;
        }

        public BookshelfModel FindShelfNearestScreenCenter()
        {
            List<BookshelfModel> shelves = BookshelvesInScene;
            BookshelfModel bestBookshelfFocusMatch = null;
            float bestScore = float.MaxValue;
            if (shelves != null)
            {
                for (int i = 0; i < shelves.Count; i++)
                {
                    Vector3 viewportPoint = worldCamera.WorldToViewportPoint(shelves[i].transform.position);
                    if (viewportPoint.z <= 0f) // there's a bookshelf behind the camera
                    {
                        continue;
                    }

                    float focusScore = (new Vector2(viewportPoint.x, viewportPoint.y) - new Vector2(0.5f, 0.5f)).sqrMagnitude;
                    if (focusScore < bestScore)
                    {
                        bestScore = focusScore;
                        bestBookshelfFocusMatch = shelves[i];
                    }
                }
            }
            else
            {
                return null;
            }
            return bestBookshelfFocusMatch;
        }
        public void SetActiveSourceVisible(bool visible)
        {
            if (bookUIObject != null && bookUIObject.CanvasGroup != null)
            {
                CanvasGroup canvasGroup = bookUIObject.CanvasGroup;
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}