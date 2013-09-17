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
    /// Exception that is thrown when component is upgraded to an invalid version (less or equal to current).
    /// </summary>
    public class InvalidUpgradeVersion : System.Exception
    {
        public InvalidUpgradeVersion() : base() { }
        public InvalidUpgradeVersion(string message) : base(message) { }
    }

    /// <summary>
    /// Exception that is thrown when the upgraded component owner is not the same as in previous version.
    /// </summary>
    public class InvalidUpgradeOwner : System.Exception
    {
        public InvalidUpgradeOwner() : base() { }
        public InvalidUpgradeOwner(string message) : base(message) { }
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
        public void DefineComponent(string name, Guid owner, ComponentLayout layout) {
            if (RegisteredComponents.ContainsKey(name)) {
                // We should only raise an error when we try to register a different layout or the different owner for
                // the same name. Otherwise, this can happen normally when the registry was restored from the
                // persistance database and plugin/script re-registers the layout.
                if (RegisteredComponents[name].Owner == owner && RegisteredComponents[name].Layout == layout)
                    return;

                throw new ComponentAlreadyDefinedException("There is already a component with name '" + name + "'");
            }

            RegisteredComponents[name] = new ComponentInfo();
            RegisteredComponents[name].Owner = owner;
            RegisteredComponents[name].Layout = layout;
            RegisteredComponents[name].Version = 1;
        }

        public delegate void ComponentLayoutUpgradeStarted(Object sender, 
                                                           ComponentLayoutUpgradeStartedOrFinishedEventArgs e);
        public delegate void ComponentLayoutUpgradeFinished(Object sender, 
                                                            ComponentLayoutUpgradeStartedOrFinishedEventArgs e);
        public delegate void EntityComponentUpgraded(Object sender, EntityComponentUpgradedEventArgs e);

        /// <summary>
        /// Occurs when component layout upgrade starts.
        /// </summary>
        public event ComponentLayoutUpgradeStarted OnComponentLayoutUpgradeStarted;

        /// <summary>
        /// Occurs when component layout upgrade finishes.
        /// </summary>
        public event ComponentLayoutUpgradeFinished OnComponentLayoutUpgradeFinished;

        /// <summary>
        /// Occurs when a component in an entity is upgraded. These events will always occur between 
        /// OnComponentLayoutUpgradeStarted and OnComponentLayoutUpgradeFinished for the same component.
        /// </summary>
        public event EntityComponentUpgraded OnEntityComponentUpgraded;

        /// <summary>
        /// Upgrader method used by <see cref="upgradeComponent"/> . Should convert <paramref name="oldComponent"/> to
        /// <paramref name="newComponent"/>. The attributes in <paramref name="newComponent"/> will be predefined
        /// according to the new layout, but will be set to a default value and must be updated to correct values based
        /// on the values in the <paramref name="oldComponent"/>.
        /// </summary>
        public delegate void ComponentUpgrader(Component oldComponent, ref Component newComponent);

        /// <summary>
        /// Upgrades the component.
        /// </summary>
        /// <param name="name">Name of the component to be upgraded</param>
        /// <param name="owner">Guid of the owner that introduces the component, usually a plugin or user script</param>
        /// <param name="newLayout">New Layout used to be used by the component</param>
        /// <param name="version">New version. Must be larger than the previous. The first version is always 1</param>
        /// <param name="upgrader">Upgrader function. Will be called for every component present in the database</param>
        public void UpgradeComponent(string name, Guid owner, ComponentLayout newLayout, int version,
                                     ComponentUpgrader upgrader)
        {
            if (!RegisteredComponents.ContainsKey(name))
                throw new ComponentIsNotDefinedException("Undefined component " + name + " cannot be upgraded.");

            ComponentInfo currentInfo = RegisteredComponents[name];
            if (version <= currentInfo.Version)
                throw new InvalidUpgradeVersion("Version must be larger than the one already registered.");

            if (owner != currentInfo.Owner)
                throw new InvalidUpgradeOwner("Owner of the upgraded component must remain the same.");

            if (upgrader == null)
                throw new ArgumentNullException("upgrader");

            var oldVersion = RegisteredComponents[name].Version;
            RegisteredComponents[name].Version = version;
            RegisteredComponents[name].Layout = newLayout;

            if (OnComponentLayoutUpgradeStarted != null)
                OnComponentLayoutUpgradeStarted(this, new ComponentLayoutUpgradeStartedOrFinishedEventArgs(name));

            foreach (var guid in entityRegistry.GetAllGUIDs()) {
                var entity = entityRegistry.GetEntity(guid);
                if (entity.HasComponent(name)) {
                    Component oldComponent = entity[name];
                    if (oldComponent.Version != oldVersion)
                        continue;

                    Component newComponent = GetComponentInstance(name);
                    upgrader(oldComponent, ref newComponent);

                    entity[name] = newComponent;

                    if (OnEntityComponentUpgraded != null)
                        OnEntityComponentUpgraded(this, new EntityComponentUpgradedEventArgs(entity, name));
                }
            }

            if (OnComponentLayoutUpgradeFinished != null)
                OnComponentLayoutUpgradeFinished(this, new ComponentLayoutUpgradeStartedOrFinishedEventArgs(name));
        }

        /// <summary>
        /// Returns if a component was registered under a specific <b>name</b>
        /// </summary>
        /// <returns><c>true</c>, if a component with <b>name</b> is already registered, <c>false</c> otherwise.</returns>
        /// <param name="name">Component name to check for</param>
        public bool IsRegistered(string name)
        {
            return RegisteredComponents.ContainsKey(name);
        }

        /// <summary>
        /// Returns all names of already registered components.
        /// </summary>
        /// <returns>The array of registered component names.</returns>
        public string[] RegisteredComponentNames {
            get { return this.RegisteredComponents.Keys.ToArray(); }
        }

        /// <summary>
        /// Gets the component owner.
        /// </summary>
        /// <returns>Guid of the component owner.</returns>
        /// <param name="name">Name of the component of which to return the owner</param>
        public Guid GetComponentOwner(string name)
        {
            if(!IsRegistered(name))
                throw new ComponentIsNotDefinedException("Component '" + name + "' is not defined.");
            return this.RegisteredComponents [name].Owner;
        }

        /// <summary>
        /// Gets the component version.
        /// </summary>
        /// <returns>The component version.</returns>
        /// <param name="name">Name of the component of which to return the version</param>
        public int GetComponentVersion(string name)
        {
            if(!IsRegistered(name))
                throw new ComponentIsNotDefinedException("Component '" + name + "' is not defined.");
            return this.RegisteredComponents [name].Version;
        }

        /// <summary>
        /// Gets the names of all registered attributes of a component as array.
        /// </summary>
        /// <returns>The names of registered attributes of a component as array</returns>
        /// <param name="componentName">Component name</param>
        public string[] GetRegisteredAttributesOfComponent(string componentName)
        {
            if(!IsRegistered(componentName))
                throw new ComponentIsNotDefinedException("Component '" + componentName + "' is not defined.");
            ComponentLayout layout = this.RegisteredComponents [componentName].Layout;
            return layout.Attributes.Keys.ToArray ();
        }

        /// <summary>
        /// Gets the type of an attribute of the component.
        /// </summary>
        /// <returns>The attribute type.</returns>
        /// <param name="componentName">Component name.</param>
        /// <param name="attributeName">Attribute name.</param>
        public Type GetAttributeType(string componentName, string attributeName)
        {
            if(!IsRegistered(componentName))
                throw new ComponentIsNotDefinedException("Component '" + componentName + "' is not defined.");

            return this.RegisteredComponents [componentName].Layout.Attributes [attributeName].Type;
        }

        /// <summary>
        /// Creates an instance for a component of a given layout, registered under the requested <b>componentName</b>
        /// </summary>
        /// <returns>New component instance of specific layout</returns>
        /// <param name="componentName">Component name.</param>
        internal Component GetComponentInstance(string componentName) {
            if(!IsRegistered(componentName))
                throw new ComponentIsNotDefinedException("Component '" + componentName + "' is not defined.");

            Component newComponent = new Component(componentName);
            ComponentInfo info = RegisteredComponents[componentName];
            foreach (var entry in info.Layout.Attributes)
                newComponent.AddAttribute(entry.Key, entry.Value.Type, entry.Value.DefaultValue);
            newComponent.Version = info.Version;
            return newComponent;
        }


        private class ComponentInfo {
            public Guid Owner { get; set; }
            public ComponentLayout Layout { get; set; }
            public int Version;
        }

        // Users should not construct ComponentRegistry on their own, but use ComponentRegistry.Instance instead.
        internal ComponentRegistry() {}

        /// <summary>
        /// The registered components
        /// </summary>
        private Dictionary<string, ComponentInfo> RegisteredComponents = new Dictionary<string, ComponentInfo>();

        /// <summary>
        /// The registry GUID.
        /// </summary>
        public readonly Guid RegistryGuid = new Guid("18c4a2ed-caa3-4d71-8764-268551284083");

        private IEntityRegistry entityRegistry = EntityRegistry.Instance;

        #region Testing
        internal ComponentRegistry(IEntityRegistry customComponentRegistry) {
            entityRegistry = customComponentRegistry;
        }
        #endregion
    }
}

