using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FIVES
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
                var collection = new List<ReadOnlyComponentDefinition>(registeredComponents.Values);
                return new ReadOnlyCollection<ReadOnlyComponentDefinition>(collection);
            }
        }

        /// <summary>
        /// Registers a new component definition. An exception will be raised if the component with the same name is 
        /// already registered.
        /// </summary>
        /// <param name="definition">New component definition.</param>
        public void Register(ComponentDefinition definition)
        {
            if (registeredComponents.ContainsKey(definition.Name))
                throw new ComponentRegistrationException("Component with the same name is already registered.");

            registeredComponents.Add(definition.Name, definition);
        }

        /// <summary>
        /// Delegate type which describes a function used to upgrade components. It should set attribute values in
        /// <paramref name="newComponent"/> based on values in <paramref name="oldComponent"/>. Unless modified 
        /// attributes in the <paramref name="newComponent"/> will have their default values.
        /// </summary>
        /// <param name="oldComponent">Old component.</param>
        /// <param name="newComponent">New component.</param>
        public delegate void ComponentUpgrader(Component oldComponent, Component newComponent);
        
        /// <summary>
        /// Upgrades a component to a new version. Version numbers must be sequential numbers starting from 1. This
        /// method will raise an exception if the previous version is not found or if same or newer version is found.
        /// </summary>
        /// <param name="newDefinition">New definition of the component.</param>
        /// <param name="upgrader">Upgrade function, see <see cref="ComponentUpgrader"/>.</param>
        public void Upgrade(ComponentDefinition newDefinition, ComponentUpgrader upgrader)
        {
            string name = newDefinition.Name;
            if (!registeredComponents.ContainsKey(name))
                throw new ComponentUpgradeException("Existing definition of the component is not found.");

            if (registeredComponents[name].Version != newDefinition.Version - 1)
                throw new ComponentUpgradeException("Version of the exiting definition does not precede new version.");

            registeredComponents.Remove(name);
            registeredComponents.Add(name, newDefinition);

            // Upgrade all entities.
            foreach (var entity in World.Instance)
            {
                var component = (Component)entity[name];
                component.Upgrade(newDefinition, upgrader);
                if (UpgradedComponent != null)
                    UpgradedComponent(this, new ComponentEventArgs(component));
            }
        }

        /// <summary>
        /// Finds component definition by component's name. If component is not defined, null is returned.
        /// </summary>
        /// <param name="componentName">Component name.</param>
        /// <returns>Component definition.</returns>
        public ReadOnlyComponentDefinition FindComponentDefinition(string componentName)
        {
            if (!registeredComponents.ContainsKey(componentName))
                return null;

            return registeredComponents[componentName];
        }

        /// <summary>
        /// Raised when a component has been upgraded.
        /// </summary>
        public event EventHandler<ComponentEventArgs> UpgradedComponent;

        internal ComponentRegistry()
        {
        }

        private Dictionary<string, ComponentDefinition> registeredComponents =
            new Dictionary<string, ComponentDefinition>();
    }
}
