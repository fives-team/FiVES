using ClientManagerPlugin;
using FIVES;
using SIX;
using SIXCore.Serializers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SIXstrichLDPlugin
{
    public class SIXstrichLDPluginInitializer : IPluginInitializer
    {
        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public string Name
        {
            get
            {
                return "SIXstrichLD";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {}

        public void Shutdown()
        {
            Console.WriteLine("[SIXstrichLD] shutdown");
        }

        private static Uri baseUri = new Uri("http://127.0.0.1:12345/");
        private static Uri worldUri = new Uri(baseUri.OriginalString + "world");
        private static Server server = new Server();
    }
}
