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
using FIVES;
using System.Collections.Generic;
using System.ServiceModel;
using System;
using System.Net.Sockets;

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
            Application.Controller.PluginsLoaded += HandlePluginsLoaded;
        }

        private void HandlePluginsLoaded(object sender, System.EventArgs e)
        {
            try
            {
                // Connect to the testing service and notify that server is ready.
                NetTcpBinding binding = new NetTcpBinding();
                EndpointAddress ep = new EndpointAddress(Testing.ServiceURI);
                ITestingService channel = ChannelFactory<ITestingService>.CreateChannel(binding, ep);
                channel.NotifyServerReady();
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

        }
    }
}
