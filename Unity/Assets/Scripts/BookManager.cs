using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;

namespace BookHarbour
{
    public class BookManager : MonoBehaviour
    {
        public static BookManager Instance { get; private set; }
        public List<Book> GlobalBookList = new List<Book>();

        private void Awake()
        {
            InstantiateBookManager();

        }

        private void Start()
        {
            GlobalBookList = GetDefaultBooks();
        }

        // TESTING //

        public List<Book> GetDefaultBooks()
        {
            // Placeholder sprites and transforms
            Sprite defaultCover = null; // Assign actual Sprite from Unity Editor or Resources later
            Sprite defaultSpine = null; // Assign actual Sprite from Unity Editor or Resources later
            Transform defaultTransform = null; // Assign actual Transform in the scene if needed

            // Default book size
            Vector3 defaultBookSize = new Vector3(0.17094f, 1f, 0.81175f);

            // Create the book list
            List<Book> books = new List<Book>
            {
                new Book(
                    "The Great Gatsby",
                    defaultCover,
                    defaultSpine,
                    180,
                    defaultBookSize,
                    defaultTransform,
                    false,
                    Book.BookshelfState.Spines
                ),
                new Book(
                    "1984",
                    defaultCover,
                    defaultSpine,
                    328,
                    defaultBookSize,
                    defaultTransform,
                    false,
                    Book.BookshelfState.Spines
                ),
                new Book(
                    "To Kill a Mockingbird",
                    defaultCover,
                    defaultSpine,
                    281,
                    defaultBookSize,
                    defaultTransform,
                    false,
                    Book.BookshelfState.Spines
                ),
                new Book(
                    "Pride and Prejudice",
                    defaultCover,
                    defaultSpine,
                    279,
                    defaultBookSize,
                    defaultTransform,
                    false,
                    Book.BookshelfState.Spines
                ),
                new Book(
                    "Moby-Dick",
                    defaultCover,
                    defaultSpine,
                    635,
                    defaultBookSize,
                    defaultTransform,
                    false,
                    Book.BookshelfState.Spines
                )
            };

            return books;
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

        public void LogAllBooks()
        {
            foreach (var book in GlobalBookList)
            {
                Debug.Log($"Book: {book.bookTitle}");
            }
        }
        
        public void DeserializeBooks(string json) //populate the book list from JSON received from Swift.
        {
            // Deserialize JSON from Swift into book list
            BookListWrapper wrapper = JsonUtility.FromJson<BookListWrapper>(json);
            GlobalBookList = wrapper.Books;
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
        }
        
        // Get all books
        public List<Book> GetBooks()
        {
            return new List<Book>(GlobalBookList);
        }
        
        // Nested class for JSON serialization
        [System.Serializable]
        private class BookListWrapper
        {
            public List<Book> Books;
        }
        
        // FUTURE INTEGRATIONS//
        
        // public void InitializeUnityScene(string jsonData)
        // {
        //     // Use jsonData to populate bookshelves and books in the Unity scene
        //     Debug.Log("Unity Scene Initialized with Data: " + jsonData);
        // }
        
        // public void SaveBookshelfData(string filePath)
        // {
        //     string json = JsonUtility.ToJson(this);
        //     System.IO.File.WriteAllText(filePath, json);
        // }
        //
        // public void LoadBookshelfData(string filePath)
        // {
        //     if (System.IO.File.Exists(filePath))
        //     {
        //         string json = System.IO.File.ReadAllText(filePath);
        //         JsonUtility.FromJsonOverwrite(json, this);
        //     }
        // }
    }
}
