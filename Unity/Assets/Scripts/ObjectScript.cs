using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;

[RequireComponent(typeof(Collider))]
public class ObjectScript : GeneralFunctionality
{
    // applies to all objects that will go on the shelf
    
    private BookshelfObjectData bookshelfObjectData;
    public string objectUID = "";

    public void SetUID(string uid)
    {
        objectUID = uid;
        Debug.Log($"Just got a new UID: {uid}");
    }

    public string GetUID()
    {
        return objectUID;
    }
}
