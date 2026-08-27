using UnityEngine;

/// <summary>
/// Projects a screen-space point onto a world-space plane that is defined
/// relative to a reference transform (e.g. the bookshelf root). Because the
/// plane is rebuilt from the reference transform every call, it automatically
/// follows if the shelf is repositioned or rotated - nothing is cached.
/// </summary>

namespace Assets.Scripts.Services
{
    public class DragPlaneProjector
    {
        private readonly Camera worldCamera;

        public DragPlaneProjector(Camera worldCamera)
        {
            this.worldCamera = worldCamera;
        }

        /// <summary>
        /// Builds a plane facing along the reference transform's forward axis,
        /// offset from the reference position by depthOffset along that same axis.
        /// </summary>
        public Plane BuildPlane(Transform reference, float depthOffset)
        {
            Vector3 planeNormal = reference.right;
            Vector3 pointOnPlane = reference.position + reference.right * depthOffset;
            return new Plane(planeNormal, pointOnPlane);
        }

        /// <summary>
        /// Casts a ray from the camera through the given screen point and returns
        /// where it intersects the plane. Returns false if the ray is parallel to
        /// the plane (should not happen in practice with a shelf-facing camera).
        /// </summary>
        public bool TryProject(Vector2 screenPoint, Plane plane, out Vector3 worldPoint)
        {
            Ray ray = worldCamera.ScreenPointToRay(screenPoint);

            if (plane.Raycast(ray, out float distance))
            {
                worldPoint = ray.GetPoint(distance);
                return true;
            }

            worldPoint = Vector3.zero;
            return false;
        }
    }
}