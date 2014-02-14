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
    /// TODO: For testing, mock Connection class and check that Scalability sends correct messages to other nodes on
    /// relevant changes in the domain model. Also use the same connection to send mock messages from other nodes.
    class Scalability
    {
        // We must not initialize this here, because otherwise we cannot use "logger" in the constructor. Instead we set
        // global instance in the plugin initializer.
        public static Scalability Instance;

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
            syncServer["getTime"] = (Func<DateTime>)GetTime;
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
            World.Instance.AddedEntity += HandleLocalAddedEntity;
            World.Instance.RemovedEntity += HandleLocalRemovedEntity;
            ComponentRegistry.Instance.RegisteredComponent += HandleLocalRegisteredComponent;

            CreateSyncInfoForExistingEntities();
        }

        /// <summary>
        /// Adds a sync info for all entities that are already present in the world. If a sync info is already present
        /// for some entity then it is replaced.
        /// </summary>
        private void CreateSyncInfoForExistingEntities()
        {
            lock (localSyncInfo)
            {
                foreach (Entity entity in World.Instance)
                {
                    var entitySyncInfo = CreateSyncInfoForNewEntity(entity);
                    localSyncInfo[entity.Guid] = entitySyncInfo;
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
                        new AttributeSyncInfo(LocalSyncID, component[attrDefinition.Name]);
                }
            }
            return entitySyncInfo;
        }

        /// <summary>
        /// Loads configuration options from the library configuration file.
        /// </summary>
        private void LoadConfig()
        {
            string scalabilityConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(scalabilityConfigPath);

            IsSyncRelay = Boolean.Parse(config.AppSettings.Settings["IsSyncRelay"].Value);

            logger.Debug("IsSyncRelay = " + IsSyncRelay);
            logger.Debug("LocalSyncID = " + LocalSyncID);
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
            lock (remoteEntityAdditions)
            {
                if (remoteEntityAdditions.Remove(e.Entity.Guid))
                    return;
            }

            lock (localSyncInfo)
            {
                // Local sync info may already be present if this new entity was added in response to a sync message
                // from remote sync node.
                if (!localSyncInfo.ContainsKey(e.Entity.Guid))
                {
                    var newSyncInfo = CreateSyncInfoForNewEntity(e.Entity);
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
            lock (remoteEntityRemovals)
            {
                if (remoteEntityRemovals.Remove(e.Entity.Guid))
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
            lock (remoteAttributeChanges)
            {
                foreach (RemoteAttributeChange change in remoteAttributeChanges)
                {
                    if (change.Entity == e.Entity && change.ComponentName == componentName &&
                        change.AttributeName == attributeName && change.Value == e.NewValue)
                    {
                        remoteAttributeChanges.Remove(change);
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

        private void HandleLocalRegisteredComponent(object sender, RegisteredComponentEventArgs e)
        {
            // Ignore this change if it was caused by the scalability plugin itself.
            lock (remoteComponentRegistrations)
            {
                if (remoteComponentRegistrations.Remove(e.ComponentDefinition.Guid))
                    return;
            }

            lock (remoteSyncNodes)
            {
                foreach (Connection connection in remoteSyncNodes)
                    connection["registerComponentDefinition"](e.ComponentDefinition);
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
                timeDelayToServerMs = 0;
                logger.Info("The server is local. Ignoring own handshake.");
                return;
            }

            RegisterSyncMethodHandlers(connection);
            connection["serverHandshake"](LocalSyncID);

            AddNewSyncNode(connection, clientSyncID);
            SyncExistingEntitiesToRemoteNode(connection);
            SyncExistingComponentDefinitionsToRemoteNode(connection);
        }

        /// <summary>
        /// Invoked on the client upon connection to the server.
        /// </summary>
        /// <param name="connection">Connection to the server.</param>
        private void HandleConnectedToServer(Connection connection)
        {
            Task.Factory.StartNew(() => EstimateTimeDelay(connection)).ContinueWith(delegate {
                connection.RegisterFuncImplementation("serverHandshake",
                    (Action<Connection, Guid>)HandleHandshakeFromServer);
                RegisterSyncMethodHandlers(connection);
                connection["clientHandshake"](LocalSyncID);
            });
        }

        private void SyncExistingEntitiesToRemoteNode(Connection connection)
        {
            foreach (KeyValuePair<Guid, EntitySyncInfo> entityPair in localSyncInfo)
                connection["addEntity"](entityPair.Key, entityPair.Value);
        }

        private void SyncExistingComponentDefinitionsToRemoteNode(Connection connection)
        {
            foreach (ReadOnlyComponentDefinition roCompDef in ComponentRegistry.Instance.RegisteredComponents)
                connection["registerComponentDefinition"]((ComponentDef)roCompDef);
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
            SyncExistingEntitiesToRemoteNode(connection);
            SyncExistingComponentDefinitionsToRemoteNode(connection);
        }

        /// <summary>
        /// Estimates time delay from the remote node taking into account round-trip time and averaging the value over
        /// multiple requests.
        /// </summary>
        /// <param name="connection"></param>
        private void EstimateTimeDelay(Connection connection)
        {
            // Heat-up the message sending pipeline and ignore first 100 biased samples.
            for (int i = 0; i < 100; i++)
                connection["getTime"]().Wait();

            // Average over the next 100 samples.
            double accumulatedDelayMs = 0;
            int numSyncsToAverage = 100;
            for (int i = 0; i < numSyncsToAverage; i++)
            {
                DateTime localTimeStart = DateTime.Now;
                DateTime remoteTime = connection["getTime"]().Wait<DateTime>();
                DateTime localTimeEnd = DateTime.Now;
                TimeSpan roundTripTime = localTimeStart - localTimeEnd;
                TimeSpan timeDifference = remoteTime - localTimeStart;
                accumulatedDelayMs += timeDifference.TotalMilliseconds - (roundTripTime.TotalMilliseconds / 2);
            }

            timeDelayToServerMs = Convert.ToInt32(Math.Round(accumulatedDelayMs / numSyncsToAverage));
            logger.Debug("Estimated time delay is " + timeDelayToServerMs);
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
            logger.Info("Connected to remote sync node with SyncID = " + remoteSyncID);

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
            connection.RegisterFuncImplementation("registerComponentDefinition",
                (Action<Connection, ComponentDef>)HandleRemoteRegisteredComponentDefinition);
        }

        private DateTime GetTime()
        {
            return DateTime.Now;
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

                if (!localSyncInfo.ContainsKey(guid))
                {
                    localSyncInfo.Add(guid, new EntitySyncInfo());
                    lock (remoteEntityAdditions)
                        remoteEntityAdditions.Add(guid);
                    World.Instance.Add(new Entity(guid));
                }
                else
                {
                    logger.Warn("Processing addition of already existing entity. Guid: " + guid);
                }

                ProcessChangedAttributes(guid, initialSyncInfo);
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
                    logger.Warn("Ignoring removal of entity that does not exist. Guid: " + guid);
                    return;
                }

                localSyncInfo.Remove(guid);
                lock (remoteEntityRemovals)
                    remoteEntityRemovals.Add(guid);
                World.Instance.Remove(World.Instance.FindEntity(guid));
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

        private void HandleRemoteRegisteredComponentDefinition(Connection connection, ComponentDef componentDef)
        {
            if (IsSyncRelay)
                RelaySyncMessage(connection, "registerComponentDefinition", componentDef);

            if (ComponentRegistry.Instance.FindComponentDefinition(componentDef.Name) == null)
            {
                lock (remoteComponentRegistrations)
                    remoteComponentRegistrations.Add(componentDef.Guid);
                ComponentRegistry.Instance.Register((ComponentDefinition)componentDef);
            }
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
        /// <param name="localEntity">Entity containing attribute.</param>
        /// <param name="componentName">Name of the component containing attribute.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="remoteAttributeSyncInfo">Remote sync info on this attribute.</param>
        /// <returns>True if the update should be propagated to other sync nodes.</returns>
        private bool HandleRemoteChangedAttribute(Entity localEntity, string componentName, string attributeName,
            AttributeSyncInfo remoteAttributeSyncInfo)
        {
            EntitySyncInfo localEntitySyncInfo = localSyncInfo[localEntity.Guid];

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
                    var attributeValue = Convert.ChangeType(remoteAttributeSyncInfo.LastValue, attributeType);

                    // Ignore event for this change.
                    lock (remoteAttributeChanges)
                    {
                        var remoteChange = new RemoteAttributeChange
                        {
                            Entity = localEntity,
                            ComponentName = componentName,
                            AttributeName = attributeName,
                            Value = attributeValue
                        };

                        remoteAttributeChanges.Add(remoteChange);
                    }

                    localEntity[componentName][attributeName] = attributeValue;
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

        /// <summary>
        /// A specific attribute change that should be ignored.
        /// </summary>
        private class RemoteAttributeChange
        {
            public Entity Entity;
            public string ComponentName;
            public string AttributeName;
            public object Value;
        }

        // Collection of attribute changes that should be discarded once. This is used to ignore changes to attributes
        // that were caused by the scalability plugin itself in response to an update from remote node.
        private List<RemoteAttributeChange> remoteAttributeChanges = new List<RemoteAttributeChange>();

        // Collection of entity additions or removals that should be ignored once. Similarly to the above, this is used
        // to ignored additions or removals of entities that were caused by the scalability plugin itself in response
        // to an update from the remote node.
        private List<Guid> remoteEntityAdditions = new List<Guid>();
        private List<Guid> remoteEntityRemovals = new List<Guid>();

        // Collection of component registrations that should be ignored once. Similarly to the above, this is used
        // to ignored component registrations that were caused by the scalability plugin itself in response to an
        // update from the remote node.
        private List<Guid> remoteComponentRegistrations = new List<Guid>();

        // Time difference to the server node. The value -1 means that it has not been estimated yet, however, most of
        // the code doesn't need to take care of this, because time estimation happens even before handshake exchange.
        private int timeDelayToServerMs = -1;

        // SyncID of this node.
        private Guid LocalSyncID = Guid.NewGuid();

        // Collection of remote sync nodes mapped from their SyncID to a Connection.
        private List<Connection> remoteSyncNodes = new List<Connection>();

        // Sync info for entities in the World.
        private Dictionary<Guid, EntitySyncInfo> localSyncInfo = new Dictionary<Guid, EntitySyncInfo>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
