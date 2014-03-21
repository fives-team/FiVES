using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventLoopPlugin
{
    /// <summary>
    /// Implements a simple event loop to which plugins can register. The Loop fires its event in a fixed interval that can be specified
    /// in the config file. The EventArgs with which the event is invoked contain the time in milliseconds since the application is running
    /// </summary>
    public class EventLoop
    {
        public static IEventLoop Instance;

        /// <summary>
        /// Event that is fired each time a tick is triggered.
        /// </summary>
        public static event EventHandler<TickEventArgs> TickFired
        {
            add
            {
                Instance.TickFired += value;
            }
            remove
            {
                Instance.TickFired -= value;
            }
        }
    }
}
