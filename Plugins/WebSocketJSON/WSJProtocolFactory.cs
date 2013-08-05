using System;
using KIARA;
using Newtonsoft.Json.Linq;

namespace WebSocketJSON
{
    public class WSJProtocolFactory : IProtocolFactory
    {
        public WSJProtocolFactory()
        {
        }

        public void openConnection(Server serverConfig, Action<IProtocol> onConnected)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void startServer(Server serverConfig, Action<IProtocol> onNewClient)
        {
            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", 34837);
            string ip = ProtocolUtils.retrieveProtocolSetting(serverConfig, "ip", "Any");

            WSJServer server = new WSJServer(onNewClient);
            server.Setup(ip, port);
            server.Start();
        }
    }
}

