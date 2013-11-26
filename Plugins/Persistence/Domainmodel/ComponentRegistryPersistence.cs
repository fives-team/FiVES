using System;
using FIVES;
using System.Collections.Generic;

namespace PersistencePlugin
{
    public class ComponentRegistryPersistence
    {
        public static Guid Guid = new Guid("541ff74b-7154-46b3-8346-e3651b4b4140");

        public ComponentRegistryPersistence ()
        {
            this.OwnerRegisteredComponents = new Dictionary<string, ReadOnlyComponentDefinition>();
        }

        internal void RegisterPersistedComponents()
        {
            foreach (KeyValuePair<string, ReadOnlyComponentDefinition> definitionPair in this.OwnerRegisteredComponents)
            {
                ReadOnlyComponentDefinition roDefinition = definitionPair.Value;
                ComponentDefinition definition = new ComponentDefinition(roDefinition.Name);
                foreach (ReadOnlyAttributeDefinition attrDef in definition.AttributeDefinitions)
                    definition.AddAttribute(attrDef.Name, attrDef.Type, attrDef.DefaultValue);
                Registry.Register (definition);
            }
        }

        internal void GetComponentsFromRegistry()
        {
            foreach (ReadOnlyComponentDefinition definition in Registry.RegisteredComponents) {
                this.OwnerRegisteredComponents [definition.Name] = definition;
            }
        }

        private ComponentRegistry Registry = ComponentRegistry.Instance;
        private IDictionary<string, ReadOnlyComponentDefinition> OwnerRegisteredComponents { get; set; }
    }
}

