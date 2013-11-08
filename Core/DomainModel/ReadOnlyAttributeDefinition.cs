using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents an attribute definition.
    /// </summary>
    public class ReadOnlyAttributeDefinition
    {
        /// <summary>
        /// GUID that identifies this attribute definition.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Name of the attribute.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Default value for the attribute.
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Type of the attribute.
        /// </summary>
        public Type Type { get; protected set; }

        internal ReadOnlyAttributeDefinition(string name, Type type, object defaultValue)
        {
            Guid = Guid.NewGuid();
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }
}
