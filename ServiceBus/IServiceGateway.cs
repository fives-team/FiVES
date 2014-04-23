using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public interface IServiceGateway
    {
        event EventHandler<ProposeAttributeChangeEventArgs> PublishedTransformation;
        event EventHandler<AccumulatedAttributeTransform> ReceivedResult;

        void PublishTransformation(ProposeAttributeChangeEventArgs transform);
        void PublishResult(AccumulatedAttributeTransform transformResult);
    }
}
