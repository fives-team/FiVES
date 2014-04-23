using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public class ServiceRegistry : IServiceRegistry
    {
        public void RegisterService(string serviceName, Func<AccumulatedAttributeTransform,
            AccumulatedAttributeTransform> transformFunction)
        {
            if(!registeredServiceFunctions.ContainsKey(serviceName))
                registeredServiceFunctions.Add(serviceName, transformFunction);
            else
                throw new Exception("Service with name '" + serviceName + "' is already registered");
        }

        public Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform> Lookup(string serviceName)
        {
            if (registeredServiceFunctions.ContainsKey(serviceName))
                return registeredServiceFunctions[serviceName];
            else
                throw new Exception("Service with name '" + serviceName + "' is not registered");
        }

        IDictionary<string,
            Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform>> registeredServiceFunctions
            = new Dictionary<string, Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform>>();
    }
}
