using System.Collections.Generic;
using UnityEngine;

namespace BookHarbour
{
    /// <summary>
    /// Handles loading/saving of books and objects | handles the data flow between Unity and Swift
    /// decides what data Unity needs and requests only that data from CoreData
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance;

        void Awake() {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        }
        /// <summary>
        /// whenever Unity needs to fetch the latest bookshelf data from Swift's CoreData
        /// </summary>
        public void RequestBookshelfData() {
            UnityMessageManager.Instance.RequestBookshelfData();
        }
        
        /// <summary>
        /// Calls CoreData to get stored books, objects, and bookshelf states
        /// Converts JSON from Swift to Unity objects
        /// Passes data to BookManager which updates the books in the scene
        /// </summary>
        public void LoadBookshelfData(string jsonData) {
            Dictionary<string, Book> books = ParseJSON(jsonData);
            //BookManager.Instance.LoadBooks(books); // UNCOMMENT THIS WHEN YOU HOOK UP TO SWIFT
            // TODO: update bookmanager or something to also implement the objects that we will be putting into the scene
        }
        
        /// <summary>
        /// Collects all current book and object positions and shelf states
        /// Converts all data into JSON
        /// Sends it back to CoreData to store
        /// </summary>
        public void SaveBookshelfData() { // on App Exxit
            Dictionary<string, Book> books = BookManager.Instance.GetBooksForSave();
            string jsonData = ConvertToJSON(books);
            UnityMessageManager.Instance.SendBookshelfData(jsonData); // Send to Swift
        }
        
        
        
        /// **3. Request bookshelf data from Swift (CoreData)**
        private string RequestFromSwift() {
            return UnityMessageManager.Instance.RequestMessageFromNative();
        }
        
        /// **4. Convert JSON to Dictionary**
        private Dictionary<string, Book> ParseJSON(string json) {
            return JsonUtility.FromJson<Dictionary<string, Book>>(json);
        }
        
        private string ConvertToJSON(Dictionary<string, Book> books) {
            return JsonUtility.ToJson(books);
        }

        public void SaveObjectData(string objectAlteredUID)
        {
            throw new System.NotImplementedException();
        }
    }
}