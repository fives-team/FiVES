using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    class AttributeDef
    {
        public Guid Guid;
        public string Name;
        public object DefaultValue;
        public string Type;  // contains AssemblyQualifiedName of the type
    }
}
