using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// An interface that represents a component in an entity.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// GUID that uniquely identifies this componentn.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// The definition that was used to create this component.
        /// </summary>
        ReadOnlyComponentDefinition Definition { get; }

        /// <summary>
        /// A parent entity that contains this component.
        /// </summary>
        Entity Parent { get; }

        /// <summary>
        /// Accessor that allows to get and set attribute values. Users must cast the value to correct type themselves.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>Value of the attribute.</returns>
        object this[string attributeName] { get; set; }

        /// <summary>
        /// An event that is raised when any attribute of this component is changed.
        /// </summary>
        event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;
    }
}
