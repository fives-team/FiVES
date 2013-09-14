
using System;
using System.Collections.Generic;

namespace FIVES
{
    #pragma warning disable 660, 661
    // Represents an attribute layout for the component. Use as following:
    //   layout = new ComponentLayout();
    //   layout["attrA"] = AttributeType.INT;
    //   layout["attrB"] = AttributeType.FLOAT;
    //   layout["attrC"] = AttributeType.STRING;
    public class ComponentLayout
    {
        public class AttributeDefinition
        {
            public AttributeDefinition(Type type, object defaultValue)
            {
                this.Type = type;
                this.DefaultValue = defaultValue;
            }
            public Type Type { get; set; }
            public object DefaultValue { get; set; }
        }

        public ComponentLayout() {
            this.Attributes = new Dictionary<string, AttributeDefinition> ();
        }

        public AttributeDefinition this [string name]
        {
            get { return Attributes[name]; }
            set { Attributes[name] = value; }
        }

        public void AddAttribute<T>(string name, object defaultValue)
        {
            this.Attributes [name] = new AttributeDefinition(typeof(T), defaultValue);
        }

        public void AddAttribute<T>(string name)
        {
            this.Attributes [name] = new AttributeDefinition(typeof(T), default(T));
        }

        public static bool operator ==(ComponentLayout layout_1, ComponentLayout layout_2)
        {
            bool isEqual = true;
            AttributeDefinition attribute1, attribute2;
            foreach(KeyValuePair<string, AttributeDefinition> entry in layout_1.Attributes)
            {
                if (!layout_2.Attributes.ContainsKey (entry.Key))
                    return false;

                attribute1 = layout_1.Attributes [entry.Key];
                attribute2 = layout_2.Attributes [entry.Key];

                isEqual = isEqual
                    && TypeIsEqual (attribute1, attribute2)
                    && ValueIsEqual (attribute1, attribute2);
            }
            foreach(KeyValuePair<string, AttributeDefinition> entry in layout_2.Attributes)
            {
                if (!layout_1.Attributes.ContainsKey (entry.Key))
                    return false;

                attribute1 = layout_1.Attributes [entry.Key];
                attribute2 = layout_2.Attributes [entry.Key];

                isEqual = isEqual
                        && TypeIsEqual (attribute1, attribute2)
                        && ValueIsEqual (attribute1, attribute2);
            }

            return isEqual;
        }

        internal static bool TypeIsEqual (AttributeDefinition attribute1, AttributeDefinition attribute2) {
            return attribute1.Type == attribute2.Type;
        }

        internal static bool ValueIsEqual(AttributeDefinition attribute1, AttributeDefinition attribute2) {
            if (attribute1.DefaultValue == null || attribute2.DefaultValue == null)
                return attribute1.DefaultValue == attribute2.DefaultValue;
            else
                return attribute1.DefaultValue.Equals (attribute2.DefaultValue);
        }

        public static bool operator !=(ComponentLayout layout_1, ComponentLayout layout_2)
        {
            return !(layout_1 == layout_2);
        }
        // We need to access this internally to be able to iterate over the list of the attributes when constructing a 
        // new component in ComponentRegistry::createComponent.
        private Guid Guid { get; set; }
        internal IDictionary<string, AttributeDefinition> Attributes { get; set; }
    }
    #pragma warning restore 660, 661
}

