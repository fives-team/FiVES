using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScalabilityPlugin
{
    class ScalabilityPluginInitializer : IPluginInitializer
    {
        public string GetName()
        {
            return "Scalability";
        }

        public List<string> GetDependencies()
        {
            return new List<string> { "KIARA" };
        }

        public void Initialize()
        {
            var scalability = Scalability.Instance;
            if (scalability.IsSyncRelay)
                scalability.StartSyncServer();
            scalability.ConnectToSyncServer();
            scalability.StartSync();
        }
    }
}
