using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Event args for CreatedComponent event in Entity class and UpgradedComponent event in ComponentRegistry class.
    /// </summary>
    public class ComponentEventArgs : EventArgs
    {
        internal ComponentEventArgs(Component component)
        {
            Component = component;
        }

        public Component Component { get; private set; }
    }
}
