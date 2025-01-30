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
        public String objName { get; set; }
        public GameObject objPrefab { get; set; }
        public ObjectType objType { get; set; }
        public Vector3 objTransform { get; set; }
        public bool isPlaced { get; set; }

        
        // Method to update its Transform
        public void SetVector3(Vector3 newVector3)
        {
            objTransform = newVector3;
            // Additional logic to handle transform updates, if needed
        }
        
        public void PlaceObject()
        {
            isPlaced = true;
        }

        public void RemoveObject()
        {
            isPlaced = false;
        }
    }
}