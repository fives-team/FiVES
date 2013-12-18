using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using WebSocketJSON;

namespace BinaryProtocol
{
    /// <summary>
    /// Connection factory to create connections based on raw TCP sockets but using same JSON protocol as in
    /// WebSocketJSON project.
    /// </summary>
    public class BPConnectionFactory : IConnectionFactory
    {
        public void OpenConnection(Server serverConfig, Context context, Action<Connection> onConnected)
        {
            ValidateProtocolName(serverConfig);

            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", -1);
            string host = ProtocolUtils.retrieveProtocolSetting(serverConfig, "host", (string)null);

            if (port == -1 || host == null)
                throw new Error(ErrorCode.CONNECTION_ERROR, "No port and/or IP address is present in configuration.");

            var socketAdapter = new BPSocketAdapter(host, port);
            onConnected(new WSJConnection(socketAdapter));
            socketAdapter.Open();
        }

        public void StartServer(Server serverConfig, Context context, Action<Connection> onNewClient)
        {
            ValidateProtocolName(serverConfig);

            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", 34838);
            string host = ProtocolUtils.retrieveProtocolSetting(serverConfig, "host", "Any");

            IPAddress localaddr;

            if (host != "Any")
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(host);
                if (ipAddresses.Length == 0)
                    throw new Error(ErrorCode.CONNECTION_ERROR, "Cannot identify IP address by hostname.");
                localaddr = ipAddresses[0];  // we take first entry as it does not matter which one is used
            }
            else
            {
                localaddr = IPAddress.Any;
            }

            // Create connection listener.
            TcpListener listener = new TcpListener(localaddr, port);
            listener.Start();

            // Wait for the first connection asynchronously.
            var serverState = new AsyncServerState { Listener = listener, Callback = onNewClient };
            listener.BeginAcceptTcpClient(CreateBPConnection, serverState);
        }

        public string Name
        {
            get
            {
                return "binary-protocol";
            }
        }

        /// <summary>
        /// This is used as a callback to the BeginAcceptTcpClient on the TcpListener. It creates a new connection,
        /// invokes the user callback and continues to listen for new connections.
        /// </summary>
        /// <param name="ar"></param>
        private void CreateBPConnection(IAsyncResult ar)
        {
            AsyncServerState serverState = ar.AsyncState as AsyncServerState;
            if (serverState == null)
                throw new ArgumentException("Invalid async state in async result", "ar");
            
            // Create new connection and invoke the user callback.
            TcpClient client = serverState.Listener.EndAcceptTcpClient(ar);
            var socketAdapter = new BPSocketAdapter(client);
            serverState.Callback(new WSJConnection(socketAdapter));

            // Wait for the next connection attempt.
            serverState.Listener.BeginAcceptTcpClient(CreateBPConnection, serverState);
        }

        private void ValidateProtocolName(Server serverConfig)
        {
            string protocol = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "name", null);
            if (protocol != "binary-protocol")
                throw new Error(ErrorCode.CONNECTION_ERROR, "Given сonfig is not for binary-protocol protocol.");
        }
    }
}
