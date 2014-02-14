using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;

namespace EventLoopPlugin
{
    public class EventLoopPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "EventLoop"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> { }; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> { }; }
        }

        public void Initialize()
        {
            EventLoop.Instance = new EventLoop();
        }

        public void Shutdown()
        {
        }
    }
}
