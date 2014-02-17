using System;
using System.Collections.Generic;

namespace ServerSyncPlugin
{
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

        public static bool IsSyncRelay
        {
            get
            {
                return Instance.IsSyncRelay;
            }
        }
    }
}
