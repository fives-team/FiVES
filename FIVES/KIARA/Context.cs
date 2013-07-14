using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace KIARA
{
    public class Context
    {
        public delegate void ClientHandler(Connection connection);

        public void AcceptClient(string idlURL, ClientHandler handler)
        {
            // TODO(rryk): Load IDL from idlURL, load info about port number.
            // TODO(rryk): Listen for new clients on that port number. For each client execute handler
            // on a new thread.
            throw new NotImplementedException("AcceptClient is not implemented");
        }
    }
}
