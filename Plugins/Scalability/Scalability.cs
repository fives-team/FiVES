using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScalabilityPlugin
{
    /// <summary>
    /// Class that provides shortcuts to the methods of the class implementing IScalability interface.
    /// </summary>
    public static class Scalability
    {
        public static IScalability Instance;

        public static IPAddress FindResponsibleServer(Entity entity)
        {
            return Instance.FindResponsibleServer(entity);
        }
    }
}
