using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents an entity.
    /// </summary>
    public sealed class Entity
    {
        public Entity()
        {
            Guid = Guid.NewGuid();
        }

        public Entity(Guid guid)
        {
            Guid = guid;
        }

        /// <summary>
        /// GUID that uniquely identifies this entity.
        /// </summary>  
        public Guid Guid { get; private set; }

        /// <summary>
        /// A read-only collection of components that this entity has. New components are added automatically when 
        /// accessed via [] operator, however components must be registered using ComponentRegistry before they are     
        /// accessed.
        /// </summary>
        public ReadOnlyCollection<Component> Components
        {
            get { return new ReadOnlyCollection<Component>(components.Values); }
        }

        /// <summary>
        /// Accessor that allows to quickly get a component with a given name. Components that are registered with
        /// ComponentRegistry are created automatically when accessed.
        /// </summary>
        /// <param name="componentName">Name of the component, which is to be returned.</param>
        /// <returns>Component.</returns>
        public Component this[string componentName]
        {
            get 
            {
                if (!components.ContainsKey(componentName))
                    CreateComponent(componentName);

                return components[componentName];
            }
        }

		
        /// <summary>git
        /// An event that is raised when a new component is created in this entity.
        /// </summary>
        public event EventHandler<ComponentEventArgs> CreatedComponent;

        /// <summary>
        /// An event that is raised when any attribute in any of the components of this entity is changed.
        /// </summary>
        /// TODO
        public event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;

        /// <summary>
        /// Verifies whether this entity contains a component with a given name.
        /// </summary>
        /// <param name="name">Component name.</param>
        /// <returns>True if a component with given name is present, false otherwise.</returns>
        public bool ContainsComponent(string name)
        {
            return components.ContainsKey(name);
        }

        private void CreateComponent(string componentName)
        {
            var definition = componentRegistry.FindComponentDefinition(componentName);
            if (definition == null)
                throw new ComponentAccessException("Component with given name is not registered.");

            Component component = new Component(definition, this);
            components[componentName] = component;

            // Register for attribute updates in new component.
            component.ChangedAttribute += HandleChangedComponentAttribute;

            if (CreatedComponent != null)
                CreatedComponent(this, new ComponentEventArgs(component));
        }

        private void HandleChangedComponentAttribute(object sender, ChangedAttributeEventArgs e)
        {
            if (ChangedAttribute != null)
                ChangedAttribute(this, e);
        }

        private Dictionary<string, Component> components = new Dictionary<string, Component>();

        internal IComponentRegistry componentRegistry = ComponentRegistry.Instance;
    }
}
