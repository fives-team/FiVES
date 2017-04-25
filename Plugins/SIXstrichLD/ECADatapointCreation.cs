using FIVES;
using SIX;
using SIXCore.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIXstrichLDPlugin
{
    public static class ECADatapointCreation
    {
        public static void createEntityCollectionDatapoint(this Server server, Uri collectionUri, EntityCollection entityCollection)
        {
            server.CreateServerDatapoint(
                collectionUri, new EntityCollectionDatapointAdapter<ChangedAttributeEventArgs>(new JsonSerialization(), entityCollection)
            );

            lock (entityCollection)
            {
                foreach (var entity in entityCollection)
                {
                    server.createEntityDatapoint(collectionUri, entity);
                }
            }
            Console.WriteLine("created EC datapoint: " + collectionUri);
        }

        public static void createEntityDatapoint(this Server server, Uri baseUri, Entity entity)
        {
            var entityUri = new Uri(baseUri.OriginalString + "/" + entity.Guid);
            server.CreateServerDatapoint(
                entityUri, new EntityDatapointAdapter<ChangedAttributeEventArgs>(new JsonSerialization(), entity)
            );

            lock (entity)
            {
                foreach (var component in entity.Components)
                {
                    server.createComponentDatapoint(entityUri, component);
                }
            }
            Console.WriteLine("created E datapoint: " + entityUri);
        }

        public static void createComponentDatapoint(this Server server, Uri entityUri, Component component)
        {
            var componentUri = new Uri(entityUri.OriginalString + "/" + component.Name);
            server.CreateServerDatapoint(
                componentUri, new ComponentDatapointAdapter<ChangedAttributeEventArgs>(new JsonSerialization(), component)
            );

            lock (component)
            {
                foreach (var attribute in component.Definition.AttributeDefinitions)
                {
                    server.createAttributeDatapoint(componentUri, component[attribute.Name]);
                }
            }
            Console.WriteLine("created C datapoint: " + componentUri);
        }

        public static void createAttributeDatapoint(this Server server, Uri componentUri, FIVES.Attribute attribute)
        {
            var attributeDefinition = attribute.Definition;
            var attributeUri = new Uri(componentUri.OriginalString + "/" + attributeDefinition.Name);
            server.CreateServerDatapoint(
                attributeUri, new AttributeDatapointAdapter<ChangedAttributeEventArgs>(new JsonSerialization(), attribute)
            );
            Console.WriteLine("created A datapoint: " + attributeUri);
        }
    }
}
