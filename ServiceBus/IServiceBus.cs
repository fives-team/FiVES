using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public interface IServiceBus
    {        
        ServiceRegistry ServiceRegistry { get; }
        event EventHandler<AccumulatedAttributeTransform> ComputedResult;

        void Initialize();
        void IntroduceTopic(string topic, string services);
        void CloseComputation(AccumulatedAttributeTransform accumulatedResult);
    }
}
