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

        public event EventHandler ServerStarted;

        internal void WaitForTerminate()
        {
            if (ServerStarted != null)
                ServerStarted(this, new EventArgs());

            applicationTerminated.WaitOne();
        }

        private AutoResetEvent applicationTerminated = new AutoResetEvent(false);
    }
}
