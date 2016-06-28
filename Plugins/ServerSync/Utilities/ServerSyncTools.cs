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
using SINFONI;
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
