using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public interface IServiceGateway
    {
        event EventHandler<AttributeChangeEventArgs> PublishedTransformation;
        event EventHandler<AccumulatedAttributeTransform> ReceivedResult;

        void PublishTransformation(AttributeChangeEventArgs transform);
        void PublishResult(AccumulatedAttributeTransform transformResult);
    }
}
