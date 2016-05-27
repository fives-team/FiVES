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
        public Attribute this[string attributeName]
        {
            get
            {
                if (!Definition.ContainsAttributeDefinition(attributeName))
                    throw new KeyNotFoundException("Attribute is not present in the component definition.");

                return attributes[attributeName];
            }
        }

        /// <summary>
        /// An event that is raised when any attribute of this component is changed.
        /// </summary>
        public event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;

        internal void raiseChangeEvent(string attributeName, object oldValue, object newValue)
        {
            if (ChangedAttribute != null)
            {
                if ((oldValue == null && newValue != null) || (oldValue != null && !oldValue.Equals(newValue)))
                    ChangedAttribute(this, new ChangedAttributeEventArgs(this, attributeName, oldValue, newValue));
            }
        }

        internal void raiseChangeEventFromInternalChange(string attributeName, object newValue)
        {
            if (ChangedAttribute != null)
            {
                ChangedAttribute(this, new ChangedAttributeEventArgs(this, attributeName, newValue, newValue));
            }
        }

        internal Component(ReadOnlyComponentDefinition definition, Entity containingEntity)
        {
            Guid = Guid.NewGuid();
            ContainingEntity = containingEntity;
            Definition = definition;
            InitializeAttributes();
        }

        private void InitializeAttributes()
        {
            attributes = new Dictionary<string, Attribute>();
            foreach (ReadOnlyAttributeDefinition attributeDefinition in Definition.AttributeDefinitions)
            {
                Attribute attribute = new Attribute(attributeDefinition, this);
                attributes.Add(attributeDefinition.Name, attribute);
            }
        }

        private IDictionary<string, Attribute> attributes { get; set; }

        // Needed by persistence plugin.
        internal Component() { }
    }
}
