using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventLoopPlugin
{
    class EventLoopScriptingInterface
    {
        public int intervalMs
        {
            get
            {
                return EventLoop.IntervalMs;
            }
        }
    }
}
