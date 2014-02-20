using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScalabilityPlugin
{
    public interface IScalability
    {
        IPAddress FindResponsibleServer(Entity entity);
    }
}
