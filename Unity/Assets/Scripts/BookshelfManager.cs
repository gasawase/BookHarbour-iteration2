using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;
using Unity.VisualScripting;

public class BookshelfManager : GeneralFunctionality
{
    public Book defaultBook;
    public float bookPadding = 0.05f;
    public Bookshelf bookshelf;
    public BookshelfMapping bookshelfMapping;
    private IndividualShelf individualShelf;
    private PerShelfObjectMapping perShelfObjectMapping;
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    [SerializeField] private int bookshelfIndex;
    [SerializeField] private GameObject objectStandIn;
    [SerializeField] private GameObject tempBook;

    private void Start()
    {
        // if (bookshelfMapping == null)
        // {
        //     bookshelf.bookshelfIdx = bookshelfIndex;
        //     if (bookshelfMapping.ShelfMappings[bookshelf.bookshelfIdx] != null)
        //     {
        //         Debug.Log($"This bookshelf has no shelves");
        //     }
        // }
        // else
        // {
            Debug.Log($"There is no bookshelf with id {bookshelfIndex}");
            CreateBookshelf(bookshelf);
        // }
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
            Debug.Log($"This individual shelf has the size of x: {boxCollider.bounds.size.x}, y: {boxCollider.bounds.size.y}");
            int slotsHolder = CalculateSlots(boxCollider.bounds.size.x, defaultBook.bookSize.x, bookPadding);
            Debug.Log($"This shelf has {slotsHolder} slots");
            float remainingSpace = individualShelf.GetRemainingSpace(i);
            Debug.Log($"This shelf has {remainingSpace} remaining space");
            
            individualShelf.shelfLocation = shelf.transform.position;
            //CalculateSlotLocations(individualShelf, defaultBook.bookSize.x, bookPadding, bookshelf.arrayOfShelves);

            GenerateStandIns(individualShelf, tempBook, bookPadding, shelf, objectStandIn);
        }
        
    }
    public bool CanFit(BookshelfObject obj, IndividualShelf individualShelf)
    {
        
        return false;
    }

    public void PlaceObj(Vector2 position, BookshelfObject obj, IndividualShelf individualShelf)
    {
        throw new NotImplementedException();
    }

    public Vector3 GetObjPosition(BookshelfObject obj, IndividualShelf individualShelf)
    {
        return obj.objTransform.position;
    }

    public int GetObjIdx(BookshelfObject obj, IndividualShelf individualShelf)
    {
        return 0;
    }

    public bool IsSpaceOccupied(Vector2 objLocationOnBookshelf)
    {
        return false;
    }

    public void StackObject(BookshelfObject obj, IndividualShelf individualShelf)
    {
        throw new NotImplementedException();
    }

    public void SaveObjectToShelf(BookshelfObject obj, IndividualShelf individualShelf)
    {
        throw new NotImplementedException();
    }

    public void Calculations()
    {
        
    }
    
    // Calculate the number of slots a shelf can hold
    public int CalculateSlots(float shelfWidth, float bookWidth, float bookPadding)
    {
        if (bookWidth + bookPadding <= 0)
        {
            Debug.LogError("Book width must be greater than 0.");
            return 0;
        }
        
        // Divide the shelf width by the book width and floor the result
        int numOfSlots = Mathf.FloorToInt(shelfWidth/(bookWidth + bookPadding));
        return numOfSlots;
        
    }

    // this function should probably be in the drag controller but we're going to leave it here just to get it working for now
    public void GenerateStandIns(IndividualShelf individualShelf, GameObject objectToPlace, float bookPadding,
        GameObject singleShelf, GameObject objectStandIn)
    {
        if (objectToPlace == null || objectStandIn == null || singleShelf == null)
        {
            Debug.LogError("Assign tempBook, bookPrefab, and individualShelf in the inspector.");
            return;
        }
        
        // Get the shelf's dimensions
        BoxCollider shelfCollider = singleShelf.GetComponent<BoxCollider>();
        if (shelfCollider == null)
        {
            Debug.LogError("The IndividualShelf must have a BoxCollider.");
            return;
        }
        
        Vector3 shelfSize = shelfCollider.size; // Local size of the shelf
        Vector3 shelfCenter = shelfCollider.bounds.center; // World center of the shelf
        float bottomOfShelf = shelfCollider.bounds.min.y;
        float frontOfShelf = shelfCollider.bounds.min.z;
        
        Renderer objectPlacedRenderer = objectToPlace.GetComponent<Renderer>();
        if (objectPlacedRenderer == null)
        {
            objectPlacedRenderer = objectToPlace.GetComponentInChildren<Renderer>();
            if (objectPlacedRenderer == null)
            {
                Debug.LogError("The object to place must have a Renderer component.");
                return;
            }
        }

        Vector3 objectSize = objectPlacedRenderer.bounds.size;
        
        // Starting posiiton for the first book
        Vector3 currentPosition = new Vector3(shelfCenter.x - (shelfSize.x / 2) + (objectSize.x / 2),
            bottomOfShelf + (objectSize.y / 2),                      // Align to the bottom of the shelf
            frontOfShelf + (objectSize.z / 2)
        );
        
        // loop to fill the shelf
        while (currentPosition.x + (objectSize.x / 2) <= shelfCenter.x + (shelfSize.x / 2))
        {
            // Resize the new book to match the dimensions of tempBook
            Vector3 newSize = MatchSize(objectToPlace, objectStandIn);
            Debug.Log($"The new size is {newSize}");
            // Instantiate the book at the current position
            GameObject newBook = Instantiate(objectStandIn, currentPosition, Quaternion.identity);

            
            // Update the position for the next book
            currentPosition.x += objectSize.x + bookPadding;
        }

    }
    // public void CalculateSlotLocations(IndividualShelf shelf, float bookWidth, float bookPadding, GameObject[] shelfArray)
    // {
    //     float fullWidth = shelf.floatShelfWidth;
    //     if (bookWidth + bookPadding <= 0)
    //     {
    //         Debug.LogError("Book width must be greater than 0.");
    //     }
    //
    //     while (fullWidth > 0f)
    //     {
    //         fullWidth -= bookPadding;
    //         Vector3 newBoxLoc = new Vector3((shelf.shelfLocation.x + bookPadding), shelf.shelfLocation.y, shelf.shelfLocation.z);
    //         //Instantiate(objectStandIn, shelf.shelfLocation, Quaternion.identity);
    //         
    //         Debug.Log($"object stand in has renderer: {objectStandIn.GetComponent<Renderer>()}");
    //         Debug.Log($"book has renderer: {tempBook.GetComponent<Renderer>()}");
    //         
    //         //Vector3 newObjectStandInSize = MatchSize(tempBook, objectStandIn);
    //         fullWidth -= bookWidth;
    //         Debug.Log($"New box location: {newBoxLoc}");
    //         Debug.Log($"The full width is {fullWidth}");
    //     }
    // }
    
    public float CalculateManualWidth()
    {
        if (leftEdge != null && rightEdge != null)
        {
            float interiorWidth = Vector3.Distance(leftEdge.position, rightEdge.position);
            Debug.Log("Manual Interior Width: " + interiorWidth);
            return interiorWidth;
        }
        else
        {
            Debug.LogError("Left or Right Edge Transform is missing!");
            return 0f;
        }
    }
}
