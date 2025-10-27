using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DataProcessing : MonoBehaviour
{
    [SerializeField] public TextAsset JSONfile;
    public Dictionary<string, Book> ParseJSON() {
        return JsonUtility.FromJson<Dictionary<string, Book>>(JSONfile.text);
    }

    [System.Serializable]
    public class BookList
    {
        public Book[] books;
    }

    public BookList myBookList = new BookList();
    public Dictionary<string, Book> bookDictionary = new Dictionary<string, Book>();
    [SerializeField] public int bookCount = 0;
    private void Start()
    {
        myBookList = JsonUtility.FromJson<BookList>(JSONfile.text);
        foreach (Book book in myBookList.books)
        {
            bookDictionary.Add(book.uid, book);
        }
        bookCount = bookDictionary.Count;
    }
}
