using FIVES;
using KIARAPlugin;
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
        public void RegisterComponentSyncAPI(IService service)
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
                if (ignoredComponentRegistrations.Remove(e.ComponentDefinition.Guid))
                    return;
            }

            foreach (IRemoteServer server in ServerSync.RemoteServers)
                server.Connection["serverSync.registerComponentDefinition"]((ComponentDef)e.ComponentDefinition);
        }

        private List<Guid> ignoredComponentRegistrations = new List<Guid>();
    }
}
