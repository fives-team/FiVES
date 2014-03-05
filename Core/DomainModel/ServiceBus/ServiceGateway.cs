using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace FIVESServiceBus
{
    public class ServiceGateway : IServiceGateway
    {
        public event EventHandler<AttributeChangeEventArgs> PublishedTransformation;

        public event EventHandler<AccumulatedAttributeTransform> ReceivedResult;

        public void PublishTransformation(AttributeChangeEventArgs transform)
        {
            if (PublishedTransformation != null)
                PublishedTransformation(this, transform);
        }

        public void PublishResult(AccumulatedAttributeTransform transformResult)
        {
            if (ReceivedResult != null)
                ReceivedResult(this, transformResult);
        }
    }
}
