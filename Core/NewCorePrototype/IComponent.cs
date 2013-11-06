using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Utils;

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
        IComponentDefinition Definition { get; }

        /// <summary>
        /// A parent entity that contains this component.
        /// </summary>
        IEntity Parent { get; }

        /// <summary>
        /// A read-only collection of attributes. Created dynamically when accessed which may cause additional cost 
        /// when accessing this property. Please use operator [] to get and set attribute values instead.
        /// </summary>
        /// <remarks>
        /// An implementation should keep attributes as a simple name-to-value (string-to-object) dictionary and only 
        /// dynamically create this list when accessed. However, as soon as this list is created it should be used 
        /// instead of the dictionary. This allows to keep the common use-case scenario efficient, yet provides the 
        /// users with rich interface when necessary.
        /// </remarks>
        ReadOnlyCollection<IAttribute> Attributes { get; }

        /// <summary>
        /// Accessor that allows to get and set attribute values. Users must cast the value to correct type themselves.
        /// </summary>
        /// <param name="attributeName">Name of the attribute, whose value is to be returned or set.</param>
        /// <returns></returns>
        object this[string attributeName] { get; set; }

        /// <summary>
        /// An event that is raised when any attribute of this component is changed.
        /// </summary>
        event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;
    }
}
