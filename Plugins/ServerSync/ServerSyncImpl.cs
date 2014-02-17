using FIVES;
using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.Configuration;
using TerminalPlugin;

namespace ServerSyncPlugin
{
    class ServerSyncImpl : IServerSync
    {
        public void Initialize()
        {
            localServer = new LocalServerImpl();
            remoteServers = new Dictionary<Connection, IRemoteServer>();

            domainSync = new DomainSync();
            worldSync = new WorldSync();
            componentSync = new ComponentSync();

            localServer.Service.OnNewClient += HandleNewServerConnected;

            ConnectToRemoteServers();

            PluginManager.Instance.AddPluginLoadedHandler("Terminal", RegisterTerminalCommands);
        }

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

        public ILocalServer LocalServer
        {
            get { return localServer; }
        }

        public event EventHandler<ServerEventArgs> AddedServer;

        public event EventHandler<ServerEventArgs> RemovedServer;

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
            string configURI = ConversionTools.ConvertFileNameToURI("serverSyncClient.json");
            string fragment;
            Config config = Context.DefaultContext.RetrieveConfig(configURI, out fragment);
            for (int i = 0; i < config.servers.Count; i++)
            {
                ServiceWrapper remoteService = ServiceFactory.Discover(configURI + "#" + i);

                localServer.RegisterSyncIDAPI(remoteService);
                worldSync.RegisterWorldSyncAPI(remoteService);
                domainSync.RegisterDomainSyncAPI(remoteService);
                componentSync.RegisterComponentSyncAPI(remoteService);

                remoteService.OnConnected += HandleNewServerConnected;
            }
        }

        void HandleNewServerConnected(Connection connection)
        {
            IDomainOfInterest doi = null;
            IDomainOfResponsibility dor = null;
            Guid syncID = Guid.Empty;

            connection["serverSync.getDoR"]().OnSuccess<string>(delegate(string serializedDoR) {
                dor = Serialization.DeserializeObject<IDomainOfResponsibility>(serializedDoR);
                if (doi != null && syncID != Guid.Empty)
                    AddRemoteServer(connection, dor, doi, syncID);
            });

            connection["serverSync.getDoI"]().OnSuccess<string>(delegate(string serializedDoI)
            {
                doi = Serialization.DeserializeObject<IDomainOfInterest>(serializedDoI);
                if (dor != null && syncID != Guid.Empty)
                    AddRemoteServer(connection, dor, doi, syncID);
            });

            connection["serverSync.getSyncID"]().OnSuccess<Guid>(delegate(Guid remoteSyncID)
            {
                syncID = remoteSyncID;
                if (dor != null && doi != null)
                    AddRemoteServer(connection, dor, doi, remoteSyncID);
            });
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

        Dictionary<Connection, IRemoteServer> remoteServers;
        LocalServerImpl localServer;
        WorldSync worldSync;
        DomainSync domainSync;
        ComponentSync componentSync;
    }
}
