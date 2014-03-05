using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FIVES;

namespace FIVESServiceBus
{
    public static class ServiceBus
    {
        public static IServiceBus Instance;

        public static ServiceRegistry ServiceRegistry
        {
            get
            {
                return Instance.ServiceRegistry;
            }
        }

        public static ServiceGateway ServiceGateway
        {
            get
            {
                return Instance.ServiceGateway;
            }
        }

        public static void CloseComputation(AccumulatedAttributeTransform accumulatedResult)
        {
            Instance.CloseComputation(accumulatedResult);
        }
    }
}
