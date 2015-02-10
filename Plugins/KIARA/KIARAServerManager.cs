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

using FiVESJson;
using KIARA;
using KIARA.Protocols.JsonRPC;
using KIARA.Transport.WebSocketTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KIARAPlugin
{
    public class KIARAServerManager
    {
        public static KIARAServerManager Instance;

        public string ServerURI { get; private set; }
        public string ServerPath { get; private set; }
        public int ServerPort { get; private set; }
        public string ServiceTransport { get; private set; }
        public string ServiceProtocol { get; private set; }
        public int ServicePort { get; private set; }
        public KIARAServer KiaraServer { get; private set; }
        public ServiceImplementation KiaraService { get; private set; }
        public KIARAServerManager()
        {
            ReadConfig();
            RegisterModules();
            StartKiaraServer();
        }

        private void ReadConfig()
        {
            string pluginConfigPath = this.GetType().Assembly.Location;
            XmlDocument configDocument = new XmlDocument();
            configDocument.Load(pluginConfigPath + ".config");
            var serverConfig = configDocument.SelectSingleNode("configuration/ServerConfiguration");

            ReadConnectionListenerConfig(serverConfig);
            ReadServiceConfiguration(serverConfig);
        }

        private void ReadConnectionListenerConfig(XmlNode serverConfig)
        {
            var listenerConfig = serverConfig.SelectSingleNode("ConnectionListener");
            ServerURI = listenerConfig.Attributes["host"].Value;
            ServerPort = int.Parse(listenerConfig.Attributes["port"].Value);
            ServerPath = listenerConfig.Attributes["path"].Value;
        }

        private void ReadServiceConfiguration(XmlNode serverConfig)
        {
            var serviceConfig = serverConfig.SelectSingleNode("ServiceConfiguration");
            ServiceTransport = serviceConfig.Attributes["transport"].Value;
            ServiceProtocol = serviceConfig.Attributes["protocol"].Value;
            ServicePort = int.Parse(serviceConfig.Attributes["port"].Value);
        }

        private void RegisterModules()
        {
            var JsonRPCProtocol = new JsonRpcProtocol();
            var FiVESJsonProtocol = new FiVESJsonProtocol();
            var WebsocketTransport = new WebSocketTransport();
            KIARA.ProtocolRegistry.Instance.RegisterProtocol(JsonRPCProtocol);
            KIARA.ProtocolRegistry.Instance.RegisterProtocol(FiVESJsonProtocol);
            KIARA.TransportRegistry.Instance.RegisterTransport(WebsocketTransport);
        }

        private void StartKiaraServer()
        {
            KiaraServer = new KIARAServer(ServerURI, ServerPort, ServerPath, "fives.kiara");
            KiaraService = KiaraServer.StartService(ServerURI, ServicePort, "/service/", ServiceTransport, ServiceProtocol);
        }
    }
}
