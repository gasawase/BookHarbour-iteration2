using System.Collections;
using System.Collections.Generic;
using BookHarbour;
using UnityEngine;

public class UIBookshelfObj : GeneralFunctionality
{
    [SerializeField] public GameObject objPrefab;
    public string objUID;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TODO: replace the InitializeUIObject in UIBookScript
    public void InitializeUIObject()
    {
        
    }
    
    public void SetUID(string uid)
    {
        objUID = uid;
    }

    public string GetUID()
    {
        return objUID;
    }
}
