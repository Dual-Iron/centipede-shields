using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFisobs
{
    public abstract class Critob : Fisob
    {
        protected Critob(string id) : base(id)
        {
        }

        private CreatureTemplate.Type? type;

        new public CreatureTemplate.Type Type {
            get {
                if (type == null) {
                    type = RWCustom.Custom.ParseEnum<CreatureTemplate.Type>(ID);
                }
                return type.Value;
            }
        }
    }
}
