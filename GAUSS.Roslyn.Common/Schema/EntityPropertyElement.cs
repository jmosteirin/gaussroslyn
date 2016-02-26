using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAUSS.Roslyn.Common.Schema
{
    public class EntityPropertyElement : PropertyElement
    {
        public Guid EntityElementId { get; set; }
        public virtual EntityElement Entity { get; set; }
    }
}
