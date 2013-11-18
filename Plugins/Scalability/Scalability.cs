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
    // TODO: Optimization: currently when an update arrives from remote end and gets applied locally, it also triggers
    // sync events which send this very update back to the original sender. The latter will ignore it as it will
    // contain same LastTimestamp and SyncID, but it will waste bandwidth. This can be optimized by sending updates
    // manually to all nodes expect the original sender. However, we will also need to somehow disable sync handlers
    // temporarily to avoid duplicate updates.

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
            // Add sync info for all entities already present in the database. Sync all of them to other nodes if any.

            World.Instance.AddedEntity += HandleLocalAddedEntity;
            World.Instance.RemovedEntity += HandleLocalRemovedEntity;
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
                foreach (Connection connection in remoteSyncNodes.Values)
                    connection["addEntity"](e.Entity.Guid, localSyncInfo[e.Entity.Guid]);
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
            lock (localSyncInfo)
            {
                // Local sync info may already have been removed if this new entity was removed in response to a sync
                // message from remote sync node.
                if (localSyncInfo.ContainsKey(e.Entity.Guid))
                    localSyncInfo.Remove(e.Entity.Guid);
            }

            foreach (Connection connection in remoteSyncNodes.Values)
                connection["remoteEntity"](e.Entity.Guid);
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
            foreach (Connection connection in remoteSyncNodes.Values)
                connection["changeAttributes"](e.Entity.Guid, changedAttributes);
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
            connection.Closed += (sender, e) => remoteSyncNodes.TryRemove(remoteSyncID, out connection);

            remoteSyncNodes.TryAdd(remoteSyncID, connection);
            logger.Debug("Connected to remote sync node with SyncID = " + remoteSyncID);

            // TODO: Sync all existing entities to the remote node.
        }

        /// <summary>
        /// Registers sychronization method handler on a given connection.
        /// </summary>
        /// <param name="connection"></param>
        private void RegisterSyncMethodHandlers(Connection connection)
        {
            connection.RegisterFuncImplementation("addEntity", (Action<Guid, EntitySyncInfo>)HandleRemoteAddedEntity);
            connection.RegisterFuncImplementation("removeEntity", (Action<Guid>)HandleRemoteRemovedEntity);
            connection.RegisterFuncImplementation("changeAttributes",
                (Action<Guid, EntitySyncInfo>)HandleRemoteChangedAttributes);
        }

        /// <summary>
        /// Handles an entity addition update from the remote sync node and adds a new entity locally.
        /// </summary>
        /// <param name="guid">Guid of the new entity.</param>
        /// <param name="initialSyncInfo">Initial sync info for the entity.</param>
        private void HandleRemoteAddedEntity(Guid guid, EntitySyncInfo initialSyncInfo)
        {
            lock (localSyncInfo)
            {
                if (localSyncInfo.ContainsKey(guid))
                {
                    logger.Warn("Received addition of entity that is already present. Guid: " + guid);
                    return;
                }

                localSyncInfo.Add(guid, initialSyncInfo);
                World.Instance.Add(new Entity(guid));
                logger.Debug("Added an entity in response to sync message. Guid: " + guid);
            }
        }

        /// <summary>
        /// Handles an entity removal update from the remote sync node and removes the entity locally.
        /// </summary>
        /// <param name="guid">Guid of the removed entity.</param>
        private void HandleRemoteRemovedEntity(Guid guid)
        {
            lock (localSyncInfo)
            {
                if (!localSyncInfo.ContainsKey(guid))
                {
                    logger.Warn("Received removal of entity that does not exist. Guid: " + guid);
                    return;
                }

                localSyncInfo.Remove(guid);
                World.Instance.Remove(World.Instance.FindEntity(guid));
                logger.Debug("Removed an entity in response to sync message. Guid: " + guid);
            }
        }

        /// <summary>
        /// Handles an update to a set of attributes on the remote sync node and performs updates locally to those
        /// attributes whose value is older.
        /// </summary>
        /// <param name="guid">Guid of the entity containing affected attributes.</param>
        /// <param name="changedAttributes">A set of modified attributes with their remote sync info.</param>
        private void HandleRemoteChangedAttributes(Guid guid, EntitySyncInfo changedAttributes)
        {
            lock (localSyncInfo)
            {
                if (!localSyncInfo.ContainsKey(guid))
                {
                    logger.Warn("Received updated properties for entity that does not exist. Guid: " + guid);
                    return;
                }

                foreach (KeyValuePair<string, ComponentSyncInfo> componentPair in changedAttributes.Components)
                    foreach (KeyValuePair<string, AttributeSyncInfo> attributePair in componentPair.Value.Attributes)
                        HandleRemoteChangedAttribute(guid, componentPair.Key, attributePair.Key, attributePair.Value);
            }
        }

        private void HandleRemoteChangedAttribute(Guid entityGuid, string componentName, string attributeName,
            AttributeSyncInfo remoteAttributeSyncInfo)
        {
            EntitySyncInfo localEntitySyncInfo = localSyncInfo[entityGuid];
            Entity localEntity = World.Instance.FindEntity(entityGuid);

            if (!localEntitySyncInfo.Components.ContainsKey(componentName) ||
                !localEntitySyncInfo[componentName].Attributes.ContainsKey(attributeName))
            {
                localEntitySyncInfo[componentName][attributeName] = remoteAttributeSyncInfo;
            }
            else if (!localEntitySyncInfo[componentName][componentName].Sync(remoteAttributeSyncInfo))
            {
                logger.Debug("Ignored an update to the attribute. Entity guid: " + entityGuid + ". " +
                    "Attribute path: " + componentName + "." + attributeName + ". New value: " +
                    remoteAttributeSyncInfo.LastValue + ". Remote timestamp: " +
                    remoteAttributeSyncInfo.LastTimestamp + ". Local timestamp: " +
                    localEntitySyncInfo[componentName][attributeName].LastTimestamp + ". Remote SyncID: " +
                    remoteAttributeSyncInfo.LastSyncID + ". Local SyncID: " +
                    localEntitySyncInfo[componentName][attributeName].LastSyncID);
                return;  // ignore this attribute because sync discarded remote value
            }

            try
            {
                logger.Debug("Updating an attribute in response to sync message. Entity guid: " + entityGuid + ". " +
                    "Attribute path: " + componentName + "." + attributeName + ". New value: " +
                    remoteAttributeSyncInfo.LastValue + ". Remote timestamp: " +
                    remoteAttributeSyncInfo.LastTimestamp + ". Local timestamp: " +
                    localEntitySyncInfo[componentName][attributeName].LastTimestamp + ". Remote SyncID: " +
                    remoteAttributeSyncInfo.LastSyncID + ". Local SyncID: " +
                    localEntitySyncInfo[componentName][attributeName].LastSyncID);
                localEntity[componentName][attributeName] = remoteAttributeSyncInfo.LastValue;
            }
            catch (ComponentAccessException e)
            {
                // This is fine, because we may have some plugins not loaded on this node.
                logger.DebugException("Ignoring an update an attribute. Component is not defined.", e);
            }
        }

        // SyncID of this node.
        private Guid LocalSyncID = Guid.NewGuid();

        // Collection of remote sync nodes mapped from their SyncID to a Connection.
        private ConcurrentDictionary<Guid, Connection> remoteSyncNodes = new ConcurrentDictionary<Guid, Connection>();

        // Sync info for entities in the World.
        private Dictionary<Guid, EntitySyncInfo> localSyncInfo = new Dictionary<Guid, EntitySyncInfo>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
