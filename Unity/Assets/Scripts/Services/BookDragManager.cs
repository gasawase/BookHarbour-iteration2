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

        [SerializeField] public string currentlyDraggedObjectUID;
        [Header("Ghost")]
        [SerializeField] GameObject ghostPrefab;

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
        private GameObject ghostRoot;
        private Transform ghostVisual;
        private MeshFilter ghostFilter;

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

            ghostRoot = Instantiate(ghostPrefab);
            ghostVisual = ghostRoot.transform.GetChild(0);
            ghostFilter = ghostVisual.GetComponent<MeshFilter>();
            ghostRoot.SetActive(false);
        }
        public void BeginDragUIObject(UIBookDragSource source, Vector2 screenPoint)
        {
            bookUIObject = source;
            UIBookData locUIBookData = bookUIObject.GetComponent<UIBookData>();
            isOffPanel = false;
            currentlyDraggedObjectUID = locUIBookData.bookId;
            Debug.Log($"Currently dragged object: {currentlyDraggedObjectUID}");
            draggedObject = SpawningService.Instance.SpawnBook(currentlyDraggedObjectUID);
            PrepareGhost();
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
            PrepareGhost();
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
            if (TryFindDropTarget(out ShelfModel shelf, out float offset)) { ShowGhost(shelf, draggedObject.GetComponent<IShelfPlaceable>(), offset); }
            else { HideGhost(); }
            // get collider and the shelfid of the collider 

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
            bool canPlace = TryFindDropTarget(out ShelfModel targetShelf, out float snapOffset);
            ShelfLayoutService layoutService = PersistanceManager.Instance.shelfLayoutService;
            bool cameFromPanel = bookUIObject != null;
            draggedObject.TryGetComponent<IShelfPlaceable>(out IShelfPlaceable placeable);
            string draggedBookID = placeable?.GetObjectUID();

            if (canPlace) // 1b PANEL → SHELF | 2a SHELF → SHELF
            {
                if (cameFromPanel)
                {
                    Destroy(bookUIObject.gameObject);
                    PersistanceManager.Instance.RemoveBookEntryFromUIBookDataDict(draggedBookID);
                }
                layoutService.PlaceItem(targetShelf.shelfId, placeable, snapOffset);
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
            HideGhost();
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
        }

        #region Focused Bookshelf
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
        #endregion

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
            hasDragPlane = true;
            return true;
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

        public bool TryFindDropTarget(out ShelfModel shelf, out float offset)
        {
            shelf = null; offset = 0f;
            if (!draggedObject.TryGetComponent<IShelfPlaceable>(out IShelfPlaceable shelfPlaceable)) { return false; }
            if (bookshelfInFocus == null) { return false; }

            shelf = bookshelfInFocus.FindShelfUnder(draggedObject.transform.position);
            if (shelf == null) { return false; }

            float desiredLeft = shelf.layoutInfo.ToU(draggedObject.transform.position) - shelfPlaceable.GetPlacementWidth() / 2; // get the center
            float? snapOffset = PersistanceManager.Instance.shelfLayoutService.FindSnapOffset(shelf.shelfId, shelfPlaceable, desiredLeft);

            if (!snapOffset.HasValue) { return false; } // the shelf we're trying to get into is full

            offset = snapOffset.Value;
            return true;
        }

        #region Ghost
        public void PrepareGhost()
        {
            MeshFilter sourceFilter = draggedObject.GetComponentInChildren<MeshFilter>();
            ghostFilter.sharedMesh = sourceFilter.sharedMesh;
            ghostVisual.localPosition = sourceFilter.transform.localPosition;
            ghostVisual.localRotation = sourceFilter.transform.localRotation;
            ghostVisual.localScale = sourceFilter.transform.localScale;
        }
        public void ShowGhost(ShelfModel shelf, IShelfPlaceable placeable, float? offset)
        {
            float? center = offset + placeable.GetPlacementWidth() / 2;
            if (!center.HasValue) { Debug.LogWarning($"{placeable.GetObjectUID()} threw an error when calculating the width."); return; }
            ghostRoot.transform.position = shelf.layoutInfo.ToWorldPosition(center.Value);
            ghostRoot.transform.rotation = draggedObject.transform.rotation;
            ghostRoot.SetActive(true);
        }
        public void HideGhost()
        {
            ghostRoot.SetActive(false);
        }
        #endregion
    }
}