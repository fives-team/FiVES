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
