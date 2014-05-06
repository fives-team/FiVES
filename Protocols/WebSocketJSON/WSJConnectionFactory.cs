// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using KIARAPlugin;
using Newtonsoft.Json.Linq;
using WebSocket4Net;
using System.Net;
using NLog;
using SuperSocket.SocketBase.Config;

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

            ISocket socket = webSocketFactory.Construct("ws://" + host + ":" + port + "/");
            socket.Opened += (sender, e) => onConnected(new WSJConnection(socket));
            socket.Error += (sender, e) => {
                logger.WarnException("Error in connection to " + host + ":" + port, e.Exception);
            };
            socket.Open();
        }

        public void StartServer(Server serverConfig, Context context, Action<Connection> onNewClient)
        {
            ValidateProtocolName(serverConfig);

            ServerConfig config = new ServerConfig();
            config.Port = ProtocolUtils.retrieveProtocolSetting(serverConfig, "port", 34837);
            config.MaxRequestLength = 100000;

            string host = ProtocolUtils.retrieveProtocolSetting(serverConfig, "host", "Any");
            if (host != "Any")
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(host);
                if (ipAddresses.Length == 0)
                    throw new Error(ErrorCode.CONNECTION_ERROR, "Cannot identify IP address by hostname.");
                config.Ip = ipAddresses[0].ToString();  // we take first entry as it does not matter which one is used
            }
            else
            {
                config.Ip = "Any";
            }

            IWSJServer server = wsjServerFactory.Construct(onNewClient);
            server.Setup(config);
            server.Start();
        }

        public string Name
        {
            get
            {
                return "websocket-json";
            }
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

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}

