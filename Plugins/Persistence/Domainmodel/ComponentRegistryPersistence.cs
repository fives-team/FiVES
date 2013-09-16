using System;
using FIVES;
using System.Collections.Generic;

namespace Persistence
{
    internal class ComponentOwnerLayout
    {
        public ComponentOwnerLayout() {

        }

        private Guid Guid {get; set; }
        internal Guid Owner { get; set; }
        internal int Version { get; set; }
        internal ComponentLayout Layout { get; set; }
    }

    public class ComponentRegistryPersistence
    {
        public ComponentRegistryPersistence ()
        {
            this.Guid = ComponentRegistry.Instance.RegistryGuid;
            this.OwnerRegisteredComponents = new Dictionary<string, ComponentOwnerLayout> ();
        }

        internal void registerPersistedComponents()
        {
            foreach (KeyValuePair<string, ComponentOwnerLayout> definitionPair in this.OwnerRegisteredComponents) {
                ComponentOwnerLayout currentDefinition = definitionPair.Value;
                Registry.DefineComponent (definitionPair.Key, currentDefinition.Owner, currentDefinition.Layout);
            }
        }

        internal void getComponentsFromRegistry()
        {
            string[] definedComponentNames = Registry.RegisteredComponentNames;
            foreach (string componentName in definedComponentNames) {
                ComponentOwnerLayout ownedLayout = new ComponentOwnerLayout ();

                ownedLayout.Owner = Registry.GetComponentOwner (componentName);
                ownedLayout.Layout = this.createLayoutToPersistForComponent (componentName);
                ownedLayout.Version = Registry.GetComponentVersion (componentName);
                this.OwnerRegisteredComponents [componentName] = ownedLayout;
            }
        }

        private ComponentLayout createLayoutToPersistForComponent(string componentName)
        {
            ComponentLayout layout = new ComponentLayout ();
            string[] registeredAttributes = Registry.GetRegisteredAttributesOfComponent (componentName);
            foreach(string attributeName in registeredAttributes)
            {
                Type attributeType = Registry.GetAttributeType (componentName, attributeName);
                //layout.addAttribute<attributeType.GetType()> (attributeName);
            }

            return layout;
        }

        private ComponentRegistry Registry = ComponentRegistry.Instance;
        private Guid Guid { get; set; }
        private IDictionary<string, ComponentOwnerLayout> OwnerRegisteredComponents { get; set; }
    }
}

