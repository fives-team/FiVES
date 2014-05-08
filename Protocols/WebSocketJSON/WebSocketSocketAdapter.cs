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
