using Assets.Scripts.Extensions;
using Assets.Scripts.Models;
using Assets.Scripts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Services
{
    /// <summary>
    /// Knows, per shelf, what's currently on it and in what order
    /// Converts an ordered list of objects with widths into actual world-space positions along the shelf, the flow-layout pas
    /// Owns math that answers "given a drag point, which index would this object be inserted at" — comparing a projected drag position against the midpoints between neighbors.
    /// Previews what the layout would look like when dragging and holding a book in a specific position without committing anything
    /// </summary>
    internal class ShelfLayoutService
    {
        private Dictionary<string, List<PlacedItem>> objectsOnShelvesByShelfID; // shelfContent
        public Dictionary<string, ShelfLayoutInfo> shelfLayoutInfoDict; // actual location and physical spaces of objects by their UID
        private SQLiteService sqliteService;
        private SpawningService bookSpawnerService;

        public ShelfLayoutService(SQLiteService sqliteServiceConstructor, SpawningService bookSpawnerServiceConstructor)
        {
            this.sqliteService = sqliteServiceConstructor;
            this.bookSpawnerService = bookSpawnerServiceConstructor;
            objectsOnShelvesByShelfID = new Dictionary<string, List<PlacedItem>>();
            shelfLayoutInfoDict = new Dictionary<string, ShelfLayoutInfo>();
        }

        public void RegisterShelf(string shelfID, ShelfLayoutInfo layoutInfo)
        {
            if (shelfLayoutInfoDict.ContainsKey(shelfID))
            {
                Debug.LogWarning($"Shelf {shelfID} has already been registered. Skipping...");
                return;
            }
            List<string> savedBookIds = sqliteService.SelectBookIdsFromShelf(shelfID); // ordered by SlotIndex
            List<PlacedItem> newList = new List<PlacedItem>();

            // TODO: Need to figure out what to do when savedBookIds is empty AND for when objectsOnShelvesByShelfID is empty

            if (savedBookIds != null && savedBookIds.Any()) // if it's valid and not empty
            {
                foreach (string bookID in savedBookIds)
                {
                    GameObject spawnedObject = bookSpawnerService.SpawnBook(bookID);
                    if (spawnedObject != null && spawnedObject.TryGetComponent<IShelfPlaceable>(out IShelfPlaceable placeableObject))
                    {
                        newList.Add(new PlacedItem { item = placeableObject, offsetAlongAxis = 0f }); // real offset comes from DB once that column exists — placeholder for now
                    }
                }
            }
            if (savedBookIds == null)
            {
                savedBookIds = new List<string>();
            }
            this.objectsOnShelvesByShelfID[shelfID] = newList;
            shelfLayoutInfoDict[shelfID] = layoutInfo;
            //ApplyLayout(shelfID);
        }

        public Dictionary<string, Vector3> CalculateLayoutFor(string shelfID)
        {
            Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
            List<PlacedItem> placedItemList = new List<PlacedItem>();
            ShelfLayoutInfo shelfLayoutInfo = null;
            if (!TryGetShelfPlaceableAndLayoutInfo(shelfID, out placedItemList, out shelfLayoutInfo))
            {
                Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
                return null;
            }
            foreach (PlacedItem item in placedItemList)
            {
                // origin + (axis * offsetAlongAxis)
                float centerOffset = item.offsetAlongAxis + (item.item.GetPlacementWidth() / 2);
                Vector3 position = shelfLayoutInfo.origin + (shelfLayoutInfo.localAxis * centerOffset);
                position.y = shelfLayoutInfo.shelfBottom;
                positions[item.item.GetObjectUID()] = position;
            }
            return positions;
        }

        public void FindFreeSpans(string shelfID)
        {
            List<PlacedItem> placedItemList = new List<PlacedItem>();
            ShelfLayoutInfo shelfLayoutInfo = null;
            // where's the open space, and how much
            // Sorts items by offset, walks left to right, and reports every gap it finds
            // computed fresh every call
            if (!TryGetShelfPlaceableAndLayoutInfo(shelfID, out placedItemList, out shelfLayoutInfo))
            {
                Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
                return;
            }
            // sorting

        }

        public void FindSnapOffset(string shelfID, GameObject draggedObject, float desiredOffset)
        {
            // where does this actually land
            // Takes wherever the user dropped it, checks whether that spot would overlap any existing item, and
            // only nudges the position if it does — sliding it to the nearest free edge rather than forcing it into any predetermined slot
            // If there's no collision, the item lands exactly where you released it
        }

        /// <summary>
        /// Calculates the layout of the objects on the shelf
        /// </summary>
        /// <param name="objectsOnShelf"></param>
        /// <param name="shelfInfo"></param>
        /// <returns> a list of objects and their Vector3's on the shelf</returns>
        /// 
        //public Dictionary<string, Vector3> CalculateLayoutFor(List<IShelfPlaceable> objectsOnShelf, ShelfLayoutInfo shelfInfo)
        //{
        //    Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
        //    float cursor = 0f; // our current location on the list of objects on the shelf

        //    foreach (IShelfPlaceable item in objectsOnShelf)
        //    {
        //        float width = item.GetPlacementWidth();
        //        float centerOffset = cursor + (width / 2);
        //        Vector3 worldPos = shelfInfo.origin + (shelfInfo.localAxis * centerOffset);
        //        worldPos.y = shelfInfo.shelfBottom;
        //        positions[item.GetObjectUID()] = worldPos;
        //        cursor += width;
        //    }

        //    return positions;
        //}
        //public float CalculateLayoutFor(string shelfID)
        //{
        //    //Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();

        //    if (TryGetShelfLayoutInfo(shelfID, out ShelfLayoutInfo outLayoutInfo))
        //    {
        //        return outLayoutInfo.origin + (outLayoutInfo.localAxis * )
        //    }
        //    else
        //    {

        //    }
        //        foreach (PlacedItem item in objectsOnShelvesByShelfID)
        //        {
        //            float width = item.item.
        //            Vector3 worldPos = shelfInfo.origin + (shelfInfo.localAxis * centerOffset);
        //            worldPos.y = shelfInfo.shelfBottom;
        //            positions[item.GetObjectUID()] = worldPos;
        //        }

        //    return positions;
        //}

        //public void ApplyLayout(string shelfID)
        //{
        //    if (!TryGetShelfPlaceableAndLayoutInfo(shelfID, out List<IShelfPlaceable> objectsOnShelf, out ShelfLayoutInfo info))
        //    {
        //        Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
        //        return; 
        //    }
        //    //Dictionary<string, Vector3> positions = CalculateLayoutFor(objectsOnShelf, info);

        //    foreach (IShelfPlaceable item in objectsOnShelf)
        //    {
        //        item.SetPlacementPosition(positions[item.GetObjectUID()]);
        //    }
        //}

        //public int FindInsertionIndex(string shelfID, IShelfPlaceable draggedItem, Vector3 projectedPositionAlongAxis)
        //{
        //    if (!TryGetShelfPlaceableAndLayoutInfo(shelfID, out List<IShelfPlaceable> objectsOnShelf, out ShelfLayoutInfo info))
        //    {
        //        Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
        //        return -1;
        //    }
        //    List<IShelfPlaceable> copyOfObjectsOnShelf = new List<IShelfPlaceable>(objectsOnShelf);
        //    if (copyOfObjectsOnShelf.Contains(draggedItem))
        //    {
        //        copyOfObjectsOnShelf.Remove(draggedItem);
        //    }
        //    //Dictionary<string, Vector3> positions = CalculateLayoutFor(objectsOnShelf, info);

        //    for (int i = 0; i < objectsOnShelf.Count; i++)
        //    {
        //        if (i == objectsOnShelf.Count)
        //        {
        //            return objectsOnShelf.Count;
        //        }
        //        Vector3 candidatePosition = positions[objectsOnShelf[i].GetObjectUID()];
        //        Vector3 distanceToOrigin = candidatePosition - info.origin;
        //        float scalarBetweenCandidateAndShelf = Vector3.Dot(distanceToOrigin, info.localAxis.normalized); // scalar between the distance (between the origin and the candidate) and the local axis from the shelf layout

        //        Vector3 distanceToOriginFromProjectedPosition = projectedPositionAlongAxis - info.origin;
        //        float scalarBetweenProjectedPosAndShelf = Vector3.Dot(distanceToOriginFromProjectedPosition, info.localAxis.normalized);
        //        if (scalarBetweenProjectedPosAndShelf < scalarBetweenCandidateAndShelf)
        //        {
        //            return i;
        //        }
        //    }
        //    return objectsOnShelf.Count;
        //}

        // Live preview of drag
        //public Dictionary<string, Vector3> PreviewInsertion(string shelfID, IShelfPlaceable draggedItem, Vector3 objectPlainWorldSpacePosition)
        //{
        //    if (!TryGetShelfPlaceable(shelfID, out List<IShelfPlaceable> objectsOnShelf))
        //    {
        //        Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
        //        return null;
        //    }
        //    List<IShelfPlaceable> hypotheticalList = new List<IShelfPlaceable>(objectsOnShelf);

        //    if (hypotheticalList.Contains(draggedItem))
        //    {
        //        hypotheticalList.Remove(draggedItem);
        //    }

        //    int index = FindInsertionIndex(shelfID, draggedItem, objectPlainWorldSpacePosition);
        //    if (index < 0)
        //    {
        //        Debug.Log("Shelf does not exist");
        //        return null;
        //    }
        //    hypotheticalList.Insert(index, draggedItem);

        //    return CalculateLayoutFor(hypotheticalList, shelfLayoutInfoDict[shelfID]);
        //}

        // when we want to save the item from a shelf
        //public void CommitPlacement(string shelfID, IShelfPlaceable item, int index)
        //{
        //    if (!TryGetShelfPlaceableAndLayoutInfo(shelfID, out List<IShelfPlaceable> objectsOnShelf, out ShelfLayoutInfo info))
        //    {
        //        Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
        //        return;
        //    }
        //    try
        //    {
        //        if (index < 0)
        //        {
        //            throw new Exception("Index cannot be -1. Cannot commit placement.");
        //        }
        //        RemoveItemFromAnyShelf(item.GetObjectUID());
        //        objectsOnShelf.Insert(index, item);
        //        ApplyLayout(shelfID);
        //        SaveShelfOrder(shelfID);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogException(ex);
        //    }
        //}
        // be aware of possible memory leak or failure since CommitPlacement and RemoveItemFromAnyShelf could cause an error further on (an object removed from the database and in-memory list
        //public void RemoveItemFromAnyShelf(string placeableID)
        //{
        //    // find whichever shelf's list currently contains this ID
        //    string shelfID = sqliteService.SelectShelfByObjectID(placeableID);

        //    if (!shelfID.IsNullOrWhiteSpace())
        //    {
        //        sqliteService.DeleteBookFromLocation(placeableID);
        //    }
        //    ApplyLayout(shelfID);
        //    SaveShelfOrder(shelfID);
        //}

        public void SaveShelfOrder(string shelfID)
        {
            if (!TryGetShelfPlacedItem(shelfID, out List<PlacedItem> objectsOnShelf))
            {
                Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
                return;
            }
            for (int i = 0; i < objectsOnShelf.Count; i++)
            {
                sqliteService.UpdateBookLocation(objectsOnShelf[i].item.GetObjectUID(), shelfID, i, DateTime.Now);
            }
        }

        public void ResetObjectsOnShelf(string shelfID)
        {
            if (!TryGetShelfPlacedItem(shelfID, out List<PlacedItem> objectsOnShelf))
            {
                Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
                return;
            }

            if (objectsOnShelf.Count == 0)
            {
                Debug.Log($"No objects on shelf {shelfID}. Do nothing...");
                return;
            }
            List<PlacedItem> copyOfObjectsOnShelf = objectsOnShelf;
            foreach (PlacedItem placedItem in copyOfObjectsOnShelf)
            {
                try
                {
                    PersistanceManager.Instance.sqliteService.DeleteBookFromLocation(placedItem.item.GetObjectUID());
                    PersistanceManager.Instance.RestoreBookToPanel(placedItem.item.GetObjectUID());
                    UnityEngine.Object.Destroy(placedItem.item.GetObjectGameObject());

                    objectsOnShelf.Remove(placedItem);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogWarning($"Failed to reset '{placedItem.item.GetObjectUID()}' — left as-is on shelf '{shelfID}'.");
                }
            }
            SaveShelfOrder(shelfID);
        }

        public bool TryGetShelfPlacedItem(string shelfID, out List<PlacedItem> objectsOnShelf)
        {
            if (shelfID.IsNullOrWhiteSpace())// if the shelf we are receiving has no actual value
            {
                objectsOnShelf = null;
                return false;
            }

            return objectsOnShelvesByShelfID.TryGetValue(shelfID, out objectsOnShelf);
        }

        public bool TryGetShelfLayoutInfo(string shelfID, out ShelfLayoutInfo layoutInfo)
        {
            if (shelfID.IsNullOrWhiteSpace())// if the shelf we are receiving has no actual value
            {
                layoutInfo = default;
                return false;
            }

            bool foundInfo = shelfLayoutInfoDict.TryGetValue(shelfID, out layoutInfo);
            return foundInfo;
        }

        public bool TryGetShelfPlaceableAndLayoutInfo(string shelfID, out List<PlacedItem> objectsOnShelf, out ShelfLayoutInfo layoutInfo)
        {
            bool foundObjects = TryGetShelfPlacedItem(shelfID, out objectsOnShelf);
            bool foundInfo = TryGetShelfLayoutInfo(shelfID, out layoutInfo);

            return foundObjects && foundInfo;
        }
    }
}
