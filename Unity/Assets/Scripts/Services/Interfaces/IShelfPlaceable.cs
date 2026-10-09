using UnityEngine;

namespace Assets.Scripts.Services.Interfaces
{
    public interface IShelfPlaceable
    {
        public string GetObjectUID();
        public GameObject GetObjectGameObject();
        public float GetPlacementWidth();
        public float GetPlacementHeight();
        void SetPlacementPosition(Vector3 worldPosition);
    }
}
