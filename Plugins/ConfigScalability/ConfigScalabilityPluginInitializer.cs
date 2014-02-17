using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

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
                return new List<string> { "ServerSync" };
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
            ConfigScalability.Instance = new ConfigScalabilityImpl();
        }

        public void Shutdown()
        {
        }
    }
}
