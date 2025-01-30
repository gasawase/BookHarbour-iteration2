using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BookHarbour;
using Newtonsoft.Json;
using UnityEngine.Rendering;

namespace BookHarbour
{
    /// <summary>
    /// Handles the books that are present only in this session (not ALL books)
    /// </summary>
    public class BookManager : MonoBehaviour
    {
        public static BookManager Instance { get; private set; }
        // public static Dictionary<string, Book> GlobalBookList = new Dictionary<string, Book>();
        private static Dictionary<string, Book> activeBooks = new Dictionary<string, Book>();
        public static event Action OnActiveBooksCreated;
        public static bool IsInstanceCreated { get; private set; }

        
        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            InstantiateBookManager();
        }

        private void Start()
        {
            //GlobalBookList = GetDefaultBooks();
            //GetDefaultBooks();
            Dictionary<string, Book> tempBookList = LoadBooksFromJson("Assets/Scripts/bookListDefaultJson.json");
            LoadBooks(tempBookList);
        }

        public void LoadBooks(Dictionary<string, Book> books)
        {
            activeBooks = books;
            
            // if the object is placed, load them in their spots on the shelf
            // if the object is not placed, put it in the UIBookListScript

            foreach (Book book in books.Values)
            {
                if (book.isPlaced)
                {
                    // if it's placed, put it in its position on the bookshelf
                    ObjectPooling.Instance.InitializeSingleBook(book);
                }
                else
                {
                    UIBookListScript.Instance.AddBookToPanel(book);
                }
            }
            OnActiveBooksCreated?.Invoke();

        }

        public Dictionary<string, Book> GetBooksForSave()
        {
            return activeBooks;
        }
        
        public static void AddBookJustTitleAndPages(string uid, string title, int bookPageCount)
        {
            if (!activeBooks.ContainsKey(uid))
            {
                activeBooks[uid] = new Book(uid, title, bookPageCount);
            }
            else
            {
                Console.WriteLine($"Duplicate uid found: {activeBooks[uid]}. Skipping entry.");
            }
        }
        public static Book GetBookByUID(string uid)
        {
            return activeBooks.TryGetValue(uid, out Book book) ? book : null;
            //return GlobalBookList.TryGetValue(uid, out Book book) ? book : null;
        }
        
        public Book GetBookByUIDNonStatic(string uid)
        {
            return activeBooks.TryGetValue(uid, out Book book) ? book : null;
            //return GlobalBookList.TryGetValue(uid, out Book book) ? book : null;
        }

        public static Dictionary<string, Book> LoadBooksFromJson(string filePath)
        {
            Dictionary<string, Book> tempBookList = new Dictionary<string, Book>();
            try
            {
                string jsonData = File.ReadAllText(filePath);

                BookList bookList = JsonConvert.DeserializeObject<BookList>(jsonData);
                if (bookList != null)
                {
                    foreach (var bookJson in bookList.books)
                    {
                        Debug.Log($"UID: {bookJson.uid} | Title: {bookJson.title} | Page Count: {bookJson.pageCount}");
                        //AddBookJustTitleAndPages(bookJson.uid, bookJson.title, bookJson.pageCount);
                        if (!tempBookList.ContainsKey(bookJson.uid))
                        {
                            tempBookList[bookJson.uid] = new Book(bookJson.uid, bookJson.title, bookJson.pageCount);
                        }
                        else
                        {
                            Console.WriteLine($"Duplicate uid found: {tempBookList[bookJson.uid]}. Skipping entry.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books from JSON: {ex.Message}");
                throw;
            }
            return tempBookList;
        }
        
        private void InstantiateBookManager() // used in Awake; don't need in separate method, but wanted to keep things clean
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            IsInstanceCreated = true;
        }

        public Dictionary<string, Book> GetAllBooks()
        {
            return new Dictionary<string, Book>(activeBooks); // Returns a COPY of activeBooks so no altering of data can happen
        }

        public void UpdateBookLocation(string objectUID, Vector3 movedLoc)
        {
            Book bookMoved = GetBookByUID(objectUID);
            if (bookMoved.objTransform != movedLoc)
            {
                bookMoved.SetVector3(movedLoc);
                bookMoved.PlaceObject();
            }
        }
    }
}
