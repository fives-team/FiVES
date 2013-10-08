using System;

namespace Events
{
    public class AttributeInComponentEventArgs : EventArgs
    {
        public AttributeInComponentEventArgs (string componentName, string attributeName, Guid attributeGuid, object newValue)
        {
            this.componentName = componentName;
            this.attributeName = attributeName;
            this.newValue = newValue;
            this.AttributeGuid = attributeGuid;
        }

        public string componentName {get; private set;}
        public string attributeName { get; private set; }
        public object newValue { get; private set; }
        public Guid AttributeGuid { get; private set; }
    }
}

