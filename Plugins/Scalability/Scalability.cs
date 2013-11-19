using FIVES;
using KIARAPlugin;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScalabilityPlugin
{
    /// <summary>
    /// Implements synchronization algorithm. Manages remote synchronization nodes and local synchronization state.
    /// Processes local changes and distributes them to other nodes. Optionally also relays updates between nodes.
    /// </summary>
    class Scalability
    {
        public static Scalability Instance = new Scalability();

        /// <summary>
        /// True if this instance of FiVES listens to clients and distributes updates.
        /// </summary>
        public bool IsSyncRelay { get; private set; }

        /// <summary>
        /// Constructs a new entity of Scalability class.
        /// </summary>
        public Scalability()
        {
            LoadConfig();
        }

        /// <summary>
        /// Starts a local sync server.
        /// </summary>
        public void StartSyncServer()
        {
            var syncServer = ServiceFactory.Create(ConvertFileNameToURI("scalabilitySyncServer.json"));
            syncServer["clientHandshake"] = (Action<Connection, Guid>)HandleHandshakeFromClient;
        }

        /// <summary>
        /// Connects to a remove sync server.
        /// </summary>
        public void ConnectToSyncServer()
        {
            var remoteSyncServer = ServiceFactory.Discover(ConvertFileNameToURI("scalabilitySyncClient.json"));
            remoteSyncServer.OnConnected += HandleConnectedToServer;
        }


        /// <summary>
        /// Starts synchornization.
        /// </summary>
        public void StartSync()
        {
            CreateSyncInfoForExistingEntities();

            World.Instance.AddedEntity += HandleLocalAddedEntity;
            World.Instance.RemovedEntity += HandleLocalRemovedEntity;
        }

        /// <summary>
        /// Adds a sync info for all entities that are already present in the world. If a sync info is already present
        /// for some entity then it is replaced.
        /// </summary>
        private void CreateSyncInfoForExistingEntities()
        {
            foreach (Entity entity in World.Instance)
            {
                var entitySyncInfo = new EntitySyncInfo();
                foreach (Component component in entity.Components)
                {
                    foreach (ReadOnlyAttributeDefinition attrDefinition in component.Definition.AttributeDefinitions)
                    {
                        entitySyncInfo[component.Name][attrDefinition.Name] =
                            new AttributeSyncInfo(LocalSyncID, component[attrDefinition.Name]);
                    }
                }

                localSyncInfo[entity.Guid] = entitySyncInfo;
            }
        }

        /// <summary>
        /// Loads configuration options from the library configuration file.
        /// </summary>
        private void LoadConfig()
        {
            string scalabilityConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(scalabilityConfigPath);

            IsSyncRelay = Boolean.Parse(config.AppSettings.Settings["IsSyncRelay"].Value);
        }

        /// <summary>
        /// Converts a file name to the URI that point to the file as if it was located in the same directory as the
        /// current assembly.
        /// </summary>
        /// <param name="configFilename"></param>
        /// <returns></returns>
        private string ConvertFileNameToURI(string configFilename)
        {
            var configFullPath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), configFilename);
            return "file://" + configFullPath;
        }

        /// <summary>
        /// Handler for the event AddedEntity event in the World. Invokes addEntity method on the connected sync nodes
        /// to notify them about new entity.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleLocalAddedEntity(object sender, EntityEventArgs e)
        {
            e.Entity.ChangedAttribute += HandleLocalChangedAttribute;

            // Ignore this change if it was caused by the scalability plugin itself.
            lock (entityAdditions)
            {
                if (entityAdditions.Remove(e.Entity.Guid))
                    return;
            }

            lock (localSyncInfo)
            {
                // Local sync info may already be present if this new entity was added in response to a sync message
                // from remote sync node.
                if (!localSyncInfo.ContainsKey(e.Entity.Guid))
                {
                    var newSyncInfo = new EntitySyncInfo();
                    localSyncInfo.Add(e.Entity.Guid, newSyncInfo);
                }

                // This must be inside the lock to prevent concurrent changes to entity's sync info.
                lock (remoteSyncNodes)
                {
                    foreach (Connection connection in remoteSyncNodes)
                        connection["addEntity"](e.Entity.Guid, localSyncInfo[e.Entity.Guid]);
                }
            }
        }

        /// <summary>
        /// Handler for the event RemovedEntity event in the World. Invokes removeEntity method on the connected sync
        /// nodes to notify them about removed entity.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleLocalRemovedEntity(object sender, EntityEventArgs e)
        {
            // Ignore this change if it was caused by the scalability plugin itself.
            lock (entityRemovals)
            {
                if (entityRemovals.Remove(e.Entity.Guid))
                    return;
            }

            lock (localSyncInfo)
            {
                // Local sync info may already have been removed if this new entity was removed in response to a sync
                // message from remote sync node.
                if (localSyncInfo.ContainsKey(e.Entity.Guid))
                    localSyncInfo.Remove(e.Entity.Guid);
            }

            lock (remoteSyncNodes)
            {
                foreach (Connection connection in remoteSyncNodes)
                    connection["remoteEntity"](e.Entity.Guid);
            }
        }

        /// <summary>
        /// Handler for the event ChangedAttribute event in the Entity. Update local sync info for the attribute and
        /// invokes changedAttributes method on the connected sync nodes to notify them about the change.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleLocalChangedAttribute(object sender, ChangedAttributeEventArgs e)
        {
            var componentName = e.Component.Name;
            var attributeName = e.AttributeName;

            // Ignore this change if it was caused by the scalability plugin itself.
            lock (ignoredAttributeChanges)
            {
                foreach (IgnoredAttributeChange change in ignoredAttributeChanges)
                {
                    if (change.EntityGuid == e.Entity.Guid && change.ComponentName == componentName &&
                        change.AttributeName == attributeName && change.Value == e.NewValue)
                    {
                        ignoredAttributeChanges.Remove(change);
                        return;
                    }
                }
            }

            var newAttributeSyncInfo = new AttributeSyncInfo(LocalSyncID, e.NewValue);

            lock (localSyncInfo)
            {
                if (!localSyncInfo.ContainsKey(e.Entity.Guid))
                {
                    logger.Warn("Local attribute changed in an entity which has no sync info.");
                    return;
                }

                EntitySyncInfo entitySyncInfo = localSyncInfo[e.Entity.Guid];
                entitySyncInfo[componentName][attributeName] = newAttributeSyncInfo;
            }

            // TODO: Optimization: we can send batch updates to improve performance. Received code is written to
            // process batches already, but sending is trickier as we don't have network load feedback from KIARA yet.
            var changedAttributes = new EntitySyncInfo();
            changedAttributes[componentName][attributeName] = newAttributeSyncInfo;

            lock (remoteSyncNodes)
            {
                foreach (Connection connection in remoteSyncNodes)
                    connection["changeAttributes"](e.Entity.Guid, changedAttributes);
            }
        }

        /// <summary>
        /// Invoked by the client to transmit their SyncID and request server's SyncID at the same time. Registers
        /// client in the list of remote nodes and transmits back server's SyncID in a separate call. Not implemented
        /// on the client side and thus should not be invoked by the server.
        /// </summary>
        /// <param name="connection">Connection to the client.</param>
        /// <param name="clientSyncID">Client's SyncID.</param>
        private void HandleHandshakeFromClient(Connection connection, Guid clientSyncID)
        {
            if (clientSyncID == LocalSyncID)
            {
                logger.Debug("The server is local. Ignoring own handshake.");
                return;
            }

            AddNewSyncNode(connection, clientSyncID);
            RegisterSyncMethodHandlers(connection);
            connection["serverHandshake"](LocalSyncID);
            SyncExistingEntitiesToRemoteNode(connection);
        }

        /// <summary>
        /// Invoked on the client upon connection to the server.
        /// </summary>
        /// <param name="connection">Connection to the server.</param>
        private void HandleConnectedToServer(Connection connection)
        {
            connection.RegisterFuncImplementation("serverHandshake",
                (Action<Connection, Guid>)HandleHandshakeFromServer);
            RegisterSyncMethodHandlers(connection);
            connection["clientHandshake"](LocalSyncID);
            SyncExistingEntitiesToRemoteNode(connection);
        }

        private void SyncExistingEntitiesToRemoteNode(Connection connection)
        {
            foreach (KeyValuePair<Guid, EntitySyncInfo> entityPair in localSyncInfo)
                connection["addEntity"](entityPair.Key, entityPair.Value);
        }

        /// <summary>
        /// Invoked by the server in response to the clientHandshake to transmit their SyncID. Registers server in the
        /// list of remote nodes. Not implemented on the server side and thus should not be invoked by the client.
        /// </summary>
        /// <param name="connection">Connection to the server.</param>
        /// <param name="serverSyncID">Server's SyncID.</param>
        private void HandleHandshakeFromServer(Connection connection, Guid serverSyncID)
        {
            AddNewSyncNode(connection, serverSyncID);
        }

        /// <summary>
        /// Adds a new sync node to the list of remote sync nodes.
        /// </summary>
        /// <param name="connection">Connection to the remote sync node.</param>
        /// <param name="remoteSyncID">Remote node's SyncID.</param>
        private void AddNewSyncNode(Connection connection, Guid remoteSyncID)
        {
            // Set up function to remove the sync node when the connection is closed.
            connection.Closed += delegate (object sender, EventArgs e) {
                lock (remoteSyncNodes)
                    remoteSyncNodes.Remove(connection);
            };

            lock (remoteSyncNodes)
                remoteSyncNodes.Add(connection);
            logger.Debug("Connected to remote sync node with SyncID = " + remoteSyncID);

            // TODO: Sync all existing entities to the remote node.
        }

        /// <summary>
        /// Registers sychronization method handler on a given connection.
        /// </summary>
        /// <param name="connection"></param>
        private void RegisterSyncMethodHandlers(Connection connection)
        {
            connection.RegisterFuncImplementation("addEntity",
                (Action<Connection, Guid, EntitySyncInfo>)HandleRemoteAddedEntity);
            connection.RegisterFuncImplementation("removeEntity", (Action<Connection, Guid>)HandleRemoteRemovedEntity);
            connection.RegisterFuncImplementation("changeAttributes",
                (Action<Connection, Guid, EntitySyncInfo>)HandleRemoteChangedAttributes);
        }

        /// <summary>
        /// Handles an entity addition update from the remote sync node and adds a new entity locally.
        /// </summary>
        /// <param name="guid">Guid of the new entity.</param>
        /// <param name="initialSyncInfo">Initial sync info for the entity.</param>
        private void HandleRemoteAddedEntity(Connection connection, Guid guid, EntitySyncInfo initialSyncInfo)
        {
            lock (localSyncInfo)
            {
                if (IsSyncRelay)
                    RelaySyncMessage(connection, "addEntity", guid, initialSyncInfo);

                if (localSyncInfo.ContainsKey(guid))
                {
                    logger.Debug("Processing addition of entity as attribute update, because entity with guid " +
                        guid + " is already present.");
                    ProcessChangedAttributes(guid, initialSyncInfo);
                    return;
                }

                localSyncInfo.Add(guid, initialSyncInfo);
                lock (entityAdditions)
                    entityAdditions.Add(guid);
                World.Instance.Add(new Entity(guid));
                logger.Debug("Added an entity in response to sync message. Guid: " + guid);
            }
        }

        /// <summary>
        /// Handles an entity removal update from the remote sync node and removes the entity locally.
        /// </summary>
        /// <param name="guid">Guid of the removed entity.</param>
        private void HandleRemoteRemovedEntity(Connection connection, Guid guid)
        {
            lock (localSyncInfo)
            {
                if (IsSyncRelay)
                    RelaySyncMessage(connection, "removeEntity", guid);

                if (!localSyncInfo.ContainsKey(guid))
                {
                    logger.Debug("Ignoring removal of entity that does not exist. Guid: " + guid);
                    return;
                }

                localSyncInfo.Remove(guid);
                lock (entityRemovals)
                    entityRemovals.Add(guid);
                World.Instance.Remove(World.Instance.FindEntity(guid));
                logger.Debug("Removed an entity in response to sync message. Guid: " + guid);
            }
        }

        /// <summary>
        /// Relays the changes to attributes to other connected sync nodes and process them locally.
        /// </summary>
        /// <param name="guid">Guid of the entity containing affected attributes.</param>
        /// <param name="changedAttributes">A set of modified attributes with their remote sync info.</param>
        private void HandleRemoteChangedAttributes(Connection connection, Guid guid, EntitySyncInfo changedAttributes)
        {
            if (IsSyncRelay)
                RelaySyncMessage(connection, "changeAttributes", guid, changedAttributes);

            ProcessChangedAttributes(guid, changedAttributes);
        }

        /// <summary>
        /// Processes changes to a set of attributes on the remote sync node and updates local values accordingly.
        /// </summary>
        /// <param name="guid">Guid of the entity containing affected attributes.</param>
        /// <param name="changedAttributes">A set of modified attributes with their remote sync info.</param>
        private void ProcessChangedAttributes(Guid guid, EntitySyncInfo changedAttributes)
        {
            lock (localSyncInfo)
            {
                if (!localSyncInfo.ContainsKey(guid))
                {
                    logger.Warn("Ignoring changes to attributes in an entity that does not exist. Guid: " + guid);
                    return;
                }

                foreach (KeyValuePair<string, ComponentSyncInfo> componentPair in changedAttributes.Components)
                    foreach (KeyValuePair<string, AttributeSyncInfo> attributePair in componentPair.Value.Attributes)
                        HandleRemoteChangedAttribute(guid, componentPair.Key, attributePair.Key, attributePair.Value);
            }
        }

        /// <summary>
        /// Relays sync message to other connected nodes, except the source node.
        /// </summary>
        /// <param name="sourceConnection">Connection to the source node.</param>
        /// <param name="methodName">Name of the KIARA method.</param>
        /// <param name="args">Arguments to the method.</param>
        private void RelaySyncMessage(Connection sourceConnection, string methodName, params object[] args)
        {
            lock (remoteSyncNodes)
            {
                foreach (Connection connection in remoteSyncNodes)
                    if (connection != sourceConnection)
                        connection[methodName](args);
            }
        }

        /// <summary>
        /// Handles an update to a single attribute.
        /// </summary>
        /// <param name="entityGuid">Guid of the entity containing attribute.</param>
        /// <param name="componentName">Name of the component containing attribute.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="remoteAttributeSyncInfo">Remote sync info on this attribute.</param>
        /// <returns>True if the attribute has been changed, false otherwise.</returns>
        private bool HandleRemoteChangedAttribute(Guid entityGuid, string componentName, string attributeName,
            AttributeSyncInfo remoteAttributeSyncInfo)
        {
            EntitySyncInfo localEntitySyncInfo = localSyncInfo[entityGuid];
            Entity localEntity = World.Instance.FindEntity(entityGuid);

            logger.Debug("Received an update to an attribute. Entity guid: " + entityGuid + ". " +
                    "Attribute path: " + componentName + "." + attributeName + ". New value: " +
                    remoteAttributeSyncInfo.LastValue + ". Timestamp: " + remoteAttributeSyncInfo.LastTimestamp +
                    ". SyncID: " + remoteAttributeSyncInfo.LastSyncID);

            if (!localEntitySyncInfo.Components.ContainsKey(componentName) ||
                !localEntitySyncInfo[componentName].Attributes.ContainsKey(attributeName))
            {
                logger.Debug("Creating new attribute sync info.");
                localEntitySyncInfo[componentName][attributeName] = remoteAttributeSyncInfo;
            }
            else if (!localEntitySyncInfo[componentName][attributeName].Sync(remoteAttributeSyncInfo))
            {
                logger.Debug("Sync discarded the update. Local value: " +
                    localEntitySyncInfo[componentName][attributeName].LastValue + ". Local timestamp: " +
                    localEntitySyncInfo[componentName][attributeName].LastTimestamp + ". Local SyncID: " +
                    localEntitySyncInfo[componentName][attributeName].LastSyncID);
                return false;  // ignore this attribute because sync discarded remote value
            }

            try
            {
                // This is necessary, because Json.NET serializes primitive types using basic JSON values, which do not
                // retain original type (e.g. integer values always become int even if they were stored as float values
                // before) and there is no way to change this.
                var attributeType = localEntity[componentName].Definition[attributeName].Type;
                var attributeValue = Convert.ChangeType(remoteAttributeSyncInfo.LastValue, attributeType);

                // Ignore event for this change.
                lock (ignoredAttributeChanges)
                {
                    var remoteChange = new IgnoredAttributeChange
                    {
                        EntityGuid = entityGuid,
                        ComponentName = componentName,
                        AttributeName = attributeName,
                        Value = attributeValue
                    };

                    ignoredAttributeChanges.Add(remoteChange);
                }

                localEntity[componentName][attributeName] = attributeValue;

                return true;
            }
            catch (ComponentAccessException e)
            {
                // This is fine, because we may have some plugins not loaded on this node.
                logger.DebugException("Update is not applied to the World. Component is not defined.", e);
                return false;
            }
        }

        /// <summary>
        /// A specific attribute change that should be ignored.
        /// </summary>
        private class IgnoredAttributeChange
        {
            public Guid EntityGuid;
            public string ComponentName;
            public string AttributeName;
            public object Value;
        }

        // Collection of attribute changes that should be discarded once. This is used to ignore changes to attributes
        // that were caused by the scalability plugin itself in response to an update from remote node.
        private List<IgnoredAttributeChange> ignoredAttributeChanges = new List<IgnoredAttributeChange>();

        // Collection of entity additions or removals that should be ignored once. Similarly to the above, this is used
        // to ignored additions or removals of entities that were caused by the scalability plugin itself in response
        // to an update from the remote node.
        private List<Guid> entityAdditions = new List<Guid>();
        private List<Guid> entityRemovals = new List<Guid>();

        // SyncID of this node.
        private Guid LocalSyncID = Guid.NewGuid();

        // Collection of remote sync nodes mapped from their SyncID to a Connection.
        private List<Connection> remoteSyncNodes = new List<Connection>();

        // Sync info for entities in the World.
        private Dictionary<Guid, EntitySyncInfo> localSyncInfo = new Dictionary<Guid, EntitySyncInfo>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
