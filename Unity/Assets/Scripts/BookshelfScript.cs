using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookshelfScript : MonoBehaviour
{
    [SerializeField] public List<GameObject> shelfGOsList;
    List<ShelfScript> shelfScriptList = new List<ShelfScript>();
    public int numOfShelves;
    public static event System.Action<GameObject> OnSpawned;
    
    void OnEnable()
    {
        shelfScriptList = GetShelfScriptList();
        numOfShelves = shelfScriptList.Count;
        Debug.Log($"Shelves: {numOfShelves}");
        SendToPerstistenceManager();
    }

    private void SendToPerstistenceManager()
    {
        OnSpawned?.Invoke(gameObject);
    }

    private List<ShelfScript> GetShelfScriptList()
    {
        if (shelfScriptList.Count > 0)
        {
            ResetBookshelf();
        }
        List<ShelfScript> localShelfScriptList = new List<ShelfScript>();
        foreach (GameObject shelfGO in shelfGOsList)
        {
            localShelfScriptList.Add(shelfGO.GetComponent<ShelfScript>());
        }
        return localShelfScriptList;
    }

    public void ResetBookshelf()
    {
        shelfScriptList.Clear();
    }
}
