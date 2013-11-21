using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace ScalabilityPlugin
{
    class ComponentDef
    {
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
