using System;
using KIARAPlugin;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

namespace WebSocketJSON
{
    /// <summary>
    /// WebSocketJSON connection factory implementation.
    /// </summary>
    public class WSJConnectionFactory : IConnectionFactory
    {
        #region IConnectionFactory implementation

        public void OpenConnection(Server serverConfig, Context context, Action<Connection> onConnected)
        {
            // TODO: Tests
            string protocol = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "name", null);
            if (protocol != "websocket-json")
                throw new Error(ErrorCode.CONNECTION_ERROR, "Given сonfig is not for websocket-json protocol.");

            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", 34837);
            string ip = ProtocolUtils.retrieveProtocolSetting(serverConfig, "ip", "Any");

            WebSocket socket = new WebSocket("ws://" + ip + ":" + port + "/");
            socket.Opened += (sender, e) => onConnected(new WSJConnection(socket));
            socket.Open();
        }

        public void StartServer(Server serverConfig, Context context, Action<Connection> onNewClient)
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

        internal IWSJServerFactory wsjServerFactory = new WSJServerFactory();
    }
}

