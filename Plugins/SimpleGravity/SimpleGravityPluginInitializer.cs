using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;

namespace SimpleGravityPlugin
{
    public class SimpleGravityPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "SimpleGravity"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string>(); }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> {"position"}; }
        }

        public void Initialize()
        {
            RegisterComponents();
            RegisterToEvents();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        private void RegisterComponents() { }

        private void RegisterToEvents() { }
    }
}
