using Assets.Scripts.Services;
using UnityEngine;
using UnityEngine.EventSystems;

public class Object3DDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string bookId { get; set; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("pew pew");

        BookDragManager.Instance.BeginDrag3DObject(this, eventData.position);
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
