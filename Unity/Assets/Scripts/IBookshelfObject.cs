using UnityEngine;

namespace BookHarbour
{
    /// <summary>
    /// what all bookshelf objects must have, but they do not contain implementations
    /// </summary>
    public interface IBookshelfObject
    {
        string GetUID();
        void SetUID(string uid);
        void PlaceObject();
        void RemoveObject();
        Vector3 GetPosition();
        void SetPosition(Vector3 newPosition);
    }
}