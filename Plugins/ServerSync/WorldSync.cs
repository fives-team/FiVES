using FIVES;
using KIARAPlugin;
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

        void SyncFullEntityToServer(IRemoteServer server, Entity entity)
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
            ServerSync.LocalServer.Service["serverSync.addEntity"] = (Action<Connection, Guid>)AddEntity;
            ServerSync.LocalServer.Service["serverSync.removeEntity"] = (Action<Connection, Guid>)RemoveEntity;
            ServerSync.LocalServer.Service["serverSync.changeAttribute"] =
                (Action<Connection, Guid, string, string, object>)ChangeAttribute;
        }

        void AddEntity(Connection connection, Guid entityGuid)
        {
            if (World.Instance.ContainsEntity(entityGuid))
                throw new Exception("Requested addition of an existing entity");

            // TODO: Forward update to other servers.

            // TODO: Create entity sync info.

            ignoredEntityAdditions.Add(entityGuid);
            World.Instance.Add(new Entity(entityGuid));

            throw new NotImplementedException();
        }

        void RemoveEntity(Connection connection, Guid entityGuid)
        {
            if (!World.Instance.ContainsEntity(entityGuid))
                throw new Exception("Requested removal of non-existing entity");

            // TODO: Forward update to other servers.

            // TODO: Mark entity as removed in sync info.

            ignoredEntityRemovals.Add(entityGuid);
            World.Instance.Remove(World.Instance.FindEntity(entityGuid));

            throw new NotImplementedException();
        }

        void ChangeAttribute(Connection connection, Guid entityGuid, string componentName, string attributeName,
            object newValue)
        {
            if (!World.Instance.ContainsEntity(entityGuid))
                throw new Exception("Requested attribute change for non-existing entity");

            // TODO: Forward update to other servers.

            // TODO: Update sync info.

            ignoredAttributeUpdates.Add(new AttributeUpdate(entityGuid, componentName, attributeName, newValue));
        }

        void RegisterToEntityUpdates()
        {
            World.Instance.AddedEntity += HandleAddedEntityToWorld;
            World.Instance.RemovedEntity += HandleRemovedEntityFromWorld;
        }

        void HandleAddedEntityToWorld(object sender, EntityEventArgs e)
        {
            if (ignoredEntityAdditions.Remove(e.Entity.Guid))
                return;

            foreach (var server in ServerSync.RemoteServers)
                if (server.DoI.IsInterestedInEntity(e))
                    server.Connection["serverSync.addEntity"](e.Entity.Guid);

            e.Entity.ChangedAttribute += HandleEntityAttributeChanged;
        }

        void HandleRemovedEntityFromWorld(object sender, EntityEventArgs e)
        {
            if (ignoredEntityRemovals.Remove(e.Entity.Guid))
                return;

            foreach (var server in ServerSync.RemoteServers)
                if (server.DoI.IsInterestedInEntity(e))
                    server.Connection["serverSync.removeEntity"](e.Entity.Guid);
        }

        private void HandleEntityAttributeChanged(object sender, ChangedAttributeEventArgs e)
        {
            var update = new AttributeUpdate(e.Entity.Guid, e.Component.Name, e.AttributeName, e.NewValue);
            if (ignoredAttributeUpdates.Remove(update))
                return;

            foreach (var server in ServerSync.RemoteServers)
            {
                if (server.DoI.IsInterestedInAttributeChange(e))
                {
                    var changeAttribute = server.Connection["serverSync.changeAttribute"];
                    changeAttribute(e.Entity.Guid, e.Component.Name, e.AttributeName, e.NewValue);
                }
            }
        }

        private class AttributeUpdate
        {
            public AttributeUpdate(Guid entityGuid, string componentName, string attributeName, object newValue)
            {
                EntityGuid = entityGuid;
                ComponentName = componentName;
                AttributeName = attributeName;
                NewValue = newValue;
            }

            public override bool Equals(object otherObject)
            {
                if (otherObject is AttributeUpdate)
                {
                    var otherUpdate = otherObject as AttributeUpdate;
                    return EntityGuid.Equals(otherUpdate.EntityGuid)
                        && ComponentName.Equals(otherUpdate.ComponentName)
                        && AttributeName.Equals(otherUpdate.AttributeName)
                        && NewValue.Equals(otherUpdate.NewValue);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return EntityGuid.GetHashCode();
            }

            Guid EntityGuid;
            string ComponentName;
            string AttributeName;
            object NewValue;
        }

        private HashSet<Guid> ignoredEntityAdditions = new HashSet<Guid>();
        private HashSet<Guid> ignoredEntityRemovals = new HashSet<Guid>();
        private HashSet<AttributeUpdate> ignoredAttributeUpdates = new HashSet<AttributeUpdate>();

        private Dictionary<Guid, EntitySyncInfo> syncInfo = new Dictionary<Guid, EntitySyncInfo>();
    }
}
