using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketJSON
{
    class WSJSessionSocketAdapter : ISocket
    {
        public WSJSessionSocketAdapter(WSJSession aSession)
        {
            session = aSession;
            session.Closed += HandleClosed;
            session.MessageReceived += HandleMessageReceived;
        }

        void HandleMessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            if (Message != null)
                Message(sender, new MessageEventArgs(e.Message));
        }

        void HandleClosed(object sender, EventArgs e)
        {
            if (Closed != null)
                Closed(sender, e);
        }

        public event EventHandler Closed;

        public event EventHandler<MessageEventArgs> Message;

        public bool IsConnected
        {
            get { return session.Connected; }
        }

        public void Close()
        {
            session.Close();
        }

        public void Send(string message)
        {
            session.Send(message);
        }

        WSJSession session;

        #region This part of the interface is only used for client sockets
        event EventHandler ISocket.Opened
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler<SocketErrorEventArgs> ISocket.Error
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        void ISocket.Open()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
