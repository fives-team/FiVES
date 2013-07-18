using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Linq;

namespace KIARA
{
    public class Context
    {
        public delegate void ClientHandler(Connection connection);

        public void AcceptClient(string idlURL, ClientHandler handler)
        {
            // TODO(rryk): Retrieve port number from the confiuration in idlURL.
            int port = Int32.Parse(ConfigurationManager.AppSettings["ServerPort"]);

            // Start the server.
            SWSServer server = new SWSServer(handler);
            server.Setup(port);
            server.Start();
        }
    }
}
