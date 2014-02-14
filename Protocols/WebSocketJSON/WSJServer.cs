using System;
using SuperWebSocket;
using System.Collections.Generic;
using System.Diagnostics;
using KIARAPlugin;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Logging;
using SuperWebSocket.Protocol;
using SuperSocket.SocketBase.Config;

namespace WebSocketJSON
{
    #region Testing
    public interface IWSJServer
    {
        bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory = null,
            IReceiveFilterFactory<IWebSocketFragment> receiveFilterFactory = null, ILogFactory logFactory = null,
            IEnumerable<IConnectionFilter> connectionFilters = null,
            IEnumerable<SuperSocket.SocketBase.Command.ICommandLoader> commandLoaders = null);
        bool Start();
    }
    #endregion

    /// <summary>
    /// A simple WebSocket server based on SuperWebSocket library.
    /// </summary>
    public class WSJServer : WebSocketServer<WSJSession>, IWSJServer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketJSON.WSJServer"/> class.
        /// </summary>
        /// <param name="onNewClient">The handler to be called for each new client.</param>
        public WSJServer(Action<Connection> onNewClient)
        {
            NewSessionConnected += (session) =>
            {
                session.SocketSession.Closed += (genericSession, reason) => session.HandleClosed(reason.ToString());

                var socketAdapter = new WSJSessionSocketAdapter(session);
                onNewClient(new WSJConnection(socketAdapter));
            };
            NewMessageReceived += (session, value) => session.HandleMessageReceived(value);
        }
    }
}

