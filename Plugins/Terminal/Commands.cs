using ClientManagerPlugin;
using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerminalPlugin
{
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
            Terminal.Instance.RegisterCommand("numClients", "Prints number of authenticated clients.", false,
                PrintNumClients, new List<string> { "nc" });
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

        public void PrintNumClients(string commandLine)
        {
            if (!PluginManager.Instance.IsPluginLoaded("ClientManager"))
                Terminal.Instance.WriteLine("ClientManager plugin is not loaded on this server");
            else
                Terminal.Instance.WriteLine("Number of authenticated clients: " + GetNumClients());
        }

        private int GetNumClients()
        {
            return ClientManager.Instance.GetNumAuthenticatedClients();
        }

        private ApplicationController controller;
    }
}
