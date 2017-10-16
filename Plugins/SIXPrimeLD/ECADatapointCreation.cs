// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using ClientManagerPlugin;
using FIVES;
using SIXPrime.Core.Serialization;
using SIXPrime.Server;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace SIXPrimeLDPlugin
{
    public static class ECADatapointCreation
    {
        public static void createEntityCollectionDatapoint(this Server server, Uri collectionUri, EntityCollection entityCollection)
        {
            var datapoint = server.CreateServerDatapoint(
                collectionUri, new EntityCollectionDatapointAdapter<UpdateInfo>(new JsonSerialization(), entityCollection)
            );

            IObservable<UpdateInfo> observable = null;

            lock (entityCollection)
            {
                foreach (var entity in entityCollection)
                {
                    var tempObservable = entity.getUpdateObservable(args => true);
                    observable = observable == null ? tempObservable : observable.Merge(tempObservable);
                    datapoint.Subscribe(observable);
                    server.createEntityDatapoint(collectionUri, entity);
                }
            }
            entityCollection.AddedEntity += (entity, eventArgs) =>
            {
                var tempObservable = eventArgs.Entity.getUpdateObservable(args => true);
                observable = observable == null ? tempObservable : observable.Merge(tempObservable);
                datapoint.Subscribe(observable);
                server.createEntityDatapoint(collectionUri, eventArgs.Entity);
            };
        }

        public static void createEntityDatapoint(this Server server, Uri baseUri, Entity entity)
        {
            var entityUri = new Uri(baseUri.OriginalString + "/" + entity.Guid);
            var datapoint = server.CreateServerDatapoint(
                entityUri, new EntityDatapointAdapter<UpdateInfo>(new JsonSerialization(), entity)
            );

            lock (entity)
            {
                foreach (var component in entity.Components)
                {
                    var tempObservable = entity.getUpdateObservable(args => args.Component.Name == component.Name);
                    observable = observable == null ? tempObservable : observable.Merge(tempObservable);
                    datapoint.Subscribe(observable);
                    server.createComponentDatapoint(serializer, entityUri, component);
                }
            }
            entity.CreatedComponent += (sender, eventArgs) =>
            {
                var tempObservable = eventArgs.Component.ContainingEntity.getUpdateObservable(
                    args => args.Component.Name == eventArgs.Component.Name
                );
                observable = observable == null ? tempObservable : observable.Merge(tempObservable);
                datapoint.Subscribe(observable);
                server.createComponentDatapoint(serializer, entityUri, eventArgs.Component);
            };
        }

        public static void createComponentDatapoint(this Server server, Uri entityUri, Component component)
        {
            var entity = component.ContainingEntity;
            var componentUri = new Uri(entityUri.OriginalString + "/" + component.Name);
            var datapoint = server.CreateServerDatapoint(
                componentUri, new ComponentDatapointAdapter<UpdateInfo>(new JsonSerialization(), component)
            );

            lock (component)
            {
                foreach (var attribute in component.Definition.AttributeDefinitions)
                {
                    server.createAttributeDatapoint(componentUri, component[attribute.Name]);
                }
            }

            var observable = entity.getUpdateObservable(
                args => args.Component.Name == component.Name
            );
            datapoint.Subscribe(observable);
        }

        public static void createAttributeDatapoint(this Server server, Uri componentUri, FIVES.Attribute attribute)
        {
            var component = attribute.ParentComponent;
            var entity = component.ContainingEntity;
            var attributeName = attribute.Definition.Name;
            var attributeUri = new Uri(componentUri.OriginalString + "/" + attributeName);
            var attributeType = attribute.Type;
            var datapoint = server.CreateServerDatapoint(
                attributeUri, new AttributeDatapointAdapter<UpdateInfo>(new JsonSerialization(), attribute, server.typeBaseUri, attributeType)
            );
            server.registerDatapointType(attributeType, server.typeBaseUri);
            var observable = entity.getUpdateObservable(
                args => args.Component.Name == component.Name && args.AttributeName == attributeName
            );
            datapoint.Subscribe(observable);
        }

        private static IObservable<UpdateInfo> getUpdateObservable(this Entity entity, Func<ChangedAttributeEventArgs, bool> selector)
        {
            var observable = entity.getEntityObservable()
                .Select(args => args.EventArgs)
                .Where(selector)
                .Select(argsToUpdateInfo)
                .CompleteOnEntityRemoval(entity);
            return observable;
        }

        private static IObservable<System.Reactive.EventPattern<ChangedAttributeEventArgs>> getEntityObservable(this Entity entity)
        {
            return Observable.FromEventPattern<ChangedAttributeEventArgs>(
                handler => entity.ChangedAttribute += handler,
                handler => entity.ChangedAttribute -= handler
            );
        }

        private static UpdateInfo argsToUpdateInfo(ChangedAttributeEventArgs args)
        {
            var updateInfo = new UpdateInfo();
            updateInfo.attributeName = args.AttributeName;
            updateInfo.componentName = args.Component.Name;
            updateInfo.entityGuid = args.Component.ContainingEntity.Guid.ToString();
            updateInfo.value = args.NewValue;
            return updateInfo;
        }

        private static IObservable<T> CompleteOnEntityRemoval<T>(this IObservable<T> original, Entity entity)
        {
            return Observable.Create<T>(o =>
            {
                var subscription = original.Subscribe(o);
                EventHandler<EntityEventArgs> removalCallback = null;
                removalCallback = (sender, ev) =>
                {
                    if (ev.Entity == entity)
                    {
                        subscription.Dispose();
                        o.OnCompleted();
                        World.Instance.RemovedEntity -= removalCallback;
                    }
                };
                World.Instance.RemovedEntity += removalCallback;
                return Disposable.Create(() =>
                {
                    subscription.Dispose();
                    World.Instance.RemovedEntity -= removalCallback;
                });
            });
        }
    }
}
