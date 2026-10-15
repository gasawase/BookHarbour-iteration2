using Assets.Scripts.Extensions;
using Assets.Scripts.Models;
using Assets.Scripts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Models.Enums;

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
            List<PlacedItem> newList = new List<PlacedItem>();
            objectsOnShelvesByShelfID[shelfID] = newList;

            foreach ((string bookID, float offset, eOrientation orientation) in sqliteService.SelectBookIdsFromShelf(shelfID))
            {
                GameObject spawnedObject = bookSpawnerService.SpawnBook(bookID);
                if (spawnedObject != null && spawnedObject.TryGetComponent<IShelfPlaceable>(out IShelfPlaceable placeableObject))
                {
                    placeableObject.SetOrientation(orientation);
                    newList.Add(new PlacedItem { item = placeableObject, offsetAlongAxis = offset });
                    var center = offset + placeableObject.GetPlacementWidth() / 2;
                    placeableObject.SetPlacementPosition(layoutInfo.ToWorldPosition(center));
                }
            }
            shelfLayoutInfoDict[shelfID] = layoutInfo;
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

        public List<(float, float)> FindFreeSpans(string shelfID, string excludeUID)
        {
            List<PlacedItem> placedItemList = new List<PlacedItem>();
            ShelfLayoutInfo shelfLayoutInfo = null;
            // where's the open space, and how much
            // Sorts items by offset, walks left to right, and reports every gap it finds
            // computed fresh every call
            if (!TryGetShelfPlaceableAndLayoutInfo(shelfID, out placedItemList, out shelfLayoutInfo))
            {
                Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
                return new List<(float, float)> { };
            }
            List<PlacedItem> orderedItems = placedItemList
                .Where(x => x.item.GetObjectUID() != excludeUID)
                .OrderBy(x => x.offsetAlongAxis)
                .ToList();

            List<(float, float)> spans = new List<(float, float)>();
            float cursor = 0f;
            foreach (PlacedItem item in orderedItems)
            {
                float itemLeft = item.offsetAlongAxis;
                float itemRight = itemLeft + item.item.GetPlacementWidth();
                if (itemLeft > cursor)
                {
                    spans.Add((cursor, itemLeft)); // leading gap
                }
                cursor = Mathf.Max(cursor, itemRight);
            }

            if (cursor < shelfLayoutInfo.shelfWidth)
            {
                spans.Add((cursor, shelfLayoutInfo.shelfWidth)); // trailing gap
            }
            return spans;
        }

        public float? FindSnapOffset(string shelfID, IShelfPlaceable placeableItem, float desiredLeft) // null = doesn't fit anywhere and should go back to list
        {
            float itemHeight = placeableItem.GetPlacementHeight();
            float itemWidth = placeableItem.GetPlacementWidth();
            float? bestLoc = null;

            if (!TryGetShelfLayoutInfo(shelfID, out ShelfLayoutInfo layoutInfo)) { return null; }

            if (itemHeight > layoutInfo.shelfHeight) { return null; }

            foreach ((float start, float end) in FindFreeSpans(shelfID, placeableItem.GetObjectUID()))
            {
                float minLeft = start + PersistanceManager.ITEMPADDING;
                float maxLeft = end - PersistanceManager.ITEMPADDING - itemWidth;
                if (maxLeft < minLeft) { continue; } // not enough room for this book

                float left = Mathf.Clamp(desiredLeft, minLeft, maxLeft);
                if ((left - minLeft) < PersistanceManager.MAGNET) { left = minLeft; } // snap to left neighbor
                else if ((maxLeft - left) < PersistanceManager.MAGNET) { left = maxLeft; } // snap to right neighbor

                if (bestLoc == null || Mathf.Abs(left - desiredLeft) < Mathf.Abs(bestLoc.Value - desiredLeft))
                {
                    bestLoc = left;
                }
            }
            return bestLoc;
        }

        public void RemoveItemFromShelves(string itemID)
        {
            foreach (List<PlacedItem> itemList in objectsOnShelvesByShelfID.Values)
            {
                itemList.RemoveAll(p => p.item.GetObjectUID() == itemID);
            }
        }

        public void PlaceItem(string shelfID, IShelfPlaceable placeableItem, float leftOffset)
        {
            RemoveItemFromShelves(placeableItem.GetObjectUID()); // in case it was on another shelf
            List<PlacedItem> itemsOnThisShelf = new List<PlacedItem>();
            ShelfLayoutInfo shelfLayoutInfo = null;
            if (!TryGetShelfPlaceableAndLayoutInfo(shelfID, out itemsOnThisShelf, out shelfLayoutInfo))
            {
                Debug.LogWarning($"Shelf '{shelfID}' not registered. Skipping...");
                return;
            }
            itemsOnThisShelf.Add(new PlacedItem { item = placeableItem, offsetAlongAxis = leftOffset });
            itemsOnThisShelf.Sort((a, b) => a.offsetAlongAxis.CompareTo(b.offsetAlongAxis));

            float itemCenter = leftOffset + placeableItem.GetPlacementWidth() / 2;
            placeableItem.SetPlacementPosition(shelfLayoutInfo.ToWorldPosition(itemCenter));
            sqliteService.UpdateBookLocation(placeableItem.GetObjectUID(), shelfID, leftOffset, placeableItem.Orientation, DateTime.Now);
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

        public void ClearShelf(string shelfID)
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
            List<PlacedItem> snapshotOfObjects = new List<PlacedItem>(objectsOnShelf);
            foreach (PlacedItem item in snapshotOfObjects)
            {
                string itemID = item.item.GetObjectUID();
                try
                {
                    PersistanceManager.Instance.RestoreBookToPanel(itemID);
                    PersistanceManager.Instance.sqliteService.DeleteBookFromLocation(itemID);
                    objectsOnShelf.Remove(item);
                    UnityEngine.Object.Destroy(item.item.GetObjectGameObject());
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
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

        public bool TryGetPlacement(string objectID, out string shelfID, out float? offset)
        {
            foreach (KeyValuePair<string, List<PlacedItem>> kvp in objectsOnShelvesByShelfID)
            {
                if (kvp.Value.Where(i => i.item.GetObjectUID() == objectID).Any())
                {
                    PlacedItem item = kvp.Value.Where(i => i.item.GetObjectUID() == objectID).FirstOrDefault();
                    shelfID = kvp.Key;
                    offset = item.offsetAlongAxis;
                    return true;
                }
            }
            shelfID = string.Empty;
            offset = null;
            return false;
        }
    }
}
