using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Event arguments for ChangedAttribute event found in IEntity, IComponent and IAttribute interfaces.
    /// </summary>
    public class ChangedAttributeEventArgs : EventArgs
    {
        internal ChangedAttributeEventArgs(IComponent component, string attributeName, object oldValue, object newValue)
        {
            Component = component;
            AttributeName = attributeName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public IComponent Component { get; private set;  }
        public string AttributeName { get; private set; }
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }
    }
}
