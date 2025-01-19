using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;

[RequireComponent(typeof(Collider))]
public class ObjectScript : MonoBehaviour
{
    // applies to all objects that will go on the shelf
    
    
    private BookshelfObject bookshelfObject;
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{collision.gameObject.name} is colliding with this {this.gameObject.name}");
        // if (!isDragging)
        // {
        //     
        // }
    }
    
}
