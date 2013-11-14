using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Event arguments for ChangedAttribute event in Entity and Component classes.
    /// </summary>
    public class ChangedAttributeEventArgs : EventArgs
    {
        public ChangedAttributeEventArgs(Component component, string attributeName, object oldValue, object newValue)
        {
            Component = component;
            AttributeName = attributeName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public Component Component { get; private set;  }
        public string AttributeName { get; private set; }
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }
    }
}
