using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    static class RelayTools
    {
        /// <summary>
        /// Relays sync message to all connected servers, except the source connection.
        /// </summary>
        /// <param name="sourceConnection">Connection to the source node.</param>
        /// <param name="methodName">Name of the KIARA method.</param>
        /// <param name="args">Arguments to the method.</param>
        public static void RelaySyncMessage(Connection sourceConnection, string methodName, params object[] args)
        {
            foreach (IRemoteServer server in ServerSync.RemoteServers)
                if (server.Connection != sourceConnection)
                    server.Connection[methodName](args);
        }
    }
}
