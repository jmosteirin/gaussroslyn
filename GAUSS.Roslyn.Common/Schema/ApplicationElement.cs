using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAUSS.Roslyn.Common.Schema
{
    public class ApplicationElement : Element
    {
        public ApplicationElement()
        {
            Entities = new List<EntityElement>();
        }
        public virtual ICollection<EntityElement> Entities { get; set; }
    }
}
