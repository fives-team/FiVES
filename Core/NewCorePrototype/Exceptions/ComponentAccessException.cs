using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Raised when a component could not be accessed.
    /// </summary>
    public class ComponentAccessException : Exception
    {
        public ComponentAccessException() : base() {}
        public ComponentAccessException(string message) : base(message) { }
    }
}
