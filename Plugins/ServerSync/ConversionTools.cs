using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    static class ConversionTools
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
    }
}
