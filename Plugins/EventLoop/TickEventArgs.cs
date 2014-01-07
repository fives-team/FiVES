using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLoopPlugin
{
    /// <summary>
    /// EventArgs that are used by the TickFired-Event of the eventloop provided by the EventLoop-Plugin. The EventArgs contain the duration of the last tick
    /// and the time at which the event was fired relative to the start of the server.
    /// </summary>
    public class TickEventArgs : EventArgs
    {
        public TickEventArgs(TimeSpan tickDuration, TimeSpan timestamp)
        {
            TickDuration = tickDuration;
            TimeStamp = timestamp;
        }
        public TimeSpan TimeStamp { get; private set; }
        public TimeSpan TickDuration { get; private set; }
    }
}
