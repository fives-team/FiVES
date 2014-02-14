using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketJSON
{
    /// <summary>
    /// An exception that is thrown when the number of passed arguments does not match the number of arguments in the
    /// registered call handler.
    /// </summary>
    public class InvalidNumberOfArgs : Exception
    {
        public InvalidNumberOfArgs() : base() { }
        public InvalidNumberOfArgs(string message) : base(message) { }
    }
}
