using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BookHarbour;
using TMPro;

namespace BookHarbour
{
    public class UIBookScript : MonoBehaviour
    {
        [SerializeField] public Book book;

        public TMP_Text bookTitleLoc;

        public Image bookCoverLoc;

        public void SetBookTitle(string title)
        {
            bookTitleLoc.SetText(title);
        }

        public void SetBookCover(Sprite cover)
        {
            bookCoverLoc.sprite = cover;
        }

        public void SetBook(Book settingBook)
        {
            this.book = settingBook;
            SetBookTitle(book.bookTitle);
            if (book.bookCover != null)
            {
                SetBookCover(book.bookCover);

            }
            Debug.Log($"{book.bookTitle} was spawned");

        }
    }
}

