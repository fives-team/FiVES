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
using FIVES;
using System.Collections.Generic;
using System.ServiceModel;
using System;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Configuration;

namespace TestingPlugin
{
    public class TestingPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "Testing";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
            var testingConfig = ConfigurationManager.GetSection("Testing") as NameValueCollection;
            if (testingConfig != null )
            {
                clientURI = testingConfig.Get("ClientURI");
                serverURI = testingConfig.Get("ServerURI");

                if (clientURI != null && serverURI != null)
                    Application.Controller.PluginsLoaded += HandlePluginsLoaded;
            }

        }

        private void HandlePluginsLoaded(object sender, System.EventArgs e)
        {
            try
            {
                testingServer = new TestingServer(Application.Controller);
                serviceHost = new ServiceHost(testingServer);
                serviceHost.AddServiceEndpoint(typeof(ITestingServer), new NetTcpBinding(), serverURI);
                serviceHost.Open();

                // Connect to the testing service and notify that server is ready.
                NetTcpBinding binding = new NetTcpBinding();
                EndpointAddress ep = new EndpointAddress(clientURI);
                ITestingClient channel = ChannelFactory<ITestingClient>.CreateChannel(binding, ep);
                channel.NotifyServerReady(serverURI);
            }
            catch (EndpointNotFoundException)
            {
                // No endpoint means that we are not being run within a test on Windows. This is normal :-)
            }
            catch (SocketException)
            {
                // Socket exception means that we are not being run within a test on Linux. This is normal :-)
            }
        }

        public void Shutdown()
        {
            serviceHost.Close();
        }

        private static ServiceHost serviceHost;
        private static TestingServer testingServer;

        private string clientURI;
        private string serverURI;
    }
}
