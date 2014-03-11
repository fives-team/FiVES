using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using SuperSocket.ClientEngine;

namespace WebSocketJSON
{
    public class WebSocketSocketAdapter : WebSocket, ISocket
    {
        public WebSocketSocketAdapter(string uri)
            : base(uri)
        {
            base.MessageReceived += HandleMessageReceived;
            base.Error += HandleError;
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            if (Error != null)
                Error(sender, new SocketErrorEventArgs(e.Exception));
        }

        void HandleMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (Message != null)
                Message(sender, new MessageEventArgs(e.Message));
        }

        public event EventHandler<MessageEventArgs> Message;
        public new event EventHandler<SocketErrorEventArgs> Error;

        public bool IsConnected
        {
            get { return base.State == WebSocketState.Open; }
        }
    }
}
