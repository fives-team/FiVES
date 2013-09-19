using System;

namespace Events
{
    public class AttributeChangedEventArgs : EventArgs
    {
        public AttributeChangedEventArgs (string attributeName, object value)
        {
            this.AttributeName = attributeName;
            this.NewValue = value;
        }

        public string AttributeName { get; private set; }
        public object NewValue { get; private set; }
    }
}

