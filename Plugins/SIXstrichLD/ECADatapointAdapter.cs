using FIVES;
using SIX;
using SIXCore.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace SIXstrichLDPlugin
{
    public class EntityCollectionDatapointAdapter<T> : DatapointAdapter<T>
    {
        private EntityCollection entityCollection;
        public EntityCollectionDatapointAdapter(ISerialization serializer, EntityCollection entityCollection) : base(serializer)
        {
            this.entityCollection = entityCollection;
        }

        public override byte[] getAlternativeGETResponse(Uri uri)
        {
            return Encoding.UTF8.GetBytes(getLDFromEntityCollection(entityCollection, uri));
        }

        private string getLDFromEntityCollection(EntityCollection entityCollection, Uri uri)
        {
            var serializer = new JavaScriptSerializer();
            var collectionLDDict = LDContext.getEntityCollectionBase();
            var datapointUri = uri.OriginalString;
            var entityUris = new List<string>();
            foreach(var entity in entityCollection)
            {
                entityUris.Add(datapointUri + "/" + entity.Guid);
            }
            collectionLDDict.Add("entities", entityUris);
            return serializer.Serialize(collectionLDDict);
        }
    }

    public class EntityDatapointAdapter<T> : DatapointAdapter<T>
    {
        private Entity entity;
        public EntityDatapointAdapter(ISerialization serializer, Entity entity) : base(serializer)
        {
            this.entity = entity;
        }

        public override byte[] getAlternativeGETResponse(Uri uri)
        {
            return Encoding.UTF8.GetBytes(getLDFromEntity(entity, uri));
        }

        private string getLDFromEntity(Entity entity, Uri uri)
        {
            var serializer = new JavaScriptSerializer();
            var entityLDDict = LDContext.getEntityBase();
            var datapointUri = uri.OriginalString;
            var componentUris = new List<string>();
            foreach (var component in entity.Components)
            {
                componentUris.Add(datapointUri + "/" + component.Name);
            }
            entityLDDict.Add(LD.ID, datapointUri);
            entityLDDict.Add(LD.TYPE, "http://www.dfki.de/fives/entity");
            entityLDDict.Add("guid", entity.Guid);
            entityLDDict.Add("owner", entity.Owner);
            entityLDDict.Add("world", datapointUri.RemoveFromTail(entity.Guid.ToString()).TrimEnd('/'));
            entityLDDict.Add("components", componentUris);
            return serializer.Serialize(entityLDDict);
        }
    }

    public class ComponentDatapointAdapter<T> : DatapointAdapter<T>
    {
        private Component component;
        public ComponentDatapointAdapter(ISerialization serializer, Component component) : base(serializer)
        {
            this.component = component;
        }

        public override byte[] getAlternativeGETResponse(Uri uri)
        {
            return Encoding.UTF8.GetBytes(getLDFromComponent(component, uri));
        }

        private string getLDFromComponent(Component component, Uri uri)
        {
            var serializer = new JavaScriptSerializer();
            var componentLDDict = LDContext.getComponentBase();
            var datapointUri = uri.OriginalString;
            var attributeUris = new List<string>();
            foreach(var attribute in component.Definition.AttributeDefinitions)
            {
                attributeUris.Add(datapointUri + "/" + attribute.Name);
            }
            componentLDDict.Add(LD.ID, datapointUri);
            componentLDDict.Add(LD.TYPE, "http://www.dfki.de/fives/component");
            componentLDDict.Add("name", component.Name);
            componentLDDict.Add("containingEntity", datapointUri.RemoveFromTail(component.Name).TrimEnd('/'));
            componentLDDict.Add("attributes", attributeUris);
            return serializer.Serialize(componentLDDict);
        }
    }

    public class AttributeDatapointAdapter<T> : DatapointAdapter<T>
    {
        private FIVES.Attribute attribute;

        public AttributeDatapointAdapter(ISerialization serializer, FIVES.Attribute attribute) : base(serializer) {
            this.attribute = attribute;
        }

        public override byte[] getAlternativeGETResponse(Uri uri)
        {
            return Encoding.UTF8.GetBytes(getLDFromAttribute(attribute, uri));
        }

        private string getLDFromAttribute(FIVES.Attribute attribute, Uri uri)
        {
            var serializer = new JavaScriptSerializer();
            var attributeLDDict = LDContext.getAttributeBase();
            var datapointUri = uri.OriginalString;
            attributeLDDict.Add(LD.ID, datapointUri);
            attributeLDDict.Add(LD.TYPE, "http://www.dfki.de/fives/attribute");
            attributeLDDict.Add("name", attribute.Definition.Name);
            attributeLDDict.Add("parentComponent", datapointUri.RemoveFromTail(attribute.Definition.Name).TrimEnd('/'));
            attributeLDDict.Add("currentValue", serializer.Serialize(attribute.Value));
            return serializer.Serialize(attributeLDDict);
        }
    }
}
