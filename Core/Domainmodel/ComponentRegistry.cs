using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FIVES
{
    /// <summary>
    /// Exception that is thrown when a component with the same name already exists upon registering a new component
    /// </summary>
    public class ComponentAlreadyDefinedException : System.Exception 
    { 
        public ComponentAlreadyDefinedException() : base() { }
        public ComponentAlreadyDefinedException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception that is thrown when requesting a component by a name that was not registered before via <see cref="defineComponent"/>
    /// </summary>
    public class ComponentIsNotDefinedException : System.Exception 
    { 
        public ComponentIsNotDefinedException() : base() { }
        public ComponentIsNotDefinedException(string message) : base(message) { }
    }

    /// <summary>
    /// Manages component types in the database. Each component has a name and a attribute layout that it uses. Also each
    /// component must have an associated plugin/script that has defined it.
    /// </summary>
    public class ComponentRegistry
    {
        /// <summary>
        /// Handler to the instance of the static component registry class. Applications and plugins should use this central instance
        /// instead of creating instances on their owns
        /// </summary>
        public readonly static ComponentRegistry Instance = new ComponentRegistry();

        /// <summary>
        /// Defines a new component type with a <b>name</b> and a <b>layout</b>. <b>owner</b> can be anything as long as its unique.
        /// Typically it's plugin's or script's GUID. Will throw an exception if component is already defined.
        /// </summary>
        /// <param name="name">Name under which the new component should be registered</param>
        /// <param name="owner">Guid of the owner that introduces the component, usually a plugin or user script</param>
        /// <param name="layout">Layout used by the component, defining the set of attributes and their types</param>
        public void defineComponent(string name, Guid owner, ComponentLayout layout) {
            if (registeredComponents.ContainsKey(name)) {
                // We should only raise an error when we try to register a different layout or the different owner for
                // the same name. Otherwise, this can happen normally when the registry was restored from the
                // persistance database and plugin/script re-registers the layout.
                if (registeredComponents[name].owner == owner && registeredComponents[name].layout == layout)
                    return;

                throw new ComponentAlreadyDefinedException("There is already a component with name '" + name + "'");
            }

            registeredComponents[name] = new ComponentInfo();
            registeredComponents[name].owner = owner;
            registeredComponents[name].layout = layout;
        }

        /// <summary>
        /// Returns if a component was registered under a specific <b>name</b>
        /// </summary>
        /// <returns><c>true</c>, if a component with <b>name</b> is already registered, <c>false</c> otherwise.</returns>
        /// <param name="name">Component name to check for</param>
        public bool isRegistered(string name)
        {
            return registeredComponents.ContainsKey(name);
        }

        /// <summary>
        /// Returns all names of already registered components.
        /// </summary>
        /// <returns>The array of registered component names.</returns>
        public string[] getArrayOfRegisteredComponentNames()
        {
            return this.registeredComponents.Keys.ToArray ();;
        }

        /// <summary>
        /// Gets the component owner.
        /// </summary>
        /// <returns>Guid of the component owner.</returns>
        /// <param name="name">Name of the component of which to return the owner</param>
        public Guid getComponentOwner(string name)
        {
            if(!isRegistered(name))
                throw new ComponentIsNotDefinedException("Component '" + name + "' is not defined.");
            return this.registeredComponents [name].owner;
        }

        /// <summary>
        /// Gets the names of all registered attributes of a component as array.
        /// </summary>
        /// <returns>The names of registered attributes of a component as array</returns>
        /// <param name="componentName">Component name</param>
        public string[] getRegisteredAttributesOfComponent(string componentName)
        {
            if(!isRegistered(componentName))
                throw new ComponentIsNotDefinedException("Component '" + componentName + "' is not defined.");
            ComponentLayout layout = this.registeredComponents [componentName].layout;
            return layout.attributes.Keys.ToArray ();
        }

        /// <summary>
        /// Gets the type of an attribute of the component.
        /// </summary>
        /// <returns>The attribute type.</returns>
        /// <param name="componentName">Component name.</param>
        /// <param name="attributeName">Attribute name.</param>
        public Type getAttributeType(string componentName, string attributeName)
        {
            if(!isRegistered(componentName))
                throw new ComponentIsNotDefinedException("Component '" + componentName + "' is not defined.");

            return this.registeredComponents [componentName].layout.attributes [attributeName].type;
        }

        /// <summary>
        /// Creates an instance for a component of a given layout, registered under the requested <b>componentName</b>
        /// </summary>
        /// <returns>New component instance of specific layout</returns>
        /// <param name="componentName">Component name.</param>
        internal Component getComponentInstance(string componentName) {
            if(!isRegistered(componentName))
                throw new ComponentIsNotDefinedException("Component '" + componentName + "' is not defined.");

            Component newComponent = new Component(componentName);
            foreach (var entry in registeredComponents[componentName].layout.attributes)
                newComponent.addAttribute(entry.Key, entry.Value.type, entry.Value.defaultValue);

            return newComponent;
        }


        private class ComponentInfo {
            public Guid owner { get; set; }
            public ComponentLayout layout { get; set; }
        }

        // Users should not construct ComponentRegistry on their own, but use ComponentRegistry.Instance instead.
        internal ComponentRegistry() {}

        /// <summary>
        /// The registered components
        /// </summary>
        private Dictionary<string, ComponentInfo> registeredComponents = new Dictionary<string, ComponentInfo>();

        /// <summary>
        /// The registry GUID.
        /// </summary>
        public readonly Guid RegistryGuid = new Guid("18c4a2ed-caa3-4d71-8764-268551284083");
    }
}

