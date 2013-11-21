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
    public sealed class Component
    {
        /// <summary>
        /// Returns the name of the component. This is a shorthand for Definition.Name
        /// </summary>
        public string Name
        {
            get
            {
                return Definition.Name;
            }
        }

        /// <summary>
        /// GUID that uniquely identifies this componentn.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// The definition that was used to create this component.
        /// </summary>
        public ReadOnlyComponentDefinition Definition { get; private set; }

        /// <summary>
        /// An entity that contains this component.
        /// </summary>
        public Entity ContainingEntity { get; private set; }

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

        internal Component()
        {
        }

        internal Component(ReadOnlyComponentDefinition definition, Entity containingEntity)
        {
            Guid = Guid.NewGuid();
            ContainingEntity = containingEntity;
            Definition = definition;
            InitializeAttributes();
        }

        internal void Upgrade(ReadOnlyComponentDefinition newDefinition, ComponentUpgrader upgrader)
        {
            Component newComponent = new Component(newDefinition, ContainingEntity);
            upgrader(this, newComponent);
            attributes = newComponent.attributes;
            Definition = newDefinition;
        }

        private void InitializeAttributes()
        {
            attributes = new Dictionary<string, object>();
            foreach (ReadOnlyAttributeDefinition attributeDefinition in Definition.AttributeDefinitions)
                attributes.Add(attributeDefinition.Name, attributeDefinition.DefaultValue);
        }

        private IDictionary<string, object> attributes {get; set;}
    }
}
