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
using System;
using System.Collections.Generic;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Static containter of and interface to the IServerSync object.
    /// </summary>
    public static class ServerSync
    {
        public static IServerSync Instance;

        public static IEnumerable<IRemoteServer> RemoteServers
        {
            get
            {
                return Instance.RemoteServers;
            }
        }

        public static ILocalServer LocalServer
        {
            get
            {
                return Instance.LocalServer;
            }
        }

        public static event EventHandler<ServerEventArgs> AddedServer
        {
            add
            {
                Instance.AddedServer += value;
            }
            remove
            {
                Instance.AddedServer -= value;
            }
        }

        public static event EventHandler<ServerEventArgs> RemovedServer
        {
            add
            {
                Instance.RemovedServer += value;
            }
            remove
            {
                Instance.RemovedServer -= value;
            }
        }
    }
}
