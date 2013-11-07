using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    class Component : IComponent
    {
        public Guid Guid { get; private set; }

        public ReadOnlyComponentDefinition Definition { get; private set; }

        public Entity Parent { get; private set; }

        public object this[string attributeName]
        {
            get
            {
                var attributeDefinition = Definition[attributeName];
                if (attributeName == null)
                    throw new AttributeAssignmentException("Attribute is not present in component definition.");

                return attributes[attributeName];
            }
            set
            {
                var attributeDefinition = Definition[attributeName];
                if (attributeName == null)
                    throw new AttributeAssignmentException("Attribute is not present in component definition.");

                if (!attributeDefinition.Type.IsAssignableFrom(value.GetType()))
                    throw new AttributeAssignmentException("Attribute can not be assigned from provided value.");
                
                var oldValue = attributes[attributeName];
                attributes[attributeName] = value;
                
                if (ChangedAttribute != null)
                    ChangedAttribute(this, new ChangedAttributeEventArgs(this, attributeName, oldValue, value));
            }
        }

        public event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;

        internal Component(ReadOnlyComponentDefinition definition, Entity parent)
        {
            Guid = Guid.NewGuid();
            Parent = parent;
            Definition = definition;
            CreateAttributes(Definition);
        }

        internal void Upgrade(ReadOnlyComponentDefinition newDefinition, ComponentRegistry.ComponentUpgrader upgrader)
        {
            Component newComponent = new Component(newDefinition, Parent);
            upgrader(this, newComponent);
            attributes = newComponent.attributes;
            Definition = newDefinition;
        }

        private void CreateAttributes(ReadOnlyComponentDefinition Definition)
        {
            foreach (IAttributeDefinition attributeDefinition in Definition.AttributeDefinitions)
                attributes.Add(attributeDefinition.Name, attributeDefinition.DefaultValue);
        }

        private Dictionary<string, object> attributes = new Dictionary<string, object>();
    }
}
