using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class ShelfOverlapDetector : MonoBehaviour
    {
        public Collider? shelfCollider;
        public string currentShelfID;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Entered Trigger");
            if (other.GetComponent<ShelfModel>())
            {
                shelfCollider = other;
                currentShelfID = other.GetComponent<ShelfModel>().shelfId;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exited Trigger");
            if (other == shelfCollider)
            {
                shelfCollider = null;
                currentShelfID = null;
            }
        }
    }
}
