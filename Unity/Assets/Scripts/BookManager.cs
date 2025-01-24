using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using BookHarbour;
using Newtonsoft.Json;

namespace BookHarbour
{
    public class BookManager : MonoBehaviour
    {
        public static BookManager Instance { get; private set; }
        public static Dictionary<string, Book> GlobalBookList = new Dictionary<string, Book>();

        private void Awake()
        {
            InstantiateBookManager();

        }

        private void Start()
        {
            //GlobalBookList = GetDefaultBooks();
            //GetDefaultBooks();
            LoadBooksFromJson("Assets/Scripts/bookListDefaultJson.json");
            Debug.Log(GlobalBookList.Count);
        }

        public void AddBook(string uid, string title, string bookCover, string bookSpine, int bookPageCount)
        {
            if (!GlobalBookList.ContainsKey(uid))
            {
                GlobalBookList[uid] = new Book(uid, title, bookCover, bookSpine, bookPageCount, null, false, Book.BookshelfState.Spines);
            }
        }
        public static void AddBookJustTitleAndPages(string uid, string title, int bookPageCount)
        {
            if (!GlobalBookList.ContainsKey(uid))
            {
                GlobalBookList[uid] = new Book(uid, title, bookPageCount);
            }
            else
            {
                Console.WriteLine($"Duplicate uid found: {GlobalBookList[uid]}. Skipping entry.");
            }
        }

        // TESTING //

        //public Dictionary<string, Book> GetDefaultBooks()
        public void GetDefaultBooks()
        {
            // Placeholder sprites and transforms
            //string defaultCover = ""; // Assign actual Sprite from Unity Editor or Resources later
            //string defaultSpine = ""; // Assign actual Sprite from Unity Editor or Resources later
            //Transform defaultTransform = null; // Assign actual Transform in the scene if needed

            // Default book size
            Vector3 defaultBookSize = new Vector3(0.17094f, 1f, 0.81175f);
            
            AddBookJustTitleAndPages("1", "The Great Gatsby", 180);
            AddBookJustTitleAndPages("2", "1984", 328);
            AddBookJustTitleAndPages("3", "To Kill a Mockingbird", 2810);
            AddBookJustTitleAndPages("4", "Pride and Prejudice", 1000);
            AddBookJustTitleAndPages("5", "Moby-Dick", 635);
        }
        
        public void GetBookInfo(string uid) // temp function; testing for correct communication
        {
            //GlobalBookList.TryGetValue(uid, out Book book);
            //Debug.Log(GlobalBookList.TryGetValue(uid, out Book book) ? "No book found at UID" : book.bookTitle);
            Debug.Log($"The UID is {uid}");
            foreach (Book book in GlobalBookList.Values)
            {
                Debug.Log($"BookTitle: {book.bookTitle}");
            }

            foreach (string key in GlobalBookList.Keys)
            {
                Debug.Log($"Key: {key}");
                if (key.Equals(uid))
                {
                    Debug.Log("found you!");
                }
            }
        }
        
        public static Book GetBookByUID(string uid)
        {
            GlobalBookList.TryGetValue(uid, out Book book);
            return book;
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
        public Dictionary<string, Book> GetBooks()
        {
            return new Dictionary<string, Book>(GlobalBookList);
        }
        
    }
}
