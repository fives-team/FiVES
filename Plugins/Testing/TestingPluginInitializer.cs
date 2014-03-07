using FIVES;
using System.Collections.Generic;
using System.ServiceModel;
using System;

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
                // No endpoint means that we are not being run within a test. This is normal :-)
            }
            catch (TimeoutException)
            {
                // On Mono in Linux, TimeoutException is thrown when the testing server is shut down shortly after
                // receiving the NotifyServerReady call. This is normal as we close the service upong receiving the call
                // to reuse the endpoint address for other servers.
            }
        }

        public void Shutdown()
        {

        }
    }
}
