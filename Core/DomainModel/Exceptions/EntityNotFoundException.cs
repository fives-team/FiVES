using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Raised when entity is not found in the Entitycollection.
    /// </summary>
    class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base() {}
        public EntityNotFoundException(string message) : base(message) { }
    }
}
