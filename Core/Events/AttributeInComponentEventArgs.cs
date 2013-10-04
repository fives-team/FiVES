using System;

namespace Events
{
    public class AttributeInComponentEventArgs : EventArgs
    {
        public AttributeInComponentEventArgs (string componentName, string attributeName, object newValue)           
        {
            this.componentName = componentName;
            this.attributeName = attributeName;
            this.newValue = newValue;
        }

        public string componentName;
        public string attributeName;
        public object newValue;
    }
}

