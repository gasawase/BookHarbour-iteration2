using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;

[RequireComponent(typeof(Collider))]
public class ObjectScript : GeneralFunctionality
{
    // handle generic object behavior (for all bookshelf objects)
    
    protected BookshelfObjectData bookshelfObjectData;
    public string objectUID = "";

    public void SetUID(string uid)
    {
        objectUID = uid;
        Debug.Log($"Just got a new UID: {uid}");
    }

    public string GetUID() => bookshelfObjectData?.objUID;

    public virtual void ApplyAppearance() // uses virtual so BookScript can override methods
    {
        throw new NotImplementedException();
    }
    
    public void Initialize() 
    {
        // set object information here
        throw new NotImplementedException();

    }
}
