using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}
