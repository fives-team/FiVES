using KIARA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientManagerPlugin
{
    public class ClientConnectionEventArgs : EventArgs
    {
        public Connection ClientConnection { get; private set; }

        public ClientConnectionEventArgs(Connection connection)
        {
            ClientConnection = connection;
        }
    }
}
