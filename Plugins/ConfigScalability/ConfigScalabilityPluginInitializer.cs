using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;
using ScalabilityPlugin;

namespace ConfigScalabilityPlugin
{
    public class ConfigScalabilityPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "ConfigScalability";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string> { "ServerSync", "Scalability" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string> { "location" };
            }
        }

        public void Initialize()
        {
            // Registers ConfigScalability as the class implementing IScalability interface.
            Scalability.Instance = new ConfigScalability();
        }

        public void Shutdown()
        {
        }
    }
}
