using System;
using KIARA;
using Newtonsoft.Json.Linq;

namespace WebSocketJSON
{
    public class WSJProtocolFactory : IProtocolFactory
    {
        public WSJProtocolFactory() : this(new WSJServerFactory()) {}

        public void openConnection(Server serverConfig, Action<IProtocol> onConnected)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void startServer(Server serverConfig, Action<IProtocol> onNewClient)
        {
            string protocol = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "name", null);
            if (protocol != "websocket-json")
                throw new Error(ErrorCode.CONNECTION_ERROR, "Given сonfig is not for websocket-json protocol.");

            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", 34837);
            string ip = ProtocolUtils.retrieveProtocolSetting(serverConfig, "ip", "Any");

            IWSJServer server = wsjServerFactory.construct(onNewClient);
            server.Setup(ip, port);
            server.Start();
        }

        #region Testing

        internal WSJProtocolFactory(IWSJServerFactory factory)
        {
            wsjServerFactory = factory;
        }

        IWSJServerFactory wsjServerFactory;

        #endregion
    }
}

