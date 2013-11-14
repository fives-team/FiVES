using System;
using KIARAPlugin;
using WebSocket4Net;

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

    public interface IWebSocket
    {
        event EventHandler Opened;
        event EventHandler Closed;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        void Open();
        void Close();
        void Send(string message);
    }

    public class WebSocketWrapper : WebSocket, IWebSocket
    {
        public WebSocketWrapper(string uri) : base(uri) { }
    }

    public interface IWebSocketFactory
    {
        IWebSocket Construct(string uri);
    }

    public class WebSocketFactory : IWebSocketFactory
    {
        public IWebSocket Construct(string uri)
        {
            return new WebSocketWrapper(uri);
        }
    }
}

