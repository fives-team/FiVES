using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventLoopPlugin
{
    /// <summary>
    /// EventLoop plugin interface.
    /// </summary>
    public interface IEventLoop
    {
        /// <summary>
        /// Event that is fired each time a tick is triggered.
        /// </summary>
        event EventHandler<TickEventArgs> TickFired;

        /// <summary>
        /// Returns the interface in milliseconds at which the tick is triggered.
        /// </summary>
        int IntervalMs { get; }
    }
}
