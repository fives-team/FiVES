using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Represents an attribute in a component.
    /// </summary>
    /// TODO: Do we really need this class? It seems it only contains one useful method - Reset, which can be moved to 
    /// IComponent. Persistance should be able to persist the attribute dictionary and get the rest of the data from 
    /// the IComponentDefinition/IAttributeDefinition.
    public interface IAttribute
    {
        /// <summary>
        /// GUID that identifies this attribute.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// A parent component that contains this attribute.
        /// </summary>
        IComponent Parent { get; }

        /// <summary>
        /// The definition that was used to create this attribute.
        /// </summary>
        IAttributeDefinition Definition { get; }

        /// <summary>
        /// Current value of the attribute.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Resets the value of the attribute to a default value (as specified in the component definition).
        /// </summary>
        void Reset();

        /// <summary>
        /// An event that is raised when this attribute is changed.
        /// </summary>
        event EventHandler<ChangedAttributeEventArgs> Changed;
    }
}
