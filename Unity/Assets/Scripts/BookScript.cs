using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;
using BookHarbour;

public class BookScript : ObjectScript
{
    // when instantiated, get the book information from the book manager
    
    private int bookPages;

    private Vector3 bookPosition;
    public String bookTitle;
    public ObjectType objectType;
    //public string UID { get; private set; }
    
    private void Start()
    {
        Debug.Log("Spawned!");
        Debug.Log(objectUID);
        string bookTitleChecker = BookManager.GetBookByUID(objectUID).bookTitle;
        Debug.Log(bookTitleChecker);
        //Book book = BookManager.GetBookByUID(objectUID); need to get this UID of whatever is got
        //Debug.Log(book.bookTitle);
    }
    
    // method for getting the cover and spine and applying those to the book
    
    // method for changing the size of the book by pages; could also change the height by arbitrary means
    public void SetBookSize(Book book)
    {
        //BoxCollider thisBoxCollider = this.GetComponent<BoxCollider>();
        Renderer thisRenderer = this.GetComponentInChildren<Renderer>();

        Vector3 ogSize = thisRenderer.bounds.size;
        float ogSizeX = ogSize.x;
        
        // using the equation S=mP + b where S = spine width, m = growth rate (so the spine width per page),
        // P = page count, and b = fixed width from the page thickness
        // if we assume that the size of the bookPrefab has 300 pages and that is our base, we get
        // S = 0.06P+2 so we're going to use that
        float newSizeX = ((0.06f * ogSize.x) + 2f);
        
        Vector3 scaleFactor = new Vector3(
            newSizeX / ogSizeX,
            ogSize.y,
            ogSize.z);
        
        // apply the scaling factor to the book
        
        this.gameObject.transform.localScale = scaleFactor;
        Debug.Log($"The object has been resized to {scaleFactor.ToString()}");
    }
    // 
}



