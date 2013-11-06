using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    public interface IAttributeDefinition
    {
        /// <summary>
        /// GUID that identifies this attribute definition.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Name of the attribute.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Default value for the attribute.
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Type of the attribute.
        /// </summary>
        Type Type { get; }
    }
}
