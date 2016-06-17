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
using System.Linq;
using System.Text;
using FIVES;

namespace ServerSyncPlugin
{
    class ComponentDef
    {
        /// <summary>
        /// Converts the ReadOnlyComponentDefinition object to the ComponentDef object.
        /// </summary>
        /// <param name="originalDefinition">The ReadOnlyComponentDefinition object.</param>
        /// <returns>The converted ComponentDef object.</returns>
        public static explicit operator ComponentDef(ReadOnlyComponentDefinition originalDefinition)
        {
            var componentDef = new ComponentDef();
            componentDef.Guid = originalDefinition.Guid.ToString();
            componentDef.Name = originalDefinition.Name;
            foreach (ReadOnlyAttributeDefinition attributeDefinition in originalDefinition.AttributeDefinitions)
            {
                var attributeDef = new AttributeDef();
                attributeDef.Guid = attributeDefinition.Guid.ToString();
                attributeDef.Name = attributeDefinition.Name;
                attributeDef.Type = attributeDefinition.Type.AssemblyQualifiedName;
                attributeDef.DefaultValue = attributeDefinition.DefaultValue;
                componentDef.AttributeDefs.Add(attributeDef);
            }
            return componentDef;
        }

        /// <summary>
        /// Converts the ComponentDef object to the ComponentDefinition object.
        /// </summary>
        /// <param name="componentDef">The ComponentDef object.</param>
        /// <returns>The converted ComponentDefinition object.</returns>
        public static explicit operator ComponentDefinition(ComponentDef componentDef)
        {
            ComponentDefinition newComponent = new ComponentDefinition(componentDef.Name, new Guid(componentDef.Guid));
            foreach (AttributeDef attributeDef in componentDef.AttributeDefs)
            {
                Type type = Type.GetType(attributeDef.Type);
                object defaultValue = Convert.ChangeType(attributeDef.DefaultValue, type);
                newComponent.AddAttribute(new ReadOnlyAttributeDefinition(attributeDef.Name, type, defaultValue,
                                                                          new Guid(attributeDef.Guid)));
            }
            return newComponent;
        }

        public string Guid;
        public string Name;
        public List<AttributeDef> AttributeDefs = new List<AttributeDef>();
    }
}
