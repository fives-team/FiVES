using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketJSON
{
    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}
