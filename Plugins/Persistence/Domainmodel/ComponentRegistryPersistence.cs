// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using FIVES;
using System.Collections.Generic;

namespace PersistencePlugin
{
    public class ComponentRegistryPersistence
    {
        public static Guid Guid = new Guid("541ff74b-7154-46b3-8346-e3651b4b4140");

        public ComponentRegistryPersistence ()
        {
            this.OwnerRegisteredComponents = new Dictionary<string, ReadOnlyComponentDefinition>();
        }

        internal void RegisterPersistedComponents()
        {
            foreach (KeyValuePair<string, ReadOnlyComponentDefinition> definitionPair in this.OwnerRegisteredComponents)
            {
                ReadOnlyComponentDefinition roDefinition = definitionPair.Value;
                ComponentDefinition definition = new ComponentDefinition(roDefinition.Name);
                foreach (ReadOnlyAttributeDefinition attrDef in definition.AttributeDefinitions)
                    definition.AddAttribute(attrDef.Name, attrDef.Type, attrDef.DefaultValue);
                Registry.Register (definition);
            }
        }

        internal void GetComponentsFromRegistry()
        {
            foreach (ReadOnlyComponentDefinition definition in Registry.RegisteredComponents) {
                this.OwnerRegisteredComponents [definition.Name] = definition;
            }
        }

        private IComponentRegistry Registry = ComponentRegistry.Instance;
        private IDictionary<string, ReadOnlyComponentDefinition> OwnerRegisteredComponents { get; set; }
    }
}

