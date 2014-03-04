using KIARAPlugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    /// <summary>
    /// General tool functions used in the ServerSync plugin.
    /// </summary>
    static class ServerSyncTools
    {
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

        /// <summary>
        /// Configures a JsonSerializer used in a given connection to include types names when the declared type of an
        /// object is different from the actual type.
        /// </summary>
        /// <param name="connection">A given connection.</param>
        public static void ConfigureJsonSerializer(Connection connection)
        {
            object settingsObj;
            if (connection.GetProperty("JsonSerializerSettings", out settingsObj))
            {
                JsonSerializerSettings settings = settingsObj as JsonSerializerSettings;
                if (settings != null)
                {
                    settings.TypeNameHandling = TypeNameHandling.Auto;
                    connection.SetProperty("JsonSerializerSettings", settings);
                }
            }
        }
    }
}
