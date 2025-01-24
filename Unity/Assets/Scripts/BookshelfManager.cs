using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private int bookshelfIndex;
    [SerializeField] private BoxCollider bookshelfDropZone;
    public bool isValidDropAreaInBookshelf {get; set;}
    
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
}
