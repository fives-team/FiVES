using System;
using SuperWebSocket;
using System.Collections.Generic;
using System.Diagnostics;
using KIARA;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Logging;
using SuperWebSocket.Protocol;

namespace WebSocketJSON
{
    #region Testing
    public interface IWSJServer
    {
        bool Setup(string ip, int port, ISocketServerFactory socketServerFactory = null,
                   IReceiveFilterFactory<IWebSocketFragment> receiveFilterFactory = null, ILogFactory logFactory = null,
                   IEnumerable<IConnectionFilter> connectionFilters = null,
                   IEnumerable<ICommandLoader> commandLoaders = null);
        bool Start();
    }
    #endregion

    /// <summary>
    /// A simple WebSocket server based on SuperWebSocket library.
    /// </summary>
    public class WSJServer : WebSocketServer<WSJProtocol>, IWSJServer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketJSON.WSJServer"/> class.
        /// </summary>
        /// <param name="onNewClient">The handler to be called for each new client.</param>
        public WSJServer(Action<IProtocol> onNewClient)
        {
            NewSessionConnected += (session) => onNewClient(session);
            NewMessageReceived += (session, value) => session.HandleMessage(value);
            SessionClosed += (session, reason) => session.HandleClose(reason);
        }
    }
}

