using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Raised when a component could not be upgraded.
    /// </summary>
    public class ComponentUpgradeException : Exception
    {
        public ComponentUpgradeException() : base() {}
        public ComponentUpgradeException(string message) : base(message) { }
    }
}
