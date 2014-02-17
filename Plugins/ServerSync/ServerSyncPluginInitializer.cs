using FIVES;
using System.Collections.Generic;

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
            var serverSync = new ServerSyncImpl();
            ServerSync.Instance = serverSync;
            serverSync.Initialize();
        }

        public void Shutdown()
        {

        }
    }
}
