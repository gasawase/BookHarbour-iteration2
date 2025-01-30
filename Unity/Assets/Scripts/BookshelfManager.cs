using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using BookHarbour;
using Unity.VisualScripting;

public class BookshelfManager : GeneralFunctionality
{
    public float bookPadding = 0.05f;
    public Bookshelf bookshelf;
    public BookshelfMapping bookshelfMapping;
    private IndividualShelf individualShelf;
    private PerShelfObjectMapping perShelfObjectMapping;
    private Dictionary<string, Vector3> trackedObjects; // key: object UID | value: its location
    [SerializeField] private int bookshelfIndex;
    [SerializeField] private BoxCollider bookshelfDropZone;
    public bool isValidDropAreaInBookshelf {get; set;}
    
    private void Start()
    {
            Debug.Log($"There is no bookshelf with id {bookshelfIndex}");
            CreateBookshelf(bookshelf);
            Debug.Log($"This bookshelf has the id of {bookshelfIndex}");
    }

    private void Awake()
    {
        BookManager.OnActiveBooksCreated += FunctionToRunWhenBMInstantiated;
        if (BookManager.IsInstanceCreated)
        {
            FunctionToRunWhenBMInstantiated();
        }
    }

    private void OnDisable()
    {
        BookManager.OnActiveBooksCreated -= FunctionToRunWhenBMInstantiated;
    }

    public void MakeShelves()
    {
        bookshelf.bookshelfMapping.GenerateShelves(bookshelf.arrayOfShelves);
        Debug.Log($"Number of BookshelfMappings: {bookshelf.bookshelfMapping.ShelfMappings.Count}");
    }

    private void CreateBookshelf(Bookshelf bookshelf)
    {
        bookshelf.bookshelfIdx = bookshelfIndex;
        bookshelfMapping = new BookshelfMapping();
        bookshelf.bookshelfMapping = bookshelfMapping;

        for (int i = 0; i < bookshelf.arrayOfShelves.Length; i++)
        {
            GameObject shelf = bookshelf.arrayOfShelves[i];
            BoxCollider boxCollider = shelf.GetComponent<BoxCollider>();
            int shelfIndex = i;
            IndividualShelf individualShelf = new IndividualShelf(boxCollider.bounds.size.x, boxCollider.bounds.size.y);
            individualShelf.shelfIndex = shelfIndex;
            individualShelf.shelfLocation = shelf.transform.position;
        }
    }
    public bool CanFit(BookshelfObjectData obj, IndividualShelf individualShelf)
    {
        
        return false;
    }

    public bool IsSpaceOccupied(Vector2 objLocationOnBookshelf)
    {
        return false;
    }

    public void StackObject(BookshelfObjectData obj, IndividualShelf individualShelf)
    {
        throw new NotImplementedException();
    }

    public void SaveObjectToShelf(BookshelfObjectData obj, IndividualShelf individualShelf)
    {
        throw new NotImplementedException();
        // get the shelf that it's on, save it there based on the individual mapping and such
        // update the book's location, add it to the dictionary?
    }

    public void Calculations()
    {
        throw new NotImplementedException();
    }

    // specifically when the 3D object is inside the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            isValidDropAreaInBookshelf = false;
            return;
        }
        if (other.CompareTag("Draggable"))
        {
            isValidDropAreaInBookshelf = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            isValidDropAreaInBookshelf = false;
            return;
        }
        if (other.CompareTag("Draggable"))
        {
            isValidDropAreaInBookshelf = true;
        }
        
    }

    private void FunctionToRunWhenBMInstantiated()
    {
        GetTrackedObjects();
    }

    public void GetTrackedObjects()
    {
        // on start, get all of the objects in this scene and save them to a new Dictionary
        trackedObjects = new Dictionary<string, Vector3>();
        Dictionary<string, Book> tempActiveBooks = BookManager.Instance.GetAllBooks();
        //cycle through tempActiveBooks and note down their Vectors if they are there
        foreach (var book in tempActiveBooks)
        {
            //Debug.Log($"tracked Object: {trackedObjects[book.Key]}");
            if (book.Key != null)
            {
                trackedObjects[book.Key] = book.Value.objTransform;
            }
        }
    }

    // specifically when the 3D object is outside the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            isValidDropAreaInBookshelf = false;
            return;
        }
        if (other.CompareTag("Draggable"))
        {
            isValidDropAreaInBookshelf = false;
        }
    }

    public void TrackObjectMovement(string objectMovedUID, GameObject objectMoved)
    {
        // get the master list of all objects and find this object by UID
        Book tempBook = BookManager.Instance.GetBookByUIDNonStatic(objectMovedUID);
        // validation to make sure that this is a valid UID
        if (tempBook == null || tempBook.objTransform == null)
        {
            Debug.LogWarning("[Bookshelf Manager] BookshelfObjectData is null");
            return;
        }
        
        // Get the new position
        Vector3 newPosition = objectMoved.transform.position; // this needs to get its current location
        
        // check to see if its position changed
        if (trackedObjects.ContainsKey(objectMovedUID))
        {
            Vector3 oldPosition = trackedObjects[objectMovedUID];
            if (oldPosition == newPosition)
            {
                Debug.Log("[Bookshelf Manager] The old position and new position are the same");
                return; // do nothing and break because it's in the same position as before
            }
            Debug.Log($"[Bookshelf Manager] the old position for obj {objectMovedUID} is {oldPosition} and the new position is {newPosition}");

            // update the tracking list for this object so that it's got the same position
            trackedObjects[objectMovedUID] = newPosition;
            
            // Save to the CoreData
            //DataManager.Instance.SaveObjectData(objectMovedUID);
            
            // until move to Swift, use the following function
            BookManager.Instance.UpdateBookLocation(objectMovedUID, newPosition);
        }
    }
}
