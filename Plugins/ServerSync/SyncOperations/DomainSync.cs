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
using SINFONI;
using System;

namespace ServerSyncPlugin
{
    /// <summary>
    /// Performs synchronization of the DoI and DoR between changes.
    /// </summary>
    class DomainSync
    {
        /// <summary>
        /// Constructs a DomainSync object.
        /// </summary>
        public DomainSync()
        {
            RegisterDomainSyncAPI(ServerSync.LocalServer.Service);
            RegisterForDomainChanges();
        }

        /// <summary>
        /// Registers the domain sync APIs in the provided service.
        /// </summary>
        /// <param name="service">The provided service.</param>
        public void RegisterDomainSyncAPI(Service service)
        {
            service["serverSync.getDoR"] = (Func<string>)GetDoR;
            service["serverSync.getDoI"] = (Func<string>)GetDoI;
            service["serverSync.updateDoI"] = (Action<Connection, string>)HandleRemoteDoIChanged;
            service["serverSync.updateDoR"] = (Action<Connection, string>)HandleRemoteDoRChanged;
        }

        void RegisterForDomainChanges()
        {
            ServerSync.LocalServer.DoIChanged += HandleLocalDoIChanged;
            ServerSync.LocalServer.DoRChanged += HandleLocalDoRChanged;
        }

        internal void HandleLocalDoIChanged(object sender, EventArgs e)
        {
            string serializedDoI = StringSerialization.SerializeObject<IDomainOfInterest>(ServerSync.LocalServer.DoI);
            foreach (var server in ServerSync.RemoteServers)
                server.Connection["serverSync.updateDoI"](serializedDoI);
        }

        internal void HandleLocalDoRChanged(object sender, EventArgs e)
        {
            string serializedDoR = StringSerialization.SerializeObject<IDomainOfResponsibility>(
                ServerSync.LocalServer.DoR);
            foreach (var server in ServerSync.RemoteServers)
                server.Connection["serverSync.updateDoR"](serializedDoR);
        }

        /// <summary>
        /// Updates domain-of-interest on the remote server associated with a given connection.
        /// </summary>
        /// <param name="connection">Connection to the remote server.</param>
        /// <param name="serializedDoI">New serialized domain-of-interest.</param>
        internal void HandleRemoteDoIChanged(Connection connection, string serializedDoI)
        {
            foreach (var server in ServerSync.RemoteServers)
            {
                if (server.Connection == connection)
                {
                    var newDoI = StringSerialization.DeserializeObject<IDomainOfInterest>(serializedDoI);
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
        internal void HandleRemoteDoRChanged(Connection connection, string serializedDoR)
        {
            foreach (var server in ServerSync.RemoteServers)
            {
                if (server.Connection == connection)
                {
                    var newDoR = StringSerialization.DeserializeObject<IDomainOfResponsibility>(serializedDoR);
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
            return StringSerialization.SerializeObject<IDomainOfInterest>(ServerSync.LocalServer.DoI);
        }

        /// <summary>
        /// Returns serialized DoR for this server.
        /// </summary>
        /// <returns>Serialized DoR.</returns>
        string GetDoR()
        {
            return StringSerialization.SerializeObject<IDomainOfResponsibility>(ServerSync.LocalServer.DoR);
        }
    }
}
