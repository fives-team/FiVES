using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace KIARA
{
    public class Context
    {
        public delegate void ClientHandler(Connection connection);

        public void acceptClients(string service, ClientHandler handler)
        {
            // TODO(rryk): Retrieve port number from the confiuration for |service|.
            int port = 34837;

            // Start the server.
            SWSServer server = new SWSServer(handler);
            server.Setup(port);
            server.Start();
        }
    }
}
