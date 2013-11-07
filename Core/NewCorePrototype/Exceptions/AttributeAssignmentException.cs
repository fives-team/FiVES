using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewCorePrototype
{
    /// <summary>
    /// Raised when an attribute could not be assigned a value.
    /// </summary>
    public class AttributeAssignmentException : Exception
    {
        public AttributeAssignmentException() : base() {}
        public AttributeAssignmentException(string message) : base(message) { }
    }
}
