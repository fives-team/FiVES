using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using FIVES;

namespace FIVESServiceBus
{
    public class ServiceBusImplementation : IServiceBus
    {
        public event EventHandler<AccumulatedAttributeTransform> ComputedResult;

        public ServiceBusImplementation()
        {            
            this.serviceRegistry = new ServiceRegistry();
            Application.Controller.PluginsLoaded += new EventHandler((o, e) => { Initialize(); });
            registerToEvents();            
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
            try
            {
                ReadConfig();
            }
            catch (Exception)
            {
                return;
            }
        }

        public void IntroduceTopic(string topicName, string services)
        {
            BuildServiceChain(topicName, services);
        }

        private void registerToEvents()
        {
            // Register to Proposed Attribute Changes of existing entities
            foreach (Entity e in World.Instance)
            {
                e.ProposedAttributeChange += new EventHandler<ProposeAttributeChangeEventArgs>(HandleTransformation);
            }

            // Register to Proposed Attribute Changes of new entities
            World.Instance.AddedEntity += new EventHandler<EntityEventArgs>((o,e) => {
                e.Entity.ProposedAttributeChange += new EventHandler<ProposeAttributeChangeEventArgs>(HandleTransformation);                               
            });
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

        private void HandleTransformation(object sender, ProposeAttributeChangeEventArgs transform)
        {
            Dictionary<string, Dictionary<string, object>> initialAccumulation = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> initialAttributeUpdates = new Dictionary<string, object>();
            initialAttributeUpdates.Add(transform.AttributeName, transform.Value);
            initialAccumulation.Add(transform.ComponentName, initialAttributeUpdates);

            AccumulatedAttributeTransform initialTransform = new AccumulatedAttributeTransform(transform.Entity, initialAccumulation);

            string topic = transform.ComponentName + "." + transform.AttributeName;
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
            if (ComputedResult != null)
                ComputedResult(this, accumulatedResult);            
        }

        private void applyAttributeUpdates(AccumulatedAttributeTransform accumulatedResult)
        {
            Entity changedEntity = accumulatedResult.Entity;
            var accumulatedTransformations = accumulatedResult.AccumulatedTransformations;
            foreach (KeyValuePair<string, Dictionary<string, object>> changedComponent in accumulatedTransformations)
            {
                foreach(KeyValuePair<string, object> changedAttribute in changedComponent.Value)
                {
                    changedEntity[changedComponent.Key][changedAttribute.Key].Value = changedAttribute.Value;
                }
            }
        }
        
        private ServiceRegistry serviceRegistry;
        IDictionary<string, ISet<TransformationAction>> topicSubscriptions = new Dictionary<string, ISet<TransformationAction>>();
    }
}
