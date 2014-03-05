using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace ServiceBusPlugin
{
    public interface ITransformerPlugin : IPluginInitializer
    {
        AccumulatedAttributeTransform Transform(AccumulatedAttributeTransform accumulatedTransformations);
        void Register();
    }
}
