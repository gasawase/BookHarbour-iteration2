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
        public Transform objTransform { get; set; }
        public bool isPlaced { get; set; }

        
        // Method to update its Transform
        public void SetTransform(Transform newTransform)
        {
            objTransform = newTransform;
            // Additional logic to handle transform updates, if needed
        }
    }
}