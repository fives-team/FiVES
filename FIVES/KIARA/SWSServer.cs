using System;
using SuperWebSocket;
using KIARA;
using System.Collections.Generic;
using System.Diagnostics;

namespace KIARA
{
    public class SWSServer : WebSocketServer<WebSocketSession>
    {
        public SWSServer(Context.ClientHandler aClientHandler)
        {
            clientHandler = aClientHandler;

            NewMessageReceived += HandleNewMessageReceived;
            NewSessionConnected += HandleNewSessionConnected;
            SessionClosed += HandleSessionClosed;
        }

        private void HandleNewSessionConnected(WebSocketSession session)
        {
            Debug.Assert(!wrappers.ContainsKey(session));
            wrappers[session] = new SWSSessionWrapper(session);
            clientHandler(new Connection(wrappers[session]));
        }

        private void HandleSessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason reason)
        {
            Debug.Assert(wrappers.ContainsKey(session));
            wrappers[session].HandleClose();
        }

        private void HandleNewMessageReceived(WebSocketSession session, string value)
        {
            Debug.Assert(wrappers.ContainsKey(session));
            wrappers[session].HandleMessage(value);
        }

        private Context.ClientHandler clientHandler;
        private Dictionary<WebSocketSession, SWSSessionWrapper> wrappers = 
            new Dictionary<WebSocketSession, SWSSessionWrapper>();
    }

    internal class SWSSessionWrapper : IWebSocketJSONConnection 
    {
        internal SWSSessionWrapper(WebSocketSession aSession) {
            session = aSession;
        }

        // Event should be triggered on every new message.
        public event ConnectionMessageDelegate OnMessage;

        // Event should be triggered when the connection is closed.
        public event ConnectionCloseDelegate OnClose;

        // Event should be triggered when an error is encountered.
        public event ConnectionErrorDelegate OnError;

        // Sends a message.
        public bool Send(string message)
        {
            session.Send(message);
            return true;
        }

        // Receive a message.
        internal void HandleMessage(string message)
        {
            if (isListening)
                OnMessage(message);
            else
                cachedMessages.Add(message);
        }

        internal void HandleClose()
        {
            OnClose();
        }

        internal void HandleError(string reason)
        {
            OnError(reason);
        }

        // Starts receiving messages (and triggering OnMessage). Previous messages should be cached 
        // until this method is called.
        public void Listen()
        {
            if (!isListening) {
                isListening = true;
                cachedMessages.ForEach(OnMessage.Invoke);
                cachedMessages.Clear();
            }
        }

        private bool isListening = false;
        private List<string> cachedMessages = new List<string>();
        private WebSocketSession session;
    }
}

