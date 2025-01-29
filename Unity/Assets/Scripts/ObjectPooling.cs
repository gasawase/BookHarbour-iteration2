using System.Collections.Generic;
using UnityEngine;

namespace BookHarbour
{
    /// <summary>
    /// Prevents unnecessary instantiation/destruction.
    /// </summary>
    public class ObjectPooling : MonoBehaviour
    {
        public static ObjectPooling Instance;
        private Queue<GameObject> bookPool = new Queue<GameObject>();
        public GameObject bookPrefab;

        void Awake() { if (Instance == null) { Instance = this; } }

        public void InitializeBooks(Dictionary<string, Book> books)
        {
            foreach (var book in books)
            {
                GameObject bookObj = GetBook();
                bookObj.SetActive(true);
                bookObj.GetComponent<BookScript>().Initialize(book);
            }
        }

        public GameObject GetBook()
        {
            if (bookPool.Count > 0)
            {
                return bookPool.Dequeue();
            }
            else
            {
                return Instantiate(bookPrefab);
            }
        }

        public void ReturnBook(GameObject bookObj)
        {
            bookObj.SetActive(false);
            bookPool.Enqueue(bookObj);
        }
    }
}