using KIARAPlugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    static class Tools
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
