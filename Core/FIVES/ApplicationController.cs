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

        internal void WaitForTerminate()
        {
            applicationTerminated.WaitOne();
        }

        private AutoResetEvent applicationTerminated = new AutoResetEvent(false);
    }
}
