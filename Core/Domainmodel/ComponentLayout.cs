
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
                this.type = type;
                this.defaultValue = defaultValue;
            }
            public Type type { get; set; }
            public object defaultValue { get; set; }
        }

        public ComponentLayout() {
            this.attributes = new Dictionary<string, AttributeDefinition> ();
        }

        public AttributeDefinition this [string name]
        {
            get { return attributes[name]; }
            set { attributes[name] = value; }
        }

        public void addAttribute<T>(string name, object defaultValue)
        {
            this.attributes [name] = new AttributeDefinition(typeof(T), defaultValue);
        }

        public void addAttribute<T>(string name)
        {
            this.attributes [name] = new AttributeDefinition(typeof(T), default(T));
        }

        public static bool operator ==(ComponentLayout layout_1, ComponentLayout layout_2)
        {
            bool isEqual = true;
            AttributeDefinition attribute1, attribute2;
            foreach(KeyValuePair<string, AttributeDefinition> entry in layout_1.attributes)
            {
                if (!layout_2.attributes.ContainsKey (entry.Key))
                    return false;

                attribute1 = layout_1.attributes [entry.Key];
                attribute2 = layout_2.attributes [entry.Key];

                isEqual = isEqual
                    && typeIsEqual (attribute1, attribute2)
                    && valueIsEqual (attribute1, attribute2);
            }
            foreach(KeyValuePair<string, AttributeDefinition> entry in layout_2.attributes)
            {
                if (!layout_1.attributes.ContainsKey (entry.Key))
                    return false;

                attribute1 = layout_1.attributes [entry.Key];
                attribute2 = layout_2.attributes [entry.Key];

                isEqual = isEqual
                        && typeIsEqual (attribute1, attribute2)
                        && valueIsEqual (attribute1, attribute2);
            }

            return isEqual;
        }

        internal static bool typeIsEqual (AttributeDefinition attribute1, AttributeDefinition attribute2) {
            return attribute1.type == attribute2.type;
        }

        internal static bool valueIsEqual(AttributeDefinition attribute1, AttributeDefinition attribute2) {
            if (attribute1.defaultValue == null || attribute2.defaultValue == null)
                return attribute1.defaultValue == attribute2.defaultValue;
            else
                return attribute1.defaultValue.Equals (attribute2.defaultValue);
        }

        public static bool operator !=(ComponentLayout layout_1, ComponentLayout layout_2)
        {
            return !(layout_1 == layout_2);
        }
        // We need to access this internally to be able to iterate over the list of the attributes when constructing a 
        // new component in ComponentRegistry::createComponent.
        private Guid Id { get; set; }
        internal IDictionary<string, AttributeDefinition> attributes { get; set; }
    }
    #pragma warning restore 660, 661
}

