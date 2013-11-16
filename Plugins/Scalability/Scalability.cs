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
            syncServer.OnNewClient += HandleNewSyncNode;
            syncServer["getActorID"] = (Func<string>)GetActorID;
        }

        internal void ConnectToSyncServer()
        {
            var remoteSyncServer = ServiceFactory.Discover(GetKIARAConfigURI());
            remoteSyncServer.OnConnected += HandleNewSyncNode;
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

        private void HandleNewSyncNode(Connection connection)
        {
            var syncNode = new SyncNode(connection);  // this will block until other actor will reply with its ActorID
            logger.Debug("Connected to remote sync node with ActorID = " + syncNode.RemoteActorID);

            if (!syncNode.RemoteActorID.Equals(LocalActorID))
                remoteSyncNodes.Add(syncNode);
        }

        private string GetActorID()
        {
            return LocalActorID.ToString();
        }

        private Guid LocalActorID = Guid.NewGuid();
        private ConcurrentBag<SyncNode> remoteSyncNodes = new ConcurrentBag<SyncNode>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
