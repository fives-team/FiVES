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
using FIVES;
using SINFONI;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Peforms synchronization of the changes to the ECA model across servers. Uses DoI to only send the relevant
    /// updates.
    /// </summary>
    class WorldSync
    {
        /// <summary>
        /// Creates a WorldSync object.
        /// </summary>
        public WorldSync()
        {
            RegisterWorldSyncAPI(ServerSync.LocalServer.Service);
            RegisterToEntityUpdates();
            RegisterToNewRemoteServers();
            PerformInitialSync();
        }

        /// <summary>
        /// Registers the world sync API in a given service.
        /// </summary>
        /// <param name="service">A given service.</param>
        public void RegisterWorldSyncAPI(Service service)
        {
            service["serverSync.addEntity"] = (Action<Connection, string, string, EntitySyncInfo>)HandleRemoteAddedEntity;
            service["serverSync.removeEntity"] = (Action<Connection, string>)HandleRemoteRemovedEntity;
            service["serverSync.changeAttributes"] =
                (Action<Connection, string, EntitySyncInfo>)HandleRemoteChangedAttributes;
        }

        void RegisterToEntityUpdates()
        {
            World.Instance.AddedEntity += HandleLocalAddedEntity;
            World.Instance.RemovedEntity += HandleLocalRemovedEntity;

            CreateSyncInfoForExistingEntities();
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

        internal void HandleRemoteAddedEntity(Connection connection, string id, string owner, EntitySyncInfo initialSyncInfo)
        {
            var guid = new Guid(id);
            lock (syncInfo)
            {
                if (!syncInfo.ContainsKey(guid))
                {
                    syncInfo.Add(guid, new EntitySyncInfo());
                    lock (ignoredEntityAdditions)
                        ignoredEntityAdditions.Add(guid);
                    World.Instance.Add(new Entity(guid, new Guid(owner)));
                }
                else
                {
                    logger.Warn("Processing addition of already existing entity. Guid: " + guid);
                    return;
                }

                ProcessChangedAttributes(guid, initialSyncInfo);

                Entity entity = World.Instance.FindEntity(guid);
                foreach (IRemoteServer server in ServerSync.RemoteServers)
                    if (server.Connection != connection && server.DoI.IsInterestedInEntity(entity))
                        server.Connection["serverSync.addEntity"](id, owner, initialSyncInfo);
            }
        }

        /// <summary>
        /// Handles an entity removal update from the remote sync node and removes the entity locally.
        /// </summary>
        /// <param name="guid">Guid of the removed entity.</param>
        internal void HandleRemoteRemovedEntity(Connection connection, string id)
        {
            Guid guid = new Guid(id);
            lock (syncInfo)
            {
                Entity entity = World.Instance.FindEntity(guid);
                foreach (IRemoteServer server in ServerSync.RemoteServers)
                    if (server.Connection != connection && server.DoI.IsInterestedInEntity(entity))
                        server.Connection["serverSync.removeEntity"](id);

                if (!syncInfo.ContainsKey(guid))
                {
                    logger.Warn("Ignoring removal of entity that does not exist. Guid: " + guid);
                    return;
                }

                syncInfo.Remove(guid);
                lock (ignoredEntityRemovals)
                    ignoredEntityRemovals.Add(guid);
                World.Instance.Remove(World.Instance.FindEntity(guid));
            }
        }

        /// <summary>
        /// Relays the changes to attributes to other connected sync nodes and process them locally.
        /// </summary>
        /// <param name="guid">Guid of the entity containing affected attributes.</param>
        /// <param name="changedAttributes">A set of modified attributes with their remote sync info.</param>
        internal void HandleRemoteChangedAttributes(Connection connection, string id, EntitySyncInfo changedAttributes)
        {
            Guid guid = new Guid(id);
            Entity entity = World.Instance.FindEntity(guid);
            foreach (IRemoteServer server in ServerSync.RemoteServers)
            {
                if (server.Connection == connection)
                    continue;

                EntitySyncInfo filteredAttributes = new EntitySyncInfo();
                bool containsAttributesToSync = false;
                foreach (var component in changedAttributes.Components)
                {
                    foreach (var attribute in component.Value.Attributes)
                    {
                        if (server.DoI.IsInterestedInAttributeChange(entity, component.Key, attribute.Key))
                        {
                            filteredAttributes[component.Key][attribute.Key] = attribute.Value;
                            containsAttributesToSync = true;
                        }
                    }
                }

                if (containsAttributesToSync)
                    server.Connection["serverSync.changeAttributes"](id, filteredAttributes);
            }

            ProcessChangedAttributes(guid, changedAttributes);
        }

        /// <summary>
        /// Adds a sync info for all entities that are already present in the world. If a sync info is already present
        /// for some entity then it is replaced.
        /// </summary>
        private void CreateSyncInfoForExistingEntities()
        {
            lock (syncInfo)
            {
                foreach (Entity entity in World.Instance)
                {
                    var entitySyncInfo = CreateSyncInfoForNewEntity(entity);
                    syncInfo[entity.Guid] = entitySyncInfo;
                }
            }
        }

        /// <summary>
        /// Creates a new sync for an entity.
        /// </summary>
        /// <param name="entity">Entity for which sync info is to be created.</param>
        /// <returns>Created sync info.</returns>
        private EntitySyncInfo CreateSyncInfoForNewEntity(Entity entity)
        {
            var entitySyncInfo = new EntitySyncInfo();
            foreach (Component component in entity.Components)
            {
                foreach (ReadOnlyAttributeDefinition attrDefinition in component.Definition.AttributeDefinitions)
                {
                    entitySyncInfo[component.Name][attrDefinition.Name] =
                        new AttributeSyncInfo(
                            ServerSync.LocalServer.SyncID.ToString(),component[attrDefinition.Name].Value);
                }
            }
            return entitySyncInfo;
        }

        /// <summary>
        /// Handler for the event AddedEntity event in the World. Invokes addEntity method on the connected sync nodes
        /// to notify them about new entity.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        internal void HandleLocalAddedEntity(object sender, EntityEventArgs e)
        {
            e.Entity.ChangedAttribute += HandleLocalChangedAttribute;

            // Ignore this change if it was caused by the scalability plugin itself.
            lock (ignoredEntityAdditions)
            {
                if (ignoredEntityAdditions.Remove(e.Entity.Guid))
                    return;
            }

            lock (syncInfo)
            {
                // Local sync info may already be present if this new entity was added in response to a sync message
                // from remote sync node.
                if (!syncInfo.ContainsKey(e.Entity.Guid))
                {
                    var newSyncInfo = CreateSyncInfoForNewEntity(e.Entity);
                    syncInfo.Add(e.Entity.Guid, newSyncInfo);
                }

                // This must be inside the lock to prevent concurrent changes to entity's sync info.
                foreach (IRemoteServer server in ServerSync.RemoteServers)
                    if (server.DoI.IsInterestedInEntity(e.Entity))
                        server.Connection["serverSync.addEntity"](
                            e.Entity.Guid.ToString(),
                            World.Instance.ID.ToString(),
                            syncInfo[e.Entity.Guid]);
            }
        }

        /// <summary>
        /// Handler for the event RemovedEntity event in the World. Invokes removeEntity method on the connected sync
        /// nodes to notify them about removed entity.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        internal void HandleLocalRemovedEntity(object sender, EntityEventArgs e)
        {
            // Ignore this change if it was caused by the scalability plugin itself.
            lock (ignoredEntityRemovals)
            {
                if (ignoredEntityRemovals.Remove(e.Entity.Guid))
                    return;
            }

            lock (syncInfo)
            {
                // Local sync info may already have been removed if this new entity was removed in response to a sync
                // message from remote sync node.
                if (syncInfo.ContainsKey(e.Entity.Guid))
                    syncInfo.Remove(e.Entity.Guid);
            }

            foreach (IRemoteServer server in ServerSync.RemoteServers)
                if (server.DoI.IsInterestedInEntity(e.Entity))
                    server.Connection["serverSync.removeEntity"](e.Entity.Guid.ToString());
        }

        /// <summary>
        /// Handler for the event ChangedAttribute event in the Entity. Update local sync info for the attribute and
        /// invokes changedAttributes method on the connected sync nodes to notify them about the change.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        internal void HandleLocalChangedAttribute(object sender, ChangedAttributeEventArgs e)
        {
            var componentName = e.Component.Name;
            var attributeName = e.AttributeName;

            // Ignore this change if it was caused by the scalability plugin itself.
            lock (ignoredAttributeChanges)
            {
                var attributeUpdate = new AttributeUpdate(e.Entity.Guid, componentName, attributeName, e.NewValue);
                if (ignoredAttributeChanges.Remove(attributeUpdate))
                    return;
            }

            var newAttributeSyncInfo =
                new AttributeSyncInfo(ServerSync.LocalServer.SyncID.ToString(), e.NewValue);
            lock (syncInfo)
            {
                if (!syncInfo.ContainsKey(e.Entity.Guid))
                {
                    logger.Warn("Local attribute changed in an entity which has no sync info.");
                    return;
                }

                EntitySyncInfo entitySyncInfo = syncInfo[e.Entity.Guid];
                entitySyncInfo[componentName][attributeName] = newAttributeSyncInfo;
            }

            var changedAttributes = new EntitySyncInfo();
            changedAttributes[componentName][attributeName] = newAttributeSyncInfo;
            foreach (IRemoteServer server in ServerSync.RemoteServers)
                if (server.DoI.IsInterestedInAttributeChange(e.Entity, e.Component.Name, e.AttributeName))
                    server.Connection["serverSync.changeAttributes"](e.Entity.Guid.ToString(), changedAttributes);
        }

        void HandleRemoteServerAdded(object sender, ServerEventArgs e)
        {
            SyncExistingEntitiesToServer(e.Server);
        }

        void SyncExistingEntitiesToServer(IRemoteServer server)
        {
            foreach (var entity in World.Instance)
            {
                if (server.DoI.IsInterestedInEntity(entity))
                    server.Connection["serverSync.addEntity"](
                        entity.Guid.ToString(),
                        World.Instance.ID.ToString(),
                        syncInfo[entity.Guid]);
            }
        }

        /// <summary>
        /// Processes changes to a set of attributes on the remote sync node and updates local values accordingly.
        /// </summary>
        /// <param name="guid">Guid of the entity containing affected attributes.</param>
        /// <param name="changedAttributes">A set of modified attributes with their remote sync info.</param>
        private void ProcessChangedAttributes(Guid guid, EntitySyncInfo changedAttributes)
        {
            lock (syncInfo)
            {
                if (!syncInfo.ContainsKey(guid))
                {
                    logger.Warn("Ignoring changes to attributes in an entity that does not exist. Guid: " + guid);
                    return;
                }

                Entity localEntity = World.Instance.FindEntity(guid);
                foreach (KeyValuePair<string, ComponentSyncInfo> componentPair in changedAttributes.Components)
                {
                    foreach (KeyValuePair<string, AttributeSyncInfo> attributePair in componentPair.Value.Attributes)
                    {
                        HandleRemoteChangedAttribute(localEntity, componentPair.Key, attributePair.Key,
                                                     attributePair.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Handles an update to a single attribute.
        /// </summary>
        /// <param name="localEntity">Entity containing attribute.</param>
        /// <param name="componentName">Name of the component containing attribute.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="remoteAttributeSyncInfo">Remote sync info on this attribute.</param>
        /// <returns>True if the update should be propagated to other sync nodes.</returns>
        private bool HandleRemoteChangedAttribute(Entity localEntity, string componentName, string attributeName,
            AttributeSyncInfo remoteAttributeSyncInfo)
        {
            EntitySyncInfo localEntitySyncInfo = syncInfo[localEntity.Guid];

            bool shouldUpdateLocalAttribute = false;
            if (!localEntitySyncInfo.Components.ContainsKey(componentName) ||
                !localEntitySyncInfo[componentName].Attributes.ContainsKey(attributeName))
            {
                shouldUpdateLocalAttribute = true;
                localEntitySyncInfo[componentName][attributeName] = remoteAttributeSyncInfo;
            }
            else
            {
                AttributeSyncInfo localAttributeSyncInfo = localEntitySyncInfo[componentName][attributeName];
                shouldUpdateLocalAttribute =
                    (localAttributeSyncInfo.LastValue == null && remoteAttributeSyncInfo.LastValue != null) ||
                    (localAttributeSyncInfo.LastValue != null &&
                     !localAttributeSyncInfo.LastValue.Equals(remoteAttributeSyncInfo.LastValue));
                if (!localAttributeSyncInfo.Sync(remoteAttributeSyncInfo))
                    return false;  // ignore this update because sync discarded it
            }

            if (shouldUpdateLocalAttribute)
            {
                try
                {
                    // This is necessary, because Json.NET serializes primitive types using basic JSON values, which do
                    // not retain original type (e.g. integer values always become int even if they were stored as
                    // float values before) and there is no way to change this.
                    var attributeType = localEntity[componentName].Definition[attributeName].Type;
                    var attributeValue = remoteAttributeSyncInfo.LastValue;

                    // Ignore event for this change.
                    lock (ignoredAttributeChanges)
                    {
                        ignoredAttributeChanges.Add(new AttributeUpdate(localEntity.Guid, componentName, attributeName,
                            attributeValue));
                    }

                    localEntity[componentName][attributeName].Suggest(attributeValue);
                    return true;
                }
                catch (ComponentAccessException)
                {
                    // This is fine, because we may have some plugins not loaded on this node.
                }
            }

            // If we are here, it means that this update was not discarded by sync and thus should be propagated to
            // other nodes. This is still the case even if the value has been the same and local attribute has not been
            // update, because we still need to update sync info on other nodes.
            return true;
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

        // Collection of attribute changes that should be discarded once. This is used to ignore changes to attributes
        // that were caused by the scalability plugin itself in response to an update from remote node.
        private HashSet<AttributeUpdate> ignoredAttributeChanges = new HashSet<AttributeUpdate>();

        // Collection of entity additions or removals that should be ignored once. Similarly to the above, this is used
        // to ignored additions or removals of entities that were caused by the scalability plugin itself in response
        // to an update from the remote node.
        private HashSet<Guid> ignoredEntityAdditions = new HashSet<Guid>();
        private HashSet<Guid> ignoredEntityRemovals = new HashSet<Guid>();

        // Sync info for entities in the World.
        internal Dictionary<Guid, EntitySyncInfo> syncInfo = new Dictionary<Guid, EntitySyncInfo>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
