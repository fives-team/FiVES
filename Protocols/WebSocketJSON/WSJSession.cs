using System;
using KIARAPlugin;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Reflection;
using Dynamitey;
using System.Runtime.InteropServices;
using WebSocket4Net;
using NLog;

namespace WebSocketJSON
{
    /// <summary>
    /// WebSocketJSON session implementation. Contains Connection adapter for KIARA.
    /// </summary>
    public class WSJSession : WebSocketSession<WSJSession>
    {
        public event EventHandler Closed;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void HandleClosed(string reason)
        {
            if (Closed != null)
                Closed(this, new ClosedEventArgs(reason));

            logger.Warn("Connection closed: " + reason);
        }

        public void HandleMessageReceived(string message)
        {
            if (MessageReceived != null)
                MessageReceived(this, new MessageReceivedEventArgs(message));
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}

