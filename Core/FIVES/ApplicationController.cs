using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FIVES
{
    public class ApplicationController
    {
        public void Terminate()
        {
            applicationTerminated.Set();
        }

        public bool ControlTaken { get; set; }

        public event EventHandler PluginsLoaded;

        internal void WaitForTerminate()
        {
            applicationTerminated.WaitOne();
        }

        internal void NotifyPluginsLoaded()
        {
            if (PluginsLoaded != null)
                PluginsLoaded(this, new EventArgs());
        }

        private AutoResetEvent applicationTerminated = new AutoResetEvent(false);
    }
}
