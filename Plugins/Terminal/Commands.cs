// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerminalPlugin
{
    /// <summary>
    /// This class will contain commands that operate with core classes (not specific to any plugin).
    /// </summary>
    class Commands
    {
        public static Commands Instance;

        public Commands(ApplicationController controller)
        {
            this.controller = controller;

            Terminal.Instance.RegisterCommand("quit", "Shuts the server down.", false,
                ShutDown, new List<string> { "q", "exit" });
            Terminal.Instance.RegisterCommand("removeEntities", "Removes all entities from the server.", false,
                RemoveAllEntities, new List<string> { "re", "clean" });
            Terminal.Instance.RegisterCommand("numEntities", "Prints number of entities in the world.", false,
                PrintNumEntities, new List<string> { "ne" });
            Terminal.Instance.RegisterCommand("plugins", "Prints list of loaded and deferred plugins if any.", false,
                                              ListPlugins);
        }

        public void ShutDown(string commandLine)
        {
            Terminal.Instance.WriteLine("Shutting down the server...");
            controller.Terminate();
        }

        public void RemoveAllEntities(string commandLine)
        {
            World.Instance.Clear();
            Terminal.Instance.WriteLine("Removed all entities");
        }

        public void PrintNumEntities(string commandLine)
        {
            Terminal.Instance.WriteLine("Number of entities: " + World.Instance.Count);
        }

        public void ListPlugins(string commandLine)
        {
            Terminal.Instance.WriteLine("Loaded plugins:");
            foreach (PluginManager.PluginInfo info in PluginManager.Instance.LoadedPlugins)
                Terminal.Instance.WriteLine("  " + info.Name);

            if (PluginManager.Instance.DeferredPlugins.Count > 0) {
                Terminal.Instance.WriteLine("Deferred plugins:");
                foreach (PluginManager.PluginInfo info in PluginManager.Instance.DeferredPlugins)
                {
                    string deferredPluginInfo = String.Format("  {0}: (plugin deps: [{1}], component deps: [{2}])",
                                                              info.Name, String.Join(", ", info.RemainingPluginDeps),
                                                              String.Join(", ", info.RemainingComponentDeps));
                    Terminal.Instance.WriteLine(deferredPluginInfo);
                }
            }
        }

        private ApplicationController controller;
    }
}
