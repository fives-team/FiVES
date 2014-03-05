using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace ServiceBusPlugin
{
    public interface IServiceBus
    {
        ServiceGateway ServiceGateway { get; }
        ServiceRegistry ServiceRegistry { get; }

        void Initialize();
        void CloseComputation(AccumulatedAttributeTransform accumulatedResult);
    }
}
