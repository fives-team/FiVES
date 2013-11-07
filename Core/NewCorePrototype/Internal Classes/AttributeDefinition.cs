using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    class AttributeDefinition : IAttributeDefinition
    {
        public AttributeDefinition(string name, Type type, object defaultValue)
        {
            Guid = Guid.NewGuid();
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }

        public Guid Guid { get; private set; }

        public string Name { get; private set; }

        public object DefaultValue { get; private set; }

        public Type Type { get; private set; }
    }
}
