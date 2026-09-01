using Assets.Scripts.Models;
using Assets.Scripts.Services;
using System;
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
        [SerializeField] private Transform shelfReference;
        [SerializeField] private float dragPlaneDepthOffset;

        private DragPlaneProjector projector;
        private UIBookDragSource bookUIObject;
        private GameObject spawned3DBookObject;
        private bool isOffPanel;
        private bool insidePanel;

        [SerializeField] public string currentlyDraggedObjectUID;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
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
            spawned3DBookObject = Instantiate(locUIBookData.book3dPrefab);
            spawned3DBookObject.GetComponent<Object3DDragSource>().bookId = locUIBookData.bookId;
            spawned3DBookObject.GetComponent<BookObject3D>().Initialize(locUIBookData.bookId);
            UpdateBookPosition(screenPoint);

        }

        public void BeginDrag3DObject(Object3DDragSource source, Vector2 screenPoint)
        {
            bookUIObject = null;
            spawned3DBookObject = source.gameObject;
            isOffPanel = false;
            UpdateBookPosition(screenPoint);
        }

        public void UpdateDrag(Vector2 screenPoint)
        {

            if (spawned3DBookObject == null)
            {
                return;
            }

            UpdateBookPosition(screenPoint);

            //if (activeUISource == null)
            //{
            //    return; // No grid button to mask/fade for a shelf-origin drag
            //}

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
// 1a) GRID → PANEL (same place):
//     - Destroy/despawn 3D object.
//     - Do NOT change DB/location.
//     - UI Book remains as-is (UI should look like nothing happened).
//
// 1b) GRID → SHELF:
//     - Destroy/despawn UI Book.
//     - Remove book from PersistenceManager's "available UI books" dictionary.
//     - Update book's DB/location to the shelf.
//     - 3D shelf object becomes the book's representation/data holder (at minimum bookId).
//
// 1c) GRID → INVALID/OTHER:
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
            if (spawned3DBookObject == null)
            {
                return;
            }
            string bookId = spawned3DBookObject.GetComponent<Object3DDragSource>().bookId;
            Collider locCollider = spawned3DBookObject.GetComponent<ShelfOverlapDetector>().shelfCollider;

            if (locCollider != null) // 1b, 2a, 
            {
                Debug.Log($"BookID: '{bookId}'");
                Debug.Log($"ShelfID: '{locCollider.GetComponent<ShelfModel>().shelfId}'");
                if (bookUIObject != null) // 1b
                {
                    Destroy(bookUIObject.gameObject);
                    PersistanceManager.Instance.RemoveBookEntryFromUIBookDataDict(bookId);
                }
                // 2a would be another else statement but nothing else happens other than updating the location so wee don't need another explicit else statement here
                PersistanceManager.Instance.sqliteService.UpdateBookLocation(bookId, locCollider.GetComponent<ShelfModel>().shelfId, null, DateTime.Now);
            }
            else // 1a, 1c, 2b, 2c
            {
                if (bookUIObject != null) // 1a
                {
                    SetActiveSourceVisible(true);
                }
                else // 2b
                {
                    PersistanceManager.Instance.sqliteService.DeleteBookFromLocation(bookId);
                    PersistanceManager.Instance.RestoreBookToPanel(bookId);
                }
                Destroy(spawned3DBookObject); // TODO: will change when you move to pooling objects 
            }
            bookUIObject = null;
            spawned3DBookObject = null;
        }

        private void UpdateBookPosition(Vector2 screenPoint)
        {
            if (spawned3DBookObject == null)
            {
                return;
            }

            Plane dragPlane = projector.BuildPlane(shelfReference, dragPlaneDepthOffset);

            if (projector.TryProject(screenPoint, dragPlane, out Vector3 worldPoint))
            {
                spawned3DBookObject.transform.position = worldPoint;
            }
        }

        private void SetActiveSourceVisible(bool visible)
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