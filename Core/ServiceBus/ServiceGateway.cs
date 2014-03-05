using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace ServiceBusPlugin
{
    public class ServiceGateway : IServiceGateway
    {
        public event EventHandler<ChangedAttributeEventArgs> PublishedTransformation;

        public event EventHandler<AccumulatedAttributeTransform> ReceivedResult;

        public void PublishTransformation(ChangedAttributeEventArgs transform)
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
