using System;
using System.Collections.Generic;
using System.Diagnostics;
using Events;
using System.Dynamic;
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

    public class Component
    {
        public Guid Id {get; set; }

        public delegate void AttributeChanged (Object sender, AttributeChangedEventArgs e);
        public event AttributeChanged OnAttributeChanged;

        public void SubscribeToAttributeChanged(AttributeChanged eventHandler)
        {
            this.OnAttributeChanged += eventHandler;
        }

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
                if (CheckAttributeExistsAndTypeMatches(attributeName, value.GetType()))
                {
                    this.Attributes[attributeName].Value = value;
                    if (this.OnAttributeChanged != null)
                        this.OnAttributeChanged(this, new AttributeChangedEventArgs(attributeName, value));
                }
            }
        }

        public int Version { get; internal set; }

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

        private IDictionary<string, Attribute> Attributes {get ; set;}
        private string ComponentName;
    }
}
