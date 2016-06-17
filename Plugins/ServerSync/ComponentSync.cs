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
using SINFONI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Peforms synchronization of the registered component definitions across servers.
    /// </summary>
    class ComponentSync
    {
        /// <summary>
        /// Construts the ComponentSync object.
        /// </summary>
        public ComponentSync()
        {
            RegisterForLocalComponentRegistrations();
            RegisterComponentSyncAPI(ServerSync.LocalServer.Service);
            RegisterToNewRemoteServers();
            PerformInitialSync();
        }

        /// <summary>
        /// Registers the component sync APIs in the provided service.
        /// </summary>
        /// <param name="service">The provided service.</param>
        public void RegisterComponentSyncAPI(Service service)
        {
            service["serverSync.registerComponentDefinition"] =
                (Action<Connection, ComponentDef>)HandleRemoteRegisteredComponentDefinition;
        }

        private void RegisterToNewRemoteServers()
        {
            ServerSync.AddedServer += HandleRemoteServerAdded;
        }

        void HandleRemoteServerAdded(object sender, ServerEventArgs e)
        {
            SyncExistingComponentsToServer(e.Server);
        }

        private void SyncExistingComponentsToServer(IRemoteServer remoteServer)
        {
            foreach (ReadOnlyComponentDefinition compDef in ComponentRegistry.Instance.RegisteredComponents)
                remoteServer.Connection["serverSync.registerComponentDefinition"]((ComponentDef)compDef);
        }

        private void RegisterForLocalComponentRegistrations()
        {
            ComponentRegistry.Instance.RegisteredComponent += HandleLocalRegisteredComponent;
        }

        private void PerformInitialSync()
        {
            foreach (var server in ServerSync.RemoteServers)
                SyncExistingComponentsToServer(server);
        }

        internal void HandleRemoteRegisteredComponentDefinition(Connection connection, ComponentDef componentDef)
        {
            foreach (IRemoteServer server in ServerSync.RemoteServers)
                if (server.Connection != connection)
                    server.Connection["serverSync.registerComponentDefinition"](componentDef);

            if (ComponentRegistry.Instance.FindComponentDefinition(componentDef.Name) == null)
            {
                lock (ignoredComponentRegistrations)
                    ignoredComponentRegistrations.Add(componentDef.Guid);
                ComponentRegistry.Instance.Register((ComponentDefinition)componentDef);
            }
        }

        internal void HandleLocalRegisteredComponent(object sender, RegisteredComponentEventArgs e)
        {
            lock (ignoredComponentRegistrations)
            {
                if (ignoredComponentRegistrations.Remove(e.ComponentDefinition.Guid.ToString()))
                    return;
            }

            foreach (IRemoteServer server in ServerSync.RemoteServers)
                server.Connection["serverSync.registerComponentDefinition"]((ComponentDef)e.ComponentDefinition);
        }

        private List<string> ignoredComponentRegistrations = new List<string>();
    }
}
