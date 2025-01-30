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
        private Dictionary<GameObject, Queue<GameObject>> objectPool = new Dictionary<GameObject, Queue<GameObject>>();
        private Queue<GameObject> bookUIPool = new Queue<GameObject>();
        [SerializeField] private GameObject bookPrefab;
        [SerializeField] private GameObject plantPrefab;
        [SerializeField] private GameObject figurinePrefab;
        [SerializeField] private GameObject otherObjectPrefab;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void InitializeSingleBook(BookshelfObjectData bookshelfObj)
        {
            GameObject objectPrefab = GetPrefabForType(bookshelfObj.objType);

            if (objectPrefab == null)
            {
                Debug.LogError($"No prefab found for object type: {bookshelfObj.objType}");
                return;
            }

            GameObject objInstance = GetObjectFromPool(objectPrefab);
            objInstance.SetActive(true);

            // Set position & transform
            objInstance.transform.position = bookshelfObj.objTransform;
            // objInstance.transform.rotation = new Quaternion(); // not sure if this is needed

            if (bookshelfObj is Book bookData)
            {
                objInstance.GetComponent<BookScript>().Initialize(bookData);
            }
            else
            {
                objInstance.GetComponent<ObjectScript>().SetUID(bookshelfObj.objName); // Generic setup
            }
        }

        /// <summary>
        /// Checks if the prefab has a pool.
        /// If not, it creates an empty queue.
        /// Checks if the pool has available objects.
        /// If yes, it returns a recycled object.
        /// If no, it creates a new object.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        private GameObject GetObjectFromPool(GameObject prefab)
        {
            if (objectPool.ContainsKey(prefab) && objectPool[prefab].Count > 0)
            {
                return objectPool[prefab].Dequeue();
            }

            return Instantiate(prefab);
        }

        /// <summary>
        /// The object is disabled and stored for future use.
        /// Prevents unnecessary destruction & creation.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        private void ReturnObjectToPool(GameObject obj, GameObject prefab)
        {
            obj.SetActive(false);

            if (!objectPool.ContainsKey(prefab))
            {
                objectPool[prefab] = new Queue<GameObject>();
            }

            objectPool[prefab].Enqueue(obj);
        }

    private GameObject GetPrefabForType(ObjectType objType)
        {
            switch (objType)
            {
                case ObjectType.BookObj:
                    return bookPrefab;
                case ObjectType.PlantObj:
                    return plantPrefab;
                case ObjectType.FigurineObj:
                    return figurinePrefab;
                case ObjectType.OtherObj:
                    return otherObjectPrefab;
                default:
                    return null;
            }
        }
    }
}