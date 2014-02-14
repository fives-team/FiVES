using KIARAPlugin;
using System;
using System.Collections.Generic;

namespace ServerSyncPlugin
{
    class ServerSyncImpl : IServerSync
    {
        public ServerSyncImpl()
        {
            localServer = new LocalServerImpl();
            remoteServers = new Dictionary<Connection, IRemoteServer>();

            domainSync = new DomainSync();
            worldSync = new WorldSync();

            localServer.Service.OnNewClient += HandleNewServerConnected;

            ConnectToRemoteServers();
        }

        public IEnumerable<IRemoteServer> RemoteServers
        {
            get { return new List<IRemoteServer>(remoteServers.Values); }
        }

        public ILocalServer LocalServer
        {
            get { return localServer; }
        }

        public event EventHandler<ServerEventArgs> AddedServer;

        public event EventHandler<ServerEventArgs> RemovedServer;

        void ConnectToRemoteServers()
        {
            // TODO: read list of remote servers from config
            // TODO: connect to each of them
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
            var newServer = new RemoteServerImpl(connection, doi, dor, syncID);
            remoteServers.Add(connection, newServer);
            if (AddedServer != null)
                AddedServer(this, new ServerEventArgs(newServer));

            connection.Closed += delegate(object sender, EventArgs args)
            {
                if (RemovedServer != null)
                    RemovedServer(this, new ServerEventArgs(newServer));
            };
        }

        Dictionary<Connection, IRemoteServer> remoteServers;
        ILocalServer localServer;
        WorldSync worldSync;
        DomainSync domainSync;
    }
}
