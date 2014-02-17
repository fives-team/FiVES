using System;
using System.Collections.Generic;

namespace ServerSyncPlugin
{
    public interface IServerSync
    {
        // Collection of remote servers.
        IEnumerable<IRemoteServer> RemoteServers { get; }

        // Local server.
        ILocalServer LocalServer { get; }

        // Events when a server is added or removed from the RemoteServers collection.
        event EventHandler<ServerEventArgs> AddedServer;
        event EventHandler<ServerEventArgs> RemovedServer;

        /// <summary>
        /// True if this server relays updates to other connected servers.
        /// </summary>
        bool IsSyncRelay { get; }
    }
}
