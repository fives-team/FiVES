using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace ServiceBusPlugin
{
    public interface IConsumerPlugin : IPluginInitializer
    {
        void Consume(Object sender, AccumulatedAttributeTransform attributeTransformations);
    }
}
