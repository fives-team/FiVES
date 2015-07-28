// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using SINFONI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SINFONIPlugin
{
    public class SINFONIServerManager
    {
        public static SINFONIServerManager Instance;

        public string ServerURI { get; private set; }
        public string ServerPath { get; private set; }
        public int ServerPort { get; private set; }
        public string ServiceTransport { get; private set; }
        public string ServiceProtocol { get; private set; }
        public string ServiceHost { get; private set; }
        public int ServicePort { get; private set; }
        public SINFONIServer SinfoniServer { get; private set; }
        public ServiceImplementation SinfoniService { get; private set; }

        public SINFONIServerManager()
        {
            ReadConfig();
            RegisterModules();
            StartSinfoniServer();
        }

        private void ReadConfig()
        {
            string pluginConfigPath = this.GetType().Assembly.Location;
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
            ServiceHost = serviceConfig.Attributes["host"].Value;
            ServiceTransport = serviceConfig.Attributes["transport"].Value;
            ServiceProtocol = serviceConfig.Attributes["protocol"].Value;
            ServicePort = int.Parse(serviceConfig.Attributes["port"].Value);
        }

        private void RegisterModules()
        {
            var protocolPathNode = configDocument.SelectSingleNode("configuration/Paths/ProtocolPath");
            var transportPathNode = configDocument.SelectSingleNode("configuration/Paths/TransportPath");
            moduleLoader.LoadModulesFrom<IProtocol>(protocolPathNode.Attributes["value"].Value);
            moduleLoader.LoadModulesFrom<ITransport>(protocolPathNode.Attributes["value"].Value);
        }

        private void StartSinfoniServer()
        {
            SinfoniServer = new SINFONIServer(ServerURI, ServerPort, ServerPath, "fives.kiara");
            SinfoniService = SinfoniServer.StartService(ServiceHost, ServicePort, "/service/", ServiceTransport, ServiceProtocol);
        }

        private ModuleLoader moduleLoader = new ModuleLoader();
        private XmlDocument configDocument = new XmlDocument();
    }
}
