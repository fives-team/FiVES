using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public interface IServiceRegistry
    {
        void RegisterService(string serviceName, Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform> transformFunction);

        Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform> Lookup(string serviceName);
    }
}
