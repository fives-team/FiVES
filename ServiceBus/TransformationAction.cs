using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    class TransformationAction
    {

        public TransformationAction(string transformName)
        {
            TransformationFunction = ServiceBus.ServiceRegistry.Lookup(transformName);
        }

        public void Execute(AccumulatedAttributeTransform accumulatedTransformations)
        {
            var transformationResult = TransformationFunction(accumulatedTransformations);

            if (HasNext)
            {
                next.Execute(transformationResult);
            }
            else
            {
                ServiceBus.CloseComputation(transformationResult);
            }
        }

        public TransformationAction Next
        {
            get { return Next; }
            internal set { next = value; HasNext = true; }
        }

        bool HasNext = false;
        Func<AccumulatedAttributeTransform, AccumulatedAttributeTransform> TransformationFunction;
        TransformationAction next;
    }
}
