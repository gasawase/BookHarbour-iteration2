using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistanceManager : MonoBehaviour
{
    public static PersistanceManager Instance { get; private set; }
    public GameObject currentLiveBookshelf;
    
    // on start, find the current shelf that is live

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure it persists across scenes
        }
        else
        {
            Destroy(gameObject); // If another BookManager exists, destroy the new one
        }
        //currentLiveBookshelf = GameObject.FindGameObjectWithTag("Bookshelf");
        //Debug.Log($"The current live bookshelf is {currentLiveBookshelf}");
    }
    
    void OnEnable()
    {
        BookshelfScript.OnSpawned += HandleBookshelfSpawned;
    }

    void OnDisable()
    {
        BookshelfScript.OnSpawned += HandleBookshelfSpawned;
    }

    void HandleBookshelfSpawned(GameObject bookshelfFromBookshelfScript)
    {
        currentLiveBookshelf = bookshelfFromBookshelfScript;
        Debug.Log($"The current live bookshelf is {currentLiveBookshelf}");
        Debug.Log($"It has {currentLiveBookshelf.GetComponent<BookshelfScript>().numOfShelves} shelves");
    }
}
