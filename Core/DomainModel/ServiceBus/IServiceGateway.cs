using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public interface IServiceGateway
    {
        event EventHandler<ChangedAttributeEventArgs> PublishedTransformation;
        event EventHandler<AccumulatedAttributeTransform> ReceivedResult;

        void PublishTransformation(ChangedAttributeEventArgs transform);
        void PublishResult(AccumulatedAttributeTransform transformResult);
    }
}
