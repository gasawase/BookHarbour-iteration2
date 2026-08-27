using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Services
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIBookDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public CanvasGroup CanvasGroup => canvasGroup;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            BookDragManager.Instance.BeginDrag(this, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            BookDragManager.Instance.UpdateDrag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            BookDragManager.Instance.EndDrag(eventData.position);
        }
    }
}
