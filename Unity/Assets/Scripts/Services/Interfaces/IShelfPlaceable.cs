using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Services.Interfaces
{
    internal interface IShelfPlaceable
    {
        public string GetObjectUID();
        public float GetPlacementWidth();
        public float GetPlacementHeight();
    }
}
