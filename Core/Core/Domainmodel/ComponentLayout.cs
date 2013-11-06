
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

        /// <summary>
        /// Adds the definition of a new Attribute of given type and with set default value to the componentlayout
        /// </summary>
        /// <param name="name">Name of the new Attribute.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <typeparam name="T">Type of the attribute.</typeparam>
        public void AddAttribute<T>(string name, object defaultValue)
        {
            // Fail if default value is null and type T is not nullable or if default value has different type than T.
            bool canBeNull = !typeof(T).IsValueType || (Nullable.GetUnderlyingType(typeof(T)) != null);
            if ((defaultValue == null && !canBeNull) || (defaultValue != null && defaultValue.GetType() != typeof(T)))
                throw new ArgumentException("Invalid default value type.");

            this.Attributes [name] = new AttributeDefinition(typeof(T), defaultValue);
        }

        /// <summary>
        /// Adds the definition of a new attribute without default value to the componentLayout.
        /// </summary>
        /// <param name="name">Name of the new Attribute.</param>
        /// <typeparam name="T">Type of the Attribute.</typeparam>
        public void AddAttribute<T>(string name)
        {
            this.Attributes [name] = new AttributeDefinition(typeof(T), default(T));
        }

        /// <summary>
        /// Compares equality of two ComponentLayouts. Layouts are equal, when they both contain the same AttributeDefinitions
        /// with equal names, types and default values.
        /// </summary>
        /// <param name="layout_1">Layout_1.</param>
        /// <param name="layout_2">Layout_2.</param>
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

        /// <summary>
        /// Checks the type of two AttributeDefinitions for Equality
        /// </summary>
        /// <returns><c>true</c>, if type is equal, <c>false</c> otherwise.</returns>
        /// <param name="attribute1">Attribute1.</param>
        /// <param name="attribute2">Attribute2.</param>
        internal static bool TypeIsEqual (AttributeDefinition attribute1, AttributeDefinition attribute2) {
            return attribute1.Type == attribute2.Type;
        }

        /// <summary>
        /// Checks the default value of two AttributeDefinitions for Equality
        /// </summary>
        /// <returns><c>true</c>, if default value is equal, <c>false</c> otherwise.</returns>
        /// <param name="attribute1">Attribute1.</param>
        /// <param name="attribute2">Attribute2.</param>
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

