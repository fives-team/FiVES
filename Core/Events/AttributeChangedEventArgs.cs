using System;

namespace Events
{
    public class AttributeChangedEventArgs
    {
        public AttributeChangedEventArgs (string attributeName, object value)
        {
            this.attributeName = attributeName;
            this.value = value;
        }

        public string attributeName { get; private set; }
        public object value { get; private set; }
    }
}

