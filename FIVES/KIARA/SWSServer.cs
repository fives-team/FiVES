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

            NewMessageReceived += handleNewMessageReceived;
            NewSessionConnected += handleNewSessionConnected;
            SessionClosed += handleSessionClosed;
        }

        private void handleNewSessionConnected(WebSocketSession session)
        {
            Debug.Assert(!wrappers.ContainsKey(session));
            wrappers[session] = new SWSSessionWrapper(session);
            clientHandler(new Connection(wrappers[session]));
        }

        private void handleSessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason reason)
        {
            Debug.Assert(wrappers.ContainsKey(session));
            wrappers[session].handleClose();
        }

        private void handleNewMessageReceived(WebSocketSession session, string value)
        {
            Debug.Assert(wrappers.ContainsKey(session));
            wrappers[session].handleMessage(value);
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
        public bool send(string message)
        {
            session.Send(message);
            return true;
        }

        // Receive a message.
        internal void handleMessage(string message)
        {
            if (isListening)
                OnMessage(message);
            else
                cachedMessages.Add(message);
        }

        internal void handleClose()
        {
            OnClose();
        }

        internal void handleError(string reason)
        {
            OnError(reason);
        }

        // Starts receiving messages (and triggering OnMessage). Previous messages should be cached 
        // until this method is called.
        public void listen()
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

