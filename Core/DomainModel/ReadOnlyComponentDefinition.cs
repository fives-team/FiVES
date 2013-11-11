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
    public abstract class ReadOnlyComponentDefinition
    {
        /// <summary>
        /// Constructs an instance of the ReadOnlyComponentDefinition.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <param name="version">Version of the definition.</param>
        public ReadOnlyComponentDefinition(string name, int version)
        {
            Guid = Guid.NewGuid();
            Name = name;
            Version = version;
        }

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
        public abstract ReadOnlyCollection<ReadOnlyAttributeDefinition> AttributeDefinitions { get; }

        /// <summary>
        /// Returns a attribute definition by its name or throws KeyNotFoundException if the attribute is not defined.
        /// </summary>
        /// <param name="attributeName">Attribute name.</param>
        /// <returns>Attribute definition.</returns>
        public abstract ReadOnlyAttributeDefinition this[string attributeName] { get; }

        /// <summary>
        /// Verifies whether this component definition contains a definition for an attribute with a given name.
        /// </summary>
        /// <param name="attributeName">Attribute name.</param>
        /// <returns>True if definition for such attribute is present, false otherwise.</returns>
        public abstract bool ContainsAttributeDefinition(string attributeName);
    }
}
