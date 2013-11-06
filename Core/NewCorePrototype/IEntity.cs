using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NewCorePrototype
{
    /// <summary>
    /// An interface that represents an entity.
    /// </summary>
	public interface IEntity
	{
        /// <summary>
        /// GUID that uniquely identifies this entity.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// A read-only collection of components that this entity has. New components are added automatically when 
        /// accessed via [] operator, however components must be registered using ComponentRegistry before they are     
        /// accessed.
        /// </summary>
        ReadOnlyCollection<IComponent> Components { get; }

        /// <summary>
        /// Accessor that allows to quickly get a component with a given name. Components that are registered with
        /// ComponentRegistry are created automatically when accessed.
        /// </summary>
        /// <param name="componentName">Name of the component, which is to be returned.</param>
        /// <returns></returns>
        IComponent this[string componentName] { get; }

        /// <summary>
        /// Collection of children of this entity. Children that are added are automatically removed from the list of
        /// children of the previous parent if any.
        /// </summary>
        IList<IEntity> Children { get; }

        /// <summary>
        /// Parent of this entity. The value null is used to denote that the entity doesn't have a parent.
        /// </summary>
        IEntity Parent { get; }

        /// <summary>
        /// An event that is raised when a new component is created in this entity.
        /// </summary>
        event EventHandler<CreatedComponentEventArgs> CreatedComponent;

        /// <summary>
        /// An event that is raised when any attribute in any of the components of this entity is changed.
        /// </summary>
        event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;
	}
}

