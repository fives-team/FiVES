using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventLoopPlugin
{
    class EventLoopScriptingInterface
    {
        public void addTickFiredHandler(Delegate callback)
        {
            EventLoop.TickFired += delegate(object sender, TickEventArgs args)
            {
                callback.DynamicInvoke(args.TimeStamp);
            };
        }

        // TODO: removeTickFiredHandler

        public int intervalMs
        {
            get
            {
                return EventLoop.IntervalMs;
            }
        }
    }
}
