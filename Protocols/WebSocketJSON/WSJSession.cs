// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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

            if (!reason.Equals("ClientClosing"))
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

