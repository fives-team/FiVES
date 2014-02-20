using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScalabilityPlugin
{
    class NoScalability : IScalability
    {
        public IPAddress FindResponsibleServer(Entity entity)
        {
            return IPAddress.Loopback;
        }
    }
}
