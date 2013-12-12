using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventloopPlugin
{
    /// <summary>
    /// EventArgs that are used by the TickFired-Event of the eventloop provided by the EventLoop-Plugin. The EventArgs contain the time at which the
    /// event was fired relative to the start of the server.
    /// </summary>
    class TickEventArgs : EventArgs
    {
        public TickEventArgs(TimeSpan timestamp)
        {
            TimeStamp = timestamp;
        }
        public TimeSpan TimeStamp { get; private set; }
    }
}
