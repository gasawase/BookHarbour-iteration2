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
        if (bookshelfMapping == null || bookshelfMapping.TryGetBookshelf(bookshelfIndex, out individualShelfMapping))
        {
            bookshelf.bookshelfIdx = bookshelfIndex;
            if (bookshelfMapping.ShelfMappings[bookshelf.bookshelfIdx] != null)
            {
                Debug.Log($"This bookshelf has no shelves");
            }
        }
        else
        {
            Debug.Log($"There is no bookshelf with id {bookshelfIndex}");
        }
    }

    private void CreateBookshelf(Bookshelf bookshelf)
    {
        bookshelf.bookshelfIdx = bookshelfIndex;
        bookshelfMapping = new BookshelfMapping();
        bookshelf.bookshelfMapping = bookshelfMapping;
        individualShelfMapping = new IndividualShelfMapping();
        
    }
    private bool CanFit(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        
        return false;
    }

    private void PlaceObj(Vector2 position, BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }

    private Vector3 GetObjPosition(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        return obj.objTransform.position;
    }

    private int GetObjIdx(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        return 0;
    }

    private bool IsSpaceOccupied(Vector2 objLocationOnBookshelf)
    {
        return false;
    }

    private void StackObject(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }

    private void SaveObjectToShelf(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }

    private void Calculations()
    {
        
    }
    
    // Calculate the number of slots a shelf can hold
    private int CalculateSlots(float shelfWidth, float bookWidth, float bookPadding)
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
