using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVES;

namespace Light
{
    public class LightPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public List<string> PluginDependencies
        {
            get { throw new NotImplementedException(); }
        }

        public List<string> ComponentDependencies
        {
            get { throw new NotImplementedException(); }
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
