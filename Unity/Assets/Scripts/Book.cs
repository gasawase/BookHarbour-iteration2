using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BookHarbour
{
    [Serializable]
    public class Book
    {
        public enum BookshelfState
        {
            Facing,
            Spines,
            Side
        }

        public string bookTitle { get; set; }
        public Sprite bookCover { get; set; }
        public Sprite bookSpine { get; set; }
        public int bookPageCount { get; set; }
        public Vector3 bookSize { get; set; } //default scale is x=0.17094 y=1 z=0.81175
        public Transform locationOnShelf { get; set; }
        public bool isPlaced { get; set; }
        public BookshelfState bookshelfState { get; set; }
        public GameObject bookGO { get; set; }
        
        public Book(string bookTitle, Sprite bookCover, Sprite bookSpine, int bookPageCount, Vector3 bookSize, Transform locationOnShelf, bool isPlaced, BookshelfState bookshelfState, GameObject bookGo)
        {
            this.bookTitle = bookTitle;
            this.bookCover = bookCover;
            this.bookSpine = bookSpine;
            this.bookPageCount = bookPageCount;
            this.bookSize = bookSize;
            this.locationOnShelf = locationOnShelf;
            this.isPlaced = isPlaced;
            this.bookshelfState = bookshelfState;
            bookGO = bookGo;
        }
        

        public void SetBookDetails()
        {
            
        }
    }
}