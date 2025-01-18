using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BookHarbour
{
    [Serializable]
    public class Bookshelf
    {
        public Bookshelf(int shelfCount, int bookshelfIdx, IndividualShelfMapping individualShelfMapping, PerShelfObjectMapping perShelfObjectMapping, BookshelfMapping bookshelfMapping)
        {
            this.shelfCount = shelfCount;
            //this.intShelfHeight = intShelfHeight;
            //this.intShelfWidth = intShelfWidth;
            this.bookshelfIdx = bookshelfIdx;
            this.individualShelfMapping = individualShelfMapping;
            this.perShelfObjectMapping = perShelfObjectMapping;
            this.bookshelfMapping = bookshelfMapping;
        }

        public int shelfCount { get; set; }
        //public int intShelfHeight { get; set; }
        //public int intShelfWidth { get; set; }
        public int bookshelfIdx { get; set; }
        public IndividualShelfMapping individualShelfMapping { get; set; }
        public PerShelfObjectMapping perShelfObjectMapping { get; set; }
        public BookshelfMapping bookshelfMapping { get; set; }
        
         // key: shelfIndex (the shelf that this happens on), value: the dictionary for the locations (internal Dictionary that holds the Vector3 and the object value)
        
    }
    
    /// <summary>
    /// Represents the mapping of each shelf, sorted by index; contains the list of the actual objects
    /// </summary>
    public class IndividualShelfMapping
    {
        public int intShelfHeight { get; set; }
        public int intShelfWidth { get; set; }
        public Dictionary<int, PerShelfObjectMapping> SingleShelfMapping { get; set; }

        public IndividualShelfMapping() // initializes the custom IndividualShelfMapping dictionary
        {
            SingleShelfMapping = new Dictionary<int, PerShelfObjectMapping>();
        }

        public void TryAddNewShelf(int shelfIdx, PerShelfObjectMapping singleShelfMapping)
        {
            SingleShelfMapping.TryAdd(shelfIdx, singleShelfMapping);
        }

        public bool TryGetShelf(int shelfIdx, out PerShelfObjectMapping singleShelfMapping)
        {
            return SingleShelfMapping.TryGetValue(shelfIdx, out singleShelfMapping);
        }

        public void RemoveShelf(int shelfIdx)
        {
            SingleShelfMapping.Remove(shelfIdx);
        }
        
    }
    /// <summary>
    /// Represents the mapping of the objects on each shelf; each object is in one key but if they're in the same key, they're stacked on top of each other
    /// </summary>

    public class PerShelfObjectMapping
    {
        public Dictionary<int, GameObject> IndividualObjectMapping { get; set; }

        public PerShelfObjectMapping() // initializes the custom dictionary
        {
            IndividualObjectMapping = new Dictionary<int, GameObject>();
        }

        public void TryAddObject(int locationIdx, GameObject shelfObject)
        {
            IndividualObjectMapping.TryAdd(locationIdx, shelfObject);
        }

        public bool TryGetObject(int locationIdx, out GameObject shelfObject)
        {
            return IndividualObjectMapping.TryGetValue(locationIdx, out shelfObject);
        }

        public void RemoveObject(int locationIdx)
        {
            IndividualObjectMapping.Remove(locationIdx);
        }
    }
    /// <summary>
    /// Represents the mapping of all the shelves in one bookshelf
    /// </summary>
    public class BookshelfMapping
    {
        public Dictionary<int, IndividualShelfMapping> ShelfMappings  { get; set; }

        public BookshelfMapping()
        {
            ShelfMappings = new Dictionary<int, IndividualShelfMapping>();
        }

        public IndividualShelfMapping TryGetOrCreateShelf(int shelfId) // get or creates the individual shelf on the bookshelf
        {
            if (!ShelfMappings.TryGetValue(shelfId, out var shelfMapping))
            {
                shelfMapping = new IndividualShelfMapping();
                ShelfMappings.TryAdd(shelfId, shelfMapping);
            }
            return shelfMapping;
        }

        public bool TryGetShelf(int shelfId, out IndividualShelfMapping shelfMapping)
        {
            return ShelfMappings.TryGetValue(shelfId, out shelfMapping);
        }

        public void RemoveShelf(int shelfId)
        {
            ShelfMappings.Remove(shelfId);
        }
    }
}
