using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScalabilityPlugin
{
    /// <summary>
    /// Interface that a concrete scalability class should implement.
    /// </summary>
    public interface IScalability
    {
        /// <summary>
        /// Searches for the server responsible for a given entity and returns its IP address.
        /// </summary>
        /// <param name="entity">A given entity.</param>
        /// <returns>Responsible server's IP address.</returns>
        IPAddress FindResponsibleServer(Entity entity);
    }
}
