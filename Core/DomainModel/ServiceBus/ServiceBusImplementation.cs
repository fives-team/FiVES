using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using FIVES;

namespace ServiceBusPlugin
{
    public class ServiceBusImplementation : IServiceBus
    {
        public ServiceBusImplementation()
        {
            this.serviceGateway = new ServiceGateway();
            this.serviceRegistry = new ServiceRegistry();
        }

        public ServiceGateway ServiceGateway
        {
            get
            {
                return this.serviceGateway;
            }
        }

        public ServiceRegistry ServiceRegistry
        {
            get
            {
                return this.serviceRegistry;
            }
        }

        public void Initialize()
        {
            ReadConfig();
            ServiceGateway.PublishedTransformation += new EventHandler<ChangedAttributeEventArgs>(HandleTransformation);
        }

        private void ReadConfig()
        {
            var serviceSections = ConfigurationManager.GetSection("ServiceBusConfiguration/Services") as NameValueCollection;
            foreach (string topicName in serviceSections.AllKeys)
            {
                var value = serviceSections[topicName];
                BuildServiceChain(topicName, value);
            }
        }

        private void BuildServiceChain(string topicName, string serviceList)
        {
            string[] servicesInChain = serviceList.Split(',');
            TransformationAction predecessor = null;

            foreach (string serviceName in servicesInChain)
            {
                TransformationAction serviceItem = new TransformationAction(serviceName);
                if (serviceName != servicesInChain.First())
                {
                    predecessor.Next = serviceItem;
                }
                else
                {
                    addFirstItemOfChainToTopic(topicName, serviceItem);
                }
                predecessor = serviceItem;
            }
        }

        private void addFirstItemOfChainToTopic(string topicName, TransformationAction head)
        {
            if (!topicSubscriptions.ContainsKey(topicName))
            {
                topicSubscriptions.Add(topicName, new HashSet<TransformationAction>());
            }
            topicSubscriptions[topicName].Add(head);
        }

        private void HandleTransformation(object sender, ChangedAttributeEventArgs transform)
        {
            Dictionary<string, Dictionary<string, object>> initialAccumulation = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> initialAttributeUpdates = new Dictionary<string, object>();
            initialAttributeUpdates.Add(transform.AttributeName, transform.NewValue);
            initialAccumulation.Add(transform.Component.Name, initialAttributeUpdates);

            AccumulatedAttributeTransform initialTransform = new AccumulatedAttributeTransform(transform.Entity, initialAccumulation);

            string topic = transform.Component.Name + "." + transform.AttributeName;
            if (topicSubscriptions.ContainsKey(topic) && topicSubscriptions[topic].Count > 0)
            {
                InvokeTopicHandlers(topic, initialTransform);
            }
            else
            {
                CloseComputation(initialTransform);
            }
        }

        private void InvokeTopicHandlers(string topicName, AccumulatedAttributeTransform initialAccumulation)
        {
            foreach (TransformationAction transformAction in topicSubscriptions[topicName])
            {
                ThreadPool.QueueUserWorkItem(action =>
                    transformAction.Execute(initialAccumulation)
                    );
            }
        }

        public void CloseComputation(AccumulatedAttributeTransform accumulatedResult)
        {
            applyAttributeUpdates(accumulatedResult);
            ServiceGateway.PublishResult(accumulatedResult);
        }

        private void applyAttributeUpdates(AccumulatedAttributeTransform accumulatedResult)
        {
            Entity changedEntity = accumulatedResult.Entity;
            var accumulatedTransformations = accumulatedResult.AccumulatedTransformations;
            foreach (KeyValuePair<string, Dictionary<string, object>> changedComponent in accumulatedTransformations)
            {
                foreach(KeyValuePair<string, object> changedAttribute in changedComponent.Value)
                {
                    changedEntity[changedComponent.Key][changedAttribute.Key] = changedAttribute.Value;
                }
            }
        }

        private ServiceGateway serviceGateway;
        private ServiceRegistry serviceRegistry;
        IDictionary<string, ISet<TransformationAction>> topicSubscriptions = new Dictionary<string, ISet<TransformationAction>>();
    }
}
