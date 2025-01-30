using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;
using BookHarbour;

public class BookScript : ObjectScript, IBookshelfObject
{
    // when instantiated, get the book information from the book manager
    
    private Vector3 bookPosition;
    public int bookPageCount = 0;
    public ObjectType objectType;
    private BookManager bookManager;
    private void Start()
    {
        Debug.Log("Spawned!");
        SetBookSize(bookPageCount);
        bookManager = FindFirstObjectByType<BookManager>();
    }
    
    // method for getting the cover and spine and applying those to the book
    
    // method for changing the size of the book by pages; could also change the height by arbitrary means
    public void SetBookSize(int pageCount)
    {
        //BoxCollider thisBoxCollider = this.GetComponent<BoxCollider>();
        Renderer thisRenderer = this.GetComponentInChildren<Renderer>();

        Vector3 ogSize = thisRenderer.bounds.size;
        float ogSizeX = ogSize.x;
        
        // using the equation S=mP + b where S = spine width, m = growth rate (so the spine width per page),
        // P = page count, and b = fixed width from the page thickness
        // if we assume that the size of the bookPrefab has 300 pages and that is our base, along with altering numbers to fit the
        // unity world scale (1/1000) we get
        // S = 0.0006P + 0.02 so we're going to use that
        float newSizeX = ((0.0006f * pageCount) + 0.02f);
        
        Vector3 scaleFactor = new Vector3(
            newSizeX / ogSizeX,
            ogSize.y,
            ogSize.z);
        
        // apply the scaling factor to the book
        
        this.gameObject.transform.localScale = scaleFactor;
        Debug.Log($"The new X size is {newSizeX}");
    }

    public void SetPageCount(string uid)
    {
        Book locBook = BookManager.GetBookByUID(uid); 
        // Book locBook = bookManager.GetBookByUID(uid); 
        bookPageCount = locBook.bookPageCount;
    }

    public void Initialize(Book book) // obj of book somewhere has this information of this book
    {
        // set the book page count
        // set the cover/spine/book wrap
        // set the book size
        
        objectUID = book.UID;
        bookPageCount = book.bookPageCount;
    }
}



