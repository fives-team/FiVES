using KIARAPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace BinaryProtocol
{
    class AsyncServerState
    {
        public TcpListener Listener;
        public Action<Connection> Callback;
    }
}
