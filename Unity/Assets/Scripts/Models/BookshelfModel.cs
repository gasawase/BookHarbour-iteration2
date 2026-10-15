using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class BookshelfModel : MonoBehaviour
    {
        [SerializeField]
        public List<ShelfModel> shelves = new List<ShelfModel>();

        private void Awake()
        {
            shelves = new List<ShelfModel>(GetComponentsInChildren<ShelfModel>());
            if (PersistanceManager.Instance == null)
            {
                Debug.LogError($"{name}: PersistanceManager not initialized. Check execution order.");
                return;
            }
            PersistanceManager.Instance.RegisterShelf(this);
        }

        public ShelfModel FindShelfUnder(Vector3 point)
        {
            ShelfModel bestModel = null;
            foreach (ShelfModel shelf in shelves)
            {
                if (shelf == null || shelf.layoutInfo == null) { continue; }
                ShelfLayoutInfo layoutInfo = shelf.layoutInfo;

                float u = layoutInfo.ToU(point);
                if (u < 0f || u > layoutInfo.shelfWidth) { continue; }
                if (layoutInfo.shelfBottom > point.y) { continue; }
                if (point.y > layoutInfo.shelfBottom + layoutInfo.shelfHeight) { continue; }

                if (bestModel == null || layoutInfo.shelfBottom > bestModel.layoutInfo.shelfBottom)
                {
                    bestModel = shelf;
                }
            }
            return bestModel;
        }

    }
}
