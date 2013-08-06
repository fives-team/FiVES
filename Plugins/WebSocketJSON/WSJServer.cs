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

    public class WSJServer : WebSocketServer<WSJProtocol>, IWSJServer
    {
        public WSJServer(Action<IProtocol> onNewClient)
        {
            NewSessionConnected += (session) => onNewClient(session);
            NewMessageReceived += (session, value) => session.handleMessage(value);
            SessionClosed += (session, reason) => session.handleClose(reason);
        }
    }
}

