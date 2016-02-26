using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAUSS.Roslyn.Common.Schema
{
    public class EntityElement : Element
    {
        public EntityElement()
        {
            Properties = new List<EntityPropertyElement>();
        }
        public EntityElement(params string[] paramTypesAndNames)
        {
            Properties = new List<EntityPropertyElement>();
            var typesAndNames = paramTypesAndNames;

            if (typesAndNames.Count() <= 1)
                return;

            if ((typesAndNames.Count() % 2) != 0)
                typesAndNames = typesAndNames.Reverse().Skip(1).Reverse().ToArray();

            for (int i = 0; i < typesAndNames.Count(); i += 2)
                Properties.Add(new EntityPropertyElement()
                {
                    Id = Guid.NewGuid(),
                    Type = typesAndNames[i],
                    Name = typesAndNames[i + 1]
                });
        }
        public Guid? BaseEntityId { get; set; }
        public virtual EntityElement BaseEntity { get; set; }
        public virtual ICollection<EntityPropertyElement> Properties { get; set; }
    }
}
