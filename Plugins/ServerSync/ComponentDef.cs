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
            componentDef.Guid = originalDefinition.Guid;
            componentDef.Name = originalDefinition.Name;
            foreach (ReadOnlyAttributeDefinition attributeDefinition in originalDefinition.AttributeDefinitions)
            {
                var attributeDef = new AttributeDef();
                attributeDef.Guid = attributeDefinition.Guid;
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
            ComponentDefinition newComponent = new ComponentDefinition(componentDef.Name, componentDef.Guid);
            foreach (AttributeDef attributeDef in componentDef.AttributeDefs)
            {
                Type type = Type.GetType(attributeDef.Type);
                object defaultValue = Convert.ChangeType(attributeDef.DefaultValue, type);
                newComponent.AddAttribute(new ReadOnlyAttributeDefinition(attributeDef.Name, type, defaultValue,
                                                                          attributeDef.Guid));
            }
            return newComponent;
        }

        public Guid Guid;
        public string Name;
        public List<AttributeDef> AttributeDefs = new List<AttributeDef>();
    }
}
