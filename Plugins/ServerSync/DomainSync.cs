using KIARAPlugin;
using System;

namespace ServerSyncPlugin
{
    class DomainSync
    {
        public DomainSync()
        {
            RegisterDomainSyncAPI(ServerSync.LocalServer.Service);
            RegisterForDomainChanges();
        }

        public void RegisterDomainSyncAPI(IService service)
        {
            service["serverSync.getDoR"] = (Func<string>)GetDoR;
            service["serverSync.getDoI"] = (Func<string>)GetDoI;
            service["serverSync.updateDoI"] = (Action<Connection, string>)UpdateDoI;
            service["serverSync.updateDoR"] = (Action<Connection, string>)UpdateDoR;
        }

        void RegisterForDomainChanges()
        {
            ServerSync.LocalServer.DoIChanged += HandleLocalDoIChanged;
            ServerSync.LocalServer.DoRChanged += HandleLocalDoRChanged;
        }

        void HandleLocalDoIChanged(object sender, EventArgs e)
        {
            string serializedDoI = Serialization.SerializeObject<IDomainOfInterest>(ServerSync.LocalServer.DoI);
            foreach (var server in ServerSync.RemoteServers)
                server.Connection["serverSync.updateDoI"](serializedDoI);
        }

        void HandleLocalDoRChanged(object sender, EventArgs e)
        {
            string serializedDoR = Serialization.SerializeObject<IDomainOfResponsibility>(ServerSync.LocalServer.DoR);
            foreach (var server in ServerSync.RemoteServers)
                server.Connection["serverSync.updateDoR"](serializedDoR);
        }

        /// <summary>
        /// Updates domain-of-interest on the remote server associated with a given connection.
        /// </summary>
        /// <param name="connection">Connection to the remote server.</param>
        /// <param name="serializedDoI">New serialized domain-of-interest.</param>
        void UpdateDoI(Connection connection, string serializedDoI)
        {
            foreach (var server in ServerSync.RemoteServers)
            {
                if (server.Connection == connection)
                {
                    var newDoI = Serialization.DeserializeObject<IDomainOfInterest>(serializedDoI);
                    (server as RemoteServerImpl).DoI = newDoI;
                    return;
                }
            }

            throw new Exception("Received a DoR update for an unregistered server");
        }

        /// <summary>
        /// Updates domain-of-responsibility on the remote server associated with a given connection.
        /// </summary>
        /// <param name="connection">Connection to the remote server.</param>
        /// <param name="serializedDoR">New serialized domain-of-responsibility.</param>
        void UpdateDoR(Connection connection, string serializedDoR)
        {
            foreach (var server in ServerSync.RemoteServers)
            {
                if (server.Connection == connection)
                {
                    var newDoR = Serialization.DeserializeObject<IDomainOfResponsibility>(serializedDoR);
                    (server as RemoteServerImpl).DoR = newDoR;
                    return;
                }
            }

            throw new Exception("Received a DoR update for an unregistered server");
        }

        /// <summary>
        /// Returns serialized DoI for this server.
        /// </summary>
        /// <returns>Serialized DoI.</returns>
        string GetDoI()
        {
            return Serialization.SerializeObject<IDomainOfInterest>(ServerSync.LocalServer.DoI);
        }

        /// <summary>
        /// Returns serialized DoR for this server.
        /// </summary>
        /// <returns>Serialized DoR.</returns>
        string GetDoR()
        {
            return Serialization.SerializeObject<IDomainOfResponsibility>(ServerSync.LocalServer.DoR);
        }
    }
}
