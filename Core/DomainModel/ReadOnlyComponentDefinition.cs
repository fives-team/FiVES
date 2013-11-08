using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Component definition that can not be modified.
    /// </summary>
    public class ReadOnlyComponentDefinition
    {
        /// <summary>
        /// GUID that uniquely identifies this component definition.
        /// </summary>
        public Guid Guid { get; private set; }
        
        /// <summary>
        /// Name of the component.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Version of the component.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// A collection of attribute definitions.
        /// </summary>
        public ReadOnlyCollection<ReadOnlyAttributeDefinition> AttributeDefinitions
        {
            get
            {
                return attributeDefinitions.AsReadOnly();
            }
        }

        /// <summary>
        /// Returns a attribute definition by its name or null if it is not defined.
        /// </summary>
        /// <param name="attributeName">Attribute name.</param>
        /// <returns>Attribute definition.</returns>
        public ReadOnlyAttributeDefinition this[string attributeName]
        {
            get
            {
                return attributeDefinitions.Find(d => d.Name == attributeName);
            }
        }

        internal ReadOnlyComponentDefinition(string name, int version)
        {
            Guid = Guid.NewGuid();
            Name = name;
            Version = version;
        }

        protected void AddAttributeDefinition(string name, Type type, object defaultValue)
        {
            if (attributeDefinitions.Find(d => d.Name == name) != null)
                throw new AttributeDefinitionException("Attribute with such name is already defined.");

            attributeDefinitions.Add(new ReadOnlyAttributeDefinition(name, type, defaultValue));
        }

        private List<ReadOnlyAttributeDefinition> attributeDefinitions = new List<ReadOnlyAttributeDefinition>();
    }
}
