// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Delegate type which describes a function used to upgrade components. It should set attribute values in
    /// <paramref name="newComponent"/> based on values in <paramref name="oldComponent"/>. Unless modified
    /// attributes in the <paramref name="newComponent"/> will have their default values.
    /// </summary>
    /// <param name="oldComponent">Old component.</param>
    /// <param name="newComponent">New component.</param>
    public delegate void ComponentUpgrader(Component oldComponent, Component newComponent);

    /// <summary>
    /// Manages component definitions.
    /// </summary>
    public sealed class ComponentRegistry : IComponentRegistry
    {
        public static IComponentRegistry Instance = new ComponentRegistry();

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

            if (RegisteredComponent != null)
                RegisteredComponent(this, new RegisteredComponentEventArgs(definition));
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

        public event EventHandler<RegisteredComponentEventArgs> RegisteredComponent;

        private Dictionary<string, ComponentDefinition> registeredComponents =
            new Dictionary<string, ComponentDefinition>();

        internal IEnumerable<Entity> world = World.Instance;
    }
}
