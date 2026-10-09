using Assets.Scripts.Extensions;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class SpawningService: MonoBehaviour
    {
        public GameObject book3DPrefab;
        private SQLiteService sqliteService;

        public static SpawningService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }
        public GameObject SpawnBook(string bookID)
        {
            GameObject spawnedBook = Instantiate(book3DPrefab);
            if (!bookID.IsNullOrWhiteSpace())
            {
                spawnedBook.GetComponent<Object3DDragSource>().bookId = bookID;
                if (spawnedBook.GetComponent<BookObject3D>().objectWithMeshHolder.name == "Cube")
                {
                    spawnedBook.GetComponent<BookObject3D>().InitializeCube(bookID);

                }
                else
                {
                    spawnedBook.GetComponent<BookObject3D>().Initialize(bookID);

                }
            }
            return spawnedBook;
        }
    }
}
