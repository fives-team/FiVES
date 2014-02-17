using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalPlugin;

namespace ConfigScalabilityPlugin
{
    class ConfigScalabilityImpl : IConfigScalability
    {
        public ConfigScalabilityImpl()
        {
            LoadConfig();

            PluginManager.Instance.AddPluginLoadedHandler("Terminal", RegisterTerminalCommands);
        }

        private void LoadConfig()
        {

        }

        private void RegisterTerminalCommands()
        {
            //Terminal.Instance.RegisterCommand();
        }
    }
}
