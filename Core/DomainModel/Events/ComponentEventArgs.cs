using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Event args for CreatedComponent event in Entity class and UpgradedComponent event in ComponentRegistry class.
    /// </summary>
    public class ComponentEventArgs : EventArgs
    {
        public ComponentEventArgs(Component component)
        {
            Component = component;
        }

        public Component Component { get; private set; }
    }
}
