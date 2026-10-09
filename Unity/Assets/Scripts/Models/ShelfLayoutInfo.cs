using UnityEngine;

namespace Assets.Scripts.Models
{
    public class ShelfLayoutInfo
    {
        public Vector3 origin;
        public Vector3 localAxis;
        public float shelfWidth;
        public float shelfHeight;
        public float shelfBottom;

        public float ToU(Vector3 worldPoint) // This world point is how far along me?
        {
            return Vector3.Dot(worldPoint - origin, localAxis);
        }

        public Vector3 ToWorldPosition(float uDistance) // Distance u along me is which world point
        {
            var point = origin + (localAxis * uDistance);
            point.y = shelfBottom;
            return point;
        }
    }
}
