using FIVES;
using System.Collections.Generic;
using System.ServiceModel;

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
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(Testing.ServiceURI);
            ITestingService channel = ChannelFactory<ITestingService>.CreateChannel(binding, ep);
            channel.NotifyServerReady();
        }

        public void Shutdown()
        {

        }
    }
}
