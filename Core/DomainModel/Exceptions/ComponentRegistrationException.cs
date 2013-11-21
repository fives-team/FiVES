using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Raised when a component could not be registered.
    /// </summary>
    public class ComponentRegistrationException : Exception
    {
        public ComponentRegistrationException() : base() {}
        public ComponentRegistrationException(string message) : base(message) {}
    }
}
