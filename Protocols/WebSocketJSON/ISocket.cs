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
    /// <summary>
    /// Raw socket interface for the WSJConnection class. Allows to replace underlying connection with custom socket
    /// or a mock class in tests.
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Raised when the connection is opened.
        /// </summary>
        event EventHandler Opened;

        /// <summary>
        /// Raised when the connection is closed.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Raised when an error is occured in the connection.
        /// </summary>
        event EventHandler<SocketErrorEventArgs> Error;

        /// <summary>
        /// Raised when a message is received by the connection.
        /// </summary>
        event EventHandler<MessageEventArgs> Message;

        /// <summary>
        /// Returns true if the socket is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Sends a message over this socket.
        /// </summary>
        /// <param name="message">Message to be sent.</param>
        void Send(string message);
    }
}
