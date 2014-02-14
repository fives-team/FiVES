using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    public class ServerSyncPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "ServerSync";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string> { "KIARA" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {

        }

        public void Shutdown()
        {

        }
    }
}
