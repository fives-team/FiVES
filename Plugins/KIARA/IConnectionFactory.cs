// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace KIARAPlugin
{
    /// <summary>
    /// The interface that should be implemented by the connection factories in protocols. Used to initiate connections 
    /// and construct <see cref="Connection"/> objects of the respective protocol.
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// Opens a connection to the remote server specified by the <paramref name="serverConfig"/> and executes
        /// <paramref name="onConnected"/> when the connection is established.
        /// </summary>
        /// <param name="serverConfig">Server config.</param>
        /// <param name="onConnected">Callback to be called when the connection is established.</param>
        void OpenConnection(Server serverConfig, Context context, Action<Connection> onConnected);

        /// <summary>
        /// Starts the server listening for new clients according to the configuration in the
        /// <paramref name="serverConfig"/>. For each new client <paramref name="onNewClient"/> is called.
        /// </summary>
        /// <param name="serverConfig">Server config.</param>
        /// <param name="onNewClient">Callback to be called for each new client.</param>
        void StartServer(Server serverConfig, Context context, Action<Connection> onNewClient);

        /// <summary>
        /// Returns the name of the protocol.
        /// </summary>
        /// <returns>The name of the protocol.</returns>
        string Name { get; }
    }
}

