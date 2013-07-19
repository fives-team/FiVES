using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FIVES
{
    public class AttributeTypeMismatchException : System.Exception
    {
        public AttributeTypeMismatchException() : base() { }
        public AttributeTypeMismatchException(string message) : base(message) { }
    }

    public class AttributeIsNotDefinedException : System.Exception
    {
        public AttributeIsNotDefinedException() : base() { }
        public AttributeIsNotDefinedException(string message) : base(message) { }
    }

    public class Component
    {
        #region Typed Attribute Setters
        public void setIntAttribute(string name, int? value) {
            this.setAttribute (name, value, AttributeType.INT);
        }

        public void setFloatAttribute(string name, float? value) {
            this.setAttribute (name, value, AttributeType.FLOAT);
        }

        public void setStringAttribute(string name, string value) {
            this.setAttribute (name, value, AttributeType.STRING);
        }

        public void setBoolAttribute(string name, bool? value) {
            this.setAttribute (name, value, AttributeType.BOOL);
        }
        #endregion

        #region Typed Attribute Getters
        public int? getIntAttribute(string name) {
            checkAttributeExistsAndTypeMatches(name, AttributeType.INT);
            return attributes[name].value as int?;
        }

        public float? getFloatAttribute(string name) {
            checkAttributeExistsAndTypeMatches(name, AttributeType.FLOAT);
            return attributes[name].value as float?;
        }

        public string getStringAttribute(string name) {
            checkAttributeExistsAndTypeMatches(name, AttributeType.STRING);
            return attributes[name].value as string;
        }

        public bool? getBoolAttribute(string name) {
            checkAttributeExistsAndTypeMatches(name, AttributeType.BOOL);
            return attributes[name].value as bool?;
        }
        #endregion

        // Can only be constructed by ComponentRegistry to ensure correct attributes.
        internal Component (string name)
        {
            componentName = name;
        }

        // This is used to populate the attributes into a component based on it's layout.
        internal void addAttribute(string attributeName, AttributeType type) {
            // If the attribute already exists, then it's an internal error (probably in ComponentRegistry).
            Debug.Assert(!attributes.ContainsKey(attributeName));

            attributes.Add(attributeName, new Attribute(type, null));
        }

        private void checkAttributeExistsAndTypeMatches(string attributeName, AttributeType requestedType) {
            if (!attributes.ContainsKey(attributeName)) {
                throw new AttributeIsNotDefinedException(
                    "Attribute '" + attributeName + "' is not defined in the component '" + componentName + "'.");
            }

            AttributeType attributeType = attributes[attributeName].type;
            if (attributeType != requestedType) {
                throw new AttributeTypeMismatchException(
                    "Attribute '\" + attributeName + \"' has a different type in the component '" + componentName +
                    "'. Requested type is " + requestedType.ToString() + ", but attribute type is " +
                    attributeType.ToString() + ".");
            }
        }

        private void setAttribute<T>(string attributeName, T value, AttributeType type) {
            checkAttributeExistsAndTypeMatches(attributeName, type);
            attributes[attributeName].value = value;
        }

        private Dictionary<string, Attribute> attributes = new Dictionary<string, Attribute> ();
        private string componentName;
    }
}
