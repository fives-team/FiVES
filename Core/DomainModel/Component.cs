using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents a component in an entity.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// GUID that uniquely identifies this componentn.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// The definition that was used to create this component.
        /// </summary>
        public ReadOnlyComponentDefinition Definition { get; private set; }

        /// <summary>
        /// A parent entity that contains this component.
        /// </summary>
        public Entity Parent { get; private set; }

        /// <summary>
        /// Accessor that allows to get and set attribute values. Users must cast the value to correct type themselves.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>Value of the attribute.</returns>
        public object this[string attributeName]
        {
            get
            {
                if (!Definition.ContainsAttributeDefinition(attributeName))
                    throw new KeyNotFoundException("Attribute is not present in the component definition.");

                return attributes[attributeName];
            }
            set
            {
                if (!Definition.ContainsAttributeDefinition(attributeName))
                    throw new KeyNotFoundException("Attribute is not present in the component definition.");

                if (!Definition[attributeName].Type.IsAssignableFrom(value.GetType()))
                    throw new AttributeAssignmentException("Attribute can not be assigned from provided value.");

                var oldValue = attributes[attributeName];
                attributes[attributeName] = value;

                if (ChangedAttribute != null)
                    ChangedAttribute(this, new ChangedAttributeEventArgs(this, attributeName, oldValue, value));
            }
        }

        /// <summary>
        /// An event that is raised when any attribute of this component is changed.
        /// </summary>
        public event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;

        internal Component(ReadOnlyComponentDefinition definition, Entity parent)
        {
            Guid = Guid.NewGuid();
            Parent = parent;
            Definition = definition;
            InitializeAttributes();
        }

        internal void Upgrade(ReadOnlyComponentDefinition newDefinition, ComponentRegistry.ComponentUpgrader upgrader)
        {
            Component newComponent = new Component(newDefinition, Parent);
            upgrader(this, newComponent);
            attributes = newComponent.attributes;
            Definition = newDefinition;
        }

        private void InitializeAttributes()
        {
            foreach (ReadOnlyAttributeDefinition attributeDefinition in Definition.AttributeDefinitions)
                attributes.Add(attributeDefinition.Name, attributeDefinition.DefaultValue);
        }

        private Dictionary<string, object> attributes = new Dictionary<string, object>();
    }
}
