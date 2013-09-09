using System;
using FIVES;
using System.Collections.Generic;

namespace Persistence
{
    internal class ComponentOwnerLayout
    {
        public ComponentOwnerLayout() {

        }

        private Guid Id {get; set; }
        internal Guid owner { get; set; }
        internal int version { get; set; }
        internal ComponentLayout layout { get; set; }
    }

    public class ComponentRegistryPersistence
    {
        public ComponentRegistryPersistence ()
        {
            this.Id = ComponentRegistry.Instance.RegistryGuid;
            this.ownerRegisteredComponents = new Dictionary<string, ComponentOwnerLayout> ();
        }

        internal void registerPersistedComponents()
        {
            foreach (KeyValuePair<string, ComponentOwnerLayout> definitionPair in this.ownerRegisteredComponents) {
                ComponentOwnerLayout currentDefinition = definitionPair.Value;
                registry.defineComponent (definitionPair.Key, currentDefinition.owner, currentDefinition.layout);
            }
        }

        internal void getComponentsFromRegistry()
        {
            string[] definedComponentNames = registry.getArrayOfRegisteredComponentNames ();
            foreach (string componentName in definedComponentNames) {
                ComponentOwnerLayout ownedLayout = new ComponentOwnerLayout ();

                ownedLayout.owner = registry.getComponentOwner (componentName);
                ownedLayout.layout = this.createLayoutToPersistForComponent (componentName);
                ownedLayout.version = registry.getComponentVersion (componentName);
                this.ownerRegisteredComponents [componentName] = ownedLayout;
            }
        }

        private ComponentLayout createLayoutToPersistForComponent(string componentName)
        {
            ComponentLayout layout = new ComponentLayout ();
            string[] registeredAttributes = registry.getRegisteredAttributesOfComponent (componentName);
            foreach(string attributeName in registeredAttributes)
            {
                Type attributeType = registry.getAttributeType (componentName, attributeName);
                //layout.addAttribute<attributeType.GetType()> (attributeName);
            }

            return layout;
        }

        private ComponentRegistry registry = ComponentRegistry.Instance;
        private Guid Id { get; set; }
        private IDictionary<string, ComponentOwnerLayout> ownerRegisteredComponents { get; set; }
    }
}

