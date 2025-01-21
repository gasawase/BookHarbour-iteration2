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
            Debug.Log($"This bookshelf has the id of {bookshelfIndex}");
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

            //GenerateStandIns(individualShelf, tempBook, bookPadding, shelf, objectStandIn);
            List<Vector3> gotSnapPoints = GenerateSnapPoints(tempBook, bookshelf, bookPadding);
            Debug.Log($"This shelf has {gotSnapPoints.Count} snap points");
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
