using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIARAPlugin
{
    public class KIARAPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "KIARA"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string>(); }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string>(); }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
