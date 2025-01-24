using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BookHarbour;
using TMPro;
using Unity.VisualScripting;

namespace BookHarbour
{
    public class UIBookScript : UIBookshelfObj
    {
        //[SerializeField] public Book book;
        //public Book book;

        public TMP_Text bookTitleLoc;

        public Image bookCoverLoc;
        public string bookTitle;
        public string bookUID;

        void Start()
        {
        }
        public void SetBookTitle(string title)
        {
            bookTitleLoc.SetText(title);
            bookTitle = title;
        }
        

        public void SetBook(Book settingBook)
        {
            SetBookTitle(settingBook.bookTitle);
            bookUID = settingBook.UID;
        }
    }
}

