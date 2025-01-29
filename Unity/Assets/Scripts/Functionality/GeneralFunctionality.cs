using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BookHarbour
{
    public class GeneralFunctionality : MonoBehaviour
    {
        public Vector3 MatchSize(GameObject targetObject, GameObject objecttoResize)
        {
            if (targetObject == null || objecttoResize == null)
            {
                Debug.LogError("Please assign both the targetObject and cubeToResize.");
                throw new System.Exception("Please make sure that your objects are not null.");
            }

            // Get the bounds of the target object
            Renderer targetRenderer = targetObject.GetComponent<Renderer>();
            Renderer objectRenderer = objecttoResize.GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            }

            if (targetRenderer == null || objectRenderer == null)
            {
                Debug.LogError("Both objects must have a Renderer component.");
                throw new System.Exception("Please make sure that both objects have a Renderer component.");
            }

            // Calculate the size of the target object in world space
            Vector3 targetSize = targetRenderer.bounds.size;

            // Get the current size of the cube in world space
            Vector3 objectSize = objectRenderer.bounds.size;

            // Calculate the scale factor for each dimension
            Vector3 scaleFactor = new Vector3(
                targetSize.x / objectSize.x,
                targetSize.y / objectSize.y,
                targetSize.z / objectSize.z
            );

            // Apply the scaling factor to the cube
            objecttoResize.transform.localScale = Vector3.Scale(objecttoResize.transform.localScale, scaleFactor);
            return objecttoResize.transform.localScale;
        }
        
        //public GameObject individualShelf; // The shelf object with a BoxCollider
        public float defaultPadding = 0.05f; // Default padding between objects
        
        /// <summary>
        /// Generates snap points dynamically based on the size of the object to be placed.
        /// </summary>
        /// <param name="referenceObject">The object used as a reference for snap point size.</param>
        /// <param name="padding">Optional: Padding between objects.</param>
        public List<Vector3> GenerateSnapPoints(GameObject referenceObject, Bookshelf bookshelf, float? padding = null)
        {
            List<Vector3> snapPoints = new List<Vector3>();
            // Clear previous snap points
            snapPoints.Clear();

            // Validate input
            if (bookshelf == null || referenceObject == null)
            {
                Debug.LogError("Shelf object and reference object must not be null.");
                return snapPoints;
            }
            
            // Loop through all of the shelves on this bookshelf
            for (int i = 0; i < bookshelf.arrayOfShelves.Length; i++)
            {
                GameObject shelf = bookshelf.arrayOfShelves[i];
                // Get the shelf's BoxCollider
                BoxCollider shelfCollider = shelf.GetComponent<BoxCollider>();
                if (shelfCollider == null)
                {
                    Debug.LogError("The shelf object must have a BoxCollider.");
                    return snapPoints;
                }

                Vector3 shelfSize = shelfCollider.size; // Local size of the shelf
                Vector3 shelfCenter = shelfCollider.bounds.center; // World center of the shelf

                // Calculate the bottom and front of the shelf
                float bottomOfShelf = shelfCollider.bounds.min.y;
                float frontOfShelf = shelfCollider.bounds.min.z;
                
                // Get the size of the reference object
                Renderer objectRenderer = referenceObject.GetComponent<Renderer>();
                if (objectRenderer == null)
                {
                    objectRenderer = referenceObject.GetComponentInChildren<Renderer>();
                    if (objectRenderer == null)
                    {
                        Debug.LogError("The object to place must have a Renderer component.");
                        return snapPoints;
                    }
                }

                Vector3 objectSize = objectRenderer.bounds.size;

                // Use provided padding or default padding
                float effectivePadding = padding ?? defaultPadding;

                // Starting position for the first book
                Vector3 currentPosition = new Vector3(shelfCenter.x - (shelfSize.x / 2) + (objectSize.x / 2),
                    bottomOfShelf + (objectSize.y / 2),                      // Align to the bottom of the shelf
                    frontOfShelf + (objectSize.z / 2)
                );

                // Generate snap points along the width of the shelf
                while (currentPosition.x + (objectSize.x / 2) <= shelfCenter.x + (shelfSize.x / 2))
                {
                    //Vector3 newSize = MatchSize(referenceObject, currentPosition);
                    snapPoints.Add(currentPosition);
                    currentPosition.x += objectSize.x + effectivePadding; // Move to the next snap point
                }
                //Debug.Log($"Shelf number {i} snap points: {snapPoints.Count}");
            }
            
            //Debug.Log($"Total snap points: {snapPoints.Count}");
            return snapPoints;

        }

        /// <summary>
        /// Finds the closest snap point for a given object.
        /// </summary>
        /// <param name="snapPoints">List of available snap points.</param>
        /// <param name="objectPosition">The current position of the object.</param>
        /// <returns>The closest snap point to the object position.</returns>
        public Vector3 FindClosestSnapPoint(List<Vector3> snapPoints, Vector3 objectPosition)
        {
            if (snapPoints == null || snapPoints.Count == 0)
            {
                Debug.LogError("No snap points available.");
                return objectPosition; // Return current position if no snap points are available
            }

            Vector3 closestPoint = snapPoints[0];
            float closestDistance = Vector3.Distance(objectPosition, closestPoint);

            foreach (Vector3 point in snapPoints)
            {
                float distance = Vector3.Distance(objectPosition, point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }

        public Texture2D ConvertFromBase64ToTexture2D(string base64)
        {
            throw new NullReferenceException();
        }

        public class BooleanMonitor
        {
            public UnityEvent OnBooleanChanged = new UnityEvent(); // Initialize here
            private bool monitoredValue;
            
            public BooleanMonitor(bool initialValue = false)
            {
                monitoredValue = initialValue;
            }
            public bool MonitoredValue
            {
                get => monitoredValue;
                set
                {
                    if (monitoredValue != value) // Trigger only if the value changes
                    {
                        monitoredValue = value;
                        OnBooleanChanged?.Invoke(); // Invoke the event
                    }
                }
            }
        }
    }
}
