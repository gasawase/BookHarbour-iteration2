using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;

public class BookshelfManager : MonoBehaviour
{
    public Bookshelf bookshelf;
    private void Start()
    {
        throw new NotImplementedException();
    }

    private bool CanFit(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        return false;
    }

    private void PlaceObj(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }

    private Vector3 GetObjPosition(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        return obj.objTransform.position;
    }

    private int GetObjIdx(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        return 0;
    }

    private bool IsSpaceOccupied(int bookshelfLocationIdx)
    {
        return false;
    }

    private void StackObject(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }

    private void SaveObjectToShelf(BookshelfObject obj, IndividualShelfMapping individualShelf)
    {
        throw new NotImplementedException();
    }
}
