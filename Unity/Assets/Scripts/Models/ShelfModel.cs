using System;
using UnityEngine;
using static Assets.Scripts.Models.Enums;

namespace Assets.Scripts.Models
{
    public class ShelfModel : MonoBehaviour
    {
        [SerializeField] public string shelfId = null;
        [SerializeField] public string shelfName;
        [SerializeField] public string shelfType;
        [SerializeField] public int sortOrder;

        public float shelfWidth;
        public float shelfBottom;

        public ShelfLayoutInfo layoutInfo;

        private void Awake()
        {
            shelfId = this.gameObject.name;
            shelfName = this.gameObject.name;
            shelfType = eShelfType.Manual.ToString();
            //PersistanceManager.Instance.shelvesInScene.Add(this); // this is getting called before the PersistanceManager is awake 

            BoxCollider boundingBox = GetComponent<BoxCollider>();
            Vector3 alongAxisLocal = Vector3.left;
            Vector3 localAxis = transform.TransformDirection(alongAxisLocal).normalized; // same as transform.forward.normalized
            Vector3 realSize = Vector3.Scale(boundingBox.size, transform.lossyScale);
            shelfWidth = Mathf.Abs(Vector3.Dot(realSize, alongAxisLocal));
            Vector3 centerWorld = transform.TransformPoint(boundingBox.center); // collider's center
            Vector3 origin = centerWorld - localAxis * (shelfWidth / 2);
            shelfBottom = centerWorld.y - (boundingBox.size.y * transform.lossyScale.y) / 2;

            layoutInfo = new ShelfLayoutInfo()
            {
                origin = origin,
                localAxis = localAxis,
                shelfWidth = shelfWidth,
                shelfBottom = shelfBottom,
            };
            //shelfWidth = this.gameObject.GetComponent<BoxCollider>().bounds.size.x;
            //shelfBottom = this.gameObject.GetComponent<BoxCollider>().bounds.center.y - this.gameObject.GetComponent<BoxCollider>().bounds.extents.y;
            //Vector3 leftEdgeOrigin = shelfCenter - (this.transform.forward * (shelfWidth / 2f));

            //layoutInfo = new ShelfLayoutInfo()
            //{
            //    origin = leftEdgeOrigin,
            //    localAxis = this.transform.forward,
            //    shelfWidth = this.gameObject.GetComponent<BoxCollider>().bounds.size.x,
            //    shelfBottom = shelfBottom
            //};

            if (Int32.TryParse(shelfId.Substring(shelfId.LastIndexOf("_") + 1), out int numValue))
            {
                sortOrder = numValue;
            }
            else
            {
                Debug.Log("Could not parse sort order from Shelf ID; please ensure that the Shelf ID is an int.");
                sortOrder = 0;
            }
        }

        private void Start()
        {
            PersistanceManager.Instance.sqliteService.InsertShelfIntoDatabase(this);
            if (layoutInfo != null && shelfId != null)
            {
                //PersistanceManager.Instance.shelfLayoutService.RegisterShelf(this.shelfId, layoutInfo);
            }
        }

        public void OnDrawGizmosSelected()
        {
            if (layoutInfo == null) { return; }
            Vector3 start = layoutInfo.ToWorldPosition(0);
            Vector3 end = layoutInfo.ToWorldPosition(shelfWidth);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(start, 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(end, 0.05f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(start, end);

            float tickInterval = 0.05f;
            float tickSize = 0.02f;
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

            float lineLength = Vector3.Distance(start, end);
            int tickCount = Mathf.FloorToInt(lineLength / tickInterval);

            for (int i = 0; i <= tickCount; i++)
            {
                Vector3 tickPosition = start + direction * (i * tickInterval);

                Gizmos.DrawLine(
                    tickPosition - perpendicular * tickSize / 2f,
                    tickPosition + perpendicular * tickSize / 2f
                );
            }
        }
    }
}
