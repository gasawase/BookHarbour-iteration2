using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfScript : MonoBehaviour
{
    public ShelfModel shelfModel;
    public Vector3 shelfDimensions;
    // Start is called before the first frame update
    void Start()
    {
        shelfModel = new ShelfModel()
        {
            shelfDimensions = this.GetComponent<BoxCollider>().size
        }; 
        shelfDimensions = shelfModel.shelfDimensions;
    }
}
