using System;
using UnityEngine;
using System.Runtime.InteropServices;


namespace BookHarbour
{

    public class UnityMessageManager : MonoBehaviour {
        public static UnityMessageManager Instance;
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void UnitySendMessage(string objName, string methodName, string message);
#endif
        void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep persistent
            }
            
        }
        /// **1. Request Bookshelf Data from Swift (CoreData)**
        public void RequestBookshelfData() {
#if UNITY_IOS
            UnitySendMessage("iOSMessageReceiver", "OnUnityRequestBooks", "REQUEST_BOOKS");
#endif
        }
        
        /// **2. Receive Bookshelf Data from Swift (JSON)**
        public void OnBookshelfDataReceived(string jsonData) {
            Debug.Log($"[Unity] Received bookshelf data: {jsonData}");
            DataManager.Instance.LoadBookshelfData(jsonData); // Pass data to DataManager
        }
        
        /// **3. Send Updated Bookshelf Data to Swift**
        public void SendBookshelfData(string jsonData) {
#if UNITY_IOS
            UnitySendMessage("iOSMessageReceiver", "OnUnityUpdateBooks", jsonData);
            Debug.Log($"[Unity] Sent bookshelf data to Swift: {jsonData}");
#endif
        }
        
        public string RequestMessageFromNative() {
#if UNITY_IOS
            return "REQUEST_BOOKS"; // Example: Could trigger Swift to send book data
#endif
            return null;
        }
    }

}