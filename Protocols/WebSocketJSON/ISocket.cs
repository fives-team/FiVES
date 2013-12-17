using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketJSON
{
    public interface ISocket
    {
        event EventHandler Opened;
        event EventHandler Closed;
        event EventHandler<ErrorEventArgs> Error;
        event EventHandler<MessageEventArgs> Message;

        bool IsConnected { get; }

        void Open();
        void Close();
        void Send(string message);
    }
}
