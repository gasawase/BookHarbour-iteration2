using Assets.Scripts.Services.Interfaces;

namespace Assets.Scripts.Models
{
    public struct PlacedItem
    {
        public IShelfPlaceable item;
        public float offsetAlongAxis; // distance from the shelf's left edge in real-world units
    }
}
