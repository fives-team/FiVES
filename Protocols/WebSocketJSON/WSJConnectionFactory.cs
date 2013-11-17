using System;
using KIARAPlugin;
using Newtonsoft.Json.Linq;
using WebSocket4Net;
using System.Net;

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
            ValidateProtocolName(serverConfig);

            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", -1);
            string host = ProtocolUtils.retrieveProtocolSetting(serverConfig, "host", (string)null);

            if (port == -1 || host == null)
                throw new Error(ErrorCode.CONNECTION_ERROR, "No port and/or IP address is present in configuration.");

            IWebSocket socket = webSocketFactory.Construct("ws://" + host + ":" + port + "/");
            socket.Opened += (sender, e) => onConnected(new WSJConnection(socket));
            socket.Open();
        }

        public void StartServer(Server serverConfig, Context context, Action<Connection> onNewClient)
        {
            ValidateProtocolName(serverConfig);

            int port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", 34837);
            string host = ProtocolUtils.retrieveProtocolSetting(serverConfig, "host", "Any");
            IPAddress[] ipAddresses =  Dns.GetHostAddresses(host);

            if (ipAddresses.Length == 0)
                throw new Error(ErrorCode.CONNECTION_ERROR, "Cannot identify IP address by hostname.");

            IWSJServer server = wsjServerFactory.Construct(onNewClient);
            server.Setup(ipAddresses[0].ToString(), port);  // we take first entry as it does not matter which one to take
            server.Start();
        }

        public string GetName()
        {
            return "websocket-json";
        }

        private void ValidateProtocolName(Server serverConfig)
        {
            string protocol = ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "name", null);
            if (protocol != "websocket-json")
                throw new Error(ErrorCode.CONNECTION_ERROR, "Given сonfig is not for websocket-json protocol.");
        }

        #endregion

        internal IWSJServerFactory wsjServerFactory = new WSJServerFactory();
        internal IWebSocketFactory webSocketFactory = new WebSocketFactory();
    }
}

