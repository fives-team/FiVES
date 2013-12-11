using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventloopPlugin
{
    class TickEventArgs : EventArgs
    {
        public TickEventArgs(TimeSpan timespan)
        {
            Timespan = timespan;
        }
        public TimeSpan Timespan { get; private set; }
    }
}
