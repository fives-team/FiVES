using System;
using KIARAPlugin;
using WebSocket4Net;
using SuperSocket.ClientEngine;

namespace WebSocketJSON
{
    public interface IWSJServerFactory
    {
        IWSJServer Construct(Action<Connection> onNewClient);
    }

    public class WSJServerFactory : IWSJServerFactory
    {
        public IWSJServer Construct(Action<Connection> onNewClient)
        {
            return new WSJServer(onNewClient);
        }
    }

    public interface IWSJFuncCallFactory
    {
        IWSJFuncCall Construct();
    }

    public class WSJFuncCallFactory : IWSJFuncCallFactory
    {
        public IWSJFuncCall Construct()
        {
            return new WSJFuncCall();
        }
    }

    public interface IWebSocketFactory
    {
        ISocket Construct(string uri);
    }

    public class WebSocketFactory : IWebSocketFactory
    {
        public ISocket Construct(string uri)
        {
            return new WebSocketSocketAdapter(uri);
        }
    }
}

