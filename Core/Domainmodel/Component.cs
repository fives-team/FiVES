using System;
using System.Collections.Generic;
using System.Diagnostics;
using Events;
using Microsoft.CSharp.RuntimeBinder;

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

    /// <summary>
    /// Component. Contains a set of typed Attributes. Components of attributes and attributes of components are
    /// accessed via the [] operator. Components are registered via the <see cref="ComponentRegistry"/>
    /// at hand of a previously created <see cref="ComponentLayout"/>
    /// </summary>
    public class Component
    {
        public Guid Guid { get; set; }

        public delegate void AttributeChanged (Object sender, AttributeChangedEventArgs e);
        public event AttributeChanged OnAttributeChanged;

        public object this[string attributeName]
        {
            get
            {
                if (!Attributes.ContainsKey(attributeName))
                {
                    throw new AttributeIsNotDefinedException(
                        "Attribute '" + attributeName + "' is not defined in the component '" + ComponentName + "'.");
                }

                return Attributes[attributeName].Value;
            }
            set
            {
                if (CheckAttributeExistsAndTypeMatches(attributeName, value.GetType()) && CheckIfAttributeChanged(attributeName, value))
                {
                    this.Attributes[attributeName].Value = value;
                    if (this.OnAttributeChanged != null)
                        this.OnAttributeChanged(this, new AttributeChangedEventArgs(attributeName, value));
                }
            }
        }

        public int Version { get; internal set; }

        /// <summary>
        /// Resets the attribute value to default.
        /// </summary>
        /// <param name="attributeName">Attribute name.</param>
        public void ResetAttributeValue(string attributeName)
        {
            if (!Attributes.ContainsKey(attributeName))
                throw new AttributeIsNotDefinedException(
                    "Attribute '" + attributeName + "' is not defined in the component '" + ComponentName + "'.");

            var layout = ComponentRegistry.Instance.GetComponentLayout(ComponentName);
            this[attributeName] = layout[attributeName].DefaultValue;
        }

        internal Component() {}

        // Can only be constructed by ComponentRegistry.createComponent to ensure correct attributes.
        internal Component (string name)
        {
            ComponentName = name;
            this.Attributes = new Dictionary<string, Attribute> ();
        }

        // This is used to populate the attributes into a component based on it's layout.
        internal void AddAttribute(string attributeName, Type type, object defaultValue) {
            // If the attribute already exists, then it's an internal error (probably in ComponentRegistry).
            Debug.Assert(!Attributes.ContainsKey(attributeName));

            Attributes.Add(attributeName, new Attribute(type, defaultValue));
        }

        /// <summary>
        /// Checks the attribute exists and type matches when trying to access an attribute of the component
        /// </summary>
        /// <returns><c>true</c>, if attribute exists and type matches, <c>false</c> otherwise.</returns>
        /// <param name="attributeName">Attribute name.</param>
        /// <param name="requestedType">Requested type.</param>
        private bool CheckAttributeExistsAndTypeMatches(string attributeName, Type requestedType) {
            if (!Attributes.ContainsKey(attributeName)) {
                throw new AttributeIsNotDefinedException(
                    "Attribute '" + attributeName + "' is not defined in the component '" + ComponentName + "'.");
            }

            Type attributeType = Attributes[attributeName].Type;
            if (attributeType != requestedType) {
                throw new RuntimeBinderException(
                    "Attribute '\" + attributeName + \"' has a different type in the component '" + ComponentName +
                    "'. Requested type is " + requestedType.ToString() + ", but attribute type is " +
                    attributeType.ToString() + ".");
            }

            return true;
        }

        private bool CheckIfAttributeChanged(string attributeName, object newValue) {
            Attribute attribute = this.Attributes[attributeName];
            if (attribute.Value == null) // Setting a null attribute will always change value
                return true;

            return !attribute.Value.Equals(newValue);
        }
        /// <summary>
        /// Attributes of the component. Stored by their names
        /// </summary>
        /// <value>The attributes.</value>
        private IDictionary<string, Attribute> Attributes {get ; set;}

        /// <summary>
        /// Name under which a Component can be accessed
        /// </summary>
        private string ComponentName;
    }
}
