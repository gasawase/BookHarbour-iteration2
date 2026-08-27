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
        private UIBookDragSource activeUISource;
        private GameObject spawnedBook;
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
            activeUISource = source;
            UIBookData locUIBookData = activeUISource.GetComponent<UIBookData>();
            isOffPanel = false;
            currentlyDraggedObjectUID = locUIBookData.bookId;
            Debug.Log($"Currently dragged object: {currentlyDraggedObjectUID}");
            spawnedBook = Instantiate(locUIBookData.book3dPrefab);
            spawnedBook.GetComponent<Object3DDragSource>().bookId = locUIBookData.bookId;
            spawnedBook.GetComponent<BookObject3D>().Initialize(locUIBookData.bookId);
            UpdateBookPosition(screenPoint);

        }

        public void BeginDrag3DObject(Object3DDragSource source, Vector2 screenPoint)
        {
            activeUISource = null;
            spawnedBook = source.gameObject;
            isOffPanel = false;
            UpdateBookPosition(screenPoint);
        }

        public void UpdateDrag(Vector2 screenPoint)
        {

            if (spawnedBook == null)
            {
                return;
            }

            UpdateBookPosition(screenPoint);

            //if (activeUISource == null)
            //{
            //    return; // No grid button to mask/fade for a shelf-origin drag
            //}

            insidePanel = RectTransformUtility.RectangleContainsScreenPoint(
                panelRectTransform, screenPoint, uiCamera);

            Debug.Log($"isOffPanel: {isOffPanel}");
            Debug.Log($"isInsidePanel: {insidePanel}");

            if (!insidePanel && !isOffPanel)
            {
                isOffPanel = true;
                SetActiveSourceVisible(false);
            }
            else if (insidePanel && isOffPanel)
            {
                isOffPanel = false;
                SetActiveSourceVisible(true);
            }
        }

        public void EndDrag(Vector2 screenPoint)
        {
            if (spawnedBook == null)
            {
                return;
            }

            Collider locCollider = spawnedBook.GetComponent<ShelfOverlapDetector>().shelfCollider;
            string bookId = spawnedBook.GetComponent<Object3DDragSource>().bookId;

            if (locCollider == null) // failed drop
            {
                if (activeUISource != null)
                {
                    // Grid-origin drag that failed: this was a temporary spawn,
                    // so discard it and restore the grid button.
                    SetActiveSourceVisible(true);
                    Destroy(spawnedBook); // TODO: will change when you move to pooling objects
                }
                // Shelf-origin drag that failed: spawnedBook is the real placed
                // book, not a temp spawn - leave it where it is, nothing to destroy.
            }
            else if (locCollider != null)
            {
                Debug.Log($"BookID: '{bookId}'");
                Debug.Log($"ShelfID: '{locCollider.GetComponent<ShelfModel>().shelfId}'");

                PersistanceManager.Instance.sqliteService.UpdateBookLocation(
                    bookId, locCollider.GetComponent<ShelfModel>().shelfId, null, DateTime.Now);

                if (activeUISource != null)
                {
                    // Grid-origin drag placed successfully: remove the grid button,
                    // spawnedBook is now the book's permanent shelf representation.
                    Destroy(activeUISource.gameObject);
                }
                // Shelf-origin drag placed successfully: spawnedBook was already
                // the real object, just moved and re-recorded - nothing to destroy.
            }

            // TODO: snap-to-shelf-position logic goes here once
            // ShelfLayoutService exists. For now the book stays where dropped.

            activeUISource = null;
            spawnedBook = null;
        }

        private void UpdateBookPosition(Vector2 screenPoint)
        {
            if (spawnedBook == null)
            {
                return;
            }

            Plane dragPlane = projector.BuildPlane(shelfReference, dragPlaneDepthOffset);

            if (projector.TryProject(screenPoint, dragPlane, out Vector3 worldPoint))
            {
                spawnedBook.transform.position = worldPoint;
            }
        }

        private void SetActiveSourceVisible(bool visible)
        {
            if (activeUISource != null && activeUISource.CanvasGroup != null)
            {
                CanvasGroup canvasGroup = activeUISource.CanvasGroup;
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}