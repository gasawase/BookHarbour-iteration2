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
                ShelfLayoutInfo layoutInfo = shelf.layoutInfo;
                if (shelf == null || shelf.layoutInfo == null) { continue; }

                float u = layoutInfo.ToU(point);
                if (u < 0f || u > layoutInfo.shelfWidth) { continue; }
                if (layoutInfo.shelfBottom > point.y) { continue; }

                if (bestModel == null || layoutInfo.shelfBottom > bestModel.layoutInfo.shelfBottom)
                {
                    bestModel = shelf;
                }
            }
            return bestModel;
        }

    }
}
