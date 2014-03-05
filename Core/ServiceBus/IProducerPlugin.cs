using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace ServiceBusPlugin
{
    public interface IProducerPluginInitializer : IPluginInitializer
    {
        void Produce(Entity entity);
    }
}
