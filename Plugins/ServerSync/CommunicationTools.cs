using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    static class CommunicationTools
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

        /// <summary>
        /// Converts a file name to the URI that point to the file as if it was located in the same directory as the
        /// current assembly.
        /// </summary>
        /// <param name="configFilename"></param>
        /// <returns></returns>
        public static string ConvertFileNameToURI(string configFilename)
        {
            string assemblyPath = typeof(ServerSync).Assembly.Location;
            var configFullPath = Path.Combine(Path.GetDirectoryName(assemblyPath), configFilename);
            return "file://" + configFullPath;
        }
    }
}
