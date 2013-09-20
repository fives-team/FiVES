using System;

namespace KIARA
{
    /// <summary>
    /// The interface that should be implemented by the protocol factories. Used to initiate connections and construct
    /// <see cref="IProtocol"/> objects of the respective protocol.
    /// </summary>
    public interface IProtocolFactory
    {
        /// <summary>
        /// Opens a connection to the remote server specified by the <paramref name="serverConfig"/> and executes
        /// <paramref name="onConnected"/> when the connection is established.
        /// </summary>
        /// <param name="serverConfig">Server config.</param>
        /// <param name="onConnected">Callback to be called when the connection is established.</param>
        void OpenConnection(Server serverConfig, Context context, Action<IProtocol> onConnected);

        /// <summary>
        /// Starts the server listening for new clients according to the configuration in the
        /// <paramref name="serverConfig"/>. For each new client <paramref name="onNewClient"/> is called.
        /// </summary>
        /// <param name="serverConfig">Server config.</param>
        /// <param name="onNewClient">Callback to be called for each new client.</param>
        void StartServer(Server serverConfig, Context context, Action<IProtocol> onNewClient);

        /// <summary>
        /// Returns the name of the protocol.
        /// </summary>
        /// <returns>The name of the protocol.</returns>
        string GetName();
    }
}

