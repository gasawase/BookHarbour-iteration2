using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    internal class BookModel
    {
        public Guid bookID { get; set; }
        public Guid shelfID { get; set; }
        public Guid positionID { get; set; }
        public string slotIndex { get; set; }
    }
}
