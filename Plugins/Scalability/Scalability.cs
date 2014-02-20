using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScalabilityPlugin
{
    public static class Scalability
    {
        public static IScalability Instance;

        public static IPAddress FindResponsibleServer(Entity entity)
        {
            return Instance.FindResponsibleServer(entity);
        }
    }
}
