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
using System;
using System.Collections.Generic;
using System.Configuration;
using TerminalPlugin;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Implements the ServerSync plugin interface.
    /// </summary>
    class ServerSyncImpl : IServerSync
    {
        /// <summary>
        /// Initializes the object of this class. We have chosen to use this method instead of a constructor as various
        /// other methods called from Initilize require a ServerSync.Instance to be set, which requires a construct to
        /// have finished constructing the object already.
        /// </summary>
        public void Initialize()
        {
            localServer = new LocalServerImpl();
            remoteServers = new Dictionary<Connection, IRemoteServer>();
            AttemptedConnections = new HashSet<Connection>();

            domainSync = new DomainSync();
            worldSync = new WorldSync();
            componentSync = new ComponentSync();

            localServer.Service.OnNewClient += HandleNewServerConnected;

            if (ServerDiscovery == null)
                ServerDiscovery = new ConfigDiscovery();

            ConnectToRemoteServers();

            PluginManager.Instance.AddPluginLoadedHandler("Terminal", RegisterTerminalCommands);
        }

        /// <summary>
        /// Collection of remote servers.
        /// </summary>
        public IEnumerable<IRemoteServer> RemoteServers
        {
            get
            {
                lock (remoteServers)
                {
                    return new List<IRemoteServer>(remoteServers.Values);
                }
            }
        }

        /// <summary>
        /// Local server.
        /// </summary>
        public ILocalServer LocalServer
        {
            get { return localServer; }
        }

        /// <summary>
        /// Triggered when a remote server is added to the RemoteServers collection.
        /// </summary>
        public event EventHandler<ServerEventArgs> AddedServer;

        /// <summary>
        /// Triggered when a remote server is removed from the RemoteServers collection.
        /// </summary>
        public event EventHandler<ServerEventArgs> RemovedServer;

        public void ShutDown()
        {
            foreach (Connection connection in AttemptedConnections)
            {
                connection.Disconnect();
            }

            localServer.ShutDown();
        }

        void RegisterTerminalCommands()
        {
            Terminal.Instance.RegisterCommand("print-server-info", "Prints info on all remote servers", false,
                PrintServerInfo, new List<string> { "psi" });
        }

        void PrintServerInfo(string commandLine)
        {
            foreach (IRemoteServer server in ServerSync.RemoteServers)
            {
                string message = String.Format("{0}: doi = [{1}], dor = [{2}]", server.SyncID, server.DoI, server.DoR);
                Terminal.Instance.WriteLine(message);
            }
        }

        void ConnectToRemoteServers()
        {
            ICollection<string> remoteServerUris = ServerDiscovery.DiscoverRemoteEndpoints();
            foreach (string uri in remoteServerUris)
            {
                ServiceWrapper remoteService = ServiceFactory.Discover(uri);

                localServer.RegisterSyncIDAPI(remoteService);
                worldSync.RegisterWorldSyncAPI(remoteService);
                domainSync.RegisterDomainSyncAPI(remoteService);
                componentSync.RegisterComponentSyncAPI(remoteService);

                remoteService.OnConnected += ServerSyncTools.ConfigureJsonSerializer;
                remoteService.OnConnected += HandleNewServerConnected;
            }
        }

        void HandleNewServerConnected(Connection connection)
        {
            IDomainOfInterest doi = null;
            IDomainOfResponsibility dor = null;
            Guid syncID = Guid.Empty;

            AttemptedConnections.Add(connection);

            connection["serverSync.getDoR"]().OnSuccess<string>(delegate(string serializedDoR)
            {
                dor = StringSerialization.DeserializeObject<IDomainOfResponsibility>(serializedDoR);
                if (doi != null && syncID != Guid.Empty)
                    AddRemoteServer(connection, dor, doi, syncID);
            }).OnError(m => { Console.WriteLine("Could not retrieve DoR of remote server, reason: " + m); });

            connection["serverSync.getDoI"]().OnSuccess<string>(delegate(string serializedDoI)
            {
                doi = StringSerialization.DeserializeObject<IDomainOfInterest>(serializedDoI);
                if (dor != null && syncID != Guid.Empty)
                    AddRemoteServer(connection, dor, doi, syncID);
            }).OnError(m => { Console.WriteLine("Could not retrieve DoI of remote server, reason: " + m); }); ;

            connection["serverSync.getSyncID"]().OnSuccess<string>(delegate(string remoteSyncID)
            {
                syncID = new Guid(remoteSyncID);
                if (dor != null && doi != null)
                    AddRemoteServer(connection, dor, doi, syncID);
            }).OnError(m => { Console.WriteLine("Could not retrieve SyncID of remote server, reason: " + m); }); ;
        }

        void AddRemoteServer(Connection connection, IDomainOfResponsibility dor, IDomainOfInterest doi, Guid syncID)
        {
            if (syncID == ServerSync.LocalServer.SyncID)
                return;

            var newServer = new RemoteServerImpl(connection, doi, dor, syncID);

            lock (remoteServers)
                remoteServers.Add(connection, newServer);

            if (AddedServer != null)
                AddedServer(this, new ServerEventArgs(newServer));

            connection.Closed += delegate(object sender, EventArgs args)
            {
                lock (remoteServers)
                    remoteServers.Remove(connection);

                if (RemovedServer != null)
                    RemovedServer(this, new ServerEventArgs(newServer));
            };
        }

        HashSet<Connection> AttemptedConnections;
        IServerDiscovery ServerDiscovery;
        Dictionary<Connection, IRemoteServer> remoteServers;
        LocalServerImpl localServer;
        WorldSync worldSync;
        DomainSync domainSync;
        ComponentSync componentSync;
    }
}
