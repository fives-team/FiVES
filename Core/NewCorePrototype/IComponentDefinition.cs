using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    public interface IComponentDefinition
    {
        /// <summary>
        /// GUID that uniquely identifies this component definition.
        /// </summary>
        Guid Guid { get; } 
        
        /// <summary>
        /// Name of the component.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of the component.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// A collection of attribute definitions.
        /// </summary>
        ReadOnlyCollection<IAttributeDefinition> AttributeDefinitions;
    }
}
