using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BookHarbour
{
    [Serializable]
    public class Book : BookshelfObjectData
    {
        public enum BookshelfState
        {
            Facing,
            Spines,
            Side
        }

        public string bookTitle { get; set; }
        public string bookCover { get; set; }
        public string bookSpine { get; set; }
        public int bookPageCount { get; set; }
        public Vector3 bookSize { get; set; } = new Vector3(0.17094f, 1f, 0.81175f); //default scale is x=0.17094 y=1 z=0.81175
        public Transform locationOnShelf { get; set; }
        public BookshelfState bookshelfState { get; set; }

        public ObjectType objectType = ObjectType.BookObj;
        
        public string UID { get; set; }
        //public GameObject bookGO { get; set; }

        
        public Book(string uid, string bookTitle, string bookCover, string bookSpine, int bookPageCount, Transform locationOnShelf, bool isPlaced, BookshelfState bookshelfState)
        {
            this.UID = uid; // get this from SwiftUI
            this.bookTitle = bookTitle;
            this.bookCover = bookCover;
            this.bookSpine = bookSpine;
            this.bookPageCount = bookPageCount;
            //this.bookSize = bookSize;
            this.locationOnShelf = locationOnShelf;
            this.isPlaced = isPlaced;
            this.bookshelfState = bookshelfState;
        }

        public Book(string uid, string bookTitle, int bookPageCount)
        {
            this.UID = uid;
            this.bookTitle = bookTitle;
            this.bookPageCount = bookPageCount;
        }
        public void SetBookDetails()
        {
            
        }
    }

    [Serializable]
    public class BookJson
    {
        public string uid { get; set; }
        public string title { get; set; }
        public int pageCount { get; set; }
    }
    [Serializable]
    public class BookList
    {
        public List<BookJson> books { get; set; }
    }
}