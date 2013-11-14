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
            return new List<string>();
        }

        public void Initialize()
        {
            // TODO: Load config
            // TODO: If hub - start the sync server
            // TODO: Connect to sync node (unless local)
            // TODO: Load required plugins
            // TODO: Set up listeners for changes
        }
    }
}
