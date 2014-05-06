// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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
