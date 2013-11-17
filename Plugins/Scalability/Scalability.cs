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
            var syncServer = ServiceFactory.Create(GetKIARAConfigURI());
            syncServer.OnNewClient += AddSyncNode;
        }

        /// <summary>
        /// Connects to a remove sync server.
        /// </summary>
        public void ConnectToSyncServer()
        {
            var remoteSyncServer = ServiceFactory.Discover(GetKIARAConfigURI());
            remoteSyncServer.OnConnected += AddSyncNode;
        }


        /// <summary>
        /// Starts synchornization.
        /// </summary>
        public void StartSync()
        {
            World.Instance.AddedEntity += HandleLocalAddedEntity;
            World.Instance.RemovedEntity += HandleLocalRemovedEntity;
        }

        private void LoadConfig()
        {
            string scalabilityConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(scalabilityConfigPath);

            IsSyncRelay = Boolean.Parse(config.AppSettings.Settings["IsSyncRelay"].Value);
        }

        private string GetKIARAConfigURI()
        {
            var configFile = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "syncServer.json");
            return "file:///" + configFile;
        }

        private void HandleLocalAddedEntity(object sender, EntityEventArgs e)
        {
            e.Entity.ChangedAttribute += HandleLocalChangedAttribute;

            lock (localSyncInfo)
            {
                var newSyncInfo = new EntitySyncInfo();
                localSyncInfo.Add(e.Entity.Guid, newSyncInfo);

                // This must be inside the lock to prevent changes to the newSyncInfo.
                foreach (Connection connection in remoteSyncNodes.Values)
                    connection["addEntity"](e.Entity.Guid, newSyncInfo);
            }
        }

        private void HandleLocalRemovedEntity(object sender, EntityEventArgs e)
        {
            lock (localSyncInfo)
            {
                localSyncInfo.Remove(e.Entity.Guid);
            }

            foreach (Connection connection in remoteSyncNodes.Values)
                connection["remoteEntity"](e.Entity.Guid);
        }

        private void HandleLocalChangedAttribute(object sender, ChangedAttributeEventArgs e)
        {
            var attributePath = new AttributePath(e.Component.Name, e.AttributeName);
            var newAttributeSyncInfo = new AttributeSyncInfo(LocalSyncID, e.NewValue);

            lock (localSyncInfo)
            {
                EntitySyncInfo entitySyncInfo = localSyncInfo[e.Entity.Guid];
                entitySyncInfo.Attributes[attributePath] = newAttributeSyncInfo;
            }

            // TODO: Optimization: we can send batch updates to improve performance. Received code is written to
            // process batches already, but sending is trickier as we don't have network load feedback from KIARA yet.
            var changedAttributes = new EntitySyncInfo();
            changedAttributes.Attributes.Add(attributePath, newAttributeSyncInfo);
            foreach (Connection connection in remoteSyncNodes.Values)
                connection["changeAttributes"](e.Entity.Guid, changedAttributes);
        }

        private void AddSyncNode(Connection connection)
        {
            // Use new thread to prevent blocking the receiving thread, which will receive the remote SyncID.
            Task.Factory.StartNew(delegate()
            {
                try
                {
                    Guid remoteSyncID = connection["getSyncID"]().Wait<Guid>();

                    // Set up function to remove the sync node when the connection is closed.
                    connection.Closed += (sender, e) => remoteSyncNodes.TryRemove(remoteSyncID, out connection);

                    remoteSyncNodes.TryAdd(remoteSyncID, connection);
                    logger.Debug("Connected to remote sync node with SyncID = " + remoteSyncID);

                    RegisterMethodHandlers(connection);
                }
                catch (Exception e)
                {
                    logger.WarnException("Failed to add new sync node", e);
                    return;
                }
            });
        }

        private void RegisterMethodHandlers(Connection connection)
        {
            connection.RegisterFuncImplementation("getSyncID", (Func<Guid>)(delegate { return LocalSyncID; }));
            connection.RegisterFuncImplementation("addEntity", (Action<Guid, EntitySyncInfo>)HandleRemoteAddedEntity);
            connection.RegisterFuncImplementation("removeEntity", (Action<Guid>)HandleRemoteRemovedEntity);
            connection.RegisterFuncImplementation("changeAttributes",
                (Action<Guid, EntitySyncInfo>)HandleRemoteChangedAttributes);
        }

        private void HandleRemoteAddedEntity(Guid guid, EntitySyncInfo remoteSyncInfo)
        {
            lock (localSyncInfo)
            {
                if (localSyncInfo.ContainsKey(guid))
                {
                    logger.Warn("Received addition of entity that is already present. Guid: " + guid);
                    return;
                }

                localSyncInfo.Add(guid, remoteSyncInfo);
                World.Instance.Add(new Entity(guid));
            }
        }

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
            }
        }

        private void HandleRemoteChangedAttributes(Guid guid, EntitySyncInfo changedAttributes)
        {
            lock (localSyncInfo)
            {
                if (!localSyncInfo.ContainsKey(guid))
                {
                    logger.Warn("Received updated properties for entity that does not exist. Guid: " + guid);
                    return;
                }

                EntitySyncInfo localEntitySyncInfo = localSyncInfo[guid];
                Entity updatedEntity = World.Instance.FindEntity(guid);
                foreach (KeyValuePair<AttributePath, AttributeSyncInfo> attributePair in changedAttributes.Attributes)
                {
                    AttributePath attrPath = attributePair.Key;
                    AttributeSyncInfo remoteAttrSyncInfo = attributePair.Value;


                    if (!localEntitySyncInfo.Attributes.ContainsKey(attrPath))
                    {
                        localEntitySyncInfo.Attributes[attrPath] = remoteAttrSyncInfo;
                    }
                    else
                    {
                        if (localEntitySyncInfo.Attributes[attrPath].Sync(remoteAttrSyncInfo))
                        {
                            updatedEntity[attrPath.ComponentName][attrPath.AttributeName] =
                                remoteAttrSyncInfo.LastValue;
                        }
                    }
                }
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
