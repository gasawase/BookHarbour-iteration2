using System;
using UnityEngine;


namespace BookHarbour
{
    public enum ObjectType
    {
        BookObj,
        PlantObj,
        FigurineObj,
        OtherObj
    }
    public class BookshelfObjectData : IBookshelfObject
    {
        public string objName { get; set; }   // Name of the object
        public string objUID { get; set; }    // Unique identifier
        public GameObject objPrefab { get; set; }  // 3D model prefab
        public ObjectType objType { get; set; }  // Enum for object type (Book, Plant, etc.)
        public Vector3 objTransform { get; set; }  // Position in the bookshelf
        public bool isPlaced { get; set; }  // Whether the object is on the shelf

        public void SetPosition(Vector3 newPosition) => objTransform = newPosition;
        public Vector3 GetPosition()
        {
            return objTransform;
        }
        public void PlaceObject() => isPlaced = true;
        public void RemoveObject() => isPlaced = false;
        public string GetUID()
        {
            return objUID;
        }
        public void SetUID(string uid) => objUID = uid;
    }
}