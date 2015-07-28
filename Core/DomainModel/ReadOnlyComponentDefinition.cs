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
    /// Component definition that can not be modified.
    /// </summary>
    public abstract class ReadOnlyComponentDefinition
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

        internal ReadOnlyComponentDefinition(string name, Guid guid)
        {
            Guid = guid;
            Name = name;
        }

        // Needed by persistence plugin.
        internal ReadOnlyComponentDefinition() { }
    }
}
