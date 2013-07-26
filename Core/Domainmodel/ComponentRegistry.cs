using System;
using System.Collections.Generic;

namespace FIVES
{
    public class ComponentAlreadyDefinedException : System.Exception 
    { 
        public ComponentAlreadyDefinedException() : base() { }
        public ComponentAlreadyDefinedException(string message) : base(message) { }
    }

    public class ComponentIsNotDefinedException : System.Exception 
    { 
        public ComponentIsNotDefinedException() : base() { }
        public ComponentIsNotDefinedException(string message) : base(message) { }
    }

    // Manages component types in the database. Each component has a name and a attribute layout that it uses. Also each
    // component must have an associated plugin/script that has defined it.
    public class ComponentRegistry
    {
        public static ComponentRegistry Instance = new ComponentRegistry();

        // Defines a new component type with a |name| and a |layout|. |owner| can be anything as long as its unique.
        // Typically it's plugin's or script's GUID. Will throw an exception if component is already defined.
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

        // Constructs a component based on previosly defined type. Will throw an exception if component is not defined.
        public Component createComponent(string componentName) {
            if (!registeredComponents.ContainsKey(componentName))
                throw new ComponentIsNotDefinedException("Component '" + componentName + "' is not defined.");

            Component newComponent = new Component(componentName);
            foreach (var entry in registeredComponents[componentName].layout.attributes)
                newComponent.addAttribute(entry.Key, entry.Value);

            return newComponent;
        }

        public bool isRegistered(string name)
        {
            return registeredComponents.ContainsKey(name);
        }

        // Users should not construct ComponentRegistry on their own, but use ComponentRegistry.Instance instead.
        internal ComponentRegistry() {}

        private class ComponentInfo {
            public Guid owner;
            public ComponentLayout layout;
        }

        private Dictionary<string, ComponentInfo> registeredComponents = new Dictionary<string, ComponentInfo>();
    }
}

