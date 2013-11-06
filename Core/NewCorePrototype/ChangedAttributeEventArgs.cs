using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Event arguments for ChangedAttribute event found in IEntity, IComponent and IAttribute.
    /// </summary>
    public class ChangedAttributeEventArgs : EventArgs
    {
        public string ComponentName { get; }
        public string AttributeName { get; }
        public object OldValue { get; }
        public object NewValue { get; }
    }
}
