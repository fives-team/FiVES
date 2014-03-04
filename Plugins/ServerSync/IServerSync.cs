using System;
using System.Collections.Generic;

namespace ServerSyncPlugin
{
    /// <summary>
    /// The ServerSync plugin interface.
    /// </summary>
    public interface IServerSync
    {
        /// <summary>
        /// Collection of remote servers.
        /// </summary>
        IEnumerable<IRemoteServer> RemoteServers { get; }

        /// <summary>
        /// Local server.
        /// </summary>
        ILocalServer LocalServer { get; }

        /// <summary>
        /// Triggered when a remote server is added to the RemoteServers collection.
        /// </summary>
        event EventHandler<ServerEventArgs> AddedServer;

        /// <summary>
        /// Triggered when a remote server is removed from the RemoteServers collection.
        /// </summary>
        event EventHandler<ServerEventArgs> RemovedServer;
    }
}
