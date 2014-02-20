using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TestingPlugin;

namespace WebTests
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class TestingService : ITestingService
    {
        public event EventHandler ServerStarted;

        public void NotifyServerStarted()
        {
            if (ServerStarted != null)
                ServerStarted(this, new EventArgs());
        }
    }
}
