using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Models.Enums;

namespace Assets.Scripts.Models
{
    public class ShelfModel : MonoBehaviour
    {
        [SerializeField] public string shelfId; 
        [SerializeField] public string shelfName;
        [SerializeField] public string shelfType;
        [SerializeField] public int sortOrder;

        private void Awake()
        {
            shelfId = this.gameObject.name;
            shelfName = this.gameObject.name;
            shelfType = eShelfType.Manual.ToString();
            if (Int32.TryParse(shelfId.Substring(shelfId.LastIndexOf("_") + 1), out int numValue))
            {
                sortOrder = numValue;
            }
            else
            {
                Debug.Log("Could not parse sort order from Shelf ID; please ensure that the Shelf ID ends with an int.");
                sortOrder = 0;
            }
        }

        private void Start()
        {
            PersistanceManager.Instance.sqliteService.InsertShelfIntoDatabase(this);
        }
    }
}
