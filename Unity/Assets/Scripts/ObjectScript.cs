using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookHarbour;

[RequireComponent(typeof(Collider))]
public class ObjectScript : MonoBehaviour
{
    // applies to all objects that will go on the shelf
    
    private BookshelfObject bookshelfObject;
    
    // Not sure if I need this, but I'm leaving it for now
    // void OnCollisionEnter(Collision collision)
    // {
    //     Debug.Log($"{collision.gameObject.name} is colliding with this {this.gameObject.name}");
    // }
    
}
