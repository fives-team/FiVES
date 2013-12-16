using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;

namespace WebSocketJSON
{
    public class WebSocketSocketAdapter : WebSocket, ISocket
    {
        public WebSocketSocketAdapter(string uri)
            : base(uri)
        {
            MessageReceived += HandleMessageReceived;
            Error += HandleError;
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            if (Error != null)
                Error(sender, new ErrorEventArgs(e.Exception));
        }

        void HandleMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (Message != null)
                Message(sender, new MessageEventArgs(e.Message));
        }

        public event EventHandler<MessageEventArgs> Message;
        public new event EventHandler<ErrorEventArgs> Error;
    }
}
