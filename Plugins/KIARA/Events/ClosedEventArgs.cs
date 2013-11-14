using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KIARAPlugin
{
    /// <summary>
    /// Event arguments for Close event in Connection class.
    /// </summary>
    public class ClosedEventArgs : EventArgs
    {
        public ClosedEventArgs(string reason)
        {
            Reason = reason;
        }

        /// <summary>
        /// Reason for closing the connection.
        /// </summary>
        public string Reason { get; private set; }
    }
}
