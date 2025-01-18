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

    //[SerializeField] private GameObject objectPrefab;
    private Book book;
    private Sprite bookCover;
    private Sprite bookSpine;
    private int bookPages;

    private Vector3 bookPosition;
    
    private void Start()
    {
        Debug.Log("Spawned!");
        //book.objPrefab = objectPrefab;
        
    }
    
    // method for getting the cover and spine and applying those to the book
    
    // method for changing the size of the book by pages; could also change the height by arbitrary means
    
    // 
}



