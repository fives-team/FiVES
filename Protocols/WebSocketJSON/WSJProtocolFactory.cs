using System;
using KIARAPlugin;
using Newtonsoft.Json.Linq;

namespace WebSocketJSON
{
    /// <summary>
    /// WebSocketJSON protocol factory implementation.
    /// </summary>
    public class WSJProtocolFactory : IProtocolFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketJSON.WSJProtocolFactory"/> class.
        /// </summary>
        public WSJProtocolFactory() : this(new WSJServerFactory()) {}

        #region IProtocolFactory implementation

        public void OpenConnection(Server serverConfig, Context context, Action<IProtocol> onConnected)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void StartServer(Server serverConfig, Context context, Action<IProtocol> onNewClient)
        {
            string protocol = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "name", null);
            if (protocol != "websocket-json")
                throw new Error(ErrorCode.CONNECTION_ERROR, "Given сonfig is not for websocket-json protocol.");

            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", 34837);
            string ip = ProtocolUtils.retrieveProtocolSetting(serverConfig, "ip", "Any");

            IWSJServer server = wsjServerFactory.Construct(onNewClient);
            server.Setup(ip, port);
            server.Start();
        }

        public string GetName()
        {
            return "websocket-json";
        }

        #endregion

        #region Testing

        internal WSJProtocolFactory(IWSJServerFactory factory)
        {
            wsjServerFactory = factory;
        }

        IWSJServerFactory wsjServerFactory;

        #endregion
    }
}

