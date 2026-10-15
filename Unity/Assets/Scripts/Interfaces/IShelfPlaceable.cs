using UnityEngine;
using static Assets.Scripts.Models.Enums;

namespace Assets.Scripts.Services.Interfaces
{
    public interface IShelfPlaceable
    {
        public eOrientation Orientation { get; }
        public string GetObjectUID();
        public GameObject GetObjectGameObject();
        public float GetPlacementWidth();
        public float GetPlacementHeight();
        public void SetPlacementPosition(Vector3 worldPosition);
        public void SetOrientation(eOrientation orientation);
        public void SetSelected(bool isSelected);
    }
}
