using FIVES;
using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerSyncPlugin
{
    class ComponentSync
    {
        public ComponentSync()
        {
            RegisterForLocalComponentRegistrations();
            RegisterComponentSyncAPI();
            RegisterToNewRemoteServers();
            PerformInitialSync();
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

        private void RegisterComponentSyncAPI()
        {
            ServerSync.LocalServer.Service["serverSync.registerComponentDefinition"] =
                (Action<Connection, ComponentDef>)HandleRemoteRegisteredComponentDefinition;
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

        private void HandleRemoteRegisteredComponentDefinition(Connection connection, ComponentDef componentDef)
        {
            CommunicationTools.RelaySyncMessage(connection, "registerComponentDefinition", componentDef);

            if (ComponentRegistry.Instance.FindComponentDefinition(componentDef.Name) == null)
            {
                lock (ignoredComponentRegistrations)
                    ignoredComponentRegistrations.Add(componentDef.Guid);
                ComponentRegistry.Instance.Register((ComponentDefinition)componentDef);
            }
        }

        private void HandleLocalRegisteredComponent(object sender, RegisteredComponentEventArgs e)
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
