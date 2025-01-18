using System;
using UnityEngine;


namespace BookHarbour
{
    public enum ObjectType
    {
        Book,
        Plant,
        Figurine,
        Other
    }
    public class BookshelfObject
    {
        public String objName { get; set; }
        public GameObject objPrefab { get; set; }
        public ObjectType objType { get; set; }
        public Transform objTransform { get; set; }
    }
}