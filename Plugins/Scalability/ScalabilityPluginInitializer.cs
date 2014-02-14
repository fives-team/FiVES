using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    class ScalabilityPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "Scalability";
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
            var scalability = new Scalability();

            if (scalability.IsSyncRelay)
                scalability.StartSyncServer();

            scalability.ConnectToSyncServer();
            scalability.StartSync();

            Scalability.Instance = scalability;
        }

        public void Shutdown()
        {
        }
    }
}
