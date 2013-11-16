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
    // TODO: Document class and public members
    public class Scalability
    {
        public static Scalability Instance = new Scalability();

        /// <summary>
        /// True if this instance of FiVES listens to clients and distributes updates.
        /// </summary>
        public bool IsSyncRelay { get; private set; }

        /// <summary>
        /// True if this instance of FiVES does not need to send local updates to remote sync server.
        /// </summary>
        public bool IsSyncRoot { get; private set; }

        public Scalability()
        {
            LoadConfig();
        }

        internal void StartSyncServer()
        {
            var syncServer = ServiceFactory.Create(GetKIARAConfigURI());
            syncServer.OnNewClient += AddSyncNode;
            syncServer["getSyncID"] = (Func<string>)(LocalSyncID.ToString);
            syncServer["addEntity"] = (Action<Guid, EntitySyncInfo>)HandleAddEntity;
            syncServer["removeEntity"] = (Action<Guid>)HandleRemoveEntity;
            syncServer["updateProperties"] = (Action<Guid, EntitySyncInfo>)HandleUpdateProperties;
        }

        internal void ConnectToSyncServer()
        {
            var remoteSyncServer = ServiceFactory.Discover(GetKIARAConfigURI());
            remoteSyncServer.OnConnected += AddSyncNode;
        }

        internal void LoadConfig()
        {
            string scalabilityConfigPath = this.GetType().Assembly.Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(scalabilityConfigPath);

            IsSyncRelay = Boolean.Parse(config.AppSettings.Settings["IsSyncRelay"].Value);
        }

        internal void StartSync()
        {
            // TODO: Register handlers for change events
        }

        private string GetKIARAConfigURI()
        {
            var configFile = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "syncServer.json");
            return "file:///" + configFile;
        }

        private void AddSyncNode(Connection connection)
        {
            // Use new thread to prevent blocking the receiving thread, which will receive the remote SyncID.
            Task.Factory.StartNew(delegate()
            {
                try
                {
                    Guid remoteSyncID = Guid.Parse(connection["getSyncID"]().Wait<string>());

                    // Set up function to remove the sync node when the connection is closed.
                    connection.Closed += (sender, e) => remoteSyncNodes.TryRemove(remoteSyncID, out connection);

                    remoteSyncNodes.TryAdd(remoteSyncID, connection);
                    logger.Debug("Connected to remote sync node with SyncID = " + remoteSyncID);
                }
                catch (Exception e)
                {
                    logger.WarnException("Failed to add new sync node", e);
                    return;
                }
            });
        }

        private void HandleAddEntity(Guid guid, EntitySyncInfo info)
        {
            // TODO: if syncInfo doesn't contain guid, create new entity. info contains copy of the syncInfo on the
            // remote node. also create entity in the scene. setting properties can be done by calling
            // HandleUpdateProperties
        }

        private void HandleRemoveEntity(Guid guid)
        {
            // TODO: remove entity from syncInfo and from scene if present.
        }

        private void HandleUpdateProperties(Guid guid, EntitySyncInfo updatedProperties)
        {
            // TODO: if entity is present if the syncInfo - update its properties in the scene. use try-catch when
            // updating properties, because some components may be undefined if certain plugins are not loaded.
        }

        private Guid LocalSyncID = Guid.NewGuid();

        // Collection of remote sync nodes mapped from their SyncID to a Connection.
        private ConcurrentDictionary<Guid, Connection> remoteSyncNodes = new ConcurrentDictionary<Guid, Connection>();

        private Dictionary<Guid, EntitySyncInfo> syncInfo = new Dictionary<Guid, EntitySyncInfo>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
