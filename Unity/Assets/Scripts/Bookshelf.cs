using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BookHarbour
{
    [Serializable]
    public class Bookshelf
    {
        public Bookshelf(int shelfCount, int bookshelfIdx, BookshelfMapping bookshelfMapping, float bookPadding)
        {
            this.shelfCount = shelfCount;
            this.bookshelfIdx = bookshelfIdx;
            this.bookshelfMapping = bookshelfMapping;
            this.bookPadding = bookPadding;
        }

        public int shelfCount { get; set; }
        public int bookshelfIdx { get; set; }
        public BookshelfMapping bookshelfMapping { get; set; }
        public float bookPadding { get; set; }
        
        [SerializeField] public GameObject[] arrayOfShelves;
        [SerializeField] public int tempBookshelfIndex;

        // key: shelfIndex (the shelf that this happens on), value: the dictionary for the locations (internal Dictionary that holds the Vector3 and the object value)

    }
    
    /// <summary>
    /// Represents the mapping of each shelf, sorted by index; contains the list of the actual objects
    /// </summary>
    public class IndividualShelf 
    {
        public float floatShelfWidth { get; set; }
        public float floatShelfHeight { get; set; } // also shelfIndex
        public int shelfIndex { get; set; }
        public Vector3 shelfLocation { get; set; }

        public Dictionary<int, PerShelfObjectMapping> SingleShelfMapping { get; set; } // location of the shelf

        public IndividualShelf(float floatShelfWidth, float floatShelfHeight) // initializes the custom IndividualShelfMapping dictionary
        {
            SingleShelfMapping = new Dictionary<int, PerShelfObjectMapping>();
            this.floatShelfWidth = floatShelfWidth;
            this.floatShelfHeight = floatShelfHeight;
        }
        public IndividualShelf() // initializes the custom IndividualShelf dictionary
        {
            this.floatShelfHeight = 1.198154f; // default height
            this.floatShelfWidth = 2.825437f; // default width
            SingleShelfMapping = new Dictionary<int, PerShelfObjectMapping>();
        }

        public void TryAddNewShelf(int shelfIdx, PerShelfObjectMapping singleShelfMapping)
        {
            SingleShelfMapping.TryAdd(shelfIdx, singleShelfMapping);
        }

        public bool TryGetShelfMappingByIndex(int shelfIdx, out PerShelfObjectMapping singleShelfMapping)
        {
            return SingleShelfMapping.TryGetValue(shelfIdx, out singleShelfMapping);
        }

        public void RemoveShelf(int shelfIdx)
        {
            SingleShelfMapping.Remove(shelfIdx);
        }
        
        // based on the specific shelf,
        // get the dimensions of the shelf,
        // and then if there are objects on the shelves
        public float GetRemainingSpace(int shelfIndex)
        {
            float remainingSpace = floatShelfWidth;
                if (!SingleShelfMapping.ContainsKey(shelfIndex))
                {
                    Debug.Log("No individual shelf present");
                }
                else
                {
                    foreach (var item in SingleShelfMapping[shelfIndex].IndividualObjectMapping)
                    {
                        float objectWidth = item.Value.GetComponent<Renderer>().bounds.size.x;
                        remainingSpace -= objectWidth;
                    }
                }
            return remainingSpace;
        }
    }
    /// <summary>
    /// Represents the mapping of the objects on each shelf; each object is in one key but if they're in the same key, they're stacked on top of each other
    /// </summary>

    public class PerShelfObjectMapping
    {
        public Dictionary<float, GameObject> IndividualObjectMapping { get; set; }

        public PerShelfObjectMapping() // initializes the custom dictionary
        {
            IndividualObjectMapping = new Dictionary<float, GameObject>();
        }

        public void TryAddObject(float location, GameObject shelfObject)
        {
            IndividualObjectMapping.TryAdd(location, shelfObject);
            // create a linked list here for stacked books and objects
        }

        public bool TryGetObject(float location, out GameObject shelfObject)
        {
            return IndividualObjectMapping.TryGetValue(location, out shelfObject);
        }

        public void RemoveObject(float location)
        {
            IndividualObjectMapping.Remove(location);
        }
    }
    /// <summary>
    /// Represents the mapping of all the shelves in one bookshelf
    /// </summary>
    public class BookshelfMapping
    {
        public Dictionary<int, IndividualShelf> ShelfMappings  { get; set; }

        public BookshelfMapping()
        {
            ShelfMappings = new Dictionary<int, IndividualShelf>();
        }

        public IndividualShelf TryGetOrCreateShelf(int shelfId) // get or creates the individual shelf on the bookshelf
        {
            if (!ShelfMappings.TryGetValue(shelfId, out var shelfMapping))
            {
                shelfMapping = new IndividualShelf();
                ShelfMappings.TryAdd(shelfId, shelfMapping);
            }
            return shelfMapping;
        }

        public void GenerateShelves(GameObject[] arrayOfShelves )
        {
            for (int i = 0; i < arrayOfShelves.Length; i++)
            {
                IndividualShelf shelf = TryGetOrCreateShelf(i);
                shelf.shelfIndex = i;
            }
            Debug.Log($"Number of Shelves: {ShelfMappings.Count}");
        }

        public bool TryGetBookshelf(int shelfId, out IndividualShelf shelf)
        {
            return ShelfMappings.TryGetValue(shelfId, out shelf);
        }

        public void RemoveShelf(int shelfId)
        {
            ShelfMappings.Remove(shelfId);
        }
        

    }
}
