using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Event args for ComponentCreated events in IEntity interface and Upgraded events in IComponent.
    /// </summary>
    public class ComponentEventArgs : EventArgs
    {
        internal ComponentEventArgs(IComponent component)
        {
            Component = component;
        }

        public IComponent Component { get; private set; }
    }
}
