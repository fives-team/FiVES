using System;
using SuperWebSocket;
using System.Collections.Generic;
using System.Diagnostics;
using KIARA;

namespace WebSocketJSON
{
    public class WSJServer : WebSocketServer<WSJProtocol>
    {
        public WSJServer(Action<IProtocol> onNewClient)
        {
            NewSessionConnected += (session) => onNewClient(session);
            NewMessageReceived += (session, value) => session.handleMessage(value);
            SessionClosed += (session, reason) => session.handleClose(reason);
        }
    }
}

