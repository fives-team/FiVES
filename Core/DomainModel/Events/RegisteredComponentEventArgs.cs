using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Event args for RegistredComponent event in ComponentRegistry class.
    /// </summary>
    public class RegisteredComponentEventArgs : EventArgs
    {
        public RegisteredComponentEventArgs(ComponentDefinition definition)
        {
             ComponentDefinition = definition;
        }

        public ComponentDefinition ComponentDefinition { get; private set; }
    }
}
