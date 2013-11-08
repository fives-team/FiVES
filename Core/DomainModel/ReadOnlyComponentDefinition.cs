using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FIVES
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
                return new ReadOnlyCollection<ReadOnlyAttributeDefinition>(attributeDefinitions.Values);
            }
        }

        /// <summary>
        /// Returns a attribute definition by its name or throws KeyNotFoundException if the attribute is not defined.
        /// </summary>
        /// <param name="attributeName">Attribute name.</param>
        /// <returns>Attribute definition.</returns>
        public ReadOnlyAttributeDefinition this[string attributeName]
        {
            get
            {
                return attributeDefinitions[attributeName];
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
            if (attributeDefinitions.ContainsKey(name))
                throw new AttributeDefinitionException("Attribute with such name is already defined.");

            attributeDefinitions[name] = new ReadOnlyAttributeDefinition(name, type, defaultValue);
        }

        private Dictionary<string, ReadOnlyAttributeDefinition> attributeDefinitions =
            new Dictionary<string, ReadOnlyAttributeDefinition>();
    }
}
