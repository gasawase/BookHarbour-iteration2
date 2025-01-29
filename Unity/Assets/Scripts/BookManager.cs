using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            InstantiateBookManager();

        }

        private void Start()
        {
            //GlobalBookList = GetDefaultBooks();
            //GetDefaultBooks();
            LoadBooksFromJson("Assets/Scripts/bookListDefaultJson.json");
        }

        public void LoadBooks(Dictionary<string, Book> books)
        {
            activeBooks = books;
            ObjectPooling.Instance.InitializeBooks(books);
        }

        public Dictionary<string, Book> GetBooksForSave()
        {
            return activeBooks;
        }

        // public void AddBook(string uid, string title, string bookCover, string bookSpine, int bookPageCount)
        // {
        //     if (!GlobalBookList.ContainsKey(uid))
        //     {
        //         GlobalBookList[uid] = new Book(uid, title, bookCover, bookSpine, bookPageCount, null, false, Book.BookshelfState.Spines);
        //     }
        // }
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

        // TESTING //
        
        // public void GetBookInfo(string uid) // temp function; testing for correct communication
        // {
        //     //GlobalBookList.TryGetValue(uid, out Book book);
        //     //Debug.Log(GlobalBookList.TryGetValue(uid, out Book book) ? "No book found at UID" : book.bookTitle);
        //     Debug.Log($"The UID is {uid}");
        //     foreach (Book book in GlobalBookList.Values)
        //     {
        //         Debug.Log($"BookTitle: {book.bookTitle}");
        //     }
        //
        //     foreach (string key in GlobalBookList.Keys)
        //     {
        //         Debug.Log($"Key: {key}");
        //         if (key.Equals(uid))
        //         {
        //             Debug.Log("found you!");
        //         }
        //     }
        // }
        
        public static Book GetBookByUID(string uid)
        {
            return activeBooks.TryGetValue(uid, out Book book) ? book : null;
            //return GlobalBookList.TryGetValue(uid, out Book book) ? book : null;
        }
        
/// talking with Swift
        
        // public void SendDataToSwift(string key, string value)
        // {
        //     // Use UnitySendMessage or a plugin to send data to Swift
        //     UnityMessageManager.Instance.SendMessageToNative($"{key}:{value}");
        // }
        
        public void ReceiveDataFromSwift(string jsonData)
        {
            // Parse JSON data received from Swift
            Debug.Log("Received Data: " + jsonData);
        }

        public static void LoadBooksFromJson(string filePath)
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                
                BookList bookList = JsonConvert.DeserializeObject<BookList>(jsonData);
                if (bookList != null)
                {
                    foreach (var bookJson in bookList.books)
                    {
                        Debug.Log($"UID: {bookJson.uid} | Title: {bookJson.title} | Page Count: {bookJson.pageCount}");
                        AddBookJustTitleAndPages(bookJson.uid, bookJson.title, bookJson.pageCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books from JSON: {ex.Message}");
                throw;
            }
        }

        // public void LogAllBooks()
        // {
        //     foreach (var book in GlobalBookList)
        //     {
        //         Debug.Log($"Book: {book.bookTitle}");
        //     }
        // }
        
        
        private void InstantiateBookManager() // used in Awake; don't need in separate method, but wanted to keep things clean
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        // Get all books
        // public Dictionary<string, Book> GetBooks()
        // {
        //     return new Dictionary<string, Book>(GlobalBookList);
        // }

        public Dictionary<string, Book> GetAllBooks()
        {
            return new Dictionary<string, Book>(activeBooks); // Returns a COPY of activeBooks so no altering of data can happen
        }
    }
}
