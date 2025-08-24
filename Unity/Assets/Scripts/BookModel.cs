using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the class of the Book object
/// It defines an instance of the book and when instantiated, holds the data of that book object
/// </summary>
public class BookModel
{
    public string BookTitle { get; set; }
    public int BookPageCount { get; set; }
    public int BookISBN { get; set; }
    //public Sprite BookCoverImg { get; set; }
    public Vector3 BookScaleFactor { get; set; }
}
