using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BookHarbour;

public class UIBookListScript : MonoBehaviour
{
    public static UIBookListScript Instance { get; private set; }
    [SerializeField] private GameObject UIBookPrefab; // Assign the Book prefab in the Inspector
    [SerializeField] private Transform UIBooksParent; // Assign the Content GameObject (GridLayoutGroup parent)
    private BookManager bookManager;
    [SerializeField] private Button refreshBooksButton;
    //private List<Book> books;
    private Dictionary<string, Book> books = new Dictionary<string, Book>();
    private Queue<GameObject> bookUIPool = new Queue<GameObject>();

    public void Start()
    {
        if (bookManager == null)
        {
            // find the object in the scene and assign it here
            bookManager = FindAnyObjectByType<BookManager>();

            //refreshBooksButton.onClick.AddListener(() => PopulateBooks(books));
            //refreshBooksButton.onClick.AddListener(() => PopulateBooks());
        }
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }

    }

    // public void AddBookToPanel(Book book)
    // {
    //     GameObject uiBook = GetBookUIObject();
    //     uiBook.transform.SetParent(UIBooksParent, false);
    //     uiBook.GetComponent<UIBookshelfObj>().SetUID(book.UID);
    //     // set the cover
    // }
    
    private bool DidBookListChange() {
        // Compare current UI list with book data from BookManager
        return books.Count != BookManager.Instance.GetAllBooks().Count;
    }

    public void ClearBooks() // clears all of the children of UIBooksParent
    {
        foreach (Transform child in UIBooksParent)
        {
            Destroy(child.gameObject);
        }
    }

    // private GameObject GetBookUIObject()
    // {
    //     if (bookUIPool.Count > 0)
    //     {
    //         return bookUIPool.Dequeue();
    //     }
    //     
    //     return Instantiate(UIBookPrefab);
    // }
    
    // private void ReturnToPool(GameObject bookUIToReturn)
    // {
    //     bookUIToReturn.SetActive(false);
    //     bookUIPool.Enqueue(bookUIToReturn);
    // }
    
    // method for when the book has been returned or is still in this panel, put it back to its prior location
    
    // functionality somehow that keeps the space open for the book in case it is dragged either back in or back over; maybe
    // it resets or re-sorts?
}
