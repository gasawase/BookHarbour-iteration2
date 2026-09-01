using Assets.Scripts.Models;
using System;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class ShelfOverlapDetector : MonoBehaviour
    {
        public Collider? shelfCollider;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Entered Trigger");
            if (other.GetComponent<ShelfModel>())
            {
                shelfCollider = other;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exited Trigger");
            if (other == shelfCollider)
            {
                shelfCollider = null;
            }
        }
    }
}
