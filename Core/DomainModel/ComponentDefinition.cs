// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Represents a modifiable component definition.
    /// 
    /// It can be used to define new components as following:
    /// <example>
    ///     ComponentDefinition mesh = new ComponentDefinition("mesh");
    ///     mesh.AddAttribute<string>("uri", "mesh://default");
    ///     mesh.AddAttribute<bool>("visible");
    ///     mesh.AddAttribute<Vector>("scale", new Vector(1, 1, 1));
    ///     ComponentRegistry.Instance.Register(mesh);
    /// </example>
    /// </summary>
    public sealed class ComponentDefinition : ReadOnlyComponentDefinition
    {
        /// <summary>
        /// Constructs an instance of the ComponentDefinition.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        public ComponentDefinition(string name)
            : base(name, Guid.NewGuid())
        {
        }

        /// <summary>
        /// Constructs an instance of the ComponentDefinition with specified GUID.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <param name="guid">Guid for the component.</param>
        public ComponentDefinition(string name, Guid guid)
            : base(name, guid)
        {
        }

        /// <summary>
        /// Adds a new attribute definition to the component definition. Default value for the type is used as default
        /// value for the attribute.
        /// </summary>
        /// <typeparam name="T">Type of the new attribute.</typeparam>
        /// <param name="name">Name of the new attribute.</param>
        public void AddAttribute<T>(string name)
        {
            AddAttribute(name, typeof(T), default(T));
        }

        /// <summary>
        /// Adds a new attribute definition to the component definition. Specified default value is used.
        /// </summary>
        /// <typeparam name="T">Type of the new attribute.</typeparam>
        /// <param name="name">Name of the new attribute.</param>
        /// <param name="defaultValue">Default value of the new attribute.</param>
        public void AddAttribute<T>(string name, object defaultValue)
        {
            AddAttribute(name, typeof(T), defaultValue);
        }

        /// <summary>
        /// Adds a new attribute definition to the component definition with specified default value and type.
        /// </summary>
        /// <param name="name">Name of the new attribute.</param>
        /// <param name="type">Type of the new attribute.</param>
        /// <param name="defaultValue">Default value of the new attribute.</param>
        public void AddAttribute(string name, Type type, object defaultValue)
        {
            AddAttribute(new ReadOnlyAttributeDefinition(name, type, defaultValue, Guid.NewGuid()));
        }

        /// <summary>
        /// Add a new attribute definition to the component definition.
        /// </summary>
        /// <param name="definition">Attribute definition.</param>
        public void AddAttribute(ReadOnlyAttributeDefinition definition)
        {
            if (attributeDefinitions.ContainsKey(definition.Name))
                throw new AttributeDefinitionException("Attribute with such name is already defined.");

            attributeDefinitions[definition.Name] = definition;
        }

        public override ReadOnlyCollection<ReadOnlyAttributeDefinition> AttributeDefinitions
        {
            get
            {
                return new ReadOnlyCollection<ReadOnlyAttributeDefinition>(attributeDefinitions.Values);
            }
        }

        public override ReadOnlyAttributeDefinition this[string attributeName]
        {
            get
            {
                return attributeDefinitions[attributeName];
            }
        }

        public override bool ContainsAttributeDefinition(string attributeName)
        {
            return attributeDefinitions.ContainsKey(attributeName);
        }

        // Type converted from Dictionary<string, ReadOnlyAttributeDefinition> for persistence plugin.
        private IDictionary<string, ReadOnlyAttributeDefinition> attributeDefinitions =
            new Dictionary<string, ReadOnlyAttributeDefinition>();

        // Needed by persistence plugin.
        private IDictionary<string, ReadOnlyAttributeDefinition> attributeDefinitionsHandler
        {
            get { return attributeDefinitions; }
            set { attributeDefinitions = value; }
        }

        // Needed by persistence plugin.
        internal ComponentDefinition() { }
    }
}
;