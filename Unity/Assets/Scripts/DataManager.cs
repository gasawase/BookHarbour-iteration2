using System.Collections.Generic;
using UnityEngine;

namespace BookHarbour
{
    /// <summary>
    /// Handles loading/saving of books and objects
    /// decides what data Unity needs and requests only that data from CoreData
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance;

        void Awake() {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        }
        
        public void LoadBookshelfData() {
            // string jsonData = RequestFromSwift(); // Calls CoreData
            // Dictionary<string, BookData> books = ParseJSON(jsonData);
            // BookManager.Instance.LoadBooks(books);
        }
        
        public void SaveBookshelfData() {
            // Dictionary<string, BookData> books = BookManager.Instance.GetBooksForSave();
            // string jsonData = ConvertToJSON(books);
            // SendToSwift(jsonData); // Sends to CoreData
        }
    }
}