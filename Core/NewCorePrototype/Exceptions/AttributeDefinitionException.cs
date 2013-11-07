using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Raised when an attribute could not be added to the component definition.
    /// </summary>
    public class AttributeDefinitionException : Exception
    {
        public AttributeDefinitionException() : base() {}
        public AttributeDefinitionException(string message) : base(message) { }
    }
}
