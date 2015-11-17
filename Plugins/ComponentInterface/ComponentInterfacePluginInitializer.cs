using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentInterface
{
    public class ComponentInterfacePluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "ComponentInterface"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string>{"ClientManager"}; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }
    }
}
