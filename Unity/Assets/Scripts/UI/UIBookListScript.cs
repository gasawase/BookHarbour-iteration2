using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BookHarbour;

public class UIBookListScript : MonoBehaviour
{
    [SerializeField] private GameObject UIBookPrefab; // Assign the Book prefab in the Inspector
    [SerializeField] private Transform UIBooksParent; // Assign the Content GameObject (GridLayoutGroup parent)
    private BookManager bookManager;
    [SerializeField] private Button refreshBooksButton;
    //private List<Book> books;
    private Dictionary<string, Book> books = new Dictionary<string, Book>();

    public void Start()
    {
        if (bookManager == null)
        {
            // find the object in the scene and assign it here
            bookManager = FindAnyObjectByType<BookManager>();

            refreshBooksButton.onClick.AddListener(() => PopulateBooks(books));
        }
    }
    
    /// <summary>
    /// Clears the books currently in this panel
    /// fetches the GlobalBookList and instantiates a book for each key,value pair
    /// sets the book in the uibookscript (which sets the information that is displayed on each book in the UI panel)
    /// sets the object's uid in the UIBookshelfObj instance
    /// </summary>
    /// <param name="books"></param>
    public void PopulateBooks(Dictionary<string, Book> books)
    {
        ClearBooks();
        //books = bookManager.GlobalBookList;
        books = BookManager.GlobalBookList;
        foreach (KeyValuePair<string, Book> book in books)
        {
            UIBookPrefab.GetComponent<UIBookScript>().SetBook(book.Value);
            UIBookPrefab.GetComponent<UIBookshelfObj>().SetUID(book.Key);
            Instantiate(UIBookPrefab, UIBooksParent);
        }
    }

    public void ClearBooks() // clears all of the children of UIBooksParent
    {
        foreach (Transform child in UIBooksParent)
        {
            Destroy(child.gameObject);
        }
    }
    
    // method for when the book has been returned or is still in this panel, put it back to it's prior location
    
    // functionality somehow that keeps the space open for the book in case it is dragged either back in or back over; maybe
    // it resets or re-sorts?
}
