using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Manages component definitions.
    /// </summary>
    public class ComponentRegistry
    {
        public static ComponentRegistry Instance = new ComponentRegistry();

        /// <summary>
        /// A collection of registered components.
        /// </summary>
        public ReadOnlyCollection<ReadOnlyComponentDefinition> RegisteredComponents 
        {
            get
            {
                return registeredComponents.AsReadOnly();
            }
        }

        /// <summary>
        /// Registers a new component definition. An exception will be raised if the component with the same name is 
        /// already registered.
        /// </summary>
        /// <param name="definition">New component definition.</param>
        public void Register(ReadOnlyComponentDefinition definition)
        {
            if (registeredComponents.Find(c => c.Name == definition.Name) != null)
                throw new ComponentRegistrationException("Component with the same name is already registered.");
            
            registeredComponents.Add(definition);
        }

        /// <summary>
        /// Delegate type which describes a function used to upgrade components. It should set attribute values in
        /// <paramref name="newComponent"/> based on values in <paramref name="oldComponent"/>. Unless modified 
        /// attributes in the <paramref name="newComponent"/> will have their default values.
        /// </summary>
        /// <param name="oldComponent">Old component.</param>
        /// <param name="newComponent">New component.</param>
        public delegate void ComponentUpgrader(IComponent oldComponent, IComponent newComponent);
        
        /// <summary>
        /// Upgrades a component to a new version. Version numbers must be sequential numbers starting from 1. This
        /// method will raise an exception if the previous version is not found or if same or newer version is found.
        /// </summary>
        /// <param name="newDefinition">New definition of the component.</param>
        /// <param name="upgrader">Upgrade function, see <see cref="ComponentUpgrader"/>.</param>
        public void Upgrade(ReadOnlyComponentDefinition newDefinition, ComponentUpgrader upgrader)
        {
            if (registeredComponents.Find(c => c.Name == newDefinition.Name &&
                                               c.Version >= newDefinition.Version) != null)
                throw new ComponentUpgradeException("Later or same version of the component is found.");

            var previousVersionDefinition = 
                registeredComponents.Find(c => c.Name == newDefinition.Name && c.Version == newDefinition.Version - 1);
            if (previousVersionDefinition == null)
                throw new ComponentUpgradeException("Definition with previous version of the component is not found.");

            registeredComponents.Remove(previousVersionDefinition);
            registeredComponents.Add(newDefinition);

            // Upgrade all entities.
            foreach (var entity in World.Instance)
            {
                var component = (Component)entity[newDefinition.Name];
                component.Upgrade(newDefinition, upgrader);
                if (ComponentUpgraded != null)
                    ComponentUpgraded(this, new ComponentEventArgs(component));
            }
        }

        /// <summary>
        /// Finds component definition by component's name. If component is not defined, null is returned.
        /// </summary>
        /// <param name="componentName">Component name.</param>
        /// <returns>Component definition.</returns>
        public ReadOnlyComponentDefinition FindComponentDefinition(string componentName)
        {
            return registeredComponents.Find(c => c.Name == componentName);
        }

        /// <summary>
        /// Raised when a component has been upgraded.
        /// </summary>
        public event EventHandler<ComponentEventArgs> ComponentUpgraded;

        internal ComponentRegistry()
        {
        }

        private List<ReadOnlyComponentDefinition> registeredComponents = new List<ReadOnlyComponentDefinition>();
    }
}
