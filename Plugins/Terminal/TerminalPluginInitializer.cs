using FIVES;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerminalPlugin
{
    public class TerminalPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "Terminal";
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
            Application.Controller = new ApplicationController();

            // TODO: redirect all NLog logs to the console to some filter which will correctly interleave logs and 
            // terminal input

            Terminal.Instance = new Terminal(Application.Controller);
        }
    }
}
