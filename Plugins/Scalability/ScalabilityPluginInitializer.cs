using FIVES;
using System;
using System.Collections.Generic;
using ScalabilityPlugin;

namespace ScalabilityPlugin
{
    public class ScalabilityPluginInitializer : IPluginInitializer
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
                return new List<string>();
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
            Scalability.Instance = new NoScalability();
        }

        public void Shutdown()
        {
        }
    }
}
