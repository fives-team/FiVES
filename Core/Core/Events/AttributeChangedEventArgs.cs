using System;

namespace Events
{
    public class AttributeChangedEventArgs : EventArgs
    {
        public AttributeChangedEventArgs (string attributeName, Guid attributeGuid, object value)
        {
            this.AttributeName = attributeName;
            this.NewValue = value;
            this.AttributeGuid = attributeGuid;
        }

        public string AttributeName { get; private set; }
        public Guid AttributeGuid { get; private set; }
        public object NewValue { get; private set; }
    }
}

