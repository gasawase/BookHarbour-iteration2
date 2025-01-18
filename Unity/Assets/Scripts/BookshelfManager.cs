using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;
using Unity.VisualScripting;

public class BookshelfManager : MonoBehaviour
{
    public Bookshelf bookshelf;
    public BookshelfMapping bookshelfMapping;
    private IndividualShelfMapping individualShelfMapping;
    private PerShelfObjectMapping perShelfObjectMapping;
    public Book defaultBook;
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    [SerializeField] private int bookshelfIndex;

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

    private void CreateBookshelf(Bookshelf bookshelf)
    {
        bookshelf.bookshelfIdx = bookshelfIndex;
        bookshelfMapping = new BookshelfMapping();
        bookshelf.bookshelfMapping = bookshelfMapping;
        foreach (BoxCollider shelf in bookshelf.arrayOfShelves)
        {
            
        }

        for (int i = 0; i < bookshelf.arrayOfShelves.Length; i++)
        {
            BoxCollider shelf = bookshelf.arrayOfShelves[i];
            int shelfIndex = i;
            IndividualShelfMapping individualShelfMapping = new IndividualShelfMapping(shelf.bounds.size.x, shelf.bounds.size.y);
            individualShelfMapping.shelfIndex = shelfIndex;
            Debug.Log($"This individual shelf has the size of x: {shelf.bounds.size.x}, y: {shelf.bounds.size.y}");
            int slotsHolder = CalculateSlots(shelf.bounds.size.x, defaultBook.bookSize.x, 0.05f);
            Debug.Log($"This shelf has {slotsHolder} slots");
            float remainingSpace = individualShelfMapping.GetRemainingSpace(i);
            Debug.Log($"This shelf has {remainingSpace} remaining space");
        }
        
    }
    public bool CanFit(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        
        return false;
    }

    public void PlaceObj(Vector2 position, BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }

    public Vector3 GetObjPosition(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        return obj.objTransform.position;
    }

    public int GetObjIdx(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        return 0;
    }

    public bool IsSpaceOccupied(Vector2 objLocationOnBookshelf)
    {
        return false;
    }

    public void StackObject(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }

    public void SaveObjectToShelf(BookshelfObject obj, IndividualShelfMapping individualShelf)
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
