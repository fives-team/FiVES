using System;
using System.Collections.Generic;
using System.Diagnostics;
using Events;

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
        private Guid Id {get; set; }

        public delegate void AttributeChanged (Object sender, AttributeChangedEventArgs e);
        public event AttributeChanged OnAttributeChanged;

        #region Typed Attribute Setters
        public void setIntAttribute(string attributeName, int? value) {
            this.setAttribute (attributeName, value, AttributeType.INT);
        }

        public void setFloatAttribute(string attributeName, float? value) {
            this.setAttribute (attributeName, value, AttributeType.FLOAT);
        }

        public void setStringAttribute(string attributeName, string value) {
            this.setAttribute (attributeName, value, AttributeType.STRING);
        }

        public void setBoolAttribute(string attributeName, bool? value) {
            this.setAttribute (attributeName, value, AttributeType.BOOL);
        }
        #endregion

        #region Typed Attribute Getters
        public int? getIntAttribute(string attributeName) {
            checkAttributeExistsAndTypeMatches(attributeName, AttributeType.INT);
            return attributes[attributeName].value as int?;
        }

        public float? getFloatAttribute(string attributeName) {
            checkAttributeExistsAndTypeMatches(attributeName, AttributeType.FLOAT);
            return attributes[attributeName].value as float?;
        }

        public string getStringAttribute(string attributeName) {
            checkAttributeExistsAndTypeMatches(attributeName, AttributeType.STRING);
            return attributes[attributeName].value as string;
        }

        public bool? getBoolAttribute(string attributeName) {
            checkAttributeExistsAndTypeMatches(attributeName, AttributeType.BOOL);
            return attributes[attributeName].value as bool?;
        }

        #endregion
        internal Component() {}

        // Can only be constructed by ComponentRegistry.createComponent to ensure correct attributes.
        internal Component (string name)
        {
            componentName = name;
            this.attributes = new Dictionary<string, Attribute> ();
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
            if (this.OnAttributeChanged != null) {
                this.OnAttributeChanged(this, new AttributeChangedEventArgs(attributeName, value));
            }
        }

        private IDictionary<string, Attribute> attributes {get ; set;}
        private string componentName;
    }
}
