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
using System.IO;
using Newtonsoft.Json;

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
            if (ReadConfig())
                Application.Controller.PluginsLoaded += HandlePluginsLoaded;

        }

        /// <summary>
        /// Converts a file name to the URI that point to the file as if it was located in the same directory as the
        /// current assembly.
        /// </summary>
        /// <param name="configFilename"></param>
        /// <returns></returns>
        private static string FindNextToAssembly(string filename)
        {
            string assemblyPath = typeof(TestingPluginInitializer).Assembly.Location;
            var configFullPath = Path.Combine(Path.GetDirectoryName(assemblyPath), filename);
            return configFullPath;
        }

        private bool ReadConfig()
        {
            string configFilePath = FindNextToAssembly("testing.json");
            if (!File.Exists(configFilePath))
                return false;

            Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText(configFilePath));
            if (!config.ContainsKey("clientURI") || !config.ContainsKey("serverURI"))
                return false;

            clientURI = config["clientURI"];
            serverURI = config["serverURI"];
            return true;
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
