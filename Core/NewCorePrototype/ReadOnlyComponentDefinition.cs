using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
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
        public ReadOnlyCollection<IAttributeDefinition> AttributeDefinitions
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
        public IAttributeDefinition this[string attributeName]
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

        protected List<IAttributeDefinition> attributeDefinitions = new List<IAttributeDefinition>();
    }
}
