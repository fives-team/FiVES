using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    class WorldSync
    {
        public WorldSync()
        {
            RegisterWorldSyncAPI();
            RegisterToEntityUpdates();
            RegisterToNewRemoteServers();

            PerformInitialSync();
        }

        void RegisterToNewRemoteServers()
        {
            ServerSync.AddedServer += HandleRemoteServerAdded;
        }

        void PerformInitialSync()
        {
            foreach (var server in ServerSync.RemoteServers)
                SyncExistingEntitiesToServer(server);
        }

        void HandleRemoteServerAdded(object sender, ServerEventArgs e)
        {
            SyncExistingEntitiesToServer(e.Server);
        }

        void SyncExistingEntitiesToServer(IRemoteServer server)
        {
            foreach (var entity in World.Instance)
            {
                var entityArgs = new EntityEventArgs(entity);
                if (server.DoI.IsInterestedInEntity(entityArgs))
                    SyncFullEntityToServer(server, entity);
            }
        }

        private static void SyncFullEntityToServer(IRemoteServer server, Entity entity)
        {
            server.Connection["serverSync.addEntity"](entity.Guid);
            foreach (var component in entity.Components)
            {
                foreach (var attrDef in component.Definition.AttributeDefinitions)
                {
                    var newValue = component[attrDef.Name];
                    if (!newValue.Equals(attrDef.DefaultValue))
                    {
                        var attributeArgs = new ChangedAttributeEventArgs(
                            component, attrDef.Name, attrDef.DefaultValue, newValue);
                        if (server.DoI.IsInterestedInAttributeChange(attributeArgs))
                        {
                            var changeAttribute = server.Connection["serverSync.changeAttribute"];
                            changeAttribute(entity.Guid, component.Name, attrDef.Name, newValue);
                        }
                    }
                }
            }
        }

        void RegisterWorldSyncAPI()
        {
            ServerSync.LocalServer.Service["serverSync.addEntity"] = (Action<Guid>)AddEntity;
            ServerSync.LocalServer.Service["serverSync.removeEntity"] = (Action<Guid>)RemoveEntity;
            ServerSync.LocalServer.Service["serverSync.changeAttribute"] =
                (Action<Guid, string, string, object>)ChangeAttribute;
        }

        void AddEntity(Guid entityGuid)
        {
            // TODO
            throw new NotImplementedException();
        }

        void RemoveEntity(Guid entityGuid)
        {
            // TODO
            throw new NotImplementedException();
        }

        void ChangeAttribute(Guid entityGuid, string componentName, string attributeName, object newValue)
        {
            // TODO
            throw new NotImplementedException();
        }

        void RegisterToEntityUpdates()
        {
            World.Instance.AddedEntity += HandleAddedEntityToWorld;
            World.Instance.RemovedEntity += HandleRemovedEntityFromWorld;
        }

        void HandleAddedEntityToWorld(object sender, EntityEventArgs e)
        {
            foreach (var server in ServerSync.RemoteServers)
                if (server.DoI.IsInterestedInEntity(e))
                    server.Connection["serverSync.addEntity"](e.Entity.Guid);

            e.Entity.ChangedAttribute += HandleEntityAttributeChanged;
        }

        void HandleRemovedEntityFromWorld(object sender, EntityEventArgs e)
        {
            foreach (var server in ServerSync.RemoteServers)
                if (server.DoI.IsInterestedInEntity(e))
                    server.Connection["serverSync.removeEntity"](e.Entity.Guid);
        }

        private void HandleEntityAttributeChanged(object sender, ChangedAttributeEventArgs e)
        {
            foreach (var server in ServerSync.RemoteServers)
            {
                if (server.DoI.IsInterestedInAttributeChange(e))
                {
                    var changeAttribute = server.Connection["serverSync.changeAttribute"];
                    changeAttribute(e.Entity.Guid, e.Component.Name, e.AttributeName, e.NewValue);
                }
            }
        }
    }
}
