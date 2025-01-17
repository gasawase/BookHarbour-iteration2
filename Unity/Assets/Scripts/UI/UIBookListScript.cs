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
    private List<Book> books;

    public void Start()
    {
        if (bookManager == null)
        {
            // find the object in the scene and assign it here
            bookManager = FindAnyObjectByType<BookManager>();

            refreshBooksButton.onClick.AddListener(() => PopulateBooks(books));
        }
    }
    
    
    public void PopulateBooks(List<Book> books)
    {
        ClearBooks();
        books = bookManager.GlobalBookList;
        foreach (Book book in books)
        {
            Instantiate(UIBookPrefab, UIBooksParent);
            UIBookPrefab.GetComponent<UIBookScript>().SetBookTitle(book.bookTitle);
            if (book.bookCover != null)
            {
                UIBookPrefab.GetComponent<UIBookScript>().SetBookCover(book.bookCover);
            }
        }
    }

    public void ClearBooks() // clears all of the children of UIBooksParent
    {
        foreach (Transform child in UIBooksParent)
        {
            Destroy(child.gameObject);
        }
    }
}
