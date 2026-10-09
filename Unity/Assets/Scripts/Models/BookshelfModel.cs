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
            PersistanceManager.Instance.RegisterShelf(this);
        }

    }
}
